using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Sender_windows.Network.Cryptography;

public static partial class Cryptography
{   
    public static class Rsa
    {
        public class RsaCryptoDevice
        {
            public int KeySize => _rsa?.KeySize ?? 0;
            
            private readonly RSA? _rsa;

            public RsaCryptoDevice(ReadOnlySpan<char> input, bool isInputPemFormat)
            {
                _rsa = RSA.Create();
                if (isInputPemFormat) LoadFromPem(input);
                else LoadFromPkcs1(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(input.ToArray())));
            }

            private void LoadFromPem(ReadOnlySpan<char> input)
            {
                _rsa!.ImportFromPem(input);
            }
            
            private void LoadFromPkcs1(ReadOnlySpan<byte> input)
            {
                _rsa!.ImportRSAPublicKey(input, out int bytesRead);
            }
            
            public string ExportRsaPublicKeyPem() => _rsa!.ExportRSAPublicKeyPem();
            
            public byte[] Encrypt(byte[] data) => _rsa!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Encrypt(ReadOnlySpan<byte> data) => _rsa!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            
            public byte[] Decrypt(byte[] data) => _rsa!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Decrypt(ReadOnlySpan<byte> data) => _rsa!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
        }
    }
}