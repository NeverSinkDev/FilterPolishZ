using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using FilterEconomy.Model.ItemAspects;
using FilterPolishUtil;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using UserControl = System.Windows.Controls.UserControl;
using FilterPolishZ.Util;
using FilterPolishUtil.Extensions;

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
        // public ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>> ItemInformationData { get; set; } = new ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>>();

        public ObservableCollection<ItemTieringData> ItemInformationData { get; set; } = new ObservableCollection<ItemTieringData>();

        public static string CurrentBranchKey { get; set; } = "uniques"; // static because other windows need to access this without having this instance

        public string TierListState { get; set; } = string.Empty;

        private string currentDisplayFiltering;
        private IEnumerable<string> aspectDisplayFilter = new string[] {};
        private bool isOnlyDisplayingMultiBases;
        private HashSet<string> visitedBranches = new HashSet<string>(); // to track which branches have already had their saved data loaded

        public EventGridFacade EventGridFacade { get; }

        public ItemInfoView()
        {
            InitializeComponent();

            var allBranchKeys = FilterPolishConfig.TierableEconomySections;
            CurrentBranchKey = allBranchKeys.First();
            this.BranchKeyDisplaySelection.ItemsSource = allBranchKeys;
            this.BranchKeyDisplaySelection.SelectedIndex = 0;

//            this.AllAspectList.ItemsSource = Aspects; // todo
            
            this.DataContext = this;
            InnerView.BranchKey = CurrentBranchKey;

            this.EventGridFacade = EventGridFacade.GetInstance();
            this.EventGridFacade.FilterChangeEvent += EventGridFacade_FilterChangeEvent;

            RoutedCommand firstSettings = new RoutedCommand();
            firstSettings.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(firstSettings, MetaHotkey));
        }

        private void MetaHotkey(object sender, ExecutedRoutedEventArgs e)
        {
            var index = ItemInfoGrid.SelectedIndex;
            if (index != -1)
            {
                InnerView.SelectFirstItem();

                InnerView.BranchKey = CurrentBranchKey;
                if (CurrentBranchKey.ToLower().Contains("divination"))
                {
                    InnerView.ForceChangeAspect("PoorDiviAspect");
                }
                else if (CurrentBranchKey.ToLower().Contains("unique"))
                {
                    InnerView.ForceChangeAspect("MetaBiasAspect");
                }
                
            }
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
            this.ItemInformationData = new ObservableCollection<ItemTieringData>();
        }

        private void InitializeItemInformationData()
        {
            this.ItemInformationData.Clear();
            var ecoData = this.GetCurrentDisplayItems();
            ecoData.ToList().ForEach(z => this.ItemInformationData.Add(this.ConvertToItemInformation(z)));
            if (this.ItemInfoGrid != null) this.ItemInfoGrid.ItemsSource = ItemInformationData;
        }

        private ItemTieringData ConvertToItemInformation(KeyValuePair<string, ItemList<NinjaItem>> z)
        {
            var ecoData = EconomyData.EconomyTierlistOverview[this.GetBranchKey()][z.Key];
            return new ItemTieringData()
            {
                Name = z.Key,
                Value = z.Value,
                Count = z.Value.Count,
                LowestPrice = ecoData?.LowestPrice ?? 0,
                HighestPrice = ecoData?.HighestPrice ?? 0,
                Multiplier = ecoData?.ValueMultiplier ?? 0,
                Valid = ecoData?.Valid,
                Tier = StringWork.CombinePieces(", ", TierListData.GetTiersForBasetypeSafe(CurrentBranchKey, z.Key))
            };
        }

        private Dictionary<string, ItemList<NinjaItem>> GetCurrentDisplayItems()
        {
            var ecoData = this.EconomyData.EconomyTierlistOverview[this.GetBranchKey()];

            switch (this.currentDisplayFiltering)
            {
                case "ShowAspectless":
                    ecoData = this.ItemInfoData.GetItemsThatAreNotInThisList(ecoData, this.GetBranchKey(), true);
                    ecoData = ecoData.Where(x => x.Value.Any(item => item.Aspects.Where(z => z.IsActive()).ToList().Count == 0))
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
                    ecoData = ecoData.Where(x => x.Value.All(item => item.Aspects.Where(z => z.IsActive()).ToList().Count > 0))
                        .ToDictionary(x => x.Key, x => x.Value);
                    break;

                default: throw new Exception("unexpected display filtering mode");
            }

            if (this.isOnlyDisplayingMultiBases)
            {
                ecoData = ecoData.Where(x => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value);
            }

            // only show items/bases that have the specified aspects
            ecoData = ecoData
                .Where(baseType => aspectDisplayFilter
                    .All(aspect => baseType.Value
                        .Any(itemName => itemName.Aspects
                            .Any(asp => asp.Name.ToLower().Contains(aspect.ToLower()))
                        )
                    )
                )
                .ToDictionary(x => x.Key, x => x.Value);

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

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        private void ItemInfoGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ItemInfoGrid.SelectedIndex;
            if (index != -1)
            {
                InnerView.SelectFirstItem();
                InnerView.BranchKey = CurrentBranchKey;
                InnerView.RefreshAspectColoration();
            }
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

        private void OnAspectNameFilteringChange(object sender, TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                var aspectNames = textBox.Text.Split(',').Select(x => x.Trim()).Where(x => x.Length > 2);
                this.aspectDisplayFilter = aspectNames;
                this.OnUpdateUiButtonClick(null, null);
            }
        }

        private void RemoveAspectFromAll(object sender, RoutedEventArgs e)
        {
            var aspectFilter = this.AllAspectList.ItemsSource as IEnumerable<string>;
            if (aspectFilter == null) throw new Exception();
            
            var allItems = this.GetCurrentDisplayItems();
            foreach (var items in allItems.Values)
            {
                foreach (var item in items)
                {
                    foreach (var aspect in aspectFilter)
                    {
                        item.Aspects.RemoveAll(x => x.Name == aspect);
                    }
                }
            }
        }

    }
}
