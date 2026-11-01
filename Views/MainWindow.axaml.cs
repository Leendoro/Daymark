
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Daymark.ViewModels;

namespace Daymark.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void AddImage_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel ||
            !viewModel.CanAddImage ||
            viewModel.IsConfirmingDelete)
        {
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose an image for your journal",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Images")
                    {
                        Patterns = new[]
                        {
                            "*.png",
                            "*.jpg",
                            "*.jpeg",
                            "*.gif"
                        }
                    }
                }
            });

        var selectedFile = files.FirstOrDefault();

        if (selectedFile == null)
        {
            return;
        }

        string? filePath = selectedFile.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine(
                "This file does not have a local path.");
            return;
        }

        try
        {
            viewModel.AddImageFromPath(filePath);

            Console.WriteLine(
                $"Image imported: {selectedFile.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Could not import image: {ex.Message}");
        }
    }
}
