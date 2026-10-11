
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Daymark.Models;

namespace Daymark.Services;

public class JournalStorageService
{
    private readonly string _filePath;

    public JournalStorageService()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Daymark"
        );

        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "journals.json");
    }

    public void SaveEntries(List<JournalEntry> entries)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(entries, options);
        File.WriteAllText(_filePath, json);
    }

    public List<JournalEntry> LoadEntries()
    {
        if (!File.Exists(_filePath))
        {
            return new List<JournalEntry>();
        }

        string json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<List<JournalEntry>>(json)
               ?? new List<JournalEntry>();
    }
}
