namespace Receiver_linux_wayland.Payload;

public static partial class Payload
{
    public struct KeyEventDto : IPayload<KeyEventDto>
    {
        public int WpfIdentifier { get; set; }
        public bool IsActive { get; set; }
        public bool IsRepeat { get; set; }
        
        public KeyEventDto(){}

        public byte[] Serialize()
        {
            throw new NotSupportedException("This functionality is not supported on receiver side.");
        }
    }
}