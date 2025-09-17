using ReadingList.Models;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class ReadingGoalsView : BaseView
{
    private readonly IReadingGoalService _readingGoalService;
    private readonly NavigationManager _navigationManager;
    private ListView _goalsListView;
    private Label _goalsLabel;
    private Label _selectedGoalLabel;
    private Label _progressLabel;
    private List<ReadingGoal> _goals;
    private ReadingGoal? _selectedGoal;

    public ReadingGoalsView(
        IReadingGoalService readingGoalService,
        NavigationManager navigationManager) 
        : base("Reading Goals")
    {
        _readingGoalService = readingGoalService;
        _navigationManager = navigationManager;
        _goals = new List<ReadingGoal>();

        SetNavigationManager(navigationManager);
        SetupUI();
    }

    protected override void SetupUI()
    {
        // Goals list section in a frame
        FrameView goalsFrame = new FrameView("Your Goals")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = 12
        };

        _goalsLabel = new Label("Your reading goals:")
        {
            X = 1,
            Y = 0
        };

        _goalsListView = new ListView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2
        };

        _goalsListView.SelectedItemChanged += OnGoalHighlighted;
        _goalsListView.OpenSelectedItem += OnGoalSelected;

        goalsFrame.Add(_goalsLabel, _goalsListView);

        // Goal details and progress section
        FrameView detailsFrame = new FrameView("Goal Details & Progress")
        {
            X = 1,
            Y = Pos.Bottom(goalsFrame),
            Width = Dim.Fill() - 2,
            Height = 10
        };

        _selectedGoalLabel = new Label("No goal selected")
        {
            X = 1,
            Y = 0
        };

        _progressLabel = new Label("")
        {
            X = 1,
            Y = 2
        };

        Button viewDetailsButton = new Button("View Full Details")
        {
            X = 1,
            Y = 6
        };

        viewDetailsButton.Clicked += async () => await ShowGoalDetailsAsync();

        Button markCompleteButton = new Button("Mark Complete")
        {
            X = Pos.Right(viewDetailsButton) + 2,
            Y = 6
        };

        markCompleteButton.Clicked += async () => await MarkGoalCompleteAsync();

        detailsFrame.Add(_selectedGoalLabel, _progressLabel, viewDetailsButton, markCompleteButton);

        // Action buttons section
        FrameView actionFrame = new FrameView("Actions")
        {
            X = 1,
            Y = Pos.Bottom(detailsFrame),
            Width = Dim.Fill() - 2,
            Height = 6
        };

        Button createGoalButton = new Button("Create New Goal")
        {
            X = 1,
            Y = 1
        };

        createGoalButton.Clicked += () => ShowCreateGoalDialog();

        Button backButton = new Button("Back")
        {
            X = Pos.Right(createGoalButton) + 2,
            Y = 1
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Button refreshButton = new Button("Refresh")
        {
            X = 1,
            Y = 3
        };

        refreshButton.Clicked += async () => await LoadGoalsAsync();

        actionFrame.Add(createGoalButton, backButton, refreshButton);

        Add(goalsFrame, detailsFrame, actionFrame);
    }

    public override async void OnViewActivated()
    {
        await LoadGoalsAsync();
    }

    private async Task LoadGoalsAsync()
    {
        try
        {
            IEnumerable<ReadingGoal> goals = await _readingGoalService.GetActiveGoalsAsync();
            _goals = goals.ToList();

            if (_goals.Count == 0)
            {
                _goalsLabel.Text = "No active reading goals. Create your first goal!";
                _goalsListView.SetSource(new string[0]);
                _selectedGoal = null;
                UpdateSelectedGoalDisplay();
            }
            else
            {
                _goalsLabel.Text = $"Your active reading goals ({_goals.Count} total):";
                string[] goalDisplayItems = _goals.Select(FormatGoalForDisplay).ToArray();
                _goalsListView.SetSource(goalDisplayItems);
                
                // Set first item as selected by default
                _goalsListView.SelectedItem = 0;
                _selectedGoal = _goals[0];
                await UpdateSelectedGoalDisplayAsync();
            }

            SetNeedsDisplay();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load reading goals: {ex.Message}");
        }
    }

    private string FormatGoalForDisplay(ReadingGoal goal)
    {
        string deadline = goal.Deadline.ToString("MMM dd, yyyy");
        string targets = "";
        
        if (goal.TargetBooks.HasValue && goal.TargetPages.HasValue)
        {
            targets = $"{goal.TargetBooks} books, {goal.TargetPages} pages";
        }
        else if (goal.TargetBooks.HasValue)
        {
            targets = $"{goal.TargetBooks} books";
        }
        else if (goal.TargetPages.HasValue)
        {
            targets = $"{goal.TargetPages} pages";
        }
        
        return $"{goal.GoalName} - {targets} by {deadline}";
    }

    private void OnGoalHighlighted(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _goals.Count)
        {
            _selectedGoal = _goals[args.Item];
            Task.Run(UpdateSelectedGoalDisplayAsync);
        }
        else
        {
            _selectedGoal = null;
            UpdateSelectedGoalDisplay();
        }
    }

    private void OnGoalSelected(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _goals.Count)
        {
            _selectedGoal = _goals[args.Item];
            Task.Run(UpdateSelectedGoalDisplayAsync);
        }
    }

    private async Task UpdateSelectedGoalDisplayAsync()
    {
        if (_selectedGoal != null)
        {
            _selectedGoalLabel.Text = $"Selected: {_selectedGoal.GoalName}";
            
            try
            {
                GoalProgress? progress = await _readingGoalService.GetGoalProgressAsync(_selectedGoal.Id);
                
                if (progress != null)
                {
                    string progressText = "";
                    
                    if (progress.Goal.TargetBooks.HasValue)
                    {
                        progressText += $"Books: {progress.BooksAdded}/{progress.Goal.TargetBooks} ({progress.BookProgressPercentage:F1}%)\n";
                    }
                    
                    if (progress.Goal.TargetPages.HasValue)
                    {
                        progressText += $"Pages: {progress.TotalPages}/{progress.Goal.TargetPages} ({progress.PageProgressPercentage:F1}%)\n";
                    }
                    
                    if (progress.IsGoalAchieved)
                    {
                        progressText += "Status: ACHIEVED! 🎉";
                    }
                    else
                    {
                        TimeSpan timeLeft = progress.Goal.Deadline - DateTime.Now;
                        if (timeLeft.TotalDays > 0)
                        {
                            progressText += $"Time left: {timeLeft.Days} days";
                        }
                        else
                        {
                            progressText += "Status: OVERDUE";
                        }
                    }
                    
                    _progressLabel.Text = progressText;
                }
                else
                {
                    _progressLabel.Text = "Could not load progress";
                }
            }
            catch (Exception ex)
            {
                _progressLabel.Text = $"Error loading progress: {ex.Message}";
            }
        }
        else
        {
            UpdateSelectedGoalDisplay();
        }
        
        SetNeedsDisplay();
    }

    private void UpdateSelectedGoalDisplay()
    {
        _selectedGoalLabel.Text = "No goal selected";
        _progressLabel.Text = "";
        SetNeedsDisplay();
    }

    private async Task ShowGoalDetailsAsync()
    {
        if (_selectedGoal == null)
        {
            ShowMessage("Goal Details", "Please select a goal first.");
            return;
        }

        try
        {
            GoalProgress? progress = await _readingGoalService.GetGoalProgressAsync(_selectedGoal.Id);
            
            if (progress == null)
            {
                ShowError("Could not load goal details.");
                return;
            }

            string details = $"Goal: {progress.Goal.GoalName}\n\n";
            
            if (!string.IsNullOrWhiteSpace(progress.Goal.Description))
            {
                details += $"Description: {progress.Goal.Description}\n\n";
            }
            
            details += $"Start Date: {progress.Goal.StartDate:yyyy-MM-dd}\n";
            details += $"Deadline: {progress.Goal.Deadline:yyyy-MM-dd}\n\n";
            
            if (progress.Goal.TargetBooks.HasValue)
            {
                details += $"Book Target: {progress.BooksAdded}/{progress.Goal.TargetBooks} ({progress.BookProgressPercentage:F1}%)\n";
            }
            
            if (progress.Goal.TargetPages.HasValue)
            {
                details += $"Page Target: {progress.TotalPages}/{progress.Goal.TargetPages} ({progress.PageProgressPercentage:F1}%)\n";
            }
            
            details += $"\nStatus: {(progress.IsGoalAchieved ? "ACHIEVED" : "In Progress")}";

            MessageBox.Query(80, 18, "Goal Details", details, "OK");
        }
        catch (Exception ex)
        {
            ShowError($"Error loading goal details: {ex.Message}");
        }
    }

    private async Task MarkGoalCompleteAsync()
    {
        if (_selectedGoal == null)
        {
            ShowMessage("Mark Complete", "Please select a goal first.");
            return;
        }

        if (_selectedGoal.IsCompleted)
        {
            ShowMessage("Mark Complete", "This goal is already marked as completed.");
            return;
        }

        bool confirm = ShowConfirmation("Mark Complete", 
            $"Mark '{_selectedGoal.GoalName}' as completed?\n\nThis will remove it from your active goals list.");

        if (!confirm)
            return;

        try
        {
            bool success = await _readingGoalService.MarkGoalCompleteAsync(_selectedGoal.Id);
            
            if (success)
            {
                ShowMessage("Success", $"'{_selectedGoal.GoalName}' has been marked as completed!");
                await LoadGoalsAsync(); // Refresh the list
            }
            else
            {
                ShowError("Failed to mark goal as completed.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error marking goal complete: {ex.Message}");
        }
    }

    private void ShowCreateGoalDialog()
    {
        // For now, show a placeholder message
        // In a real implementation, you'd create a new dialog or view for goal creation
        ShowMessage("Create Goal", "Goal creation dialog - Coming soon!\n\nFor now, you can add goals directly to the database or we can implement a creation form in the next iteration.");
    }
}