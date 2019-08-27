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
    public partial class LoggingScreenView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<LogEntryModel> LoggingData { get; set; }

        public LoggingFacade LoggingFacade { get; set; } = LoggingFacade.GetInstance();

        public object _itemsLock = new object();

        public LoggingScreenView()
        {
            InitializeComponent();
            this.DataContext = this;
            this.LoggingData = LoggingFacade.Logs;

            LoggingFacade.SetLock(_itemsLock);
            BindingOperations.EnableCollectionSynchronization(this.LoggingData, _itemsLock);
        }

        public void Refresh()
        {
            this.LoggingData = LoggingFacade.Logs;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
