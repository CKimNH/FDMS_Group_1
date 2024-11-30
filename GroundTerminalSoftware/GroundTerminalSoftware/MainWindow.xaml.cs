using GroundTerminalSoftware.Pages;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GroundTerminalSoftware
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        public bool run_once = false;
        public bool shutdown_flag = false;
        public bool pause_real_time_output = false;
        public string real_time_output = "Start the Aircraft Transmission System...";

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            View.Content = new Start();
        }

        // Mouse event handler
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Real-Time view button handler
        private void Real_Time_View(object sender, RoutedEventArgs e)
        {
            View.Content = new RealTimeView();
        }

        // Search-View view button handler
        private void Search_View(object sender, RoutedEventArgs e)
        {
            View.Content = new SearchDatabase();
        }

        private void Output_Database(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Outputting to text will stop data reading", "Ground Terminal Software", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                shutdown_flag = true;
                Thread.Sleep(100);

                string connString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=FlightData;Integrated Security=True;Connect Timeout=30;Encrypt=False";
                //string g_force_table = "GForceParameters";

                SqlConnection connection = new SqlConnection(connString);
                connection.Open();

                string g_query = "SELECT " +
                    "GP.[TailNumber]" +
                    ",GP.[TimeStamp]" +
                    ",GP.[X]" +
                    ",GP.[Y]" +
                    ",GP.[Z]" +
                    ",GP.[Weight]" +
                    ",AP.[Altitude]" +
                    ",AP.[Pitch]" +
                    ",AP.[Bank] " +
                    "FROM [dbo].[AttitudeParameters] AP " +
                    "INNER JOIN  " +
                    "[dbo].[GForceParameters] GP " +
                    "ON AP.TailNumber = GP.TailNumber";

                SqlCommand g_command = new SqlCommand(g_query, connection);

                SqlDataReader g_reader = g_command.ExecuteReader();

                if (g_reader.HasRows)
                {
                    StreamWriter writer = new StreamWriter("Logs.txt");

                    while (g_reader.Read())
                    {
                        for (int i = 0; i < g_reader.FieldCount; i++)
                        {
                            writer.Write(g_reader[i].ToString() + ",");
                        }
                        writer.WriteLine();
                    }
                    MessageBox.Show("Output Successful");
                }
                else
                {
                    MessageBox.Show("Nothing to output");
                }
                connection.Close();
            }
            else
            {
                ; ; // Do nothing...
            }
        }


        // Exit Button handler
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Confirm Exit?", "Ground Terminal Software", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RealTimeView real_time_view = new RealTimeView();


                //Kill threads
                shutdown_flag = true;
                pause_real_time_output = true;

                //Sleeping to make sure threads finish
                Thread.Sleep(100);

                Application.Current.Shutdown();
            }
            else
            {
                ; ; // Do nothing...
            }
        }
    }
}
