
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Daymark.Models;
using Daymark.Services;

namespace Daymark.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly JournalStorageService _storageService = new();

    private List<JournalEntry> _entries;

    public MainViewModel()
    {
        _entries = _storageService.LoadEntries();
    }

    private bool _isWritingEntry = false;

    public bool IsWritingEntry
    {
        get => _isWritingEntry;
        set => SetProperty(ref _isWritingEntry, value);
    }

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

    [RelayCommand]
    private void NewEntry()
    {
        EntryTitle = "";
        EntryContent = "";
        IsWritingEntry = true;
    }

    [RelayCommand]
    private void GoHome()
    {
        IsWritingEntry = false;
    }

    [RelayCommand]
    private void SaveEntry()
    {
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
        _storageService.SaveEntries(_entries);

        IsWritingEntry = false;
    }
}
