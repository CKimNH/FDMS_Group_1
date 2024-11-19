

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AircraftTransmissionSystem
{
    internal class Program
    {
        public const string format = @"\d{1,2}_\d{1,2}_\d{4} \d{1,2}:\d{1,2}:\d{1,2},-?\d{1,4}.\d{6}, -?\d{1,4}.\d{6}, -?\d{1,4}.\d{6}, -?\d{1,4}.\d{6}, -?\d{1,4}.\d{6}, -?\d{1,4}.\d{6}, -?\d{1,4}.\d{6},";

        static int Main(string[] args)
        {
            StartClient();
            return 0;
        }

        public static void StartClient()
        {
            byte[] bytes = new byte[1024];
            // Connect to a Remote server
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
            int maxAttempts = 100;
            int attempts = 0;

            // Create a TCP/IP  socket.
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            while (attempts < maxAttempts)
            {
                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);
                    // break if connection was succeeded.
                    attempts = 0;
                    break;
                }
                catch (SocketException se)
                {
                    if (attempts >= maxAttempts)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                        return;
                    }
                    attempts++;
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    return;
                }
            }

//            Console.WriteLine("Socket connected to {0}",
//                sender.RemoteEndPoint.ToString());

            string[] fileNames = Directory.GetFiles(@".\logs");
            string lineRead = "";
            string dataForPacket = "", separator = "|";
            uint sequenceNum = 0;
            double checksumNum = 0.0;
            Regex rg = new Regex(format);
            foreach (string fileName in fileNames)
            {
                using (StreamReader fr = new StreamReader(fileName))
                {
                    sequenceNum = 0;
                    while ((lineRead = fr.ReadLine()) != null)
                    {
                        if (rg.IsMatch(lineRead))
                        {
                            string[] data = lineRead.Split(',');
                            checksumNum = checksum(Convert.ToDouble(data[5]), Convert.ToDouble(data[6]), Convert.ToDouble(data[7]));
                            dataForPacket = "";
                            dataForPacket += Path.GetFileNameWithoutExtension(fileName);
                            dataForPacket += separator;
                            dataForPacket += sequenceNum.ToString();
                            dataForPacket += separator;
                            dataForPacket += lineRead;
                            dataForPacket += separator;
                            dataForPacket += checksumNum.ToString();

                            Packet sendingPacket = new Packet(dataForPacket);

                            int byteSent = sender.Send(sendingPacket.Serialize());

                            sequenceNum++;
                        }
                        else
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }
            }

            bytes = Encoding.ASCII.GetBytes("This is a test<EOF>");

            // Send the data through the socket.
            int bytesSent = sender.Send(bytes);

            /*
                                // Receive the response from the remote device.
                                int bytesRec = sender.Receive(bytes);
                                Console.WriteLine("Echoed test = {0}",
                                    Encoding.ASCII.GetString(bytes, 0, bytesRec));
            */
            // Release the socket.
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            
        }

        public static double checksum(double alt, double pitch, double bank)
        {
            return (alt + pitch + bank) / 3;
        }
    }
}
