using FilterCore;
using FilterCore.FilterComponents.Tier;
using FilterCore.Util;
using FilterEconomy.Request;
using FilterEconomy.Request.Parsing;
using FilterPolishZ.Configuration;
using FilterPolishZ.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
            var workTiers = new HashSet<string> { "unique", "divinations", "uniqueMaps", "fossils", "resonators", "fragments", "prophecies", "shaper", "elder", "crafting", "currency" };
            var tiers = filter.ExtractTiers(workTiers);
            return tiers;
        }

        private void LoadEconomyOverviewData()
        {
            var variation = "tmpstandard";
            var league = "betrayal";

            PerformRequest("divinations", "divination", "?");
            PerformRequest("uniqueMaps","uniqueMaps", "?");
            PerformRequest("uniques", "uniqueWeapons", "?");
            PerformRequest("uniques", "uniqueFlasks", "?");
            PerformRequest("uniques", "uniqueArmours", "?");
            PerformRequest("uniques", "uniqueAccessory", "?");
            PerformRequest("basetypes", "basetypes", "&");

            void PerformRequest(string dictionaryKey, string requestKey, string prefix) => 
                EconomyData.AddToDictionary(dictionaryKey, 
                EconomyData.PerformRequest(league, variation, requestKey, prefix, this.RequestMode));
        }

        private void LoadItemInformationOverview()
        {
            var variation = "defaultSorting";

            PerformRequest(variation, "divination");
            PerformRequest(variation, "uniques");
            PerformRequest(variation, "uniqueMaps");
            PerformRequest(variation, "basetypes");

            void PerformRequest(string loadPath, string requestKey) =>
                ItemInformationFacadeData.AddToDictionary(requestKey,
                ItemInformationFacadeData.LoadItemInformation(loadPath, requestKey));
        }
    }
}
