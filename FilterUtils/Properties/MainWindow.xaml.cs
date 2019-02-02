using FilterPolishZ.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FilterPolishZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> DemoItems { get; set; } = new List<string> { "Test", "Test1" };

        // Components
        public LocalConfiguration Configuration { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeLocalConfiguration();
        }

        private void InitializeLocalConfiguration()
        {
            this.Configuration = LocalConfiguration.GetInstance();
        }

    }
}
