using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Receiver_linux_wayland.Network.Cryptography;

public static partial class Cryptography
{   
    public class RsaKeyStorage
    {
        private readonly DirectoryInfo _localKeyDir;
        private readonly DirectoryInfo _remoteKeysDir;

        private readonly string _privateKeyFilename = "id_rsa";
        private readonly string _publicKeyFilename =  "id_rsa_pub";

        public Rsa.RsaCryptoDevice? LocalHostCryptoDevice { get; private set; }

        public Rsa.RsaCryptoDevice? RemoteHostCryptoDevice { get; private set; }

        public RsaKeyStorage(DirectoryInfo etcDir)
        {
            try
            {
                _localKeyDir = etcDir.GetDirectories("local").Single();
                _remoteKeysDir = etcDir.GetDirectories("remote").Single();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Etc directory structure is ambiguous", ex);
            }
            catch (Exception ex)
            {
                throw new  InvalidOperationException("Etc directory failed to open", ex);
            }
        }
        
        public void RegisterNewLocalRsaKeyPermanent(Rsa.RsaCryptoDevice rsa, bool force = false)
        {
            bool privateKeyExists = File.Exists($"{_localKeyDir.FullName}/id_rsa");
            bool publicKeyExists = File.Exists($"{_localKeyDir.FullName}/id_rsa.pub");

            if (!force && (privateKeyExists || publicKeyExists)) throw new InvalidOperationException("Key already exists");

            bool isPrivKeyExtracted = rsa.TryExportRsaKeyPem(out string? privKey, false);
            bool isPubKeyExtracted = rsa.TryExportRsaKeyPem(out string? pubKey, true);
            
            if (!isPrivKeyExtracted || !isPubKeyExtracted) throw new ArgumentException("Rsa device does not contain required keys");
            
            var privateKeyFile = File.Create($"{_localKeyDir.FullName}/id_rsa".Replace("//", "/"));
            var publicKeyFile = File.Create($"{_localKeyDir.FullName}/id_rsa.pub".Replace("//","/"));

            privateKeyFile.Write(Encoding.UTF8.GetBytes(privKey!));
            publicKeyFile.Write(Encoding.UTF8.GetBytes(pubKey!));

            privateKeyFile.Close();
            publicKeyFile.Close();
        }
        
        public bool TryLoadLocalKeyFromStorage()
        {
            string privateKeyPath = $"{_localKeyDir.FullName}/{_privateKeyFilename}";
            string publicKeyPath = $"{_localKeyDir.FullName}/{_publicKeyFilename}";

            if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath)) return false;

            try
            {
                string privateKey = File.ReadAllText(privateKeyPath);
                string publicKey = File.ReadAllText(publicKeyPath);

                LocalHostCryptoDevice = new Rsa.RsaCryptoDevice(privateKey, publicKey);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool RegisterRemotePublicKeyPem(string username, string publicKeyPem)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(publicKeyPem)) return false;
            
            string keyPath = $"{_remoteKeysDir.FullName}/{username}.pub";
            
            if (File.Exists(keyPath)) return false;
            
            try
            {
                File.Create(_remoteKeysDir.FullName).Write(Encoding.UTF8.GetBytes(publicKeyPem));
            }
            catch (Exception ex)
            {
                return false;
            }
            
            return true;
        }
        
        public bool LoadRemoteHostCryptoDeviceIfRecognized(string username, string publicKeyPem)
        {
            try
            {
                string keyPath = $"{_remoteKeysDir.FullName}/{username}.pub";

                if (!File.Exists(keyPath)) return false;
                
                string localReference = File.ReadAllText(keyPath);
                
                if( localReference == publicKeyPem)
                {
                    RemoteHostCryptoDevice = new Rsa.RsaCryptoDevice(localReference);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        public void UnloadRemoteHostCryptoDevice() => RemoteHostCryptoDevice = null;
    }
}