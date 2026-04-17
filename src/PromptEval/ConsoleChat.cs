using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace PromptEval;

/// <summary>
/// This is the main application service.
/// This takes console input, then sends it to the configured AI service, and then prints the response.
/// All conversation history is maintained in the chat history.
/// </summary>
internal class ConsoleChat(Kernel kernel, IHostApplicationLifetime lifeTime) : IHostedService
{
    private readonly Kernel _kernel = kernel;
    private readonly IHostApplicationLifetime _lifeTime = lifeTime;

    /// <summary>
    /// Start the service.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ExecuteAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop a running service.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// The main execution loop. It will use any of the available plugins to perform actions
    /// </summary>
    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ChatHistory chatMessages = [];
        chatMessages.AddSystemMessage("Reply in plain natural language. Do not output JSON unless explicitly requested.");

        IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        // Loop till we are cancelled
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Get user input
                Console.Write("User > ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                chatMessages.AddUserMessage(userInput);

                // Get the chat completions
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.None()
                };

                IAsyncEnumerable<StreamingChatMessageContent> result =
                    chatCompletionService.GetStreamingChatMessageContentsAsync(
                        chatMessages,
                        executionSettings: null,
                        kernel: _kernel,
                        cancellationToken: cancellationToken);

                // Print and collect a single assistant message
                var assistantText = new StringBuilder();
                var printedPrefix = false;

                await foreach (var content in result.WithCancellation(cancellationToken))
                {
                    if (!printedPrefix && content.Role.HasValue)
                    {
                        Console.Write("Assistant > ");
                        printedPrefix = true;
                    }

                    if (!string.IsNullOrEmpty(content.Content))
                    {
                        Console.Write(content.Content);
                        assistantText.Append(content.Content);
                    }
                }

                Console.WriteLine();

                if (assistantText.Length > 0)
                {
                    chatMessages.AddAssistantMessage(assistantText.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
