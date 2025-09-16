using Microsoft.Data.SqlClient;

namespace ReadingList.Utils;

public static class SqlDataReaderHelper
{
    public static string GetString(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.GetString(ordinal);
    }

    public static string? GetStringOrNull(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static int GetInt(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.GetInt32(ordinal);
    }

    public static int? GetIntOrNull(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static DateTime GetDateTime(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.GetDateTime(ordinal);
    }

    public static DateTime? GetDateTimeOrNull(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    public static bool GetBool(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.GetBoolean(ordinal);
    }

    public static bool IsNull(SqlDataReader reader, string columnName)
    {
        return reader.IsDBNull(reader.GetOrdinal(columnName));
    }
}