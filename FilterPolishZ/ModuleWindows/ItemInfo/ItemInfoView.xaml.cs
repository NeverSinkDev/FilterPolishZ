using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishZ.Domain;
using FilterPolishZ.Domain.DataType;
using FilterPolishZ.ModuleWindows.ItemVariationList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using FilterEconomy.Model.ItemAspects;
using FilterPolishUtil;
using FilterPolishZ.Configuration;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using UserControl = System.Windows.Controls.UserControl;
using FilterPolishZ.Util;

namespace FilterPolishZ.ModuleWindows.ItemInfo
{
    /// <summary>
    /// Interaction logic for ItemInfoView.xaml
    /// </summary>
    public partial class ItemInfoView : UserControl, INotifyPropertyChanged
    {
        public EconomyRequestFacade EconomyData { get; set; } = EconomyRequestFacade.GetInstance();
        public ItemInformationFacade ItemInfoData { get; set; } = ItemInformationFacade.GetInstance();
        public TierListFacade TierListData { get; set; } = TierListFacade.GetInstance();
        public ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>> ItemInformationData { get; set; } = new ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>>();

        public static string CurrentBranchKey { get; set; } // static because other windows need to access this without having this instance
        private string currentDisplayFiltering;
        private bool isOnlyDisplayingMultiBases;
        private HashSet<string> visitedBranches = new HashSet<string>(); // to track which branches have already had their saved data loaded

        public Capsule LowestPriceCapsule { get; set; }
        public Capsule CurrentTierCapsule { get; set; }
        public Capsule CountCapsule { get; set; }
        public EventGridFacade EventGridFacade { get; }

        public ItemInfoView()
        {
            InitializeComponent();
            
            var allBranchKeys = this.EconomyData.EconomyTierlistOverview.Keys;
            CurrentBranchKey = allBranchKeys.First();
            this.BranchKeyDisplaySelection.ItemsSource = allBranchKeys;
            this.BranchKeyDisplaySelection.SelectedIndex = 0;
            
            this.DataContext = this;

            LowestPriceCapsule = new Capsule((s) => EconomyData.EconomyTierlistOverview[this.GetBranchKey()][s]?.LowestPrice.ToString());
            CurrentTierCapsule = new Capsule((s) => EconomyData.EconomyTierlistOverview[this.GetBranchKey()][s]?.Count.ToString());
            CountCapsule = new Capsule((s) => EconomyData.EconomyTierlistOverview[this.GetBranchKey()][s]?.Count.ToString());
            InnerView.BranchKey = CurrentBranchKey;

            this.EventGridFacade = EventGridFacade.GetInstance();
            this.EventGridFacade.FilterChangeEvent += EventGridFacade_FilterChangeEvent;
        }

        private void EventGridFacade_FilterChangeEvent(object sender, EventArgs e)
        {
            this.Reload();
        }

        public void Reload()
        {
            this.EconomyData = EconomyRequestFacade.GetInstance();
            this.ItemInfoData = ItemInformationFacade.GetInstance();
            this.TierListData = TierListFacade.GetInstance();
            this.ItemInformationData = new ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>>();
        }

        private void InitializeItemInformationData()
        {
            this.ItemInformationData.Clear();
            var ecoData = this.GetCurrentDisplayItems();
            ecoData.ToList().ForEach(z => this.ItemInformationData.Add(z));
            if (this.ItemInfoGrid != null) this.ItemInfoGrid.ItemsSource = ItemInformationData;
        }

        private Dictionary<string, ItemList<NinjaItem>> GetCurrentDisplayItems()
        {
            var ecoData = this.EconomyData.EconomyTierlistOverview[this.GetBranchKey()];

            switch (this.currentDisplayFiltering)
            {
                case "ShowAspectless":
                    ecoData = this.ItemInfoData.GetItemsThatAreNotInThisList(ecoData, this.GetBranchKey(), true);
                    ecoData = ecoData.Where(x => x.Value.Any(item => item.Aspects.Count == 0))
                        .ToDictionary(x => x.Key, x => x.Value);
                    break;

                case "ShowOnlyInItem":
                    ecoData = this.ItemInfoData.GetItemsThatAreNotInThisList(ecoData, this.GetBranchKey(), false);
                    break;

                case "ShowAll":
                    ecoData = this.ItemInfoData.GetItemsThatAreNotInThisList(ecoData, this.GetBranchKey(), true);
                    break;

                case "ShowStable":
                    ecoData = this.ItemInfoData.GetItemsThatAreNotInThisList(ecoData, this.GetBranchKey(), true);
                    ecoData = ecoData.Where(x => x.Value.All(item => item.Aspects.Count > 0))
                        .ToDictionary(x => x.Key, x => x.Value);
                    break;

                default: throw new Exception("unexpected display filtering mode");
            }

            if (this.isOnlyDisplayingMultiBases)
            {
                ecoData = ecoData.Where(x => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value);
            }

            return ecoData;
        }

