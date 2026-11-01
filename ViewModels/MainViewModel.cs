
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Daymark.Models;
using Daymark.Services;

namespace Daymark.ViewModels;

public enum JournalEditorMode
{
    New,
    Reading,
    Editing
}

public partial class MainViewModel : ViewModelBase
{
    private readonly JournalStorageService _storageService = new();
    private readonly ImageStorageService _imageStorageService = new();

    private readonly List<JournalEntry> _entries;

    public ObservableCollection<JournalEntry> JournalEntries { get; } = new();

    // Images currently displayed in the editor.
    public ObservableCollection<Bitmap> JournalImages { get; } = new();

    // Working copy of the current journal's blocks.
    private readonly List<JournalBlock> _workingBlocks = new();

    public MainViewModel()
    {
        _entries = _storageService.LoadEntries();

        foreach (JournalEntry entry in _entries)
        {
            JournalEntries.Add(entry);
        }
    }

    // NAVIGATION

    private bool _isWritingEntry;

    public bool IsWritingEntry
    {
        get => _isWritingEntry;
        set
        {
            if (SetProperty(ref _isWritingEntry, value))
            {
                OnPropertyChanged(nameof(IsHomeVisible));
            }
        }
    }

    private bool _isViewingLibrary;

    public bool IsViewingLibrary
    {
        get => _isViewingLibrary;
        set
        {
            if (SetProperty(ref _isViewingLibrary, value))
            {
                OnPropertyChanged(nameof(IsHomeVisible));
            }
        }
    }

    public bool IsHomeVisible =>
        !IsWritingEntry && !IsViewingLibrary;

    // JOURNAL TITLE AND CONTENT

    private string _entryTitle = "";

    public string EntryTitle
    {
        get => _entryTitle;
        set => SetProperty(ref _entryTitle, value);
    }

    private string _entryContent = "";

    public string EntryContent
    {
        get => _entryContent;
        set => SetProperty(ref _entryContent, value);
    }

    // LIBRARY SELECTION

    private JournalEntry? _selectedEntry;

