using FilterPolishZ.Configuration;
using FilterPolishZ.Domain;
using FilterPolishZ.Domain.DataType;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace FilterPolishZ.ModuleWindows.Configuration
{
    /// <summary>
    /// Interaction logic for ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
            InitializeLocalConfiguration();
        }

        // Components
        public LocalConfiguration Configuration { get; set; }

        // Data Elements
        public ObservableCollection<ConfigurationData> ConfigurationData { get; set; } = new ObservableCollection<ConfigurationData>();

        private void InitializeLocalConfiguration()
        {
            this.Configuration = LocalConfiguration.GetInstance();
            var data = this.Configuration.YieldConfiguration().ToList();
            data.ForEach(x => ConfigurationData.Add(x));

            this.ConfigGrid.ItemsSource = ConfigurationData;
        }

        private void ConfigGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    DataGridRow dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
                    int id = grid.SelectedIndex;
                }
            }
        }

        private void SetButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Parent is StackPanel parent)
                {
                    if (parent.DataContext is ConfigurationData data)
                    {
                        this.Configuration.Set(data.Key, data.Value);
                    }
                }
            }
        }
    }
}
