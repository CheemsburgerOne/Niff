namespace Receiver_linux_wayland.Clipboard;

public class ClipboardItem
{
    public required string Identifier { get; init; }
    public required string OriginalName { get; init; }
    public required string? Extension { get; init; }
    public required ContentType ContentType { get; init; }
    public required byte[] Data { get; init; }
}