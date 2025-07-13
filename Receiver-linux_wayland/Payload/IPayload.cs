using System.Text.Json;

namespace Receiver_linux_wayland.Payload;

public interface IPayload<T>
{
    public byte[] Serialize()
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(this, JsonSerializerOptions.Default);
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException($"Failed to serialize payload of {nameof(T)}", ex);
        }
    }

    public bool TryDeserialize(byte[] payloadBytes, out T? payload)
    {
        try
        {
            payload = JsonSerializer.Deserialize<T>(payloadBytes, JsonSerializerOptions.Default);
            return true;
        }
        catch (NotSupportedException ex)
        {
            payload = default(T);
            return false;
        }
    }
}