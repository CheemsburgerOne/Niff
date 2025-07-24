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

            public RsaCryptoDevice(ReadOnlySpan<char> inputPem)
            {
                _rsa = RSA.Create();
                ImportFromPem(inputPem);
            }
            
            public RsaCryptoDevice(ReadOnlySpan<char> privateKeyPem, ReadOnlySpan<char> publicKeyPem)
            {
                _rsa = RSA.Create();
                ImportFromPem(privateKeyPem, publicKeyPem);
            }

            public RsaCryptoDevice(int keySizeInBits)
            {
                _rsa = RSA.Create(keySizeInBits);
            }
            
            private void ImportFromPem(ReadOnlySpan<char> privateKeyPem, ReadOnlySpan<char> publicKeyPem)
            {
                ImportFromPem(privateKeyPem);
                ImportFromPem(privateKeyPem);
            }
            
            private void ImportFromPem(ReadOnlySpan<char> input)
            {
                _rsa!.ImportFromPem(input);
            }
            
            public string ExportRsaPublicKeyPem() => _rsa!.ExportRSAPublicKeyPem();
            public string ExportRsaPrivateKeyPem() => _rsa!.ExportRSAPrivateKeyPem();
            
            public byte[] Encrypt(byte[] data) => _rsa!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Encrypt(ReadOnlySpan<byte> data) => _rsa!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            
            public byte[] Decrypt(byte[] data) => _rsa!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Decrypt(ReadOnlySpan<byte> data) => _rsa!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
        }
    }
}