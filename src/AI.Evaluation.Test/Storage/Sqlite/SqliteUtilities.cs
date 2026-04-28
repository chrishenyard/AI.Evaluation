using System.Globalization;

namespace AI.Evaluation.Test.Storage.Sqlite;

public static class SqliteUtilities
{
    private const string ISO8601DateFormat = "o";

    public static string GetConnectionString(string databaseFilePath)
        => $"Data Source={Path.GetFullPath(databaseFilePath)}";

    public static string ToISO8601DateString(this DateTime dateTime)
        => dateTime.ToString(ISO8601DateFormat, CultureInfo.InvariantCulture);

    public static object ToDbNullIfNull(string? value)
        => (object?)value ?? DBNull.Value;
}
