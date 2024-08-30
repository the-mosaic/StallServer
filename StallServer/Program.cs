using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace StallServer
{
    public class POPS4Server
    {
        public static int SERVER_PORT = 8005;

        private TcpListener listener;
        private List<Thread> clientThreads;

        public POPS4Server()
        {
            this.clientThreads = new List<Thread>();

            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), SERVER_PORT);
            this.listener.Start();

            Util.Log("[POPS4Server]", $"Server started. Port is {SERVER_PORT}.");

            while (true)
            {
                TcpClient client = this.listener.AcceptTcpClient();

                Thread clientThread = new Thread(() => { ClientThreadEntry(client); });
                clientThread.Start();

                this.clientThreads.Add(clientThread);
            }
        }

        private static void ClientThreadEntry(TcpClient client)
        {
            Stall stall = new Stall(client);
            stall.Serve();
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            POPS4Server server = new POPS4Server();
        }
    }
}
