using System;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using WebSocketSharp;
using WPA_2_proof_of_concept___client;
using WPA_2_proof_of_concept___client.Object;

namespace SocketServerClient // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private static string Mac_Adress { get; set; }
        private static string ConnectionCode = "Connection_String";
        private static string KeyPairingCode = "Key_Pairing";
        private static string CommandCode = "Command_String";
        private static EncryptionGeneration Encryption { get; set; }
        static void Main(string[] args)
        {
            Encryption = new EncryptionGeneration();
            bool sendMessage = true;
            Console.WriteLine("Please enter the url");
            string URL = Console.ReadLine();
            Console.Clear();
            Mac_Adress = Guid.NewGuid().ToString();
            using (WebSocket ws = new WebSocket(URL))
            {
                ws.OnMessage += Ws_OnMessage;
                ws.Connect();
                SendMessage(Mac_Adress, ConnectionCode, ws);
                SendMessage("-", KeyPairingCode, ws);
                bool IsCommand = false;
                while (sendMessage == true)
                {
                    string i = Console.ReadLine();
                    if (i == "!close")
                    {
                        sendMessage = false;
                    }
                    else if(IsCommand == true)
                    {
                        SendMessage(i, CommandCode, ws);
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                        Console.WriteLine("You send the following command: " + i);
                        IsCommand = false;
                    }
                    else if(i == "!command")
                    {
                        IsCommand = true;
                    }
                    else
                    {
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                        Console.WriteLine("You said: " + i);
                        SendMessage(i, "Chat", ws);
                    }
                }
            }
        }
        private static void SendMessage(string message, string subject, WebSocket ws)
        {
            if(subject == ConnectionCode)
            {
                ClientMessage ConnectionMessage = new ClientMessage();
                ConnectionMessage.Subject = ConnectionCode;
                ConnectionMessage.Message = Mac_Adress;
                ConnectionMessage.To = "Server";
                string Fullmessage = JsonConvert.SerializeObject(ConnectionMessage);
                ws.Send(Fullmessage);
            }
            else if(subject == KeyPairingCode)
            {
                ClientMessage ConnectionMessage = new ClientMessage();
                ConnectionMessage.Subject = KeyPairingCode;
                ConnectionMessage.Message = Convert.ToBase64String(Encryption.publickey_read);
                ConnectionMessage.To = "Server";
                string Fullmessage = JsonConvert.SerializeObject(ConnectionMessage);
                ws.Send(Fullmessage);
            }
            else
            {
                EncryptionMessage em = Encryption.Encrypt_Send(message);
                ClientMessage SendMessage = new ClientMessage();
                SendMessage.Subject = subject;
                SendMessage.Message = Convert.ToBase64String(em.message);
                SendMessage.iv = Convert.ToBase64String(em.iv);
                SendMessage.To = "server";
                string Fullmessage = JsonConvert.SerializeObject(SendMessage);
                ws.Send(Fullmessage);
                
            }
        }
        private static void Ws_OnMessage(object? sender, MessageEventArgs e)
        {
            ClientMessage message = JsonConvert.DeserializeObject<ClientMessage>(e.Data);
            if(message.Subject == KeyPairingCode && message.To == Mac_Adress)
            {
                Encryption.GenerateKey(Convert.FromBase64String(message.Message));
            }
            else if(message.To == Mac_Adress)
            {
                EncryptionMessage em = new EncryptionMessage();
                em.message = Convert.FromBase64String(message.Message);
                em.iv = Convert.FromBase64String(message.iv);
                string FullMessage = Encryption.Decrypt_Recieve(em);
                Console.WriteLine("Message: " + FullMessage);
            }
            else
            {
                Console.WriteLine("server send to: " + message.To + " the following message: " + message.Message);
            }
            
        }
    }
}
