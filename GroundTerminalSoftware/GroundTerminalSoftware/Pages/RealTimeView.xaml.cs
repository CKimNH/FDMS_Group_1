﻿using GroundTerminalSoftware.Models;
using GroundTerminalSoftware.ViewModels;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GroundTerminalSoftware.Pages
{
    /// <summary>
    /// Interaction logic for RealTimeView.xaml
    /// </summary>
    public partial class RealTimeView : Page
    {
        SqlConnection connection;
        IPHostEntry host;
        IPAddress iPAddress;
        IPEndPoint localEndPoint;
        public Socket handler;
        private RealTimeViewModel viewModel;
        private const int rowCount = 20;

        public RealTimeView()
        {
            InitializeComponent();

            viewModel = RealTimeViewModel.Instance;
            this.DataContext = viewModel;

            if (!viewModel.IsActive)
            {
                Initialize_Connection_And_Tasks();
            }
        }

        private void Initialize_Connection_And_Tasks()
        {
            string connString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=FlightData;Integrated Security=True;Connect Timeout=30;Encrypt=False";

            connection = new SqlConnection(connString);
            connection.Open();

            if (!MainWindow.instance.run_once)
            {
                Task.Run(() => Start_listener());
                Task.Run(() => Update_Database());
                //Task.Run(() => Update_Label_Content());

                MainWindow.instance.run_once = true;
            }
            else
            {
                //Restart thread
                MainWindow.instance.pause_real_time_output = true;
                //assure thread stops
                Thread.Sleep(10);
                MainWindow.instance.pause_real_time_output = false;
            }
        }

        public void Start_listener()
        {
            host = Dns.GetHostEntry("localhost");
            iPAddress = host.AddressList[0];
            localEndPoint = new IPEndPoint(iPAddress, 11000);

            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(iPAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            //A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);
            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will use 1 request at a time. No more needed.
            listener.Listen(1);

            handler = listener.Accept();
        }

        public void Update_Database()
        {
            while (!MainWindow.instance.shutdown_flag)
            {
                if (handler != null)
                {
                    // Incoming data from the client.
                    string data = null;
                    byte[] bytes = null;

                    while (true)
                    {
                        // GForce Parameters Table
                        string tailNumber = "Invalid";
                        string timeStamp = "Invalid";
                        float x = 0;
                        float y = 0;
                        float z = 0;
                        float weight = 0;

                        // AttitudeParameters Table
                        float altitude = 0;
                        float pitch = 0;
                        float bank = 0;

                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }

                        // Parsing string sent from ATS
                        MainWindow.instance.real_time_output = Packet.PacketToString(Packet.Deserialize(bytes));
                        string[] parsedString = MainWindow.instance.real_time_output.Split(',');
                        string[] tailNum = parsedString[0].Split(' '); // tailNum[0]

                        // GForce Parameters Table
                        tailNumber = tailNum[0];

                        DateTime currentDateTime = DateTime.Now;
                        timeStamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        if (float.TryParse(parsedString[1], out float X))
                        {
                            x = X;
                        }
                        if (float.TryParse(parsedString[2], out float Y))
                        {
                            y = Y;
                        }
                        if (float.TryParse(parsedString[3], out float Z))
                        {
                            z = Z;
                        }
                        if (float.TryParse(parsedString[4], out float W))
                        {
                            weight = W;
                        }


                        // AttitudeParameters Table

                        if (float.TryParse(parsedString[5], out float A))
                        {
                            altitude = A;
                        }
                        if (float.TryParse(parsedString[6], out float P))
                        {
                            pitch = P;
                        }
                        if (float.TryParse(parsedString[7], out float B))
                        {
                            bank = B;
                        }

                        FlightDataEntry flightDataEntry = new FlightDataEntry
                        {
                            TailNum = tailNumber,
                            Timestamp = timeStamp,
                            X = x,
                            Y = y,
                            Z = z,
                            Weight = weight,
                            Altitude = altitude,
                            Pitch = pitch,
                            Bank = bank,
                        };

                        Dispatcher.Invoke(() =>
                        {
                            viewModel.FlightDataEntries.Insert(0, flightDataEntry);
                            if (viewModel.FlightDataEntries.Count > rowCount)
                            {
                                viewModel.FlightDataEntries.RemoveAt(viewModel.FlightDataEntries.Count - 1);
                            }
                        });

                        // put information into database
                        SqlCommand updateGForceParameters = new SqlCommand(
                            "INSERT INTO [dbo].[GForceParameters] " +
                            "(TailNumber,Timestamp,X,Y,Z,Weight) " +
                            "VALUES " +
                            "(@TailNumber,@Timestamp,@X,@Y,@Z,@Weight)", connection);

                        SqlCommand updateAttitudeParameters = new SqlCommand(
                            "INSERT INTO [dbo].[AttitudeParameters] " +
                            "(TailNumber,Altitude,Pitch,Bank) " +
                            "VALUES " +
                            "(@TailNumber,@Altitude,@Pitch,@Bank)", connection);


                        updateGForceParameters.Parameters.AddWithValue("@TailNumber", tailNumber.ToString());
                        updateGForceParameters.Parameters.AddWithValue("@Timestamp", timeStamp.ToString());
                        updateGForceParameters.Parameters.AddWithValue("@X", x);
                        updateGForceParameters.Parameters.AddWithValue("@Y", y);
                        updateGForceParameters.Parameters.AddWithValue("@Z", z);
                        updateGForceParameters.Parameters.AddWithValue("@Weight", weight);


                        updateAttitudeParameters.Parameters.AddWithValue("@TailNumber", tailNumber.ToString());
                        updateAttitudeParameters.Parameters.AddWithValue("@Altitude", altitude);
                        updateAttitudeParameters.Parameters.AddWithValue("@Pitch", pitch);
                        updateAttitudeParameters.Parameters.AddWithValue("@Bank", bank);

                        // Update Tables
                        updateGForceParameters.ExecuteNonQuery();
                        updateAttitudeParameters.ExecuteNonQuery();
                    }
                }
            }
            if (handler != null)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

    }
}
