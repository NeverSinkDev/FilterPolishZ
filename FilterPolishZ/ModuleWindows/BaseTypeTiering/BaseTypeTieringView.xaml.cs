using FilterCore.Constants;
using FilterEconomyProcessor.BaseTypeMatrix;
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

namespace FilterPolishZ.ModuleWindows.BaseTypeTiering
{
    /// <summary>
    /// Interaction logic for BaseTypeTieringView.xaml
    /// </summary>
    public partial class BaseTypeTieringView : UserControl, INotifyPropertyChanged
    {
        public BaseTypeMatrixFacade Facade;

        private bool Initialized = false;

        public ObservableCollection<KeyBaseTypeRow> BaseTypeMatrixTable { get; set; } = new ObservableCollection<KeyBaseTypeRow>();

        public BaseTypeTieringView()
        {
            InitializeComponent();
            this.DataContext = this;
            this.SelectedKey1.ItemsSource = BaseTypeDataProvider.BaseTypeTieringMatrixData.Keys;
            this.SelectedKey1.SelectedItem = BaseTypeDataProvider.BaseTypeTieringMatrixData.Keys.First();
            this.Facade = BaseTypeMatrixFacade.GetInstance();
            this.GenerateOutputTable();
            this.Initialized = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void GenerateOutputTable()
        {
            var results = new List<KeyBaseTypeRow>();
            var maxCount = 0;

            // EV, AR, ES OR a weapontype
            var classgroup = Facade.SortedBaseTypeMatrix[this.SelectedKey1.SelectedItem.ToString()];

            // either ALL or Boots, Gloves [...]
            var subClasses = classgroup.Keys.ToList();

            // the key represents overclasses such as amortypes or weapons
            for (int j = 0; j < subClasses.Count; j++)
            {
                // something like: ES-boots
                var itemGroup = classgroup[subClasses[j]];

                for (int i = 0; i < itemGroup.Count; i++)
                {
                    Dictionary<string, string> baseType = itemGroup[i];
                    var cell = new KeyBaseTypeCell()
                    {
                        Name = baseType["BaseType"],
                        Properties = baseType
                    };

                    if (i >= results.Count)
                    {
                        results.Add(new KeyBaseTypeRow());
                    }

                    switch (subClasses[j])
                    {
                        case "all":
                            results[i].All = cell;
                            break;
                        case "Boots":
                            results[i].Boots = cell;
                            break;
                        case "Body Armours":
                            results[i].Body = cell;
                            break;
                        case "Helmets":
                            results[i].Helmets = cell;
                            break;
                        case "Shields":
                            results[i].Shields = cell;
                            break;
                        case "Gloves":
                            results[i].Gloves = cell;
                            break;
                        default:
                            throw new Exception("unknown subclass!");
                    }
                }
            }

            BaseTypeMatrixTable = new ObservableCollection<KeyBaseTypeRow>(results);
        }

        private void SelectedKey1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            GenerateOutputTable();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class KeyBaseTypeRow
    {
        public KeyBaseTypeCell All { get; set; }
        public KeyBaseTypeCell Boots { get; set; }
        public KeyBaseTypeCell Gloves { get; set; }
        public KeyBaseTypeCell Body { get; set; }
        public KeyBaseTypeCell Shields { get; set; }
        public KeyBaseTypeCell Helmets { get; set; }
    }

    public class KeyBaseTypeCell
    {
        public Dictionary<string, string> Properties;
        public string Name { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }
    }
}
