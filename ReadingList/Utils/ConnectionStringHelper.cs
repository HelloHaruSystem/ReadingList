using Microsoft.Extensions.Configuration;
using Terminal.Gui;

namespace ReadingList.Utils;

public static class ConnectionStringHelper
{
    public static string GetRequiredConnectionString(IConfiguration configuration, string name)
    {
        string? connectionString = configuration.GetConnectionString(name);

        return connectionString
            ?? throw new ArgumentNullException(nameof(configuration), $"Connection string '{name}' is Missing.");
    }
}