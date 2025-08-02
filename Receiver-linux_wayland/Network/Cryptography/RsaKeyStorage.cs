using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Receiver_linux_wayland.Network.Cryptography;

public static partial class Cryptography
{   
    public class RsaKeyStorage
    {
        private readonly DirectoryInfo _localDataDir;
        private readonly DirectoryInfo _authorizedRemoteHostsDirectory;
        
        public Rsa.RsaCryptoDevice LocalHostCryptoDevice => _localKeyCryptoDevice;
        private Rsa.RsaCryptoDevice _localKeyCryptoDevice;

        public Rsa.RsaCryptoDevice RemoteHostCryptoDevice => _remoteKeyCryptoDevice;
        private Rsa.RsaCryptoDevice _remoteKeyCryptoDevice;

        public RsaKeyStorage(string localDataDirectoryPath)
        {
            if (string.IsNullOrEmpty(localDataDirectoryPath)) throw new ArgumentNullException(nameof(localDataDirectoryPath));

            try
            {
                _localDataDir = new DirectoryInfo(localDataDirectoryPath);
                if (!Directory.Exists($"{_localDataDir.FullName}/remote"))
                {
                    _authorizedRemoteHostsDirectory = _localDataDir.CreateSubdirectory("remote");
                    _authorizedRemoteHostsDirectory.UnixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                }
            }
            catch (Exception ex)
            {
                throw new DirectoryNotFoundException("Failed to initialize data folder structure");
            }
        }
        public bool CreateLocalPublicKeyPemPermanent(bool forceIfExists = false)
        {
            if (!forceIfExists)
            {
                bool privateKeyExists = File.Exists($"{_localDataDir.FullName}/local/id_rsa");
                bool publicKeyExists = File.Exists($"{_localDataDir.FullName}/local/id_rsa.pub");

                if (privateKeyExists && publicKeyExists) return false;
            }

            Rsa.RsaCryptoDevice cryptoDevice = new Rsa.RsaCryptoDevice(2048);
            
            try
            {
                var privateKeyFile = File.Create($"{_localDataDir.FullName}/local/id_rsa");
                var publicKeyFile = File.Create($"{_localDataDir.FullName}/local/id_rsa.pub");

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
            string privateKeyPath = $"{_localDataDir.FullName}/local/id_rsa";
            string publicKeyPath = $"{_localDataDir.FullName}/local/id_rsa.pub";

            if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath)) return false;
            
            string privateKey = File.ReadAllText($"{_localDataDir.FullName}/id_rsa");
            string publicKey = File.ReadAllText($"{_localDataDir.FullName}/id_rsa.pub");
            
            _localKeyCryptoDevice = new Rsa.RsaCryptoDevice(privateKey, publicKey);

            return true;
        }

        public bool RegisterRemotePublicKeyPem(string username, string publicKeyPem)
        {
            try
            {
                string keyPath = $"{_authorizedRemoteHostsDirectory.FullName}/remote/{username}.pub";

                if (File.Exists(keyPath)) return false;

                File.Create(_authorizedRemoteHostsDirectory.FullName).Write(Encoding.UTF8.GetBytes(publicKeyPem));
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
                string keyPath = $"{_authorizedRemoteHostsDirectory.FullName}/{username}.pub";

                if (!File.Exists(keyPath)) return false;

                if( File.ReadAllText(keyPath) == publicKeyPem)
                {
                    _remoteKeyCryptoDevice = new Rsa.RsaCryptoDevice(publicKeyPem);
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