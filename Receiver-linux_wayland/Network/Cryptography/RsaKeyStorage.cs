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
        
        public Rsa.RsaCryptoDevice LocalHostCryptoDevice => _localKeyCryptoDevice;
        private Rsa.RsaCryptoDevice _localKeyCryptoDevice;

        public Rsa.RsaCryptoDevice RemoteHostCryptoDevice => _remoteKeyCryptoDevice;
        private Rsa.RsaCryptoDevice _remoteKeyCryptoDevice;

        public RsaKeyStorage(DirectoryInfo etcDir)
        {
            _localKeyDir = etcDir.GetDirectories("local").Single();
            _remoteKeysDir = etcDir.GetDirectories("remote").Single();
        }
        
        public bool RegisterNewLocalRsaKeyPermanent(Rsa.RsaCryptoDevice rsa, bool force = false)
        {
            bool overriden = false;
            
            bool privateKeyExists = File.Exists($"{_localKeyDir.FullName}/id_rsa");
            bool publicKeyExists = File.Exists($"{_localKeyDir.FullName}/id_rsa.pub");

            if (!force && (privateKeyExists || publicKeyExists)) return false;
              
            try
            {
                var privateKeyFile = File.Create($"{_localKeyDir.FullName}/local/id_rsa");
                var publicKeyFile = File.Create($"{_localKeyDir.FullName}/local/id_rsa.pub");

                privateKeyFile.Write(Encoding.UTF8.GetBytes(cryptoDevice.ExportRsaPrivateKeyPem()));
                publicKeyFile.Write(Encoding.UTF8.GetBytes(cryptoDevice.ExportRsaPublicKeyPem()));

                privateKeyFile.Close();
                publicKeyFile.Close();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        
        public bool LoadLocalKeyFromStorage()
        {
            string privateKeyPath = $"{_localKeyDir.FullName}/id_rsa";
            string publicKeyPath = $"{_localKeyDir.FullName}/id_rsa.pub";

            if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath)) return false;
            
            string privateKey = File.ReadAllText(privateKeyPath);
            string publicKey = File.ReadAllText(publicKeyPath);
            
            _localKeyCryptoDevice = new Rsa.RsaCryptoDevice(privateKey, publicKey);

            return true;
        }

        public bool RegisterRemotePublicKeyPem(string username, string publicKeyPem)
        {
            try
            {
                string keyPath = $"{_remoteKeysDir.FullName}/remote/{username}.pub";

                if (File.Exists(keyPath)) return false;

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
                    _remoteKeyCryptoDevice = new Rsa.RsaCryptoDevice(localReference);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }
    }
}