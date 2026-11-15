using System;
using System.IO;
using Avalonia.Media.Imaging;
using Daymark.Models;

namespace Daymark.ViewModels;

public class JournalBlockViewModel : ViewModelBase, IDisposable
{
    public JournalBlock Model { get; }
    public JournalBlockType Type => Model.Type;
    public bool IsText => Type == JournalBlockType.Text;
    public bool IsImage => Type == JournalBlockType.Image;
    public Bitmap? ImagePreview { get; }

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value))
                Model.Text = value;
        }
    }

    private bool _isReadOnly;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }

    public JournalBlockViewModel(JournalBlock model, bool isReadOnly = false)
    {
        Model = model;
        _text = model.Text ?? "";
        _isReadOnly = isReadOnly;

        if (IsImage && !string.IsNullOrWhiteSpace(model.ImagePath) && File.Exists(model.ImagePath))
        {
            try { ImagePreview = new Bitmap(model.ImagePath); }
            catch (Exception ex) { Console.WriteLine($"Could not load image: {ex.Message}"); }
        }
    }

    public void Dispose() => ImagePreview?.Dispose();
}
