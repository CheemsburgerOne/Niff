using System.Security.Cryptography;

namespace Sender_windows.Network;

public static partial class Network
{
    public partial class NetworkManager
    {
        public async Task EstablishEncryptionWithRemotePeer()
        {
            //Create local CNG key for encryption
            _localCngKeyCryptoDevice = new Cryptography.Cryptography.RsaCng.RsaCngCryptoDevice("Niff");
            
            //Provide server with a rsa public key
            Payload.Payload.HelloDto helloDto = 
                new Payload.Payload.HelloDto(
                    _username,
                    _localCngKeyCryptoDevice.ExportPublicKeyPem());
            
            SendPacket(PacketFlags.Hello, helloDto);

            //Create rsa encrption device from remote rsa public key
            Packet? received = await ReceivePacket();
            Payload.Payload.HelloDto dto = received!.GetPayloadAsType<Payload.Payload.HelloDto>();
            _remoteKeyCryptoDevice = new Cryptography.Cryptography.Rsa.RsaCryptoDevice(dto.PublicRsaKey, true);
            
            _isEncryptionEstablished = true;
        }
        
    }
}