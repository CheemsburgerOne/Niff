using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network;

public static partial class Network
{
    public partial class NetworkManager
    {
        public async Task EstablishEncryptionWithRemotePeer(Payload.Payload.HelloDto helloDto)
        {
            //Check if remote public key is authorized
            if (!_rsaKeyStorage.IsRemotePublicKeyPemRecognized(helloDto.PublicRsaKey)) return;

            Payload.Payload.HelloDto helloDtoResponse =
                new Payload.Payload.HelloDto(_localKeyCryptoDevice.ExportRsaPublicKeyPem());
            
            SendPacket(PacketFlags.Hello, helloDto); ;
            
            _isEncryptionEstablished = true;
        }
    }
}