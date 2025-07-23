using System.Security.Cryptography;

namespace Sender_windows.Network;

public static partial class Network
{
    public partial class NetworkManager
    {
        public async Task EstablishEncryptionWithRemotePeer()
        {
            _localCngKeyCryptoDevice = new Cryptography.Cryptography.RsaCng.RsaCngCryptoDevice("Niff");

            string exportedKey = _localCngKeyCryptoDevice.ExportRsaPkcs8PublicKeyPem();
            
            Payload.Payload.HelloDto helloDto = new Payload.Payload.HelloDto(exportedKey);
            
            SendPacket(PacketFlags.Hello, helloDto);

            Packet? received = await ReceivePacket();
            
            Payload.Payload.HelloDto dto = received!.GetPayloadAsType<Payload.Payload.HelloDto>();

            _remoteKeyCryptoDevice = new Cryptography.Cryptography.Rsa.RsaCryptoDevice(dto.PublicRsaKey, true);
            
            _isEncryptionEstablished = true;
        }
        
    }
}