using ReadingList.Models.Enums;

namespace ReadingList.Models;

public class UserBook
{
    public int Id { get; set; }
    public required string BookISBN { get; set; }
    public ReadingStatus Status { get; set; }
    public int? PersonalRating { get; set; }
    public string? PersonalNotes { get; set; }
    public DateTime DateStarted { get; set; }
    public DateTime UpdatedAt { get; set; }

    // nav
    public required Book Book { get; set; }
}