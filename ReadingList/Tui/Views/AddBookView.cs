using ReadingList.Data;
using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class AddBookView : BaseView
{
    private readonly IBookService _bookService;
    private readonly IReadingListService _readingListService;
    private readonly NavigationManager _navigationManager;
    private TextField _searchTextField;
    private ListView _resultsListView;
    private ComboBox _statusCombo;
    private Label _resultsLabel;
    private List<Book> _searchResults;
    private Book? _selectedBook;

    public AddBookView(
        IBookService bookService,
        IReadingListService readingListService,
        NavigationManager navigationManager) 
        : base("Add Book to Reading List")
    {
        _bookService = bookService;
        _readingListService = readingListService;
        _navigationManager = navigationManager;
        _searchResults = new List<Book>();

        SetNavigationManager(navigationManager);
        SetupUI();
    }

    protected override void SetupUI()
    {
        // Search section
        Label searchLabel = new Label("Search Books:")
        {
            X = 1,
            Y = 1
        };

        _searchTextField = new TextField()
        {
            X = Pos.Right(searchLabel) + 2,
            Y = 1,
            Width = 30
        };

        Button searchButton = new Button("Search")
        {
            X = Pos.Right(_searchTextField) + 2,
            Y = 1
        };

        searchButton.Clicked += async () => await SearchBooksAsync();

        // Results section
        _resultsLabel = new Label("Search for books to add to your reading list")
        {
            X = 1,
            Y = 3
        };

        _resultsListView = new ListView()
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill() - 2,
            Height = 8
        };

        _resultsListView.SelectedItemChanged += OnBookHighlighted;
        _resultsListView.OpenSelectedItem += OnBookSelected;

        // Status selection
        Label statusLabel = new Label("Reading Status:")
        {
            X = 1,
            Y = 13
        };

        _statusCombo = new ComboBox()
        {
            X = Pos.Right(statusLabel) + 2,
            Y = 13,
            Width = 20,
            Height = 6
        };

        _statusCombo.SetSource(new string[] 
        { 
            "To Read", 
            "Currently Reading", 
            "Completed", 
            "Paused" 
        });

        _statusCombo.Text = "To Read";

        // Action buttons
        Button addButton = new Button("Add Selected Book")
        {
            X = 1,
            Y = 15
        };

        addButton.Clicked += async () => await AddBookToListAsync();

        Button backButton = new Button("Back")
        {
            X = Pos.Right(addButton) + 2,
            Y = 15
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Add(searchLabel, _searchTextField, searchButton, _resultsLabel, _resultsListView, statusLabel, _statusCombo, addButton, backButton);
    }

    public override void OnViewActivated()
    {
        _searchTextField.SetFocus();
    }

    private async Task SearchBooksAsync()
    {
        string searchTerm = _searchTextField.Text?.ToString()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _resultsLabel.Text = "Please enter a search term";
            return;
        }

        try
        {
            _resultsLabel.Text = "Searching...";
            SetNeedsDisplay();

            IEnumerable<Book> results = await _bookService.SearchBooksAsync(searchTerm);
            _searchResults = results?.ToList() ?? new List<Book>();

            UpdateResultsDisplay(searchTerm);
        }
        catch (Exception ex)
        {
            _resultsLabel.Text = "Search failed";
            _searchResults.Clear();
            _resultsListView.SetSource(new string[0]);
            SetNeedsDisplay();
        }
    }

    private void UpdateResultsDisplay(string searchTerm)
    {
        if (_searchResults.Count == 0)
        {
            _resultsLabel.Text = $"No books found for '{searchTerm}'";
            _resultsListView.SetSource(new string[0]);
            _selectedBook = null;
        }
        else
        {
            string[] bookDisplayItems = _searchResults.Select(FormatBookForDisplay).ToArray();
            _resultsListView.SetSource(bookDisplayItems);
            
            // Set first item as selected by default
            _resultsListView.SelectedItem = 0;
            _selectedBook = _searchResults[0];
            
            UpdateSelectedBookDisplay();
        }

        SetNeedsDisplay();
    }

    private string FormatBookForDisplay(Book book)
    {
        string year = book.PublicationYear?.ToString() ?? "N/A";
        return $"{book.Title} ({year})";
    }

    private void OnBookHighlighted(ListViewItemEventArgs args)
    {
        // This fires when user navigates with arrow keys
        if (args.Item >= 0 && args.Item < _searchResults.Count)
        {
            _selectedBook = _searchResults[args.Item];
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
        // This fires when user presses Enter
        if (args.Item >= 0 && args.Item < _searchResults.Count)
        {
            _selectedBook = _searchResults[args.Item];
            UpdateSelectedBookDisplay();
        }
    }

    private void UpdateSelectedBookDisplay()
    {
        if (_selectedBook != null)
        {
            _resultsLabel.Text = $"✓ Selected: {_selectedBook.Title} - Press 'Add Selected Book' to add";
        }
        else if (_searchResults.Count > 0)
        {
            _resultsLabel.Text = $"Found {_searchResults.Count} books - use arrow keys to select one";
        }
        else
        {
            _resultsLabel.Text = "Search for books to add to your reading list";
        }
        SetNeedsDisplay();
    }

    private async Task AddBookToListAsync()
    {
        if (_selectedBook == null)
        {
            ShowMessage("Add Book", "Please select a book from the search results first.");
            return;
        }

        ReadingStatus selectedStatus = GetSelectedStatus();

        try
        {
            int result = await _readingListService.AddBookToListAsync(_selectedBook.ISBN, selectedStatus);
            
            if (result > 0)
            {
                bool continueAdding = ShowConfirmation("Success", 
                    $"'{_selectedBook.Title}' added to your reading list!\n\nAdd another book?");
                
                if (continueAdding)
                {
                    ClearForm();
                }
                else
                {
                    _navigationManager.NavigateBack();
                }
            }
            else
            {
                ShowError("Failed to add book to your reading list. The book may already be in your list.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error adding book: {ex.Message}");
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
            _ => ReadingStatus.ToRead
        };
    }

    private void ClearForm()
    {
        _searchTextField.Text = "";
        _resultsLabel.Text = "Search for books to add to your reading list";
        _searchResults.Clear();
        _resultsListView.SetSource(new string[0]);
        _selectedBook = null;
        _statusCombo.SelectedItem = 0;
        _searchTextField.SetFocus();
        SetNeedsDisplay();
    }
}