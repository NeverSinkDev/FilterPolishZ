using FilterCore.Constants;
using FilterEconomyProcessor.BaseTypeMatrix;
using FilterPolishUtil.Collections;
using FilterPolishZ.ModuleWindows.Converters;
using FilterPolishZ.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
        private bool Inited = false;

        public ObservableCollection<KeyBaseTypeRow> BaseTypeMatrixTable { get; set; } = new ObservableCollection<KeyBaseTypeRow>();

        public static List<KeyBaseTypeRow> TableContent = new List<KeyBaseTypeRow>();
        public static string Key;
        public static string Section;
        public static BaseTypeMatrixFacade Facade;

        public static int CursorMode = 1;

        public BaseTypeTieringView()
        {
            InitializeComponent();
            this.DataContext = this;

            this.SelectedKey1.ItemsSource = BaseTypeDataProvider.BaseTypeTieringMatrixData.Keys;
            this.SelectedKey1.SelectedItem = BaseTypeDataProvider.BaseTypeTieringMatrixData.Keys.First();
            this.SelectedSection.ItemsSource = FilterPolishUtil.FilterPolishConfig.MatrixTiersStrategies.Keys;
            this.SelectedSection.SelectedItem = FilterPolishUtil.FilterPolishConfig.MatrixTiersStrategies.Keys.First();

            Key = this.SelectedKey1.SelectedItem.ToString();
            Section = this.SelectedSection.SelectedItem.ToString();
            Facade = BaseTypeMatrixFacade.GetInstance();
            this.GenerateOutputTable();
            this.Inited = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static string LookUpItem(string itemName)
        {
            return Facade.LookUpTierName(Section, itemName);
        }

        public void GenerateOutputTable()
        {
            GenerateOutputTableBody();
        }

        private void GenerateOutputTableBody()
        {
            var results = new List<KeyBaseTypeRow>();

            // EV, AR, ES OR a weapontype
            var classgroup = Facade.SortedBaseTypeMatrix[this.SelectedKey1.SelectedItem.ToString()];

            // either ALL or Boots, Gloves [...]
            var subClasses = classgroup.Keys.ToList();

            // the key represents overclasses such as amortypes or weapons
            var aheadList = new Dictionary<string, int>();

            foreach (var item in subClasses)
            {
                aheadList.Add(item, 0);
            }

            for (int j = 0; j < subClasses.Count; j++)
            {
                // something like: ES-boots
                var itemGroup = classgroup[subClasses[j]];

                for (int i = 0; i < itemGroup.Count; i++)
                {
                    bool isSpecialBase = false;
                    Dictionary<string, string> baseType = itemGroup[i];

                    if (FilterPolishUtil.FilterPolishConfig.SpecialBases.Contains(baseType["BaseType"]))
                    {
                        isSpecialBase = true;
                    }
                    
                    if (isSpecialBase)
                    {
                        if (FilterPolishUtil.FilterPolishConfig.MatrixTiersStrategies[Section] == FilterPolishUtil.MatrixTieringMode.rareTiering)
                        {
                            aheadList[subClasses[j]]++;
                            continue;
                        }
                    }
                    var aheadCounter = aheadList[subClasses[j]];

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
                            results[i - aheadCounter].All = cell;
                            break;
                        case "Boots":
                            results[i - aheadCounter].Boots = cell;
                            break;
                        case "Body Armours":
                            results[i - aheadCounter].Body = cell;
                            break;
                        case "Helmets":
                            results[i - aheadCounter].Helmets = cell;
                            break;
                        case "Shields":
                            results[i - aheadCounter].Shields = cell;
                            break;
                        case "Gloves":
                            results[i - aheadCounter].Gloves = cell;
                            break;
                        default:
                            throw new Exception("unknown subclass!");
                    }
                }
            }

            TableContent = results;
            BaseTypeMatrixTable = new ObservableCollection<KeyBaseTypeRow>(TableContent);
        }

        private void SelectedKey1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Inited)
            {
                return;
            }

            Key = this.SelectedKey1.SelectedItem.ToString();
            GenerateOutputTable();
        }

        private void SelectedSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Inited)
            {
                return;
            }

            Section = this.SelectedSection.SelectedItem.ToString();
            ToggleButtonEnableState();
            GenerateOutputTable();
        }

        private void ToggleButtonEnableState()
        {
            var strategy = FilterPolishUtil.FilterPolishConfig.MatrixTiersStrategies[Section];

            switch (strategy)
            {
                case FilterPolishUtil.MatrixTieringMode.singleTier:
                    TierChangeButton2.IsEnabled = false;
                    TierChangeButton3.IsEnabled = false;
                    break;
                case FilterPolishUtil.MatrixTieringMode.rareTiering:
                    TierChangeButton3.IsEnabled = true;
                    TierChangeButton2.IsEnabled = true;
                    break;
            }
        }

        private void curs1click(object sender, RoutedEventArgs e) => CursorMode = 1;
        private void curs2click(object sender, RoutedEventArgs e) => CursorMode = 2;
        private void curs3click(object sender, RoutedEventArgs e) => CursorMode = 3;
        private void curs4click(object sender, RoutedEventArgs e) => CursorMode = 4;
        private void Refresh_Click(object sender, RoutedEventArgs e) => this.GenerateOutputTable();

        private void TieringMatrixGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while (dep != null && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                return;
            }

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                KeyBaseTypeRow data = cell.DataContext as KeyBaseTypeRow;
                var itemName = BaseTypeToMatrixTierColorConverter.GetItemName(cell.Column.DisplayIndex, data);

                if (string.IsNullOrEmpty(itemName))
                {
                    return;
                }

                Facade.ChangeTier(Section, itemName, CursorMode);
                GenerateOutputTableBody();

                // BindingOperations.GetBindingExpression(dep, DataGridCell.SourceProperty).UpdateTarget();
            }
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