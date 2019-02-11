using FilterCore;
using FilterCore.FilterComponents.Tier;
using FilterPolishZ.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FilterPolishUtil;
using FilterEconomy.Facades;

namespace FilterPolishZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> DemoItems { get; set; } = new List<string> { "Test", "Test1" };

        // Components
        public LocalConfiguration Configuration { get; set; } = LocalConfiguration.GetInstance();
        public EconomyRequestFacade EconomyData { get; set; } = new EconomyRequestFacade();
        public ItemInformationFacade ItemInformationFacadeData { get; set; } = new ItemInformationFacade();
        public UserControl CurrentWindow;

        public List<string> FilterRawString { get; set; }

        public EconomyRequestFacade.RequestType RequestMode { get; set; } = EconomyRequestFacade.RequestType.Dynamic;

        public MainWindow()
        {
            this.InitializeComponent();
            this.LoadEconomyOverviewData();
            // this.LoadItemInformationOverview();
            var filter = this.PerformFilterWorkAsync();
            var economyDashBoard = this.LoadTierLists(filter);
            Task.Run(() => WriteFilter(filter));
        }

        private Filter PerformFilterWorkAsync()
        {
           this.FilterRawString = FileWork.ReadLinesFromFile(Configuration.AppSettings["SeedFile Folder"] + "/" + "NeverSink's filter - SEED (SeedFilter) .filter");
           return new Filter(this.FilterRawString);
        }

        private async Task WriteFilter(Filter filter)
        {
            var result = filter.Serialize();
            var seedFolder = LocalConfiguration.GetInstance().AppSettings["SeedFile Folder"];
            await FileWork.WriteTextAsync(seedFolder + "/" + "test" + ".filter", result);
        }
        
        private Dictionary<string,TierGroup> LoadTierLists(Filter filter)
        {
            var workTiers = new HashSet<string> { "uniques", "divination", "maps->uniques", "currency->fossil", "currency->resonator", "fragments", "currency->prophecy", "rares->shaperbases", "rares->elderbases", "crafting", "currency" };
            var tiers = filter.ExtractTiers(workTiers);
            return tiers;
        }

        private void LoadEconomyOverviewData()
        {
            var seedfolder = LocalConfiguration.GetInstance().AppSettings["SeedFile Folder"];
            var ninjaurl = LocalConfiguration.GetInstance().AppSettings["Ninja Request URL"];

            var variation = "tmpstandard";
            var league = "betrayal";

            PerformEcoRequest("divination", "divination", "?");
            PerformEcoRequest("maps->uniques", "uniqueMaps", "?");
            PerformEcoRequest("uniques", "uniqueWeapons", "?");
            PerformEcoRequest("uniques", "uniqueFlasks", "?");
            PerformEcoRequest("uniques", "uniqueArmours", "?");
            PerformEcoRequest("uniques", "uniqueAccessory", "?");
            PerformEcoRequest("basetypes", "basetypes", "&");



            void PerformEcoRequest(string dictionaryKey, string requestKey, string prefix) => 
                EconomyData.AddToDictionary(dictionaryKey, 
                EconomyData.PerformRequest(league, variation, requestKey, prefix, this.RequestMode, seedfolder, ninjaurl));
        }

        private void LoadItemInformationOverview()
        {
            var localConfig = LocalConfiguration.GetInstance();
            var baseStoragePath = localConfig.AppSettings["SeedFile Folder"];

            var variation = "defaultSorting";

            PerformItemInfoRequest(variation, "divination");
            PerformItemInfoRequest(variation, "uniques");
            PerformItemInfoRequest(variation, "uniqueMaps");
            PerformItemInfoRequest(variation, "basetypes");

            void PerformItemInfoRequest(string loadPath, string requestKey) =>
                ItemInformationFacadeData.AddToDictionary(requestKey,
                ItemInformationFacadeData.LoadItemInformation(loadPath, requestKey, baseStoragePath));
        }

        private void OnScreenTabSelect(object sender, RoutedEventArgs e)
        {
//            GenerationOptions.Vo;
//            this.DataContext = ConfigurationView;
//            GenerationOptions.OpenSc
//            MainBorder.Child = new GenerationOptions();
        }
    }
}
