using FilterEconomy;
using FilterEconomy.Processor;
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

namespace FilterPolishZ.ModuleWindows.TieringSuggestions
{
    /// <summary>
    /// Interaction logic for TieringSuggestionView.xaml
    /// </summary>
    public partial class TieringSuggestionView : UserControl, INotifyPropertyChanged
    {
        public TieringSuggestionFacade TieringSuggestionFacade { get; set; }
        public ObservableCollection<TieringCommand> TieringSuggestions { get; set; } = new ObservableCollection<TieringCommand>();

        public TieringSuggestionView()
        {
            InitializeComponent();
            this.TieringSuggestionFacade = TieringSuggestionFacade.GetInstance();
            this.InitializeTieringList();
            this.DataContext = this;
        }

        private void InitializeTieringList()
        {
            this.TieringSuggestions = new ObservableCollection<TieringCommand>( this.TieringSuggestionFacade.Suggestions["uniques"] );
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
