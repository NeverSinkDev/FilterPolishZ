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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FilterPolishZ.ModuleWindows.ItemInfo
{

    /// <summary>
    /// Interaction logic for ItemInfoView.xaml
    /// </summary>
    public partial class ItemInfoView : UserControl, INotifyPropertyChanged
    {
        private int lastIndex = -1;

        public EconomyRequestFacade EconomyData { get; set; } = EconomyRequestFacade.GetInstance();
        public ItemInformationFacade ItemInfoData { get; set; } = ItemInformationFacade.GetInstance();
        public ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>> UnhandledUniqueItems { get; private set; } = new ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>>();

        public ItemInfoView()
        {
            InitializeComponent();
            this.InitializeItemInformationData();
            this.DataContext = this;
        }

        private void InitializeItemInformationData()
        {
            this.EconomyData.EconomyTierlistOverview["uniques"]
                .Where(x => !this.ItemInfoData.EconomyTierlistOverview["uniques"].ContainsKey(x.Key))
                .ToList().ForEach(z => this.UnhandledUniqueItems.Add(z));

            this.ItemInfoGrid.ItemsSource = UnhandledUniqueItems;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
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
                (InnerView as ItemVariationListView).SelectFirstItem();
            }
        }

        private void SaveInsta_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
