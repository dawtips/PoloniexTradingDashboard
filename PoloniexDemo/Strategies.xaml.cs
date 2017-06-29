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
using PoloniexDemo.Properties;
using Jojatekok.PoloniexAPI;

namespace PoloniexDemo
{
    /// <summary>
    /// Interaction logic for Strategies.xaml
    /// </summary>
    public partial class Strategies
    {
        public Strategies()
        {
            InitializeComponent();

             var PoloniexClient = (PoloniexClient)Application.Current.Properties["PoloniexClient"];
        }





    }
}
