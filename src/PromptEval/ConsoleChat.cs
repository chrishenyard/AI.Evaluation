using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using System.Text;

namespace PromptEval;

/// <summary>
/// Interactive console chat service.
/// Reads user input, sends it to the configured chat completion service,
/// streams the assistant response back to the console, and maintains chat history.
/// </summary>
internal sealed class ConsoleChat(
    Kernel kernel,
    IHostApplicationLifetime lifeTime,
    ILogger<ConsoleChat> logger) : IHostedService
{
    private readonly Kernel _kernel = kernel;
    private readonly IHostApplicationLifetime _lifeTime = lifeTime;
    private readonly ILogger<ConsoleChat> _logger = logger;

    private static readonly Style UserStyle = new(Color.Cyan1);
    private static readonly Style AssistantStyle = new(Color.SpringGreen3);
    private static readonly Style BannerStyle = new(Color.Grey);
    private static readonly Style InfoStyle = new(Color.Yellow);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => ExecuteWithSpinnerAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Console chat service is stopping.");
        return Task.CompletedTask;
    }

    private async Task ExecuteWithSpinnerAsync(CancellationToken cancellationToken)
    {
        var chatMessages = new ChatHistory();
        chatMessages.AddSystemMessage("Reply in plain natural language. Do not output JSON unless explicitly requested.");

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        WriteBanner();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Markup("[bold cyan]User > [/]"));

                var userInput = Console.ReadLine();

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                userInput = userInput.Trim();

                if (IsExitCommand(userInput))
                {
                    _logger.LogInformation("User requested application shutdown.");
                    AnsiConsole.MarkupLine("[grey]Goodbye.[/]");
                    _lifeTime.StopApplication();
                    break;
                }

                chatMessages.AddUserMessage(userInput);

                var assistantText = new StringBuilder();
                string? firstChunk = null;

                await using var enumerator = chatCompletionService
                    .GetStreamingChatMessageContentsAsync(
                        chatMessages,
                        executionSettings: null,
                        kernel: _kernel,
                        cancellationToken: cancellationToken)
                    .GetAsyncEnumerator(cancellationToken);

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("yellow"))
                    .StartAsync("[yellow]Assistant is thinking...[/]", async _ =>
                    {
                        while (await enumerator.MoveNextAsync())
                        {
                            var content = enumerator.Current;

                            if (!string.IsNullOrWhiteSpace(content.Content))
                            {
                                firstChunk = content.Content;
                                break;
                            }
                        }
                    });

                AnsiConsole.Write(new Markup("[bold springgreen3]Assistant > [/]"));

                if (!string.IsNullOrEmpty(firstChunk))
                {
                    AnsiConsole.Write(new Text(firstChunk, AssistantStyle));
                    assistantText.Append(firstChunk);

                    while (await enumerator.MoveNextAsync())
                    {
                        var content = enumerator.Current;

                        if (string.IsNullOrWhiteSpace(content.Content))
                        {
                            continue;
                        }

                        AnsiConsole.Write(new Text(content.Content, AssistantStyle));
                        assistantText.Append(content.Content);
                    }

                    AnsiConsole.WriteLine();
                    chatMessages.AddAssistantMessage(assistantText.ToString());
                }
                else
                {
                    AnsiConsole.Write(new Text("(No response)", InfoStyle));
                    AnsiConsole.WriteLine();
                    _logger.LogWarning("The assistant returned no content.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Console chat loop cancelled.");
                break;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Markup("[bold springgreen3]Assistant > [/]"));
                AnsiConsole.Write(new Text("Sorry, something went wrong.", AssistantStyle));
                AnsiConsole.WriteLine();

                _logger.LogError(ex, "Unhandled exception in chat loop.");
            }
        }
    }

    private static bool IsExitCommand(string input) =>
        input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("q", StringComparison.OrdinalIgnoreCase);

    private static void WriteBanner()
    {
        var rule = new Rule("[bold white]Interactive Chat[/]");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);

        AnsiConsole.Write(new Text("Type your message and press Enter.", BannerStyle));
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Text("Type 'exit' or 'quit' to close the application.", BannerStyle));
        AnsiConsole.WriteLine();
    }
}