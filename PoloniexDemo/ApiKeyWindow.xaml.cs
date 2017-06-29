using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PoloniexDemo
{
    /// <summary>
    /// Interaction logic for ApiKeyInput.xaml
    /// </summary>
    public partial class ApiKeyWindow
    {
        public ApiKeyWindow()
        {
            InitializeComponent();
            PublicKey_Text.Text = Properties.Settings.Default.PublicKey;
            PrivateKey_Text.Text = Properties.Settings.Default.PrivateKey;
        }

        private void SaveApiKeys_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PublicKey = PublicKey_Text.Text.ToString();
            Properties.Settings.Default.PrivateKey = PrivateKey_Text.Text.ToString();
            Properties.Settings.Default.Save();

            MessageBoxResult result = MessageBox.Show("API Keys Saved");
            //MessageBoxResult result = MessageBox.Show(this, "API Keys Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
            {
                // Yes code here
                Visibility = Visibility.Collapsed;
            }

        }

        private void ClearApiKeys_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void PublicKey_Text_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PublicKey_Text.Clear();
        }

        private void PrivateKey_Text_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PrivateKey_Text.Clear();
        }
    }
}
