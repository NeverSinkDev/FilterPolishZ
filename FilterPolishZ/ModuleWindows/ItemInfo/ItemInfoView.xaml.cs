using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishZ.Domain;
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

namespace FilterPolishZ.ModuleWindows.ItemInfo
{

    /// <summary>
    /// Interaction logic for ItemInfoView.xaml
    /// </summary>
    public partial class ItemInfoView : UserControl
    {
        public EconomyRequestFacade EconomyData { get; set; } = EconomyRequestFacade.GetInstance();
        public ItemInformationFacade ItemInfoData { get; set; } = ItemInformationFacade.GetInstance();
        public ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>> UnhandledUniqueItems { get; private set; } = new ObservableCollection<KeyValuePair<string, ItemList<NinjaItem>>>();
        public ObservableCollection<NinjaDemoItem> UnhandledUniqueItems2 { get; set; } = new ObservableCollection<NinjaDemoItem>();

        public ItemInfoView()
        {
            InitializeComponent();
            this.InitializeItemInformationData();
        }

        private void InitializeItemInformationData()
        {
            this.UnhandledUniqueItems2.Add(new NinjaDemoItem());
            this.UnhandledUniqueItems2.Add(new NinjaDemoItem());
            this.UnhandledUniqueItems2.Add(new NinjaDemoItem());
            this.UnhandledUniqueItems2.Add(new NinjaDemoItem());

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
    }
}
