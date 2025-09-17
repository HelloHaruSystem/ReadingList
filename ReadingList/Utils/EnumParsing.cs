using ReadingList.Models.Enums;

namespace ReadingList.Utils;

public static class EnumParsing
{
    public static ReadingStatus ParseReadingStatusFromDatabase(string statusString)
    {
        return statusString.ToLower() switch
        {
            "to_read" => ReadingStatus.ToRead,
            "currently_reading" => ReadingStatus.CurrentlyReading,
            "completed" => ReadingStatus.Completed,
            "paused" => ReadingStatus.Paused,
            "abandoned" => ReadingStatus.Abandoned,
            _ => ReadingStatus.ToRead
        };
    }
    
    public static string ConvertReadingStatusToDatabase(ReadingStatus status)
    {
        return status switch
        {
            ReadingStatus.ToRead => "to_read",
            ReadingStatus.CurrentlyReading => "currently_reading", 
            ReadingStatus.Completed => "completed",
            ReadingStatus.Paused => "paused",
            ReadingStatus.Abandoned => "abandoned",
            _ => "to_read"
        };
    }
}