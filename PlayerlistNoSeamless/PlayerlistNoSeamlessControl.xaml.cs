using System.Windows;
using System.Windows.Controls;

namespace PlayerlistNoSeamless
{
    public partial class PlayerlistNoSeamlessControl : UserControl
    {

        private PlayerlistNoSeamless Plugin { get; }

        private PlayerlistNoSeamlessControl()
        {
            InitializeComponent();
        }

        public PlayerlistNoSeamlessControl(PlayerlistNoSeamless plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }
    }
}
