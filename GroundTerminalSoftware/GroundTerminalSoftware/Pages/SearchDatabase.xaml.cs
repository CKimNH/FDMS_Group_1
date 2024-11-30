using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace GroundTerminalSoftware.Pages
{
    /// <summary>
    /// Interaction logic for SearchDatabase.xaml
    /// </summary>
    public partial class SearchDatabase : Page
    {
        public SearchDatabase()
        {
            InitializeComponent();
        }

        public void Search_Database(object sender, RoutedEventArgs e)
        {
            bool invalid = false;

            string tailNum = SearchBox.Text;

            // GForce Parameters Table
            string tailNumber = "";
            string timeStamp = "";
            string x = "";
            string y = "";
            string z = "";
            string weight = "";

            // AttitudeParameters Table
            string altitude = "";
            string pitch = "";
            string bank = "";

            string connString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=FlightData;Integrated Security=True;Connect Timeout=30;Encrypt=False";

            SqlConnection connection = new SqlConnection(connString);
            connection.Open();

            SqlCommand readGForceParameters = new SqlCommand(
                "SELECT [TailNumber]" +
                ",[TimeStamp]" +
                ",[X]" +
                ",[Y]" +
                ",[Z]" +
                ",[Weight] " +
                "FROM [dbo].[GForceParameters] " +
                "WHERE [TailNumber] = @TailNumber", connection);

            SqlCommand readAttitudeParameters = new SqlCommand(
                "SELECT [TailNumber]" +
                ",[Altitude]" +
                ",[Pitch]" +
                ",[Bank] " +
                "FROM [dbo].[AttitudeParameters] " +
                "WHERE [TailNumber] = @TailNumber", connection);


            readGForceParameters.Parameters.AddWithValue("@TailNumber", tailNum);
            SqlDataReader gForceReader = readGForceParameters.ExecuteReader();

            while (gForceReader.Read())
            {
                // GForce Parameters Table
                tailNumber = gForceReader["TailNumber"].ToString();
                timeStamp = gForceReader["TimeStamp"].ToString();
                x = gForceReader["X"].ToString();
                y = gForceReader["Y"].ToString();
                z = gForceReader["Z"].ToString();
                weight = gForceReader["Weight"].ToString();
            }
            gForceReader.Close();

            if (tailNumber == "")
            {
                invalid = true;
            }

            readAttitudeParameters.Parameters.AddWithValue("@TailNumber", tailNum);
            SqlDataReader attitudeReader = readAttitudeParameters.ExecuteReader();
            while (attitudeReader.Read())
            {
                // AttitudeParameters Table
                altitude = attitudeReader["Altitude"].ToString();
                pitch = attitudeReader["Pitch"].ToString();
                bank = attitudeReader["Bank"].ToString();
            }
            attitudeReader.Close();

            if (!invalid)
            {
                string fullString = tailNumber + " " + timeStamp + "," + x + "," + y + "," + z + "," + weight + "," + altitude + "," + pitch + "," + bank;
                SearchResult.Content = fullString;
            }
            else
            {
                SearchResult.Content = "Invalid Tail Number (e.g : C-FGAX1)";
            }

            connection.Close();
        }
    }
}
