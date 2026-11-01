
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Daymark.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Opens the file picker when Add Image is clicked.
    private async void AddImage_Click(
        object? sender,
        RoutedEventArgs e)
    {
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

        // For now, confirm that we selected a file.
        Console.WriteLine(
            $"Selected image: {selectedFile.Name}");

        // Next, we'll pass the selected image
        // to our journal editor.
    }
}
