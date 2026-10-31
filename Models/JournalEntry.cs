using System;
using System.Collections.Generic;

namespace Daymark.Models;

public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

// A journal entry is made up of blocks, which can be text or images.
    public List<JournalBlock> Blocks { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}