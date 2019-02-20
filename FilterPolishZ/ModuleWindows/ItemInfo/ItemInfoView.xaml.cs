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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using FilterPolishUtil;
using FilterPolishZ.Configuration;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using UserControl = System.Windows.Controls.UserControl;

namespace FilterPolishZ.ModuleWindows.ItemInfo
{
    /// <summary>
    /// Interaction logic for ItemInfoView.xaml
    /// </summary>
    public partial class ItemInfoView : UserControl, INotifyPropertyChanged
    {
        private int lastIndex = -1;

        public EconomyRequestFacade EconomyData { get; } = EconomyRequestFacade.GetInstance();
        public ItemInformationFacade ItemInfoData { get; } = ItemInformationFacade.GetInstance();
        public ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>> UnhandledUniqueItems { get; } = new ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>>();

        public static string currentBranchKey; // todo: temporary workaround to access this info on other pages
        private string currentDisplayFiltering;

        public ItemInfoView()
        {
            InitializeComponent();
            
            var allBranchKeys = this.EconomyData.EconomyTierlistOverview.Keys;
            currentBranchKey = allBranchKeys.First();
            this.BranchKeyDisplaySelection.ItemsSource = allBranchKeys;
            this.BranchKeyDisplaySelection.SelectedIndex = 0;
            
            this.DataContext = this;
        }

        private void InitializeItemInformationData()
        {
            this.UnhandledUniqueItems.Clear();
            IEnumerable<KeyValuePair<string, ItemList<NinjaItem>>> ecoData = this.EconomyData.EconomyTierlistOverview[this.GetBranchKey()];
            
            // todo: UI updating trigger when adding aspects and ????
            
            switch (this.currentDisplayFiltering)
            {
                case "ShowAspectless":
                    ecoData = ecoData.Where(x => x.Value.Any(item => item.Aspects.Count == 0));
                    break;
                
                case "ShowOnlyInItem":
                    var resultList = new List<KeyValuePair<string, ItemList<NinjaItem>>>();
                    
                    // todo: rework/refactor this!
                    foreach (var keyValuePair in this.ItemInfoData.EconomyTierListOverview[this.GetBranchKey()])
                    {
                        if (this.EconomyData.EconomyTierlistOverview[this.GetBranchKey()].ContainsKey(keyValuePair.Key)) continue;
                        
                        var itemList = new ItemList<NinjaItem>();
                        keyValuePair.Value.ToList().ForEach(x => itemList.Add(x.ToNinjaItem()));
                        resultList.Add(new KeyValuePair<string, ItemList<NinjaItem>>(keyValuePair.Key, itemList));
                    }

                    ecoData = resultList;
                    break;
                
                case "ShowStable":
                    ecoData = ecoData.Where(x => x.Value.All(item => item.Aspects.Count > 0));
                    break;
            }
            
            ecoData.ToList().ForEach(z => this.UnhandledUniqueItems.Add(z));
            if (this.ItemInfoGrid != null) this.ItemInfoGrid.ItemsSource = UnhandledUniqueItems;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow row)
                {
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow row)
                {
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void ItemInfoGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ItemInfoGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ItemInfoGrid.SelectedIndex;
            if (index != this.lastIndex)
            {
                InnerView.SelectFirstItem();
            }
        }

        private void SaveInsta_Click(object sender, RoutedEventArgs e)
        {
            var leagueType = LocalConfiguration.GetInstance().AppSettings["Ninja League"];
            var baseStoragePath = LocalConfiguration.GetInstance().AppSettings["SeedFile Folder"];
            var branchKey = this.GetBranchKey();
            
            this.ItemInfoData.ExtractAspectDataFromEcoData(this.EconomyData, branchKey);
            this.ItemInfoData.SaveItemInformation(leagueType, branchKey, baseStoragePath);
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
            var leagueType = LocalConfiguration.GetInstance().AppSettings["Ninja League"];
            var baseStoragePath = LocalConfiguration.GetInstance().AppSettings["SeedFile Folder"];
            var branchKey = this.GetBranchKey();
            
            var filePath = ItemInformationFacade.GetItemInfoSaveFilePath(leagueType, branchKey, baseStoragePath);
            var fileText = System.IO.File.ReadAllText(filePath);
            
            this.ItemInfoData.Deserialize(branchKey, fileText);
            this.ItemInfoData.MigrateAspectDataToEcoData(this.EconomyData, branchKey);
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
        
        private string GetBranchKey() => currentBranchKey ?? this.EconomyData.EconomyTierlistOverview.Keys.First();

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
                    currentBranchKey = newValue;
                    this.InitializeItemInformationData();
                    break;
                }
                case string branchKey:
                    currentBranchKey = branchKey;
                    this.InitializeItemInformationData();
                    break;
            }
        }
    }
}
