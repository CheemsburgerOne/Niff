using System.Security.Cryptography;

namespace Sender_windows.Network;

public static partial class Network
{
    public partial class NetworkManager
    {
        public async Task<bool> EstablishEncryptionWithRemotePeer()
        {
            //Provide server with a rsa public key
            Payload.Payload.HelloDto helloDto = 
                new Payload.Payload.HelloDto(
                    _username,
                    _localCngKeyCryptoDevice!.ExportPublicKeyPem());

            if (!SendPacket(PacketFlags.Hello, helloDto)) return false;
            
            Packet? received = await ReceivePacket();
            if (received == null) return false;
            Payload.Payload.HelloDto dto = received.GetPayloadAsType<Payload.Payload.HelloDto>();
            _remoteKeyCryptoDevice = new Cryptography.Cryptography.Rsa.RsaCryptoDevice(dto.PublicRsaKey, true);
            
            _isEncryptionEstablished = true;
            return true;
        }
        
    }
}