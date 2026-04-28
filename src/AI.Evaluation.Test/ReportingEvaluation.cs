using AI.Evaluation.Test.Setup;
using DotNetEnv;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace AI.Evaluation.Test.Reporting;

[TestClass]
public partial class ReportingEvaluation
{
    [ClassInitialize]
    public static async Task InitializeAsync(TestContext _)
    {
        Env.TraversePath().Load();
    }

    public TestContext? TestContext { get; set; }

    private static string? executionName;
    public static string ExecutionName
    {
        get
        {
            executionName ??= $"{DateTime.Now:yyyyMMddTHHmmss}";

            return executionName;
        }
    }

    private string ScenarioName => $"{TestContext!.FullyQualifiedTestClassName}.{TestContext.TestName}";
    private static readonly ChatConfiguration chatConfiguration = TestSetup.GetChatConfiguration();

    private static IEnumerable<string> GetTags(string storageKind = "Disk")
    {
        foreach (string tag in GetGlobalTags(storageKind))
        {
            yield return tag;
        }

        ChatClientMetadata? metadata = chatConfiguration.ChatClient.GetService<ChatClientMetadata>();

        yield return $"Provider: {metadata?.ProviderName ?? "Unknown"}";
        yield return $"Model: {metadata?.DefaultModelId ?? "Unknown"}";
    }

    private static IEnumerable<string> GetGlobalTags(string storageKind = "Disk")
    {
        yield return $"Execution: {ExecutionName}";
        yield return $"Storage: {storageKind}";
    }
}
