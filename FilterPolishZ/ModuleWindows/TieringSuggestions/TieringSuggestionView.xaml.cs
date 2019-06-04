using FilterEconomy;
using FilterEconomy.Facades;
using FilterEconomy.Processor;
using FilterPolishUtil.Constants;
using FilterPolishZ.Util;
using MaterialDesignThemes.Wpf;
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
        public string BranchKey { get; private set; } = "uniques";
        public ObservableCollection<TieringCommand> TieringSuggestions { get; set; } = new ObservableCollection<TieringCommand>();
        public ObservableCollection<TieringCommand> FilteredTieringSuggestions { get; set; } = new ObservableCollection<TieringCommand>();

        public Dictionary<string, TieringSuggestionFilter> TieringFilters { get; set; } = new Dictionary<string, TieringSuggestionFilter>();

        public EventGridFacade EventGridFacade { get; }

        public TieringSuggestionView()
        {
            InitializeComponent();
            this.SelectedBranchComboBox.ItemsSource = FilterPolishConstants.FilterTierLists;
            this.SelectedBranchComboBox.SelectedIndex = 0;

            this.TierListFacade = TierListFacade.GetInstance();
            this.InitializeTieringList();
            this.DataContext = this;

            this.EventGridFacade = EventGridFacade.GetInstance();
            this.EventGridFacade.FilterChangeEvent += EventGridFacade_FilterChangeEvent;

            this.TieringFilters.Add("none", new TieringSuggestionFilter("none", x => true));
            this.TieringFilters.Add("Only Changed", new TieringSuggestionFilter("OnlyChangedFilter", x => x.IsChange));

            this.SelectedTieringFiltersComboBox.ItemsSource = this.TieringFilters.Keys;
            this.SelectedBranchComboBox.SelectedIndex = 0;
        }

        private void EventGridFacade_FilterChangeEvent(object sender, EventArgs e)
        {
            this.Reload();
        }

        public void Reload()
        {
            if (string.IsNullOrWhiteSpace(this.BranchKey))
            {
                return;
            }

            this.TierListFacade = TierListFacade.GetInstance();
            RefreshTieringSuggestions();
        }

        private void InitializeTieringList()
        {
            this.RefreshTieringSuggestions();
        }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

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

        private void SelectedBranchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.SelectedBranchComboBox.SelectionBoxItem.ToString()))
            {
                return;
            }

            this.BranchKey = e.AddedItems[0].ToString();

            this.RefreshTieringSuggestions();
        }

        private void SelectedFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TieringFilters.Values.ToList().ForEach(x => x.Active = false);

            if (string.IsNullOrEmpty(this.SelectedBranchComboBox.SelectionBoxItem.ToString()))
            {
                return;
            }

            var filterKey = e.AddedItems[0].ToString();

            switch (filterKey)
            {
                case "none":
                    break;
                case "Only Changed":
                    this.TieringFilters["Only Changed"].Active = true;
                    break;
            }

            this.RefreshTieringSuggestions();
        }

        private void RefreshTieringSuggestions()
        {
            this.TieringSuggestions = new ObservableCollection<TieringCommand>(this.TierListFacade.Suggestions[this.BranchKey]);
            this.FilteredTieringSuggestions = new ObservableCollection<TieringCommand>(this.TieringSuggestions.Where(x => this.IsSuggestionVisible(x)));
        }

        private bool IsSuggestionVisible(TieringCommand x)
        {
            foreach (var item in this.TieringFilters)
            {
                if (!item.Value.Applies(x))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class TieringSuggestionFilter
    {
        public string Name { get; set; } = "unnamed filter";
        public Func<TieringCommand, bool> Rule { get; set; }
        public bool Active { get; set; } = true;

        public TieringSuggestionFilter(string name, Func<TieringCommand, bool> rule)
        {
            this.Rule = rule;
            this.Name = name;
        }
        
        public bool Applies(TieringCommand x)
        {
            if (!this.Active)
            {
                return true;
            }

            return Rule(x);
        }
    }
}
