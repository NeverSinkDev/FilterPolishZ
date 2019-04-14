using FilterEconomy;
using FilterEconomy.Facades;
using FilterEconomy.Processor;
using FilterPolishZ.Util;
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
        public TierListFacade TierListFacade { get; set; }
        public ObservableCollection<TieringCommand> TieringSuggestions { get; set; } = new ObservableCollection<TieringCommand>();
        public EventGridFacade EventGridFacade { get; }

        public TieringSuggestionView()
        {
            InitializeComponent();
            this.TierListFacade = TierListFacade.GetInstance();
            this.InitializeTieringList();
            this.DataContext = this;

            this.EventGridFacade = EventGridFacade.GetInstance();
            this.EventGridFacade.FilterChangeEvent += EventGridFacade_FilterChangeEvent;
        }

        private void EventGridFacade_FilterChangeEvent(object sender, EventArgs e)
        {
            this.Reload();
        }

        public void Reload()
        {
            this.TierListFacade = TierListFacade.GetInstance();
            this.TieringSuggestions = new ObservableCollection<TieringCommand>(this.TierListFacade.Suggestions["uniques"]);
        }

        private void InitializeTieringList()
        {
            this.TieringSuggestions = new ObservableCollection<TieringCommand>( this.TierListFacade.Suggestions["uniques"] );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TieringSuggestionsGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (TieringSuggestionsGrid.SelectedItem == null)
            {
                return;
            }

            TieringCommand command = TieringSuggestionsGrid.SelectedItem as TieringCommand;

            InnerView.BranchKey = command.Group;
            InnerView.Key = command.BaseType;
            InnerView.SelectFirstItem();
        }

        private void ReloadClick(object sender, RoutedEventArgs e)
        {
            this.Reload();
        }
    }
}