        private void ItemInfoGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            InnerView.BranchKey = CurrentBranchKey;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ItemInfoGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ItemInfoGrid.SelectedIndex;
            if (index != -1)
            {
                InnerView.SelectFirstItem();
            }

            InnerView.BranchKey = CurrentBranchKey;
        }

        private void SaveInsta_Click(object sender, RoutedEventArgs e)
        {
            var branchKey = this.GetBranchKey();
            this.ItemInfoData.ExtractAspectDataFromEcoData(this.EconomyData, branchKey);
            this.ItemInfoData.SaveItemInformation(branchKey);
        }

        private void SaveInfoAs(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() != DialogResult.OK) return;
            var filePath = fd.FileName;
            
            var branchKey = this.GetBranchKey();
            
            this.ItemInfoData.ExtractAspectDataFromEcoData(this.EconomyData, branchKey);
            this.ItemInfoData.SaveItemInformation(filePath, branchKey);
        }

        private void LoadInsta_Click(object sender, RoutedEventArgs e)
        {
            this.ItemInfoData.LoadFromSaveFile(this.GetBranchKey());
        }

        private void LoadFromFile(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() != DialogResult.OK) return;
            var filePath = fd.FileName;
            var responseString = FileWork.ReadFromFile(filePath);
            
            var branchKey = this.GetBranchKey();
            
            this.ItemInfoData.Deserialize(branchKey, responseString);
            this.ItemInfoData.MigrateAspectDataToEcoData(this.EconomyData, branchKey);
        }
        
        private string GetBranchKey() => CurrentBranchKey ?? this.EconomyData.EconomyTierlistOverview.Keys.First();

        private void OnDisplayFilterChange(object sender, SelectionChangedEventArgs e)
        {
            switch (e.AddedItems[0])
            {
                // ((sender as System.Windows.Controls.ComboBox).SelectedItem as ComboBoxItem)
                case ComboBoxItem selected:
                {
                    var newValue = selected.Name;
                    this.currentDisplayFiltering = newValue;
                    this.InitializeItemInformationData();
                    break;
                }
                case string branchKey:
                    this.currentDisplayFiltering = branchKey;
                    this.InitializeItemInformationData();
                    break;
            }
        }

        private void OnBranchKeyChange(object sender, SelectionChangedEventArgs e)
        {
            switch (e.AddedItems[0])
            {
                // ((sender as System.Windows.Controls.ComboBox).SelectedItem as ComboBoxItem)
                case ComboBoxItem selected:
                {
                    var newValue = selected.Content as string; //name
                    CurrentBranchKey = newValue;
                    this.InitializeItemInformationData();
                    break;
                }
                case string branchKey:
                    CurrentBranchKey = branchKey;
                    this.InitializeItemInformationData();
                    break;
            }
            InnerView.BranchKey = CurrentBranchKey;
        }

        private void OnUpdateUiButtonClick(object sender, RoutedEventArgs e)
        {
            this.Reload();
            this.InitializeItemInformationData();
        }

        private void OnResetAspectsButtonClick(object sender, RoutedEventArgs e)
        {
            this.EconomyData.EconomyTierlistOverview[this.GetBranchKey()].ToList().ForEach(x => x.Value.ForEach(y => y.Aspects.Clear()));
            this.OnUpdateUiButtonClick(null, null);
        }

        private void ToggleMultiBaseOnlyDisplay(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                this.isOnlyDisplayingMultiBases = button.IsChecked ?? false;
                this.OnUpdateUiButtonClick(null, null);
            }
        }

        private void OnAddHandleToAllButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var itemData in this.GetCurrentDisplayItems())
            {
                foreach (var item in this.EconomyData.EconomyTierlistOverview[this.GetBranchKey()][itemData.Key])
                {
                    if (item.Aspects.Any(x => x is HandledAspect)) continue;

                    var aspect = new HandledAspect
                    {
                        HanadlingPrice = item.CVal,
                        HandlingDate = DateTime.Now
                    };
                    
                    item.Aspects.Add(aspect);
                }
            }
            
            this.OnUpdateUiButtonClick(null, null);
        }
    }
}
