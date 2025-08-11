using System.IO.Enumeration;
using System.Reflection.Metadata.Ecma335;

namespace Receiver_linux_wayland.Clipboard;

public class ClipboardManager
{
    private List<ClipboardItem> Items = new List<ClipboardItem>(10);
    private ClipboardItem Selected;

    public ClipboardManager()
    {
        
    }

    public bool Select(int index)
    {
        if (index < 0 || index >= Items.Count) return false;
        Selected = Items[index];
        return true;
    }

    public bool AddToClipboard(bool isPlaintext, string name, byte[] content)
    {
        if (isPlaintext)
        {
            ClipboardItem newItemPlaintext = new ClipboardItem()
            {
                ContentType = ContentType.Plaintext,
                Data = content,
                Extension = String.Empty,
                OriginalName = String.Empty,
                Identifier = String.Empty 
            };
            Items.Add(newItemPlaintext);
            return true;
        }
        
        if (!TryValidateNonPlaintext(name, content, out string? filename, out string? extension)) return false;
        
        ClipboardItem newItem = new ClipboardItem()
        {
            ContentType = GetContentType(extension),
            Data = content,
            Extension = extension,
            OriginalName = name,
            Identifier = Guid.NewGuid().ToString(), 
        };

        return true;
    }

    public void Clear() => Items.Clear();
    
    private bool TryValidateNonPlaintext(string name, byte[] content, out string? filename,  out string? extension)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name) || name[0] == '.' || content.Length <= 0 )
        {
            filename = null;
            extension = null;
            return false;
        }

        if (!name.Contains('.'))
        {
            filename = name;
            extension = null;
            return true;
        }

        var parts = name.Split('.');
        if (parts.Length == 2)
        {
            filename = parts[0];
            extension = parts[1];
            return true;
        }
        else
        {
            filename = string.Join("", parts.SkipLast(1));
            extension = parts.Last();
            return true;
        }
    }

    private ContentType GetContentType(string? extension)
    {
        if (string.IsNullOrEmpty(extension)) return ContentType.Other;

        return extension switch
        {
            "jpg" or "jpeg" or "png" or "bmg" or "jif" => ContentType.Image,
            "mp4" or "mp5" or "avi" or "webp" => ContentType.Video,
            _ => ContentType.Other
        };
    }
}