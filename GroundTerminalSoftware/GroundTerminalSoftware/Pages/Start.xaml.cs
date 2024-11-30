using System.Windows.Controls;

namespace GroundTerminalSoftware.Pages
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Page
    {
        public static Start instance;
        public Start()
        {
            InitializeComponent();

            instance = this;
        }
    }
}
