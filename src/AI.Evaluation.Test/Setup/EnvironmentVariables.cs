namespace AI.Evaluation.Test.Setup;

public class EnvironmentVariables
{
    private static readonly IDictionary<string, string> s_environmentVariableCache = new Dictionary<string, string>();

    private static string GetEnvironmentVariable(string variableName)
    {
        if (!s_environmentVariableCache.TryGetValue(variableName, out string? value))
        {
            value =
                Environment.GetEnvironmentVariable(variableName) ??
                throw new Exception($"Environment variable {variableName} not set.");

            s_environmentVariableCache[variableName] = value;
        }

        return value;
    }

    public static string OllamaEndpoint
        => GetEnvironmentVariable("EVAL_SAMPLE_OLLAMA_ENDPOINT");

    public static string OllamaModel
        => GetEnvironmentVariable("EVAL_SAMPLE_OLLAMA_MODEL");

    public static string StorageRootPath
    {
        get
        {
            string storageRootPath = GetEnvironmentVariable("EVAL_SAMPLE_STORAGE_ROOT_PATH");
            storageRootPath = Path.GetFullPath(storageRootPath);
            Directory.CreateDirectory(storageRootPath);
            return storageRootPath;
        }
    }
}
