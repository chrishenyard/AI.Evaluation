using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using OllamaSharp;

namespace AI.Evaluation.Test.Setup;

public class TestSetup
{
    private static ChatConfiguration? chatConfiguration;

    public static ChatConfiguration GetChatConfiguration()
    {
        chatConfiguration ??= GetOllamaChatConfiguration();

        return chatConfiguration;
    }


    private static ChatConfiguration GetOllamaChatConfiguration()
    {
        IChatClient client =
            new OllamaApiClient(
                new Uri(EnvironmentVariables.OllamaEndpoint),
                defaultModel: EnvironmentVariables.OllamaModel);

        client = client.AsBuilder().UseFunctionInvocation().Build();

        return new ChatConfiguration(client);
    }
}
