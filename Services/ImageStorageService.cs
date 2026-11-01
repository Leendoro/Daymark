
using System;
using System.IO;

namespace Daymark.Services;

public class ImageStorageService
{
    private readonly string _imageFolder;

    public ImageStorageService()
    {
        string appFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Daymark"
        );

        _imageFolder = Path.Combine(appFolder, "Images");

        Directory.CreateDirectory(_imageFolder);
    }

    // Copies an image into Daymark's own storage folder.
    public string ImportImage(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(
                "The selected image could not be found.",
                sourcePath);
        }

        string extension = Path.GetExtension(sourcePath).ToLowerInvariant();

        if (extension != ".png" &&
            extension != ".jpg" &&
            extension != ".jpeg" &&
            extension != ".gif")
        {
            throw new InvalidOperationException(
                "Only PNG, JPG, JPEG, and GIF images are supported.");
        }

        string uniqueFileName =
            $"{Guid.NewGuid()}{extension}";

        string destinationPath = Path.Combine(
            _imageFolder,
            uniqueFileName
        );

        File.Copy(sourcePath, destinationPath);

        return destinationPath;
    }
}

