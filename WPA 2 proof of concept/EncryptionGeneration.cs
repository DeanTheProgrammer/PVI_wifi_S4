using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using WPA_2_proof_of_concept.Object;

namespace WPA_2_proof_of_concept
{
    public class EncryptionGeneration
    {
        private ECDiffieHellmanCng Key;

        public byte[] PublicKey_Read { get { return PublicKey; } }
        private byte[] PublicKey { get; set; }

        private ECDiffieHellmanCng key;
        public EncryptionGeneration() 
        {
            key = new ECDiffieHellmanCng();
            key.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            key.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = key.PublicKey.ToByteArray();
        }
        public byte[] GetPrivateKey(byte[] public_client_key)
        {
            byte[] result = null;
            result = key.DeriveKeyMaterial(CngKey.Import(public_client_key, CngKeyBlobFormat.EccPublicBlob));
            return result;
        }
        
        public EncryptionMessage Encrypt_Send(byte[] inputkey, string Input_Message)
        {
            EncryptionMessage result = new EncryptionMessage();
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = inputkey;
                result.iv = aes.IV;
                using (MemoryStream ciphertext = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextMessage = Encoding.UTF8.GetBytes(Input_Message);
                    cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                    cs.Close();
                    result.message = ciphertext.ToArray();
                }
            }
            return result;
        }

        public string Decrypt_Recieve(byte[] inputKey, EncryptionMessage input)
        {
            string result;
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = inputKey;
                aes.IV = input.iv;
                // Decrypt the message
                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(input.message, 0, input.message.Length);
                        cs.Close();
                        result = Encoding.UTF8.GetString(plaintext.ToArray());
                    }
                }
            }
            return result;
        }
    }
}
