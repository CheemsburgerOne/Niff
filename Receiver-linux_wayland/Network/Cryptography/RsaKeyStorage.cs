using System.Security.Cryptography;
using System.Text;

namespace Receiver_linux_wayland.Network.Cryptography;

public static partial class Cryptography
{   
    public class RsaKeyStorage
    {
        private readonly DirectoryInfo _authorizedHostsDirectory;
        private readonly DirectoryInfo _authorizedRemoteHostsDirectory;

        public RsaKeyStorage(string directoryPath = "/etc/niff")
        {
            _authorizedHostsDirectory = new DirectoryInfo(directoryPath);
            _authorizedRemoteHostsDirectory = new DirectoryInfo($"{directoryPath}/remote");
        }
        
        public bool IsRemotePublicKeyPemRecognized(string publicKeyPem)
        {
            foreach (var files in _authorizedRemoteHostsDirectory.EnumerateFiles())
            {
                
            }

            return false;
        }
        
        public Rsa.RsaCryptoDevice? LoadLocalKeyFromStorage()
        {
            string privateKeyPath = $"{_authorizedHostsDirectory.FullName}/id_rsa";
            string publicKeyPath = $"{_authorizedHostsDirectory.FullName}/id_rsa.pub";

            if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath)) return null;
            
            string privateKey = File.ReadAllText($"{_authorizedHostsDirectory.FullName}/id_rsa");
            string publicKey = File.ReadAllText($"{_authorizedHostsDirectory.FullName}/id_rsa.pub");
            
            return new Rsa.RsaCryptoDevice(privateKey, publicKey);
        }

        public void RegisterRemotePublicKeyPem(string publicKeyPem)
        {
            return;
        }

        public void RegisterLocalPublicKeyPemPermanent(Rsa.RsaCryptoDevice cryptoDevice)
        {
            FileStream privateKeyFIle = File.Create($"{_authorizedHostsDirectory.FullName}/id_rsa");
            FileStream publicKeyFile = File.Create($"{_authorizedHostsDirectory.FullName}/id_rsa.pub");
            
            privateKeyFIle.Write(Encoding.UTF8.GetBytes(cryptoDevice.ExportRsaPrivateKeyPem()));
            publicKeyFile.Write(Encoding.UTF8.GetBytes(cryptoDevice.ExportRsaPublicKeyPem()));
            
            privateKeyFIle.Close();
            publicKeyFile.Close();
        }

    }

}