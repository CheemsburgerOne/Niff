namespace Sender_windows.Network.Payload;

public static partial class Payload
{
    public struct EmptyDto : IPayload<EmptyDto>
    {
        public EmptyDto(){}
    }
}