using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class RateBookView : BaseView
{
    private readonly IReadingListService _readingListService;
    private readonly ListView _booksListView = new ListView();
    private readonly ComboBox _ratingCombo = new ComboBox();
    private readonly TextView _notesTextView = new TextView();
    private readonly Label _booksLabel = new Label();
    private readonly Label _selectedBookLabel = new Label("No book selected");
    private readonly Label _currentRatingLabel = new Label("");
    private List<UserBook> _userBooks;
    private UserBook? _selectedBook;

    public RateBookView(
        NavigationManager navigationManager,
        IReadingListService readingListService) 
        : base("Rate a Book", navigationManager)
    {
        _readingListService = readingListService;
        _userBooks = new List<UserBook>();

        SetupUI();
    }

    protected override void SetupUI()
    {
        // Books list section in a frame
        FrameView booksFrame = new FrameView("Your Books")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = 10
        };

        _booksLabel.X = 1;
        _booksLabel.Y = 0;

        _booksListView.X = 1;
        _booksListView.Y = 1;
        _booksListView.Width = Dim.Fill() - 2;
        _booksListView.Height = Dim.Fill() - 2;

        _booksListView.SelectedItemChanged += OnBookHighlighted;
        _booksListView.OpenSelectedItem += OnBookSelected;

        booksFrame.Add(_booksLabel, _booksListView);

        // Rating section in a frame
        FrameView ratingFrame = new FrameView("Rate & Review")
        {
            X = 1,
            Y = Pos.Bottom(booksFrame),
            Width = Dim.Fill() - 2,
            Height = 18
        };

        _selectedBookLabel.X = 1;
        _selectedBookLabel.Y = 0;

        _currentRatingLabel.X = 1;
        _currentRatingLabel.Y = 1;

        Label ratingLabel = new Label("Rating (1-5):")
        {
            X = 1,
            Y = 3
        };

        _ratingCombo.X = Pos.Right(ratingLabel) + 2;
        _ratingCombo.Y = 3;
        _ratingCombo.Width = 15;
        _ratingCombo.Height = 7;

        _ratingCombo.SetSource(new string[] 
        { 
            "No Rating",
            "1 - Poor", 
            "2 - Fair", 
            "3 - Good", 
            "4 - Very Good",
            "5 - Excellent"
        });

        _ratingCombo.Text = "No Rating";

        Label notesLabel = new Label("Personal Notes:")
        {
            X = 1,
            Y = 5
        };

        _notesTextView.X = 1;
        _notesTextView.Y = 6;
        _notesTextView.Width = Dim.Fill() - 2;
        _notesTextView.Height = 7;
        _notesTextView.WordWrap = true;

        Button saveButton = new Button("Save Rating & Notes")
        {
            X = 1,
            Y = Pos.Bottom(_notesTextView) + 1
        };

        saveButton.Clicked += async () => await SaveRatingAsync();

        Button clearButton = new Button("Clear Rating")
        {
            X = Pos.Right(saveButton) + 2,
            Y = Pos.Bottom(_notesTextView) + 1
        };

        clearButton.Clicked += () => ClearRating();

        ratingFrame.Add(_selectedBookLabel, _currentRatingLabel, ratingLabel, _ratingCombo, 
                       notesLabel, _notesTextView, saveButton, clearButton);

        // Action buttons section in a frame
        FrameView actionFrame = new FrameView("Actions")
        {
            X = 1,
            Y = Pos.Bottom(ratingFrame),
            Width = Dim.Fill() - 2,
            Height = 4
        };

        Button backButton = new Button("Back")
        {
            X = 1,
            Y = 1
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Button refreshButton = new Button("Refresh")
        {
            X = Pos.Right(backButton) + 2,
            Y = 1
        };

        refreshButton.Clicked += async () => await LoadBooksAsync();

        actionFrame.Add(backButton, refreshButton);

        Add(booksFrame, ratingFrame, actionFrame);
    }

    public override async void OnViewActivated()
    {
        await LoadBooksAsync();
    }

    private async Task LoadBooksAsync()
    {
        try
        {
            IEnumerable<UserBook> books = await _readingListService.GetMyReadingListAsync();
            _userBooks = books.ToList();

            if (_userBooks.Count == 0)
            {
                _booksLabel.Text = "No books in your reading list";
                _booksListView.SetSource(new string[0]);
                _selectedBook = null;
                UpdateSelectedBookDisplay();
            }
            else
            {
                _booksLabel.Text = $"Select a book to rate ({_userBooks.Count} books):";
                string[] bookDisplayItems = _userBooks.Select(FormatUserBookForDisplay).ToArray();
                _booksListView.SetSource(bookDisplayItems);
                
                // Set first item as selected by default
                _booksListView.SelectedItem = 0;
                _selectedBook = _userBooks[0];
                UpdateSelectedBookDisplay();
            }

            SetNeedsDisplay();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load reading list: {ex.Message}");
        }
    }

    private string FormatUserBookForDisplay(UserBook userBook)
    {
        string status = GetStatusDisplayName(userBook.Status);
        string rating = userBook.PersonalRating?.ToString() ?? "Not rated";
        string year = userBook.Book.PublicationYear?.ToString() ?? "N/A";
        
        return $"{userBook.Book.Title} ({year}) - {status} - Rating: {rating}/5";
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

    private void OnBookHighlighted(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _userBooks.Count)
        {
            _selectedBook = _userBooks[args.Item];
            UpdateSelectedBookDisplay();
        }
        else
        {
            _selectedBook = null;
            UpdateSelectedBookDisplay();
        }
    }

    private void OnBookSelected(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _userBooks.Count)
        {
            _selectedBook = _userBooks[args.Item];
            UpdateSelectedBookDisplay();
        }
    }

    private void UpdateSelectedBookDisplay()
    {
        if (_selectedBook != null)
        {
            _selectedBookLabel.Text = $"Selected: {_selectedBook.Book.Title}";
            
            string currentRating = _selectedBook.PersonalRating?.ToString() ?? "No rating";
            _currentRatingLabel.Text = $"Current Rating: {currentRating}/5";
            
            // Set the combo box to the current rating
            _ratingCombo.SelectedItem = _selectedBook.PersonalRating ?? 0; // 0 = "No Rating"
            
            // Set the notes text
            _notesTextView.Text = _selectedBook.PersonalNotes ?? "";
        }
        else
        {
            _selectedBookLabel.Text = "No book selected";
            _currentRatingLabel.Text = "";
            _ratingCombo.SelectedItem = 0;
            _notesTextView.Text = "";
        }
        SetNeedsDisplay();
    }

    private async Task SaveRatingAsync()
    {
        if (_selectedBook == null)
        {
            ShowMessage("Rate Book", "Please select a book from the list first.");
            return;
        }

        int? newRating = GetSelectedRating();
        string? newNotes = _notesTextView.Text?.ToString()?.Trim();
        
        // Convert empty notes to null
        if (string.IsNullOrWhiteSpace(newNotes))
        {
            newNotes = null;
        }

        // Check if anything is actually changing
        bool ratingChanged = newRating != _selectedBook.PersonalRating;
        bool notesChanged = newNotes != _selectedBook.PersonalNotes;

        if (!ratingChanged && !notesChanged)
        {
            ShowMessage("Rate Book", "No changes detected.");
            return;
        }

        try
        {
            bool success = await _readingListService.RateBookAsync(_selectedBook.Id, newRating ?? 0, newNotes);
            
            if (success)
            {
                string ratingText = newRating?.ToString() ?? "No rating";
                ShowMessage("Success", 
                    $"'{_selectedBook.Book.Title}' has been updated!\nRating: {ratingText}/5");
                
                // Refresh the list to show updated rating
                await LoadBooksAsync();
            }
            else
            {
                ShowError("Failed to save rating and notes.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error saving rating: {ex.Message}");
        }
    }

    private int? GetSelectedRating()
    {
        return _ratingCombo.SelectedItem switch
        {
            0 => null, // No Rating
            1 => 1,    // Poor
            2 => 2,    // Fair
            3 => 3,    // Good
            4 => 4,    // Very Good
            5 => 5,    // Excellent
            _ => null
        };
    }

    private void ClearRating()
    {
        if (_selectedBook == null)
        {
            ShowMessage("Clear Rating", "Please select a book from the list first.");
            return;
        }

        _ratingCombo.SelectedItem = 0; // Set to "No Rating"
        _notesTextView.Text = "";
        SetNeedsDisplay();
    }
}