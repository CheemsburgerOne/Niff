using System.Text.Json;

namespace Sender_windows.Network.Payload;

public interface IPayload<T>
{
    public byte[] Serialize()
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes((T)this, JsonSerializerOptions.Default);
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