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
        public async Task EstablishEncryptionWithRemotePeer()
        {
            
            _localKeyCryptoDevice = new Cryptography.Cryptography.Rsa.RsaCryptoDevice()

            string exportedKey = _localKeyCryptoDevice.ExportRsaPkcs8PublicKeyPem();

            Payload.Payload.HelloDto helloDto = new Payload.Payload.HelloDto(exportedKey);

            SendPacket(PacketFlags.Hello, helloDto);

            Packet? received = await ReceivePacket();

            Payload.Payload.HelloDto dto = received!.GetPayloadAsType<Payload.Payload.HelloDto>();

            _remoteKeyCryptoDevice = new Cryptography.Cryptography.Rsa.RsaCryptoDevice(dto.PublicRsaKey, true);

            _isEncryptionEstablished = true;
        }
    }
}