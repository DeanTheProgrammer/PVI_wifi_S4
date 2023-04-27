using System;
using System.Net.Sockets;
using System.Net;
using WebSocketSharp.Server;
using WPA_2_proof_of_concept;

namespace RouterSocketServer // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private static WebSocketServer ServerWs { get; set; }
        private static string LocalIp { get; set; }
        private static string Port = ":7282";
        private static void Main(string[] args)
        {
            string password = "test123";
            string test = GetLocalIpAddres();
            if (test.Length > 1)
            {
                LocalIp = GetLocalIpAddres();
            }
            Console.WriteLine("Which password do you want use for  wifi-router?");
            password = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("WS server started on ws://" + LocalIp + Port + "/test");
            ServerWs = new WebSocketServer("ws://" + LocalIp + Port);
            Console.WriteLine("Press any key to close the server");
            ServerWs.AddWebSocketService<RouterServer>("/test");
            ServerWs.Start();
            Console.ReadKey();
            Console.WriteLine("Clossing router");
            ServerWs.Stop();
        }
        public static string GetLocalIpAddres()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {

                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                throw new Exception("No network adapters with an IPv4 address in the System!");
            }
            else
            {
                throw new Exception("You'r not connected to the network");
            }
        }
    }
}