    public JournalEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetProperty(ref _selectedEntry, value))
            {
                if (value != null && IsViewingLibrary)
                {
                    OpenEntry(value);
                }
            }
        }
    }

    // CURRENTLY OPEN JOURNAL

    private JournalEntry? _currentEntry;

    public JournalEntry? CurrentEntry
    {
        get => _currentEntry;
        private set
        {
            if (SetProperty(ref _currentEntry, value))
            {
                OnPropertyChanged(nameof(IsExistingEntry));
            }
        }
    }

    public bool IsExistingEntry => CurrentEntry != null;

    // EDITOR MODE

    private JournalEditorMode _editorMode = JournalEditorMode.New;

    public JournalEditorMode EditorMode
    {
        get => _editorMode;
        private set
        {
            if (SetProperty(ref _editorMode, value))
            {
                OnPropertyChanged(nameof(IsCreatingEntry));
                OnPropertyChanged(nameof(IsReadingEntry));
                OnPropertyChanged(nameof(IsEditingEntry));
                OnPropertyChanged(nameof(IsEditorReadOnly));
                OnPropertyChanged(nameof(EditorHeading));
                OnPropertyChanged(nameof(CanAddImage));
            }
        }
    }

    public bool IsCreatingEntry =>
        EditorMode == JournalEditorMode.New;

    public bool IsReadingEntry =>
        EditorMode == JournalEditorMode.Reading;

    public bool IsEditingEntry =>
        EditorMode == JournalEditorMode.Editing;

    public bool IsEditorReadOnly =>
        EditorMode == JournalEditorMode.Reading;

    public bool CanAddImage =>
        IsCreatingEntry || IsEditingEntry;

    public string EditorHeading => EditorMode switch
    {
        JournalEditorMode.Reading => "Journal Entry",
        JournalEditorMode.Editing => "Edit Journal Entry",
        _ => "New Journal Entry"
    };

    // DELETE CONFIRMATION

    private bool _isConfirmingDelete;

    public bool IsConfirmingDelete
    {
        get => _isConfirmingDelete;
        set
        {
            if (SetProperty(ref _isConfirmingDelete, value))
            {
                OnPropertyChanged(nameof(IsNotConfirmingDelete));
            }
        }
    }

    public bool IsNotConfirmingDelete => !IsConfirmingDelete;

    // IMAGE HANDLING

    private void ClearImagePreviews()
    {
        foreach (Bitmap image in JournalImages)
        {
            image.Dispose();
        }

        JournalImages.Clear();
    }

    private void LoadImagePreviews()
    {
        ClearImagePreviews();

        foreach (JournalBlock block in _workingBlocks)
        {
            if (block.Type != JournalBlockType.Image ||
                string.IsNullOrWhiteSpace(block.ImagePath) ||
                !File.Exists(block.ImagePath))
            {
                continue;
            }

            try
            {
                JournalImages.Add(new Bitmap(block.ImagePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Could not load image: {ex.Message}");
            }
        }
    }

    private void LoadWorkingBlocks(JournalEntry entry)
    {
        _workingBlocks.Clear();

        if (entry.Blocks != null)
        {
            foreach (JournalBlock block in entry.Blocks)
            {
                _workingBlocks.Add(new JournalBlock
                {
                    Id = block.Id,
                    Type = block.Type,
                    Text = block.Text,
                    ImagePath = block.ImagePath
                });
            }
        }

        LoadImagePreviews();
    }

    // Called by the window after selecting an image.
    public void AddImageFromPath(string sourcePath)
    {
        if (!CanAddImage || IsConfirmingDelete)
        {
            return;
        }

        // ImportImage copies the file into Daymark's folder.
        string savedPath = _imageStorageService.ImportImage(sourcePath);

        JournalBlock imageBlock = new JournalBlock
        {
            Type = JournalBlockType.Image,
            ImagePath = savedPath
        };

        _workingBlocks.Add(imageBlock);

        LoadImagePreviews();
    }

    private List<JournalBlock> CopyWorkingBlocks()
    {
        var result = new List<JournalBlock>();

        foreach (JournalBlock block in _workingBlocks)
        {
            result.Add(new JournalBlock
            {
                Id = block.Id,
                Type = block.Type,
                Text = block.Text,
                ImagePath = block.ImagePath
            });
        }

        return result;
    }

    // CREATE A NEW JOURNAL

    [RelayCommand]
    private void NewEntry()
    {
        IsConfirmingDelete = false;
        CurrentEntry = null;
        SelectedEntry = null;

        EntryTitle = "";
        EntryContent = "";

        _workingBlocks.Clear();
        ClearImagePreviews();

        EditorMode = JournalEditorMode.New;

        IsViewingLibrary = false;
        IsWritingEntry = true;
    }

    // OPEN LIBRARY

    [RelayCommand]
    private void OpenLibrary()
    {
        IsConfirmingDelete = false;

        SelectedEntry = null;
        CurrentEntry = null;

        _workingBlocks.Clear();
        ClearImagePreviews();

        IsWritingEntry = false;
        IsViewingLibrary = true;
    }

    // OPEN SAVED JOURNAL

    private void OpenEntry(JournalEntry entry)
    {
        IsConfirmingDelete = false;

        CurrentEntry = entry;

        EntryTitle = entry.Title;
        EntryContent = entry.Content;

        LoadWorkingBlocks(entry);

        EditorMode = JournalEditorMode.Reading;

        IsViewingLibrary = false;
        IsWritingEntry = true;
    }

    // EDIT EXISTING JOURNAL

    [RelayCommand]
    private void EditEntry()
    {
        if (CurrentEntry == null ||
            EditorMode != JournalEditorMode.Reading ||
            IsConfirmingDelete)
        {
            return;
        }

        EditorMode = JournalEditorMode.Editing;
    }

    // CANCEL EDITING

    [RelayCommand]
    private void CancelEditing()
    {
        if (CurrentEntry == null ||
            EditorMode != JournalEditorMode.Editing)
        {
            return;
        }

        EntryTitle = CurrentEntry.Title;
        EntryContent = CurrentEntry.Content;

        LoadWorkingBlocks(CurrentEntry);

        EditorMode = JournalEditorMode.Reading;
    }

    // SAVE EXISTING JOURNAL

    [RelayCommand]
    private void SaveChanges()
    {
        if (CurrentEntry == null ||
            EditorMode != JournalEditorMode.Editing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EntryTitle) &&
            string.IsNullOrWhiteSpace(EntryContent) &&
            _workingBlocks.Count == 0)
        {
            return;
        }

        CurrentEntry.Title = EntryTitle;
        CurrentEntry.Content = EntryContent;
        CurrentEntry.Blocks = CopyWorkingBlocks();
        CurrentEntry.UpdatedAt = DateTime.Now;

        _storageService.SaveEntries(_entries);

        // Do not replace JournalEntries items.
        EditorMode = JournalEditorMode.Reading;
    }

    // DELETE CONFIRMATION

    [RelayCommand]
    private void DeleteEntry()
    {
        if (CurrentEntry == null ||
            EditorMode != JournalEditorMode.Reading)
        {
            return;
        }

        IsConfirmingDelete = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsConfirmingDelete = false;
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (CurrentEntry == null || !IsConfirmingDelete)
        {
            return;
        }

        JournalEntry entryToDelete = CurrentEntry;

        _entries.Remove(entryToDelete);
        _storageService.SaveEntries(_entries);

        IsConfirmingDelete = false;
        CurrentEntry = null;
        SelectedEntry = null;

        _workingBlocks.Clear();
        ClearImagePreviews();

        RefreshLibrary();

        IsWritingEntry = false;
        IsViewingLibrary = true;
    }

    // LIBRARY REFRESH

    private void RefreshLibrary()
    {
        JournalEntries.Clear();

        foreach (JournalEntry entry in _entries)
        {
            JournalEntries.Add(entry);
        }
    }

    [RelayCommand]
    private void BackToLibrary()
    {
        // Clear selection before refreshing.
        SelectedEntry = null;

        RefreshLibrary();
        OpenLibrary();
    }

    // RETURN HOME

    [RelayCommand]
    private void GoHome()
    {
        IsConfirmingDelete = false;

        SelectedEntry = null;
        CurrentEntry = null;

        _workingBlocks.Clear();
        ClearImagePreviews();

        IsWritingEntry = false;
        IsViewingLibrary = false;
    }

    // SAVE NEW JOURNAL

    [RelayCommand]
    private void SaveEntry()
    {
        if (EditorMode != JournalEditorMode.New)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EntryTitle) &&
            string.IsNullOrWhiteSpace(EntryContent) &&
            _workingBlocks.Count == 0)
        {
            return;
        }

        JournalEntry entry = new JournalEntry
        {
            Title = EntryTitle,
            Content = EntryContent,
            Blocks = CopyWorkingBlocks(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _entries.Add(entry);
        JournalEntries.Add(entry);

        _storageService.SaveEntries(_entries);

        GoHome();
    }
}
