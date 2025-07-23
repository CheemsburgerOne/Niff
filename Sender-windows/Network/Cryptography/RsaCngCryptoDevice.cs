using System.Runtime.Versioning;
using System.Security.Cryptography;
namespace Sender_windows.Network.Cryptography;

public static partial class Cryptography
{   
    public static class RsaCng
    {
        [SupportedOSPlatform("windows")]
        public static void CreateCngKeyAndStoreOnLocalDevice(string keyName, CngKeyCreationParameters keyCreationParameters)
        {
            if ( string.IsNullOrEmpty(keyName) || keyCreationParameters == null) throw new ArgumentNullException("Key name cannot be null or empty.");
            
            try
            {
                CngKey.Exists(keyName);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Cannot access key storage on the device", ex);
            }

            try
            {
                CngKey.Create(CngAlgorithm.Rsa, keyName, keyCreationParameters);
            }
            catch
            {
                throw new CryptographicException($"Cannot create key: {keyName}");
            }
        }
        
        [SupportedOSPlatform("windows")]
        public static bool IsCngKeyStoredOnCurrentDevice(string keyName) => CngKey.Exists(keyName);
        
        [SupportedOSPlatform("windows")]
        public class RsaCngCryptoDevice
        {
            private static readonly string DefaultCngKeyTitle = "Niff encryption key";
            private static readonly string DefaultCngKeyDescription = "Encryption key used to generate RSA key pair for remote authorization";
            private static readonly CngKeyCreationParameters DefaultCngKeyCreationParameters = new CngKeyCreationParameters()
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextArchiving,
                KeyUsage = CngKeyUsages.AllUsages,
                Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                UIPolicy = new CngUIPolicy(CngUIProtectionLevels.ProtectKey, DefaultCngKeyTitle, DefaultCngKeyDescription),
                KeyCreationOptions = CngKeyCreationOptions.None,
            };
            
            public bool IsEncryptorCreated => _rsaCng != null;
            public int KeySize => _rsaCng?.KeySize ?? 0;
            
            private CngKey? _cngKey;
            private RSACng? _rsaCng;

            public RsaCngCryptoDevice(string keyName) => LoadOrCreateFromLocalCngKey(keyName);
            
            private void LoadOrCreateFromLocalCngKey(string keyName)
            {
                if ( string.IsNullOrEmpty(keyName) ) throw new ArgumentNullException(nameof(keyName));

                if (!IsCngKeyStoredOnCurrentDevice(keyName)) 
                    CreateCngKeyAndStoreOnLocalDevice(keyName, DefaultCngKeyCreationParameters );

                try
                {
                    _cngKey = CngKey.Open(keyName);
                }
                catch (Exception ex)
                {
                    throw new CryptographicException($"Failed to open key '{keyName}'", ex);
                }
                
                if (_cngKey == null) throw new InvalidOperationException("Key has not been loaded");

                _rsaCng = new RSACng(_cngKey);
            }
            
            public string ExportRsaPkcs8PublicKeyPem()
            {
                if (_rsaCng == null) throw new InvalidOperationException("Key has not been loaded");
                return _rsaCng.ExportRSAPublicKeyPem();
            }

            public byte[] Encrypt(byte[] data) => _rsaCng!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Encrypt(ReadOnlySpan<byte> data) => _rsaCng!.Encrypt(data, RSAEncryptionPadding.OaepSHA3_256);

            public byte[] Decrypt(byte[] data) => _rsaCng!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
            public byte[] Decrypt(ReadOnlySpan<byte> data) => _rsaCng!.Decrypt(data, RSAEncryptionPadding.OaepSHA3_256);
        }
    }
}