using FilterCore;
using FilterCore.FilterComponents.Tier;
using FilterPolishZ.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FilterPolishUtil;
using FilterEconomy.Facades;
using FilterPolishZ.Economy;
using FilterEconomy.Model.ItemInformationData;
using System.Linq;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishZ.ModuleWindows.ItemInfo;
using FilterPolishZ.ModuleWindows.Configuration;
using FilterPolishZ.Domain;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using FilterCore.Commands;
using FilterCore.Constants;
using MethodTimer;

namespace FilterPolishZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public EconomyRequestFacade.RequestType RequestMode { get; set; } = EconomyRequestFacade.RequestType.Dynamic;

        // Components
        public LocalConfiguration Configuration { get; set; } = LocalConfiguration.GetInstance();
        public EconomyRequestFacade EconomyData { get; set; }
        public ItemInformationFacade ItemInfoData { get; set; }
        public TierListFacade TierListFacade { get; set; }
        public FilterAccessFacade FilterAccessFacade { get; set; } = FilterAccessFacade.GetInstance();

        public List<string> FilterRawString { get; set; }

        public MainWindow()
        {
            ConcreteEnrichmentProcedures.Initialize();

            // Initialize Modules
            this.FilterAccessFacade.PrimaryFilter = this.PerformFilterWork();
            this.EconomyData = this.LoadEconomyOverviewData();
            this.ItemInfoData = this.LoadItemInformationOverview();
            this.TierListFacade = this.LoadTierLists(this.FilterAccessFacade.PrimaryFilter);

            // Initialize 
            var economyTieringSystem = new ConcreteEconomyRules();

            economyTieringSystem.Execute();

            this.TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());

            // Initialize Settings
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();

            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter));

        }

        [Time]
        private Filter PerformFilterWork()
        {
           this.FilterRawString = FileWork.ReadLinesFromFile(Configuration.AppSettings["SeedFile Folder"] + "/" + "NeverSink's filter - SEED (SeedFilter) .filter");
           return new Filter(this.FilterRawString);
        }

        [Time]
        private void WriteFilter(Filter baseFilter)
        {
            var baseFilterString = baseFilter.Serialize();
            var outputFolder = Configuration.AppSettings["Output Folder"];
            var styleSheetFolderPath = Configuration.AppSettings["StyleSheet Folder"];
            
            for (var i = 0; i < FilterConstants.FilterStrictnessLevels.Count; i++)
            {
                foreach (var style in FilterConstants.FilterStyles)
                {
                    GenerateAllStrictnessLevels(style, i);
                }
                
                // default style
                GenerateAllStrictnessLevels("", i);
            }

            void GenerateAllStrictnessLevels(string style, int i)
            {
                var filePath = outputFolder;
                var filter = new Filter(baseFilterString);
                new FilterTableOfContentsCreator(filter);

                new StrictnessGenerator(filter, i).Apply();

                if (style != "")
                {
                    new StyleGenerator(filter, styleSheetFolderPath + style + ".fsty").Apply();
                    filePath += style + "\\";
                }

                if (!System.IO.Directory.Exists(filePath)) System.IO.Directory.CreateDirectory(filePath);

                filePath += i + FilterConstants.FilterStrictnessLevels[i] + ".filter";
                var result = filter.Serialize();

                FileWork.WriteTextAsync(filePath, result);
            }
        }

        [Time]
        private TierListFacade LoadTierLists(Filter filter)
        {
            TierListFacade tierList = TierListFacade.GetInstance();

            var workTiers = new HashSet<string> { "uniques", "divination", "maps->uniques", "currency->fossil", "currency->resonator", "fragments", "currency->prophecy", "rares->shaperbases", "rares->elderbases", "crafting", "currency" };
            var tiers = filter.ExtractTiers(workTiers);
            tierList.TierListData = tiers;

            foreach (var item in tierList.TierListData.Values)
            {
                item.ReEvaluate();
            }
            return tierList;
        }

        [Time]
        private EconomyRequestFacade LoadEconomyOverviewData()
        {
            var result = EconomyRequestFacade.GetInstance();

            var seedfolder = Configuration.AppSettings["SeedFile Folder"];
            var ninjaurl = Configuration.AppSettings["Ninja Request URL"];
            var variation = Configuration.AppSettings["Ninja League"];
            var league = Configuration.AppSettings["betrayal"];

            PerformEcoRequest("divination", "divination", "?");
            PerformEcoRequest("maps->uniques", "uniqueMaps", "?");
            PerformEcoRequest("uniques", "uniqueWeapons", "?");
            PerformEcoRequest("uniques", "uniqueFlasks", "?");
            PerformEcoRequest("uniques", "uniqueArmours", "?");
            PerformEcoRequest("uniques", "uniqueAccessory", "?");
            PerformEcoRequest("basetypes", "basetypes", "&");

            void PerformEcoRequest(string dictionaryKey, string requestKey, string prefix) =>
                result.AddToDictionary(dictionaryKey,
                result.PerformRequest(league, variation, requestKey, prefix, this.RequestMode, seedfolder, ninjaurl));

            return result;
        }

        [Time]
        private ItemInformationFacade LoadItemInformationOverview()
        {
            ItemInformationFacade result = ItemInformationFacade.GetInstance();
            
            var leagueType = Configuration.AppSettings["Ninja League"];
            var baseStoragePath = Configuration.AppSettings["SeedFile Folder"];

            result.LeagueType = leagueType;
            result.BaseStoragePath = baseStoragePath;

            var branchKeys = new List<string>
            {
                "divination", "uniques", "maps->uniques", "basetypes"
            };
            
            branchKeys.ForEach(key => result.EconomyTierListOverview.Add(key, new Dictionary<string, List<ItemInformationData>>()));

            result.LoadFromSaveFile();
            
            return result;
        }

        private void OnScreenTabSelect(object sender, RoutedEventArgs e)
        {
//            GenerationOptions.Vo;
//            this.DataContext = ConfigurationView;
//            GenerationOptions.OpenSc
//            MainBorder.Child = new GenerationOptions();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            MenuToggleButton.IsChecked = false;
        }
    }
}
