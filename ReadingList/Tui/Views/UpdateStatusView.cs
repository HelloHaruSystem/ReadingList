using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class UpdateStatusView : BaseView
{
    private readonly IReadingListService _readingListService;
    private readonly NavigationManager _navigationManager;
    private ListView _booksListView;
    private ComboBox _statusCombo;
    private Label _booksLabel;
    private Label _selectedBookLabel;
    private List<UserBook> _userBooks;
    private UserBook? _selectedBook;

    public UpdateStatusView(
        IReadingListService readingListService,
        NavigationManager navigationManager) 
        : base("Update Reading Status")
    {
        _readingListService = readingListService;
        _navigationManager = navigationManager;
        _userBooks = new List<UserBook>();

        SetNavigationManager(navigationManager);
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
            Height = 12
        };

        _booksLabel = new Label("Select a book to update its status:")
        {
            X = 1,
            Y = 0
        };

        _booksListView = new ListView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2
        };

        _booksListView.SelectedItemChanged += OnBookHighlighted;
        _booksListView.OpenSelectedItem += OnBookSelected;

        booksFrame.Add(_booksLabel, _booksListView);

        // Status update section in a frame
        FrameView statusFrame = new FrameView("Update Status")
        {
            X = 1,
            Y = Pos.Bottom(booksFrame),
            Width = Dim.Fill() - 2,
            Height = 12
        };

        _selectedBookLabel = new Label("No book selected")
        {
            X = 1,
            Y = 0
        };

        Label newStatusLabel = new Label("New Status:")
        {
            X = 1,
            Y = 2
        };

        _statusCombo = new ComboBox()
        {
            X = Pos.Right(newStatusLabel) + 2,
            Y = 2,
            Width = 25,
            Height = 8
        };

        _statusCombo.SetSource(new string[] 
        { 
            "To Read", 
            "Currently Reading", 
            "Completed", 
            "Paused",
            "Abandoned"
        });

        _statusCombo.Text = "To Read";

        Button updateButton = new Button("Update Status")
        {
            X = 1,
            Y = 6
        };

        updateButton.Clicked += async () => await UpdateBookStatusAsync();

        statusFrame.Add(_selectedBookLabel, newStatusLabel, _statusCombo, updateButton);

        // Action buttons section in a frame
        FrameView actionFrame = new FrameView("Actions")
        {
            X = 1,
            Y = Pos.Bottom(statusFrame),
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

        Add(booksFrame, statusFrame, actionFrame);
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
                _booksLabel.Text = $"Select a book to update its status ({_userBooks.Count} books):";
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
        string year = userBook.Book.PublicationYear?.ToString() ?? "N/A";
        
        return $"{userBook.Book.Title} ({year}) - Currently: {status}";
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
            string currentStatus = GetStatusDisplayName(_selectedBook.Status);
            _selectedBookLabel.Text = $"Selected: {_selectedBook.Book.Title}\nCurrent Status: {currentStatus}";
            
            // Set the combo box to the current status
            _statusCombo.SelectedItem = _selectedBook.Status switch
            {
                ReadingStatus.ToRead => 0,
                ReadingStatus.CurrentlyReading => 1,
                ReadingStatus.Completed => 2,
                ReadingStatus.Paused => 3,
                ReadingStatus.Abandoned => 4,
                _ => 0
            };
        }
        else
        {
            _selectedBookLabel.Text = "No book selected";
            _statusCombo.SelectedItem = 0;
        }
        SetNeedsDisplay();
    }

    private async Task UpdateBookStatusAsync()
    {
        if (_selectedBook == null)
        {
            ShowMessage("Update Status", "Please select a book from the list first.");
            return;
        }

        ReadingStatus newStatus = GetSelectedStatus();

        // Check if status is actually changing
        if (_selectedBook.Status == newStatus)
        {
            ShowMessage("Update Status", "The selected status is the same as the current status.");
            return;
        }

        try
        {
            bool success = await _readingListService.UpdateReadingStatusAsync(_selectedBook.Id, newStatus);
            
            if (success)
            {
                string oldStatusName = GetStatusDisplayName(_selectedBook.Status);
                string newStatusName = GetStatusDisplayName(newStatus);
                
                ShowMessage("Success", 
                    $"'{_selectedBook.Book.Title}' status updated from '{oldStatusName}' to '{newStatusName}'!");
                
                // Refresh the list to show updated status
                await LoadBooksAsync();
            }
            else
            {
                ShowError("Failed to update reading status.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error updating status: {ex.Message}");
        }
    }

    private ReadingStatus GetSelectedStatus()
    {
        return _statusCombo.SelectedItem switch
        {
            0 => ReadingStatus.ToRead,
            1 => ReadingStatus.CurrentlyReading,
            2 => ReadingStatus.Completed,
            3 => ReadingStatus.Paused,
            4 => ReadingStatus.Abandoned,
            _ => ReadingStatus.ToRead
        };
    }
}