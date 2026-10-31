
using System;

namespace Daymark.Models;

public enum JournalBlockType
{
    Text,
    Image
}

public class JournalBlock
{
    // Unique identifier for this block.
    public Guid Id { get; set; } = Guid.NewGuid();

    // Whether this block contains text or an image.
    public JournalBlockType Type { get; set; }

    // Used for text blocks.
    public string Text { get; set; } = "";

    // Used for image blocks.
    // This will eventually store a managed image reference.
    public string ImagePath { get; set; } = "";
}
