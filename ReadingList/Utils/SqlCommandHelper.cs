using Microsoft.Data.SqlClient;

namespace ReadingList.Utils;

public static class SqlCommandHelper
{
    public static void AddParameterWithNullCheck<T>(SqlCommand command, string parameterName, T? value)
    {
        command.Parameters.AddWithValue(parameterName, value ?? (object)DBNull.Value);
    }
}