using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace FilterPolishZ.ModuleWindows.Logging
{
    /// <summary>
    /// Interaction logic for LoggingScreenView.xaml
    /// </summary>
    /// 
    public partial class LoggingScreenView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<LogEntryModel> LoggingData { get; set; }

        public ObservableCollection<LogEntryModel> FilteredLoggingData { get; set; } 

        public LoggingFacade LoggingFacade { get; set; } = LoggingFacade.GetInstance();

        public object _itemsLock = new object();

        public LoggingScreenView()
        {
            InitializeComponent();
            this.DataContext = this;

            InitializeView();
        }

        private void InitializeView()
        {
            this.LoggingData = LoggingFacade.Logs;
            this.FilteredLoggingData = new ObservableCollection<LogEntryModel>(this.LoggingData.Where(x => this.IsLevelFiltered(x.Level)));
            LoggingFacade.SetLock(_itemsLock);
            BindingOperations.EnableCollectionSynchronization(this.FilteredLoggingData, _itemsLock);
        }

        private bool IsLevelFiltered(LoggingLevel level)
        {
            if (level == LoggingLevel.Debug && DebugShown)
            {
                return true;
            }

            if (level == LoggingLevel.Info && InfoShown)
            {
                return true;
            }

            //if (level == LoggingLevel.Item && DebugShown)
            //{
            //    return true;
            //}

            if (level == LoggingLevel.Errors && ErrorShown)
            {
                return true;
            }

            if (level == LoggingLevel.Warning && WarningShown)
            {
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ItemShown = false;
        public bool DebugShown = false;
        public bool WarningShown = true;
        public bool ErrorShown = true;
        public bool InfoShown = true;


        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            ItemShown = !ItemShown;
            this.InitializeView();
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            DebugShown = !DebugShown;
            this.InitializeView();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoShown = !InfoShown;
            this.InitializeView();
        }

        private void WarningButton_Click(object sender, RoutedEventArgs e)
        {
            WarningShown = !WarningShown;
            this.InitializeView();
        }

        private void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorShown = !ErrorShown;
            this.InitializeView();
        }
    }
}
