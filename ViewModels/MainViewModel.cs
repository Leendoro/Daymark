using CommunityToolkit.Mvvm.Input;

namespace Daymark.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private bool _isWritingEntry = false;

    public bool IsWritingEntry
    {
        get => _isWritingEntry;
        set => SetProperty(ref _isWritingEntry, value);
    }

    [RelayCommand]
    private void NewEntry()
    {
        IsWritingEntry = true;
    }

    [RelayCommand]
    private void GoHome()
    {
        IsWritingEntry = false;
    }
}