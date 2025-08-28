using System.Security.Cryptography;
using System.Text;

namespace Receiver_linux_wayland.Network.Cryptography;

public static partial class Cryptography
{   
    public static class Rsa
    {
        public class RsaCryptoDevice
        {
            public int KeySize => _rsa?.KeySize ?? 0;
            
            private readonly RSA? _rsa;

            public RsaCryptoDevice(ReadOnlySpan<char> keyPem)
            {
                _rsa = RSA.Create();
                _rsa.ImportFromPem(keyPem);
            }
            
            public RsaCryptoDevice(ReadOnlySpan<char> privateKeyPem, ReadOnlySpan<char> publicKeyPem)
            {
                _rsa = RSA.Create();
                _rsa.ImportFromPem(privateKeyPem);
                _rsa.ImportFromPem(publicKeyPem);
            }

            public RsaCryptoDevice(int keySizeInBits)
            {
                _rsa = RSA.Create(keySizeInBits);
            }

            public bool TryExportRsaKeyPem(out string? key, bool publicKey)
            {
                try
                {
                    key = publicKey ? _rsa!.ExportRSAPublicKeyPem() : _rsa!.ExportRSAPrivateKeyPem();
                }
                catch
                {
                    key = null;
                    return false;
                }

                return true;
            }

            public byte[] Encrypt(byte[] data) => _rsa!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Encrypt(ReadOnlySpan<byte> data) => _rsa!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            
            public byte[] Decrypt(byte[] data) => _rsa!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Decrypt(ReadOnlySpan<byte> data) => _rsa!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
        }
    }
}