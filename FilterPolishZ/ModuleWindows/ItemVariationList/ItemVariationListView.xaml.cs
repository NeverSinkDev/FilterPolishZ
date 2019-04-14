using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemAspects;
using FilterPolishUtil.Extensions;
using FilterPolishZ.ModuleWindows.ItemInfo;
using FilterPolishZ.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public string BranchKey { get; set; } = "uniques";

        public ObservableCollection<NinjaItem> ItemVariationInformation { get; set; } = new ObservableCollection<NinjaItem>();
        public static ObservableCollection<NinjaItem> ItemVariationInformationStatic { get; set; } = new ObservableCollection<NinjaItem>();

        public ObservableCollection<AbstractItemAspect> AvailableAspects { get; set; } = AbstractItemAspect.AvailableAspects;

        public ItemVariationListView()
        {
            this.InitializeItems();
            InitializeComponent();
            this.DataContext = this;
        }

        public void SelectFirstItem()
        {
            ItemVariationTable.SelectedIndex = 0;
        }

        private void InitializeItems()
        {
            if (string.IsNullOrEmpty(Key))
            {
                return;
            }

            // todo: change this hard-coded "uniques"
            EconomyRequestFacade.GetInstance().EconomyTierlistOverview[this.BranchKey][Key].ForEach(x =>
            {
                ItemVariationInformation.Add(x);
            });

            ItemVariationInformationStatic = ItemVariationInformation;
            AvailableAspects.OrderBy(x => x.Name);

            RefreshAspectColoration();
        }

        private void RefreshAspectColoration()
        {
            for (int i = 0; i < AspectTable.Items.Count; i++)
            {
                DependencyObject obj = AspectTable.ItemContainerGenerator.ContainerFromIndex(i);
                IEnumerable<Button> buttons = WpfUtil.FindVisualChildren<Button>(obj).ToList();

                buttons.Select(x => new { label = AspectTable.Items[i], value = x }).ToList()
                    .ForEach(z =>
                    {
                        if (((ItemVariationTable.SelectedItem ?? ItemVariationTable.Items[0]) as NinjaItem).Aspects.Contains(z.label))
                        {
                            z.value.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#651fff"));
                        }
                        else
                        {
                            z.value.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9e9e9e"));
                        }
                    });
            }
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

            RefreshAspectColoration();
        }

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }
}
