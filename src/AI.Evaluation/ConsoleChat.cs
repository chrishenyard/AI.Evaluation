using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using PromptEval.ChatStrategies;

namespace PromptEval;

internal sealed class ConsoleChat(
    Kernel kernel,
    IHostApplicationLifetime lifeTime,
    ILogger<ConsoleChat> logger) : IHostedService
{
    private readonly Kernel _kernel = kernel;
    private readonly IHostApplicationLifetime _lifeTime = lifeTime;
    private readonly ILogger<ConsoleChat> _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => ChatSession.ExecuteChatSessionAsync(
            _kernel, _lifeTime, _logger, cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Console chat service is stopping.");
        return Task.CompletedTask;
    }
}