using Spectre.Console;

namespace PromptEval.ChatStrategies;

internal static class ConsoleStyles
{
    public static readonly Style AssistantStyle = new(Color.SpringGreen3);
    public static readonly Style BannerStyle = new(Color.Grey);
    public static readonly Style InfoStyle = new(Color.Yellow);
}
