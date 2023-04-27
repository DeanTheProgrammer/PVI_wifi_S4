using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WPA_2_proof_of_concept.Object;
using Newtonsoft.Json;
using WebSocketSharp;

namespace WPA_2_proof_of_concept
{
    public class RouterServer : WebSocketBehavior
    {
        private List<ConnectionObject> Connections;
        private string _prefix;
        private EncryptionGeneration Encryption { get; set; }
        private string ConnectionCode = "Connection_String";
        private string KeyPairingCode = "Key_Pairing";
        private string CommandCode = "Command_String";
        public RouterServer() 
        {
            _prefix = "password";
            Connections = new List<ConnectionObject>();
            Encryption = new EncryptionGeneration(); 
        }
        protected override void OnOpen()
        {
            Console.WriteLine("Following device just connected: " + ID);
            ConnectionObject NewDevice = new ConnectionObject();
            NewDevice.Id = ID;
            Connections.Add(NewDevice);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            ServerMessage Message = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerMessage>(e.Data);
            if (Message.Subject == KeyPairingCode)
            {
                byte[] Key = Convert.FromBase64String(Message.Message);
                byte[] PrivateKey = Encryption.GetPrivateKey(Key);
                Connections.Find(x => x.Id == ID).public_Key = Key;
                Connections.Find(x => x.Id == ID).Private_key = PrivateKey;
                ServerMessage SendBack = new ServerMessage();
                EncryptionMessage temp = Encryption.Encrypt_Send(PrivateKey, "You have succesfully connected");
                SendBack.Subject = "message";
                SendBack.To = Connections.Find(x => x.Id == ID).MAC_Adress;
                SendBack.Message = Convert.ToBase64String(temp.message);
                SendBack.iv = Convert.ToBase64String(temp.iv);
                string Fullmessage = Newtonsoft.Json.JsonConvert.SerializeObject(SendBack);
                Sessions.Broadcast(Fullmessage);
            }
            else if(Message.Subject == ConnectionCode)
            {
                Connections.Find(x => x.Id == ID).MAC_Adress = Message.Message;
                byte[] publicKey = Encryption.PublicKey_Read;
                string base64key = Convert.ToBase64String(publicKey);
                ServerMessage message = new ServerMessage();
                message.Message = base64key;
                message.Subject = KeyPairingCode;
                message.To = Message.Message;
                string FullMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                Sessions.Broadcast(FullMessage);
            }
            else
            {
                if(Connections.Find(x => x.Id == ID).Private_key != null)
                {
                    EncryptionMessage em = new EncryptionMessage();
                    em.message = Convert.FromBase64String(Message.Message);
                    em.iv = Convert.FromBase64String(Message.iv);
                    string result = Encryption.Decrypt_Recieve(Connections.Find(x => x.Id == ID).Private_key, em);
                    Console.WriteLine(result);
                }
                else
                {
                    Connections.Remove(Connections.Find(x => x.Id == ID));
                    Sessions.CloseSession(ID);
                }
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Following device just disconnected: " + ID);
        }
    }
}
