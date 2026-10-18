
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Daymark.Models;
using Daymark.Services;
using System.Collections.ObjectModel;

namespace Daymark.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly JournalStorageService _storageService = new();

    private List<JournalEntry> _entries;
    public ObservableCollection<JournalEntry> JournalEntries { get; } = new();

    public MainViewModel()
{
    _entries = _storageService.LoadEntries();

    foreach (JournalEntry entry in _entries)
    {
        JournalEntries.Add(entry);
    }
}

    private bool _isWritingEntry = false;

    public bool IsWritingEntry
    {
        get => _isWritingEntry;
        set => SetProperty(ref _isWritingEntry, value);
    }

    private bool _isViewingLibrary = false;

    public bool IsViewingLibrary
    {
        get => _isViewingLibrary;
        set => SetProperty(ref _isViewingLibrary, value);
    }

    public bool IsHomeVisible => !IsWritingEntry && !IsViewingLibrary;

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
        IsViewingLibrary = false;
        IsWritingEntry = true;
        OnPropertyChanged(nameof(IsHomeVisible));
    }

    [RelayCommand]
    private void OpenLibrary()
    {
        IsWritingEntry = false;
        IsViewingLibrary = true;
        OnPropertyChanged(nameof(IsHomeVisible));
    }

    [RelayCommand]
    private void GoHome()
    {
        IsWritingEntry = false;
        IsViewingLibrary = false;
        OnPropertyChanged(nameof(IsHomeVisible));
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
        JournalEntries.Add(entry);
        _storageService.SaveEntries(_entries);

        GoHome();
    }
}
