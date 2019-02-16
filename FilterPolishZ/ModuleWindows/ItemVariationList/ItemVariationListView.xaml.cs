using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemAspects;
using FilterPolishUtil.Extensions;
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

namespace FilterPolishZ.ModuleWindows.ItemVariationList
{
    /// <summary>
    /// Interaction logic for ItemVariationListView.xaml
    /// </summary>
    public partial class ItemVariationListView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty KeyProperty = 
            DependencyProperty.Register(
                "Key", 
                typeof(string), 
                typeof(ItemVariationListView),
                new PropertyMetadata(default(string), KeyChangeCallBack));

        private static void KeyChangeCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemVariationListView obj)
            {
                obj.ItemVariationInformation.Clear();
                obj.InitializeItems();
            }
        }

        public string Key
        {
            get { return GetValue(KeyProperty) as string; }
            set
            {
                if (Key != value)
                {
                    SetValue(KeyProperty, value);
                }
            }
        }

        public ObservableCollection<NinjaItem> ItemVariationInformation { get; set; } = new ObservableCollection<NinjaItem>();

        public ObservableCollection<AbstractItemAspect> AvailableAspects { get; set; } = AbstractItemAspect.AvailableAspects;

        public ItemVariationListView()
        {
            this.InitializeItems();
            InitializeComponent();
            this.DataContext = this;
        }

        private void InitializeItems()
        {
            if (string.IsNullOrEmpty(Key))
            {
                return;
            }
            EconomyRequestFacade.GetInstance().EconomyTierlistOverview["uniques"][Key].ForEach(x =>
            {
                //x.Aspects.Add(new HandledAspect());
                ItemVariationInformation.Add(x);
            });
        }

        private void OnAspectButtonClick(object sender, RoutedEventArgs e)
        {
            var control = (sender as ContentControl);
            if (ItemVariationTable.SelectedItem == null || control == null)
            {
                return;
            }

            NinjaItem item = (ItemVariationTable.SelectedItem as NinjaItem);
            var clickedAspect = (control.DataContext as AbstractItemAspect);

            if (item.Aspects.Any(x => x.Name == clickedAspect.Name))
            {
                item.Aspects.RemoveAll(x => x.Name == clickedAspect.Name);
            }
            else
            {
                item.Aspects.Add(this.AvailableAspects.First(x => clickedAspect.Name == x.Name));
            }
        }
    }
}
