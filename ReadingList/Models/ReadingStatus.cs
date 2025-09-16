namespace ReadingList.Models;

public enum ReadingStatus : byte
{
    ToRead = 0,
    CurrentlyReading = 1,
    Completed = 2,
    Paused = 3,
    Abandoned = 4
}