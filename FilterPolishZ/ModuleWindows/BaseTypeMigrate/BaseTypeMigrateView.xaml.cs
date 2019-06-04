using FilterCore;
using FilterCore.Entry;
using FilterDomain.LineStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

namespace FilterPolishZ.ModuleWindows.BaseTypeMigrate
{


    /// <summary>
    /// Interaction logic for BaseTypeMigrateView.xaml
    /// </summary>
    public partial class BaseTypeMigrateView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<MigratedBaseType> MigrationValues { get; set; } = new ObservableCollection<MigratedBaseType>();
        public Dictionary<string, string> MigrationValuesRaw { get; set; } = new Dictionary<string, string>();
        public Filter Filter { get; set; }

        public BaseTypeMigrateView()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Initialize();
        }

        public void Initialize()
        {
            this.MigrationValues.Add(new MigratedBaseType("Platinum Kris", "Platinum Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Skean", "Silver Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Boot Knife", "Jade Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Imp Dagger", "Imp Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Butcher Knife", "Sacrificial Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Boot Blade", "Jewelled Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Golden Kris", "Golden Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Fiend Dagger", "Fiend Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Royal Skean", "Royal Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Slaughter Knife", "Slaughter Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Imperial Skean", "Imperial Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Ezomyte Dagger", "Ezomyte Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Demon Dagger", "Demon Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Carving Knife", "Iron Rune Dagger"));
            this.MigrationValues.Add(new MigratedBaseType("Copper Kris", "Copper Rune Dagger"));

            foreach (var item in this.MigrationValues)
            {
                this.MigrationValuesRaw.Add(item.From, item.To);
            }
        }

#pragma warning disable CS4101
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS4101

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Filter = FilterAccessFacade.GetInstance().PrimaryFilter;
            this.MigrateBaseTypes();
        }

        private void MigrateBaseTypes()
        {
            foreach (var entry in this.Filter.FilterEntries)
            {
                var values = entry.GetValues<EnumValueContainer>("BaseType").FirstOrDefault();

                if (values == null)
                {
                    continue;
                }

                foreach (var basetype in values.Value)
                {
                    if (this.MigrationValuesRaw.ContainsKey(basetype.value))
                    {
                        basetype.value = this.MigrationValuesRaw[basetype.value];
                    }
                }
            }
        }
    }
    public class MigratedBaseType
    {
        public MigratedBaseType(string from, string to)
        {
            From = from;
            To = to;
        }

        public string From { get; set; }
        public string To { get; set; }
    }
}
