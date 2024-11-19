using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AircraftTransmissionSystem
{
    internal class Program
    {
        static int Main(string[] args)
        {
            StartServer();
            return 0;
        }

        public static int StartServer()
        {
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses

            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress iPAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 11000);

            try
            {
                // Create a Socket that will use Tcp protocol
                Socket listener = new Socket(iPAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                //A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will use 1 request at a time. No more needed.
                listener.Listen(1);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                // Incoming ata from the client.
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                    Console.WriteLine(Packet.PacketToString(Packet.Deserialize(bytes)));
                }


                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
            Console.WriteLine("Reading Process is done.");
            Console.ReadKey();
            return 0;
        }

    }
}
