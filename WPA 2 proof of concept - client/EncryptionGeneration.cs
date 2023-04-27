using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection.Metadata;
using WPA_2_proof_of_concept___client.Object;

namespace WPA_2_proof_of_concept___client
{
    public class EncryptionGeneration
    {
        private byte[] privatekey { get; set; }
        public byte[] publickey_read { get { return publickey; } }
        private byte[] publickey { get; set; }
        private ECDiffieHellmanCng encryptionMethod;
        public EncryptionGeneration()
        {
            privatekey = null;
            encryptionMethod = new ECDiffieHellmanCng();
            encryptionMethod.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            encryptionMethod.HashAlgorithm = CngAlgorithm.Sha256;
            publickey = encryptionMethod.PublicKey.ToByteArray();
        }

        public void GenerateKey(byte[] ServerPublicKey)
        {
            if (privatekey == null) 
            {
                privatekey = encryptionMethod.DeriveKeyMaterial(CngKey.Import(ServerPublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }
        public EncryptionMessage Encrypt_Send(string Input_Message)
        {
            EncryptionMessage result = new EncryptionMessage();
            if (privatekey != null)
            {
                using (Aes aes = new AesCryptoServiceProvider())
                {
                    aes.Key = privatekey;
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
            }
            return result;
        }

        public string Decrypt_Recieve(EncryptionMessage input)
        {
            string result = null;
            if(privatekey != null)
            {
                using (Aes aes = new AesCryptoServiceProvider())
                {
                    aes.Key = privatekey;
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
            }
            return result;
        }
    }
}
