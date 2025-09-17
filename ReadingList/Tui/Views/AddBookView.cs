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
    private readonly TextField _searchTextField = new TextField();
    private readonly ListView _resultsListView = new ListView();
    private readonly ComboBox _statusCombo = new ComboBox();
    private readonly Label _resultsLabel = new Label();
    private List<Book> _searchResults;
    private Book? _selectedBook;

    public AddBookView(
        NavigationManager navigationManager,
        IBookService bookService,
        IReadingListService readingListService) 
        : base("Add Book to Reading List", navigationManager)
    {
        _bookService = bookService;
        _readingListService = readingListService;
        _searchResults = new List<Book>();

        SetupUI();
    }

    protected override void SetupUI()
    {
        // Search section in a frame
        FrameView searchFrame = new FrameView("Search")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = 4
        };

        Label searchLabel = new Label("Books:")
        {
            X = 1,
            Y = 0
        };

        _searchTextField.X = Pos.Right(searchLabel) + 2;
        _searchTextField.Y = 0;
        _searchTextField.Width = 25;
        

        Button searchButton = new Button("Search")
        {
            X = Pos.Right(_searchTextField) + 2,
            Y = 0
        };

        searchButton.Clicked += async () => await SearchBooksAsync();

        searchFrame.Add(searchLabel, _searchTextField, searchButton);

        // Results section in a frame
        FrameView resultsFrame = new FrameView("Results")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill() - 2,
            Height = 10
        };

        _resultsLabel.X = 1;
        _resultsLabel.Y = 0;

        _resultsListView.X = 1;
        _resultsListView.Y = 1;
        _resultsListView.Width = Dim.Fill() - 2;
        _resultsListView.Height = Dim.Fill() - 2;

        _resultsListView.SelectedItemChanged += OnBookHighlighted;
        _resultsListView.OpenSelectedItem += OnBookSelected;

        resultsFrame.Add(_resultsLabel, _resultsListView);

        // Action section in a frame
        FrameView actionFrame = new FrameView("Add to List")
        {
            X = 1,
            Y = Pos.Bottom(resultsFrame),
            Width = Dim.Fill() - 2,
            Height = 10
        };

        Label statusLabel = new Label("Status:")
        {
            X = 1,
            Y = 0
        };

        _statusCombo.X = Pos.Right(statusLabel) + 2;
        _statusCombo.Y = 0;
        _statusCombo.Width = 20;
        _statusCombo.Height = 6;

        _statusCombo.SetSource(new string[] 
        { 
            "To Read", 
            "Currently Reading", 
            "Completed", 
            "Paused" 
        });

        _statusCombo.Text = "To Read";

        Button addButton = new Button("Add Selected Book")
        {
            X = 1,
            Y = 2
        };

        addButton.Clicked += async () => await AddBookToListAsync();

        Button backButton = new Button("Back")
        {
            X = Pos.Right(addButton) + 2,
            Y = 2
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        actionFrame.Add(statusLabel, _statusCombo, addButton, backButton);

        Add(searchFrame, resultsFrame, actionFrame);
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
            System.Console.Write("Error searching for books:\n{0}\n", ex.Message);
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