
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Daymark.Models;
using Daymark.Services;

namespace Daymark.ViewModels;

// The editor can only be in one mode at a time.
public enum JournalEditorMode
{
    New,
    Reading,
    Editing
}

public partial class MainViewModel : ViewModelBase
{
    private readonly JournalStorageService _storageService = new();

    private readonly List<JournalEntry> _entries;

    public ObservableCollection<JournalEntry> JournalEntries { get; } = new();

    public MainViewModel()
    {
        _entries = _storageService.LoadEntries();

        foreach (JournalEntry entry in _entries)
        {
            JournalEntries.Add(entry);
        }
    }

    // NAVIGATION

    private bool _isWritingEntry = false;

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

    private bool _isViewingLibrary = false;

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

    public bool IsHomeVisible => !IsWritingEntry && !IsViewingLibrary;

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
    // Separate from the ListBox selection to avoid our old bug.

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

    public string EditorHeading => EditorMode switch
    {
        JournalEditorMode.Reading => "Journal Entry",
        JournalEditorMode.Editing => "Edit Journal Entry",
        _ => "New Journal Entry"
    };

    // DELETE CONFIRMATION STATE

    private bool _isConfirmingDelete = false;

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

    // CREATE A NEW JOURNAL

    [RelayCommand]
    private void NewEntry()
    {
        IsConfirmingDelete = false;
        CurrentEntry = null;
        SelectedEntry = null;

        EntryTitle = "";
        EntryContent = "";

        EditorMode = JournalEditorMode.New;

        IsViewingLibrary = false;
        IsWritingEntry = true;
    }

    // OPEN THE JOURNAL LIBRARY

    [RelayCommand]
    private void OpenLibrary()
    {
        IsConfirmingDelete = false;

        SelectedEntry = null;
        CurrentEntry = null;

        IsWritingEntry = false;
        IsViewingLibrary = true;
    }

    // OPEN A SAVED JOURNAL

    private void OpenEntry(JournalEntry entry)
    {
        IsConfirmingDelete = false;

        CurrentEntry = entry;

        EntryTitle = entry.Title;
        EntryContent = entry.Content;

        EditorMode = JournalEditorMode.Reading;

        IsViewingLibrary = false;
        IsWritingEntry = true;
    }

    // START EDITING

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

        EditorMode = JournalEditorMode.Reading;
    }

    // SAVE CHANGES TO AN EXISTING JOURNAL

    [RelayCommand]
    private void SaveChanges()
    {
        if (CurrentEntry == null ||
            EditorMode != JournalEditorMode.Editing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EntryTitle) &&
            string.IsNullOrWhiteSpace(EntryContent))
        {
            return;
        }

        CurrentEntry.Title = EntryTitle;
        CurrentEntry.Content = EntryContent;
        CurrentEntry.UpdatedAt = DateTime.Now;

        _storageService.SaveEntries(_entries);

        // Do not replace JournalEntries items here.
        // That caused our previous selection bug.

        EditorMode = JournalEditorMode.Reading;
    }

    // ASK FOR DELETE CONFIRMATION

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

    // CANCEL DELETION

    [RelayCommand]
    private void CancelDelete()
    {
        IsConfirmingDelete = false;
    }

    // CONFIRM AND DELETE THE JOURNAL

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (CurrentEntry == null || !IsConfirmingDelete)
        {
            return;
        }

        JournalEntry entryToDelete = CurrentEntry;

        // Remove from the saved journal list.
        _entries.Remove(entryToDelete);

        // Save the updated list to journals.json.
        _storageService.SaveEntries(_entries);

        // Clear the active journal before updating the UI.
        IsConfirmingDelete = false;
        CurrentEntry = null;
        SelectedEntry = null;

        // Refresh the library collection.
        JournalEntries.Clear();

        foreach (JournalEntry entry in _entries)
        {
            JournalEntries.Add(entry);
        }

        // Return to the library.
        IsWritingEntry = false;
        IsViewingLibrary = true;
    }

    // RETURN TO LIBRARY

    [RelayCommand]
    private void BackToLibrary()
    {
        JournalEntries.Clear();

        foreach (JournalEntry entry in _entries)
        {
            JournalEntries.Add(entry);
        }

        OpenLibrary();
    }

    // RETURN HOME

    [RelayCommand]
    private void GoHome()
    {
        IsConfirmingDelete = false;

        SelectedEntry = null;
        CurrentEntry = null;

        IsWritingEntry = false;
        IsViewingLibrary = false;
    }

    // SAVE A BRAND-NEW JOURNAL

    [RelayCommand]
    private void SaveEntry()
    {
        if (EditorMode != JournalEditorMode.New)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EntryTitle) &&
            string.IsNullOrWhiteSpace(EntryContent))
        {
            return;
        }

        JournalEntry entry = new JournalEntry
        {
            Title = EntryTitle,
            Content = EntryContent,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _entries.Add(entry);
        JournalEntries.Add(entry);

        _storageService.SaveEntries(_entries);

        GoHome();
    }
}
