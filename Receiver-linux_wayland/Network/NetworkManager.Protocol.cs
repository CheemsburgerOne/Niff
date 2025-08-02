using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network;


public partial class NetworkManager
{
    public async Task ExchangePublicRsaKeysPemWithRemoteHost(Payload.Payload.HelloDto helloDto)
    {
        //Check if remote public key is authorized
        if (!_rsaKeyStorage.LoadRemoteHostCryptoDeviceIfRecognized("test", helloDto.PublicRsaKey))
        {
            Disconnect();
            return;
        }
        
        //Provide remote peer with a local rsa key PEM
        Payload.Payload.HelloDto helloDtoResponse = new Payload.Payload.HelloDto(_rsaKeyStorage.LocalHostCryptoDevice.ExportRsaPublicKeyPem());
        SendPacket(PacketFlags.Hello, helloDtoResponse);
        
        //Enable encryption 
        _isEncryptionEstablished = true;
        _peerState = PeerState.ConnectedEncrypted;
    }
}