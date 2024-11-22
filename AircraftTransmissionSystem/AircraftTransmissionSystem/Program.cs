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
            // Set up a connection to the server.
            // We're using localhost here, which has the IP address 127.0.0.1.
            // If the server has multiple IPs, this will just grab one of them.
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
            int maxAttempts = 100;
            int attempts = 0;

            // Create a TCP socket to communicate with the server.
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Try to connect to the server. Keep retrying until the limit is reached or it succeeds.
            while (attempts < maxAttempts)
            {
                try
                {
                    // Attempt to connect.
                    sender.Connect(remoteEP);
                    // If successful, break out of the loop.
                    attempts = 0;
                    break;
                }
                catch (SocketException se)
                {
                    if (attempts >= maxAttempts)
                    {
                        Console.WriteLine("SocketException: {0}", se.ToString());
                        return;
                    }
                    attempts++;
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException: {0}", ane.ToString());
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception: {0}", e.ToString());
                    return;
                }
            }

            // Read all the log files in the "logs" directory.
            string[] fileNames = Directory.GetFiles(@".\logs");
            string lineRead = "";
            string packetData = "", separator = "|";
            uint sequenceNumber = 0;
            double checksumNumber = 0.0;
            Regex regex = new Regex(format);

            foreach (string fileName in fileNames)
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    sequenceNumber = 0;
                    while ((lineRead = reader.ReadLine()) != null)
                    {
                        if (regex.IsMatch(lineRead))
                        {
                            // If the line matches the expected format, process it.
                            string[] data = lineRead.Split(',');
                            checksumNumber = checksum(Convert.ToDouble(data[5]), Convert.ToDouble(data[6]), Convert.ToDouble(data[7]));
                            packetData = "";
                            packetData += Path.GetFileNameWithoutExtension(fileName);
                            packetData += separator;
                            packetData += sequenceNumber.ToString();
                            packetData += separator;
                            packetData += lineRead;
                            packetData += separator;
                            packetData += checksumNumber.ToString();

                            Packet sendPacket = new Packet(packetData);

                            // Send the packet to the server.
                            int byteSent = sender.Send(sendPacket.Serialize());

                            sequenceNumber++;
                        }
                        else
                        {
                            // Stop processing if the line doesn't match the expected format.
                            break;
                        }
                        // Pause for a second before reading the next line.
                        Thread.Sleep(1000);
                    }
                }
            }

            bytes = Encoding.ASCII.GetBytes("This is a test<EOF>");

            // Send a test message to the server.
            int bytesSent = sender.Send(bytes);

            // Close the connection.
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        // Calculate the checksum as the average of altitude, pitch, and bank.
        public static double checksum(double alt, double pitch, double bank)
        {
            return (alt + pitch + bank) / 3;
        }
    }
}
