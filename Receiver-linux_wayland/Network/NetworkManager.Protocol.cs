using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network;


public partial class NetworkManager
{
    public void ExchangePublicRsaKeysPemWithRemoteHost(Payload.Payload.HelloDto helloDto)
    {
        if (string.IsNullOrEmpty(helloDto.PublicRsaKey) || string.IsNullOrEmpty(helloDto.Username))
        {
            Disconnect();
            return;
        }
        //Check if remote public key is authorized
        if (!_rsaKeyStorage.LoadRemoteHostCryptoDeviceIfRecognized(helloDto.Username, helloDto.PublicRsaKey))
        {
            Disconnect();
            return;
        }
        
        //Provide remote peer with a local rsa key PEM
        if (!_rsaKeyStorage.LocalHostCryptoDevice!.TryExportRsaKeyPem(out string? publicKeyPem, true))
        {
            Disconnect();
            return;
        }
        
        Payload.Payload.HelloDto helloDtoResponse = new Payload.Payload.HelloDto(_serverName, publicKeyPem);
        SendPacket(PacketFlags.Hello, helloDtoResponse);
        
        //Enable encryption 
        _isEncryptionEstablished = true;
        PeerState = PeerState.ConnectedEncrypted;
    }
}