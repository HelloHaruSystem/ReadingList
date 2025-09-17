using ReadingList.Models;
using ReadingList.Services;
using ReadingList.Tui.Configuration;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class ReadingGoalsView : BaseView
{
    private readonly IReadingGoalService _readingGoalService;
    private readonly IBookService _bookService;
    private readonly ListView _goalsListView = new ListView();
    private readonly Label _goalsLabel = new Label("Your reading goals:");
    private readonly Label _selectedGoalLabel = new Label("No goal selected");
    private readonly Label _progressLabel = new Label("");
    private List<ReadingGoal> _goals;
    private ReadingGoal? _selectedGoal;

    public ReadingGoalsView(
        NavigationManager navigationManager,
        IReadingGoalService readingGoalService,
        IBookService bookService) 
        : base("Reading Goals", navigationManager)
    {
        _readingGoalService = readingGoalService;
        _bookService = bookService;
        _goals = new List<ReadingGoal>();

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
            Height = UiConstants.Frames.LargeFrameHeight
        };

        _goalsLabel.X = 1;
        _goalsLabel.Y = 0;

        _goalsListView.X = 1;
        _goalsListView.Y = 1;
        _goalsListView.Width = Dim.Fill() - 2;
        _goalsListView.Height = Dim.Fill() - 2;

        _goalsListView.SelectedItemChanged += OnGoalHighlighted;
        _goalsListView.OpenSelectedItem += OnGoalSelected;

        goalsFrame.Add(_goalsLabel, _goalsListView);

        // Goal details and progress section
        FrameView detailsFrame = new FrameView("Goal Details & Progress")
        {
            X = 1,
            Y = Pos.Bottom(goalsFrame),
            Width = Dim.Fill() - 2,
            Height = UiConstants.Frames.MediumFrameHeight + 2
        };

        _selectedGoalLabel.X = 1;
        _selectedGoalLabel.Y = 0;

        _progressLabel.X = 1;
        _progressLabel.Y = 2;

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

        Button addBookButton = new Button("Add Book to Goal")
        {
            X = Pos.Right(markCompleteButton) + 2,
            Y = 6
        };

        addBookButton.Clicked += () => ShowAddBookToGoalDialog();

        detailsFrame.Add(_selectedGoalLabel, _progressLabel, viewDetailsButton, markCompleteButton, addBookButton);

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
            Y = UiConstants.Frames.DefaultMargin
        };

        createGoalButton.Clicked += () => ShowCreateGoalDialog();

        Button browseForGoalButton = new Button("Browse Books for Goal")
        {
            X = Pos.Right(createGoalButton) + 2,
            Y = UiConstants.Frames.DefaultMargin
        };

        browseForGoalButton.Clicked += () => ShowBrowseBooksForGoalDialog();

        Button backButton = new Button("Back")
        {
            X = 1,
            Y = 3
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Button refreshButton = new Button("Refresh")
        {
            X = Pos.Right(backButton) + 2,
            Y = 3
        };

        refreshButton.Clicked += async () => await LoadGoalsAsync();

        actionFrame.Add(createGoalButton, browseForGoalButton, backButton, refreshButton);

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
                
                if (progress?.Goal == null) // Check both progress and Goal
                {
                    ShowError("Could not load goal details.");
                    return;
                }
                
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
                        progressText += "Status: ACHIEVED!";
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
            
            if (progress?.Goal == null)
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
        Dialog dialog = new Dialog("Create New Reading Goal", UiConstants.Dialogs.LargeWidth, UiConstants.Dialogs.ExtraLargeHeight);
        
        // Goal name
        Label nameLabel = new Label("Goal Name:")
        {
            X = 1,
            Y = 1
        };
        
        TextField nameField = new TextField()
        {
            X = Pos.Right(nameLabel) + 2,
            Y = 1,
            Width = UiConstants.Controls.LargeTextFieldWidth
        };
        
        // Description
        Label descLabel = new Label("Description:")
        {
            X = 1,
            Y = 3
        };
        
        TextField descField = new TextField()
        {
            X = Pos.Right(descLabel) + 2,
            Y = 3,
            Width = UiConstants.Controls.LargeTextFieldWidth
        };
        
        // Start date
        Label startLabel = new Label("Start Date:")
        {
            X = 1,
            Y = 5
        };
        
        TextField startField = new TextField(DateTime.Now.ToString("yyyy-MM-dd"))
        {
            X = Pos.Right(startLabel) + 2,
            Y = 5,
            Width = UiConstants.Controls.DateFieldWidth
        };
        
        // End date
        Label endLabel = new Label("Deadline:")
        {
            X = 1,
            Y = 7
        };
        
        TextField endField = new TextField(DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd"))
        {
            X = Pos.Right(endLabel) + 2,
            Y = 7,
            Width = UiConstants.Controls.DateFieldWidth
        };
        
        // Target books
        Label booksLabel = new Label("Target Books:")
        {
            X = 1,
            Y = 9
        };
        
        TextField booksField = new TextField()
        {
            X = Pos.Right(booksLabel) + 2,
            Y = 9,
            Width = UiConstants.Controls.NumberFieldWidth
        };
        
        Label booksHint = new Label("(leave empty if no book target)")
        {
            X = Pos.Right(booksField) + 1,
            Y = 9
        };
        
        // Target pages
        Label pagesLabel = new Label("Target Pages:")
        {
            X = 1,
            Y = 11
        };
        
        TextField pagesField = new TextField()
        {
            X = Pos.Right(pagesLabel) + 2,
            Y = 11,
            Width = UiConstants.Controls.NumberFieldWidth
        };
        
        Label pagesHint = new Label("(leave empty if no page target)")
        {
            X = Pos.Right(pagesField) + 1,
            Y = 11
        };
        
        // Buttons
        Button createButton = new Button("Create Goal")
        {
            X = 1,
            Y = 14
        };
        
        Button cancelButton = new Button("Cancel")
        {
            X = Pos.Right(createButton) + 2,
            Y = 14
        };
        
        createButton.Clicked += async () =>
        {
            try
            {
                // Validate required fields
                string goalName = nameField.Text?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(goalName))
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Goal name is required.", "OK");
                    return;
                }
                
                // Parse dates
                if (!DateTime.TryParse(startField.Text?.ToString(), out DateTime startDate))
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Invalid start date format. Use YYYY-MM-DD.", "OK");
                    return;
                }
                
                if (!DateTime.TryParse(endField.Text?.ToString(), out DateTime endDate))
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Invalid deadline format. Use YYYY-MM-DD.", "OK");
                    return;
                }
                
                if (endDate <= startDate)
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Deadline must be after start date.", "OK");
                    return;
                }
                
                // Parse optional targets
                int? targetBooks = null;
                string booksText = booksField.Text?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(booksText))
                {
                    if (!int.TryParse(booksText, out int books) || books <= 0)
                    {
                        MessageBox.ErrorQuery(50, 7, "Validation Error", "Target books must be a positive number.", "OK");
                        return;
                    }
                    targetBooks = books;
                }
                
                int? targetPages = null;
                string pagesText = pagesField.Text?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(pagesText))
                {
                    if (!int.TryParse(pagesText, out int pages) || pages <= 0)
                    {
                        MessageBox.ErrorQuery(50, 7, "Validation Error", "Target pages must be a positive number.", "OK");
                        return;
                    }
                    targetPages = pages;
                }
                
                // At least one target must be specified
                if (!targetBooks.HasValue && !targetPages.HasValue)
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "At least one target (books or pages) must be specified.", "OK");
                    return;
                }
                
                // Create the goal
                ReadingGoal newGoal = new ReadingGoal
                {
                    GoalName = goalName,
                    Description = descField.Text?.ToString()?.Trim(),
                    StartDate = startDate,
                    Deadline = endDate,
                    TargetBooks = targetBooks,
                    TargetPages = targetPages,
                    IsCompleted = false
                };
                
                int goalId = await _readingGoalService.CreateGoalAsync(newGoal);
                
                if (goalId > 0)
                {
                    Application.RequestStop(dialog);
                    MessageBox.Query(50, 8, "Success", $"Reading goal '{goalName}' created successfully!", "OK");
                    await LoadGoalsAsync(); // Refresh the goals list
                }
                else
                {
                    MessageBox.ErrorQuery(50, 7, "Error", "Failed to create reading goal.", "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(60, 8, "Error", $"Error creating goal: {ex.Message}", "OK");
            }
        };
        
        cancelButton.Clicked += () => Application.RequestStop(dialog);
        
        dialog.Add(nameLabel, nameField, descLabel, descField, 
                  startLabel, startField, endLabel, endField,
                  booksLabel, booksField, booksHint,
                  pagesLabel, pagesField, pagesHint,
                  createButton, cancelButton);
        
        Application.Run(dialog);
    }

    private void ShowBrowseBooksForGoalDialog()
    {
        Dialog dialog = new Dialog("Browse Books - Create Goal", UiConstants.Dialogs.VeryLargeWidth, UiConstants.Dialogs.MaxHeight);
        
        // Search section
        Label searchLabel = new Label("Search or browse books to add to your new goal:")
        {
            X = 1,
            Y = 1
        };
        
        TextField searchField = new TextField()
        {
            X = 1,
            Y = 2,
            Width = UiConstants.Controls.LargeTextFieldWidth
        };
        
        Button searchButton = new Button("Search")
        {
            X = Pos.Right(searchField) + 2,
            Y = 2
        };

        Button browseAllButton = new Button("Browse All")
        {
            X = Pos.Right(searchButton) + 2,
            Y = 2
        };
        
        // Results section
        Label resultsLabel = new Label("Click 'Browse All' to see all books, or search for specific titles:")
        {
            X = 1,
            Y = 4
        };
        
        ListView booksListView = new ListView()
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill() - 2,
            Height = UiConstants.Frames.LargeFrameHeight
        };
        
        // Selected books section
        Label selectedLabel = new Label("Books selected for goal (0):")
        {
            X = 1,
            Y = 18
        };

        ListView selectedBooksListView = new ListView()
        {
            X = 1,
            Y = 19,
            Width = Dim.Fill() - 2,
            Height = UiConstants.Frames.SmallFrameHeight
        };
        
        List<Book> searchResults = new List<Book>();
        List<Book> selectedBooks = new List<Book>();
        
        void UpdateSelectedBooksDisplay()
        {
            selectedLabel.Text = $"Books selected for goal ({selectedBooks.Count}):";
            if (selectedBooks.Count > 0)
            {
                string[] selectedItems = selectedBooks.Select(book => 
                {
                    string year = book.PublicationYear?.ToString() ?? "N/A";
                    return $"{book.Title} ({year})";
                }).ToArray();
                selectedBooksListView.SetSource(selectedItems);
            }
            else
            {
                selectedBooksListView.SetSource(new string[] { "No books selected yet" });
            }
            dialog.SetNeedsDisplay();
        }
        
        // Search functionality
        searchButton.Clicked += async () =>
        {
            string searchTerm = searchField.Text?.ToString()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.ErrorQuery(40, 7, "Search", "Please enter a search term.", "OK");
                return;
            }
            
            try
            {
                resultsLabel.Text = "Searching...";
                dialog.SetNeedsDisplay();
                
                IEnumerable<Book> results = await _bookService.SearchBooksAsync(searchTerm);
                searchResults = results?.ToList() ?? new List<Book>();
                
                if (searchResults.Count == 0)
                {
                    resultsLabel.Text = $"No books found for '{searchTerm}'";
                    booksListView.SetSource(new string[0]);
                }
                else
                {
                    resultsLabel.Text = $"Found {searchResults.Count} books - click to select for your goal:";
                    string[] bookItems = searchResults.Select(book => 
                    {
                        string year = book.PublicationYear?.ToString() ?? "N/A";
                        string pages = book.Pages?.ToString() ?? "N/A";
                        return $"{book.Title} ({year}) - {pages} pages";
                    }).ToArray();
                    booksListView.SetSource(bookItems);
                }
                
                dialog.SetNeedsDisplay();
            }
            catch (Exception ex)
            {
                resultsLabel.Text = "Search failed";
                MessageBox.ErrorQuery(50, 8, "Error", $"Search error: {ex.Message}", "OK");
            }
        };

        // Browse all functionality
        browseAllButton.Clicked += async () =>
        {
            try
            {
                resultsLabel.Text = "Loading all books...";
                dialog.SetNeedsDisplay();
                
                IEnumerable<Book> allBooks = await _bookService.GetAllBooksAsync();
                searchResults = allBooks?.ToList() ?? new List<Book>();
                
                resultsLabel.Text = $"All books ({searchResults.Count}) - click to select for your goal:";
                string[] bookItems = searchResults.Select(book => 
                {
                    string year = book.PublicationYear?.ToString() ?? "N/A";
                    string pages = book.Pages?.ToString() ?? "N/A";
                    return $"{book.Title} ({year}) - {pages} pages";
                }).ToArray();
                booksListView.SetSource(bookItems);
                
                dialog.SetNeedsDisplay();
            }
            catch (Exception ex)
            {
                resultsLabel.Text = "Browse failed";
                MessageBox.ErrorQuery(50, 8, "Error", $"Browse error: {ex.Message}", "OK");
            }
        };
        
        // Handle book selection from search results
        booksListView.OpenSelectedItem += (args) =>
        {
            if (args.Item >= 0 && args.Item < searchResults.Count)
            {
                Book bookToAdd = searchResults[args.Item];
                
                // Check if book is already selected
                if (selectedBooks.Any(b => b.ISBN == bookToAdd.ISBN))
                {
                    MessageBox.Query(50, 7, "Already Selected", $"'{bookToAdd.Title}' is already in your goal selection.", "OK");
                    return;
                }
                
                selectedBooks.Add(bookToAdd);
                UpdateSelectedBooksDisplay();
                MessageBox.Query(50, 7, "Book Added", $"'{bookToAdd.Title}' added to your goal selection!", "OK");
            }
        };

        // Handle Enter key in search field
        searchField.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                Task.Run(async () =>
                {
                    string searchTerm = searchField.Text?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        Application.MainLoop.Invoke(() => 
                        {
                            MessageBox.ErrorQuery(40, 7, "Search", "Please enter a search term.", "OK");
                        });
                        return;
                    }
                    
                    try
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            resultsLabel.Text = "Searching...";
                            dialog.SetNeedsDisplay();
                        });
                        
                        IEnumerable<Book> results = await _bookService.SearchBooksAsync(searchTerm);
                        searchResults = results?.ToList() ?? new List<Book>();
                        
                        Application.MainLoop.Invoke(() =>
                        {
                            if (searchResults.Count == 0)
                            {
                                resultsLabel.Text = $"No books found for '{searchTerm}'";
                                booksListView.SetSource(new string[0]);
                            }
                            else
                            {
                                resultsLabel.Text = $"Found {searchResults.Count} books - click to select for your goal:";
                                string[] bookItems = searchResults.Select(book => 
                                {
                                    string year = book.PublicationYear?.ToString() ?? "N/A";
                                    string pages = book.Pages?.ToString() ?? "N/A";
                                    return $"{book.Title} ({year}) - {pages} pages";
                                }).ToArray();
                                booksListView.SetSource(bookItems);
                            }
                            
                            dialog.SetNeedsDisplay();
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            resultsLabel.Text = "Search failed";
                            MessageBox.ErrorQuery(50, 8, "Error", $"Search error: {ex.Message}", "OK");
                        });
                    }
                });
                args.Handled = true;
            }
        };
        
        // Buttons
        Button createGoalButton = new Button("Create Goal with Selected Books")
        {
            X = 1,
            Y = 25
        };
        
        Button clearSelectionButton = new Button("Clear Selection")
        {
            X = Pos.Right(createGoalButton) + 2,
            Y = 25
        };

        Button cancelButton = new Button("Cancel")
        {
            X = Pos.Right(clearSelectionButton) + 2,
            Y = 25
        };
        
        createGoalButton.Clicked += () =>
        {
            if (selectedBooks.Count == 0)
            {
                MessageBox.ErrorQuery(50, 7, "No Books Selected", "Please select at least one book for your goal.", "OK");
                return;
            }
            
            Application.RequestStop(dialog);
            ShowCreateGoalWithBooksDialog(selectedBooks);
        };

        clearSelectionButton.Clicked += () =>
        {
            selectedBooks.Clear();
            UpdateSelectedBooksDisplay();
        };
        
        cancelButton.Clicked += () => Application.RequestStop(dialog);
        
        dialog.Add(searchLabel, searchField, searchButton, browseAllButton, resultsLabel, 
                  booksListView, selectedLabel, selectedBooksListView, 
                  createGoalButton, clearSelectionButton, cancelButton);
        
        // Initialize display
        UpdateSelectedBooksDisplay();
        
        // Set initial focus to search field
        searchField.SetFocus();
        
        Application.Run(dialog);
    }

    private void ShowCreateGoalWithBooksDialog(List<Book> selectedBooks)
    {
        Dialog dialog = new Dialog("Create Goal with Selected Books", UiConstants.Dialogs.LargeWidth, UiConstants.Dialogs.VeryLargeHeight);
        
        // Show selected books
        Label booksLabel = new Label($"Creating goal with {selectedBooks.Count} selected books:")
        {
            X = 1,
            Y = 1
        };
        
        string booksList = string.Join(", ", selectedBooks.Select(b => b.Title).Take(3));
        if (selectedBooks.Count > 3)
        {
            booksList += $" and {selectedBooks.Count - 3} more";
        }
        
        Label booksDisplayLabel = new Label(booksList)
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill() - 2
        };
        
        // Goal details
        Label nameLabel = new Label("Goal Name:")
        {
            X = 1,
            Y = 4
        };
        
        TextField nameField = new TextField($"Read {selectedBooks.Count} Books")
        {
            X = Pos.Right(nameLabel) + 2,
            Y = 4,
            Width = UiConstants.Controls.LargeTextFieldWidth
        };
        
        Label descLabel = new Label("Description:")
        {
            X = 1,
            Y = 6
        };
        
        TextField descField = new TextField()
        {
            X = Pos.Right(descLabel) + 2,
            Y = 6,
            Width = UiConstants.Controls.LargeTextFieldWidth
        };
        
        Label startLabel = new Label("Start Date:")
        {
            X = 1,
            Y = 8
        };
        
        TextField startField = new TextField(DateTime.Now.ToString("yyyy-MM-dd"))
        {
            X = Pos.Right(startLabel) + 2,
            Y = 8,
            Width = UiConstants.Controls.DateFieldWidth
        };
        
        Label endLabel = new Label("Deadline:")
        {
            X = 1,
            Y = 10
        };
        
        TextField endField = new TextField(DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd"))
        {
            X = Pos.Right(endLabel) + 2,
            Y = 10,
            Width = 12
        };

        // Calculate total pages for suggestion
        int totalPages = selectedBooks.Where(b => b.Pages.HasValue).Sum(b => b.Pages.GetValueOrDefault());
        
        Label targetLabel = new Label($"Suggested targets: {selectedBooks.Count} books, {totalPages} pages")
        {
            X = 1,
            Y = 12
        };
        
        Button createButton = new Button("Create Goal")
        {
            X = 1,
            Y = 16
        };
        
        Button cancelButton = new Button("Cancel")
        {
            X = Pos.Right(createButton) + 2,
            Y = 16
        };
        
        createButton.Clicked += async () =>
        {
            try
            {
                string goalName = nameField.Text?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(goalName))
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Goal name is required.", "OK");
                    return;
                }
                
                if (!DateTime.TryParse(startField.Text?.ToString(), out DateTime startDate))
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Invalid start date format.", "OK");
                    return;
                }
                
                if (!DateTime.TryParse(endField.Text?.ToString(), out DateTime endDate))
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Invalid deadline format.", "OK");
                    return;
                }
                
                if (endDate <= startDate)
                {
                    MessageBox.ErrorQuery(50, 7, "Validation Error", "Deadline must be after start date.", "OK");
                    return;
                }
                
                // Create goal with book count and page targets based on selected books
                ReadingGoal newGoal = new ReadingGoal
                {
                    GoalName = goalName,
                    Description = descField.Text?.ToString()?.Trim(),
                    StartDate = startDate,
                    Deadline = endDate,
                    TargetBooks = selectedBooks.Count,
                    TargetPages = totalPages > 0 ? totalPages : null,
                    IsCompleted = false
                };
                
                int goalId = await _readingGoalService.CreateGoalAsync(newGoal);
                
                if (goalId > 0)
                {
                    // Add all selected books to the goal
                    int successCount = 0;
                    foreach (Book book in selectedBooks)
                    {
                        bool success = await _readingGoalService.AddBookToGoalAsync(goalId, book.ISBN);
                        if (success) successCount++;
                    }
                    
                    Application.RequestStop(dialog);
                    MessageBox.Query(60, 10, "Goal Created!", 
                        $"Reading goal '{goalName}' created successfully!\n\n" +
                        $"Added {successCount} of {selectedBooks.Count} books to the goal.\n" +
                        $"Target: {selectedBooks.Count} books" + 
                        (totalPages > 0 ? $", {totalPages} pages" : ""), "OK");
                    
                    await LoadGoalsAsync();
                }
                else
                {
                    MessageBox.ErrorQuery(50, 7, "Error", "Failed to create reading goal.", "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(60, 8, "Error", $"Error creating goal: {ex.Message}", "OK");
            }
        };
        
        cancelButton.Clicked += () => Application.RequestStop(dialog);
        
        dialog.Add(booksLabel, booksDisplayLabel, nameLabel, nameField, descLabel, descField,
                  startLabel, startField, endLabel, endField, targetLabel, createButton, cancelButton);
        
        Application.Run(dialog);
    }

    private void ShowAddBookToGoalDialog()
    {
        if (_selectedGoal == null)
        {
            ShowMessage("Add Book", "Please select a goal first.");
            return;
        }

        Dialog dialog = new Dialog($"Add Book to '{_selectedGoal.GoalName}'", UiConstants.Dialogs.ExtraLargeWidth, UiConstants.Dialogs.HugeHeight);
        
        // Search section
        Label searchLabel = new Label("Search for books:")
        {
            X = 1,
            Y = 1
        };
        
        TextField searchField = new TextField()
        {
            X = 1,
            Y = 2,
            Width = UiConstants.Controls.LargeTextFieldWidth
        };
        
        Button searchButton = new Button("Search")
        {
            X = Pos.Right(searchField) + 2,
            Y = 2
        };
        
        // Results list
        Label resultsLabel = new Label("Search results:")
        {
            X = 1,
            Y = 4
        };
        
        ListView booksListView = new ListView()
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill() - 2,
            Height = UiConstants.Frames.LargeFrameHeight
        };
        
        List<Book> searchResults = new List<Book>();
        Book? selectedBook = null;
        
        // Search functionality
        searchButton.Clicked += async () =>
        {
            string searchTerm = searchField.Text?.ToString()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.ErrorQuery(40, 7, "Search", "Please enter a search term.", "OK");
                return;
            }
            
            try
            {
                resultsLabel.Text = "Searching...";
                dialog.SetNeedsDisplay();
                
                IEnumerable<Book> results = await _bookService.SearchBooksAsync(searchTerm);
                searchResults = results?.ToList() ?? new List<Book>();
                
                if (searchResults.Count == 0)
                {
                    resultsLabel.Text = $"No books found for '{searchTerm}'";
                    booksListView.SetSource(new string[0]);
                }
                else
                {
                    resultsLabel.Text = $"Found {searchResults.Count} books:";
                    string[] bookItems = searchResults.Select(book => 
                    {
                        string year = book.PublicationYear?.ToString() ?? "N/A";
                        string pages = book.Pages?.ToString() ?? "N/A";
                        return $"{book.Title} ({year}) - {pages} pages";
                    }).ToArray();
                    booksListView.SetSource(bookItems);
                }
                
                dialog.SetNeedsDisplay();
            }
            catch (Exception ex)
            {
                resultsLabel.Text = "Search failed";
                MessageBox.ErrorQuery(50, 8, "Error", $"Search error: {ex.Message}", "OK");
            }
        };
        
        // Handle book selection
        booksListView.SelectedItemChanged += (args) =>
        {
            if (args.Item >= 0 && args.Item < searchResults.Count)
            {
                selectedBook = searchResults[args.Item];
            }
            else
            {
                selectedBook = null;
            }
        };

        // Handle double-click/Enter on book list
        booksListView.OpenSelectedItem += (args) =>
        {
            if (args.Item >= 0 && args.Item < searchResults.Count)
            {
                selectedBook = searchResults[args.Item];
            }
        };
        
        // Handle Enter key in search field
        searchField.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                Task.Run(async () =>
                {
                    string searchTerm = searchField.Text?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        Application.MainLoop.Invoke(() => 
                        {
                            MessageBox.ErrorQuery(40, 7, "Search", "Please enter a search term.", "OK");
                        });
                        return;
                    }
                    
                    try
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            resultsLabel.Text = "Searching...";
                            dialog.SetNeedsDisplay();
                        });
                        
                        IEnumerable<Book> results = await _bookService.SearchBooksAsync(searchTerm);
                        searchResults = results?.ToList() ?? new List<Book>();
                        
                        Application.MainLoop.Invoke(() =>
                        {
                            if (searchResults.Count == 0)
                            {
                                resultsLabel.Text = $"No books found for '{searchTerm}'";
                                booksListView.SetSource(new string[0]);
                            }
                            else
                            {
                                resultsLabel.Text = $"Found {searchResults.Count} books:";
                                string[] bookItems = searchResults.Select(book => 
                                {
                                    string year = book.PublicationYear?.ToString() ?? "N/A";
                                    string pages = book.Pages?.ToString() ?? "N/A";
                                    return $"{book.Title} ({year}) - {pages} pages";
                                }).ToArray();
                                booksListView.SetSource(bookItems);
                            }
                            
                            dialog.SetNeedsDisplay();
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            resultsLabel.Text = "Search failed";
                            MessageBox.ErrorQuery(50, 8, "Error", $"Search error: {ex.Message}", "OK");
                        });
                    }
                });
                args.Handled = true;
            }
        };
        
        // Buttons
        Button addButton = new Button("Add Selected Book")
        {
            X = 1,
            Y = 18
        };
        
        Button cancelButton = new Button("Cancel")
        {
            X = Pos.Right(addButton) + 2,
            Y = 18
        };
        
        addButton.Clicked += async () =>
        {
            // Get the currently selected item from the ListView instead of relying on the event handler
            int selectedIndex = booksListView.SelectedItem;
            
            if (selectedIndex < 0 || selectedIndex >= searchResults.Count || searchResults.Count == 0)
            {
                MessageBox.ErrorQuery(40, 7, "Add Book", "Please select a book from the search results.", "OK");
                return;
            }
            
            Book bookToAdd = searchResults[selectedIndex];
            
            try
            {
                bool success = await _readingGoalService.AddBookToGoalAsync(_selectedGoal.Id, bookToAdd.ISBN);
                
                if (success)
                {
                    Application.RequestStop(dialog);
                    MessageBox.Query(60, 8, "Success", 
                        $"'{bookToAdd.Title}' has been added to goal '{_selectedGoal.GoalName}'!", "OK");
                    await UpdateSelectedGoalDisplayAsync(); // Refresh goal progress
                }
                else
                {
                    MessageBox.ErrorQuery(50, 8, "Error", 
                        "Failed to add book to goal. The book may already be in this goal.", "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(60, 8, "Error", $"Error adding book: {ex.Message}", "OK");
            }
        };
        
        cancelButton.Clicked += () => Application.RequestStop(dialog);
        
        dialog.Add(searchLabel, searchField, searchButton, resultsLabel, booksListView, addButton, cancelButton);
        
        // Set initial focus to search field
        searchField.SetFocus();
        
        Application.Run(dialog);
    }
}