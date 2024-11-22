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
            // Get the host's IP address to set up the server.
            // Here, we're using localhost, which is 127.0.0.1.
            // If the host has multiple IP addresses, you'll get a list of them.
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEP = new IPEndPoint(ipAddress, 11000);

            try
            {
                // Create a TCP socket for the server.
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the endpoint (IP and port).
                listener.Bind(localEP);

                // Set the socket to listen for incoming connections.
                // Here, the server is set to handle one connection at a time.
                listener.Listen(1);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                // Prepare to receive data from the client.
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesReceived = handler.Receive(bytes);

                    // Append the received data to the existing string.
                    data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

                    // Check for the end-of-message marker (<EOF>).
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }

                    // Deserialize and display the received packet.
                    Console.WriteLine(Packet.PacketToString(Packet.Deserialize(bytes)));
                }

                // Close the connection.
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                // Handle any errors that occur.
                Console.WriteLine(ex.ToString());
                return 1;
            }

            Console.WriteLine("Reading process is done.");
            Console.ReadKey();
            return 0;
        }
    }
}
