using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Tui.Configuration;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class StatisticsView : BaseView
{
    private readonly IReadingListService _readingListService;
    private readonly IReadingGoalService _readingGoalService;
    private readonly Label _overviewLabel = new Label();
    private readonly Label _statusBreakdownLabel = new Label();
    private readonly Label _recentActivityLabel = new Label();
    private readonly Label _goalsLabel = new Label();
    private readonly ListView _topRatedListView = new ListView();
    private List<UserBook> _topRatedBooks;

    public StatisticsView(
        NavigationManager navigationManager,
        IReadingListService readingListService,
        IReadingGoalService readingGoalService) 
        : base("Reading Statistics", navigationManager)
    {
        _readingListService = readingListService;
        _readingGoalService = readingGoalService;
        _topRatedBooks = new List<UserBook>();
        
        SetupUI();
    }

    protected override void SetupUI()
    {
        // Overview statistics frame
        FrameView overviewFrame = new FrameView("Reading Overview")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = UiConstants.Frames.MediumFrameHeight - 2
        };

        _overviewLabel.X = 1;
        _overviewLabel.Y = 0;
        _overviewLabel.Width = Dim.Fill() - 2;
        _overviewLabel.Height = Dim.Fill() - 1;

        overviewFrame.Add(_overviewLabel);

        // Status breakdown frame
        FrameView statusFrame = new FrameView("Reading Status Breakdown")
        {
            X = 1,
            Y = Pos.Bottom(overviewFrame),
            Width = Dim.Percent(50),
            Height = UiConstants.Frames.MediumFrameHeight
        };

        _statusBreakdownLabel.X = 1;
        _statusBreakdownLabel.Y = 0;
        _statusBreakdownLabel.Width = Dim.Fill() - 2;
        _statusBreakdownLabel.Height = Dim.Fill() - 1;

        statusFrame.Add(_statusBreakdownLabel);

        // Goals progress frame
        FrameView goalsFrame = new FrameView("Goals Progress")
        {
            X = Pos.Right(statusFrame),
            Y = Pos.Bottom(overviewFrame),
            Width = Dim.Fill() - 1,
            Height = 8
        };

        _goalsLabel.X = 1;
        _goalsLabel.Y = 0;
        _goalsLabel.Width = Dim.Fill() - 2;
        _goalsLabel.Height = Dim.Fill() - 1;

        goalsFrame.Add(_goalsLabel);

        // Top rated books frame
        FrameView topRatedFrame = new FrameView("Top Rated Books")
        {
            X = 1,
            Y = Pos.Bottom(statusFrame),
            Width = Dim.Percent(65),
            Height = UiConstants.Frames.MediumFrameHeight + 2
        };

        Label topRatedHeaderLabel = new Label("Your highest rated books:")
        {
            X = 1,
            Y = 0
        };

        _topRatedListView.X = 1;
        _topRatedListView.Y = 1;
        _topRatedListView.Width = Dim.Fill() - 2;
        _topRatedListView.Height = Dim.Fill() - 2;

        _topRatedListView.OpenSelectedItem += OnTopRatedBookSelected;

        topRatedFrame.Add(topRatedHeaderLabel, _topRatedListView);

        // Recent activity frame
        FrameView recentFrame = new FrameView("Recent Activity")
        {
            X = Pos.Right(topRatedFrame),
            Y = Pos.Bottom(goalsFrame),
            Width = Dim.Fill() - 1,
            Height = 10
        };

        _recentActivityLabel.X = 1;
        _recentActivityLabel.Y = 0;
        _recentActivityLabel.Width = Dim.Fill() - 2;
        _recentActivityLabel.Height = Dim.Fill() - 1;

        recentFrame.Add(_recentActivityLabel);

        // Action buttons frame
        FrameView actionFrame = new FrameView("Actions")
        {
            X = 1,
            Y = Pos.Bottom(topRatedFrame),
            Width = Dim.Fill() - 2,
            Height = UiConstants.Frames.SmallFrameHeight
        };

        Button refreshButton = new Button("Refresh")
        {
            X = 1,
            Y = 1
        };

        refreshButton.Clicked += async () => await LoadStatisticsAsync();

        Button backButton = new Button("Back")
        {
            X = Pos.Right(refreshButton) + 2,
            Y = 1
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        actionFrame.Add(refreshButton, backButton);

        Add(overviewFrame, statusFrame, goalsFrame, topRatedFrame, recentFrame, actionFrame);
    }

    public override async void OnViewActivated()
    {
        await LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Load all reading data concurrently
            var allBooksTask = _readingListService.GetMyReadingListAsync();
            var recentCompletedTask = _readingListService.GetRecentlyCompletedBooksAsync(UiConstants.DataLimits.DefaultRecentCompletedCount);
            var topRatedTask = _readingListService.GetTopRatedBooksAsync(5);
            var activeGoalsTask = _readingGoalService.GetActiveGoalsAsync();

            await Task.WhenAll(allBooksTask, recentCompletedTask, topRatedTask, activeGoalsTask);

            var allBooks = allBooksTask.Result.ToList();
            var recentCompleted = recentCompletedTask.Result.ToList();
            var topRated = topRatedTask.Result.ToList();
            var activeGoals = activeGoalsTask.Result.ToList();

            // Update overview statistics
            UpdateOverviewStatistics(allBooks);

            // Update status breakdown
            UpdateStatusBreakdown(allBooks);

            // Update goals progress
            await UpdateGoalsProgress(activeGoals);

            // Update top rated books
            UpdateTopRatedBooks(topRated);

            // Update recent activity
            UpdateRecentActivity(recentCompleted);

            SetNeedsDisplay();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load statistics: {ex.Message}");
        }
    }

    private void UpdateOverviewStatistics(List<UserBook> allBooks)
    {
        int totalBooks = allBooks.Count;
        int completedBooks = allBooks.Count(b => b.Status == ReadingStatus.Completed);
        int currentlyReading = allBooks.Count(b => b.Status == ReadingStatus.CurrentlyReading);
        
        // Calculate total pages read (only for completed books with page counts)
        int totalPagesRead = allBooks
            .Where(b => b.Status == ReadingStatus.Completed && b.Book.Pages.HasValue)
            .Sum(b => b.Book.Pages.GetValueOrDefault());

        // Calculate average rating
        var ratedBooks = allBooks.Where(b => b.PersonalRating.HasValue).ToList();
        double averageRating = ratedBooks.Any() ? ratedBooks.Average(b => b.PersonalRating.GetValueOrDefault()) : 0;

        string overviewText = $"Total Books in List: {totalBooks}\n";
        overviewText += $"Books Completed: {completedBooks}\n";
        overviewText += $"Currently Reading: {currentlyReading}\n";
        overviewText += $"Total Pages Read: {totalPagesRead:N0}\n";
        
        if (ratedBooks.Any())
        {
            overviewText += $"Average Rating: {averageRating:F1}/5 ({ratedBooks.Count} rated books)";
        }
        else
        {
            overviewText += "Average Rating: No books rated yet";
        }

        _overviewLabel.Text = overviewText;
    }

    private void UpdateStatusBreakdown(List<UserBook> allBooks)
    {
        var statusCounts = allBooks
            .GroupBy(b => b.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        string statusText = "";
        foreach (ReadingStatus status in Enum.GetValues<ReadingStatus>())
        {
            int count = statusCounts.GetValueOrDefault(status, 0);
            if (count > 0)
            {
                string statusName = GetStatusDisplayName(status);
                statusText += $"{statusName}: {count}\n";
            }
        }

        if (string.IsNullOrEmpty(statusText))
        {
            statusText = "No books in your reading list";
        }

        _statusBreakdownLabel.Text = statusText.TrimEnd();
    }

    private async Task UpdateGoalsProgress(List<ReadingGoal> activeGoals)
    {
        if (!activeGoals.Any())
        {
            _goalsLabel.Text = "No active reading goals";
            return;
        }

        string goalsText = $"Active Goals: {activeGoals.Count}\n\n";
        
        int achievedGoals = 0;
        foreach (var goal in activeGoals.Take(UiConstants.DataLimits.MaxGoalsDisplayed)) // Show progress for first 3 goals
        {
            try
            {
                GoalProgress? progress = await _readingGoalService.GetGoalProgressAsync(goal.Id);
                if (progress != null)
                {
                    if (progress.IsGoalAchieved)
                        achievedGoals++;
                    
                    goalsText += $"{goal.GoalName}:\n";
                    
                    if (goal.TargetBooks.HasValue)
                    {
                        goalsText += $"  Books: {progress.BooksAdded}/{goal.TargetBooks}\n";
                    }
                    
                    if (goal.TargetPages.HasValue)
                    {
                        goalsText += $"  Pages: {progress.TotalPages}/{goal.TargetPages}\n";
                    }
                    
                    goalsText += "\n";
                }
            }
            catch
            {
                // Skip this goal if progress can't be loaded
            }
        }

        if (activeGoals.Count > 3)
        {
            goalsText += $"... and {activeGoals.Count - 3} more";
        }

        _goalsLabel.Text = goalsText.TrimEnd();
    }

    private void UpdateTopRatedBooks(List<UserBook> topRated)
    {
        _topRatedBooks = topRated; // Store for selection events
        
        if (!topRated.Any())
        {
            _topRatedListView.SetSource(new string[] { "No rated books yet" });
            return;
        }

        string[] topRatedItems = topRated.Select(book => 
        {
            string year = book.Book.PublicationYear?.ToString() ?? "N/A";
            return $"{book.Book.Title} ({year}) - {book.PersonalRating}/5";
        }).ToArray();

        _topRatedListView.SetSource(topRatedItems);
    }

    private void UpdateRecentActivity(List<UserBook> recentCompleted)
    {
        if (!recentCompleted.Any())
        {
            _recentActivityLabel.Text = "No recently completed books";
            return;
        }

        string activityText = "Recently Completed:\n\n";
        
        foreach (var book in recentCompleted.Take(UiConstants.DataLimits.MaxRecentActivityItems)) // Show last 4 completed
        {
            string rating = book.PersonalRating?.ToString() ?? "Not rated";
            activityText += $"{book.Book.Title}\n";
            activityText += $"  Completed: {book.UpdatedAt:MMM dd}\n";
            activityText += $"  Rating: {rating}/5\n\n";
        }

        _recentActivityLabel.Text = activityText.TrimEnd();
    }

    private string GetStatusDisplayName(ReadingStatus status)
    {
        return status switch
        {
            ReadingStatus.ToRead => "To Read",
            ReadingStatus.CurrentlyReading => "Currently Reading",
            ReadingStatus.Completed => "Completed",
            ReadingStatus.Paused => "Paused",
            ReadingStatus.Abandoned => "Abandoned",
            _ => "Unknown"
        };
    }

    private void OnTopRatedBookSelected(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _topRatedBooks.Count)
        {
            UserBook selectedBook = _topRatedBooks[args.Item];
            ShowBookDetails(selectedBook);
        }
    }

    private void ShowBookDetails(UserBook userBook)
    {
        string status = GetStatusDisplayName(userBook.Status);
        string rating = userBook.PersonalRating?.ToString() ?? "Not rated";
        string notes = !string.IsNullOrWhiteSpace(userBook.PersonalNotes) 
            ? userBook.PersonalNotes 
            : "No notes";
        string year = userBook.Book.PublicationYear?.ToString() ?? "N/A";
        string pages = userBook.Book.Pages?.ToString() ?? "N/A";

        string details = $"Title: {userBook.Book.Title}\n\n" +
                        $"Publication Year: {year}\n" +
                        $"Pages: {pages}\n\n" +
                        $"Reading Status: {status}\n" +
                        $"Your Rating: {rating}/5\n" +
                        $"Date Started: {userBook.DateStarted:yyyy-MM-dd}\n" +
                        $"Last Updated: {userBook.UpdatedAt:yyyy-MM-dd}\n\n" +
                        $"Your Notes:\n{notes}\n\n" +
                        $"Description:\n{userBook.Book.Description ?? "No description available."}";

        MessageBox.Query(80, 20, "Book Details", details, "OK");
    }
}