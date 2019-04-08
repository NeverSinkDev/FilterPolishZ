using System;
using FilterCore;
using FilterCore.FilterComponents.Tier;
using FilterPolishZ.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Forms;
using FilterCore.Commands;
using FilterCore.Constants;
using MethodTimer;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;

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
            InfoPopUpMessageDisplay.InitExceptionHandling();
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
        }

        [Time]
        private Filter PerformFilterWork()
        {
           this.FilterRawString = FileWork.ReadLinesFromFile(Configuration.AppSettings["SeedFile Folder"] + "/" + "NeverSink's filter - SEED (SeedFilter) .filter");
           return new Filter(this.FilterRawString);
        }

        [Time]
        private async Task WriteFilter(Filter baseFilter, bool isGeneratingStyles, string outputFolder, bool? externalSeedFilter, string filterName = "NeverSink's")
        {
            var isStopping = this.VerifyFilter(baseFilter);
            if (isStopping) return;
            
            if (outputFolder == null) outputFolder = Configuration.AppSettings["Output Folder"];
            var styleSheetFolderPath = Configuration.AppSettings["StyleSheet Folder"];
            var generationTasks = new List<Task>();

            if (externalSeedFilter.HasValue)
            {
                var seedFilterString = baseFilter.Serialize();
                var seedPath = outputFolder;
                if (externalSeedFilter.Value) seedPath += "\\ADDITIONAL-FILES\\SeedFilter\\";
                generationTasks.Add(FileWork.WriteTextAsync(seedPath + filterName + " filter - SEED (SeedFilter) .filter", seedFilterString));
            }

            baseFilter.ExecuteCommandTags();
            
            var baseFilterString = baseFilter.Serialize();
            
            for (var strictnessIndex = 0; strictnessIndex < FilterConstants.FilterStrictnessLevels.Count; strictnessIndex++)
            {
                if (isGeneratingStyles)
                {
                    generationTasks.AddRange(FilterConstants.FilterStyles.Select(style => GenerateFilter_Inner(style, strictnessIndex)));
                }

                // default style
                generationTasks.Add(GenerateFilter_Inner("", strictnessIndex));
            }

            await Task.WhenAll(generationTasks);
            InfoPopUpMessageDisplay.ShowInfoMessageBox("Filter generation successfully done!");

            // local func
            async Task GenerateFilter_Inner(string style, int strictnessIndex)
            {
                var filePath = outputFolder;
                var fileName = filterName + " filter - " + strictnessIndex + "-" + FilterConstants.FilterStrictnessLevels[strictnessIndex].ToUpper();
                var filter = new Filter(baseFilterString);
                
                new FilterTableOfContentsCreator(filter).Run();
                new StrictnessGenerator(filter, strictnessIndex).Apply();

                if (style != "")
                {
                    new StyleGenerator(filter, styleSheetFolderPath + style + ".fsty").Apply();
                    filePath += "(STYLE) " + style.ToUpper() + "\\";
                    fileName += " (" + style + ") ";
                }

                if (!System.IO.Directory.Exists(filePath)) System.IO.Directory.CreateDirectory(filePath);
                
                var result = filter.Serialize();
                await FileWork.WriteTextAsync(filePath + "\\" + fileName + ".filter", result);
            }
        }

        private bool VerifyFilter(Filter baseFilter)
        {
            var errorMsg = "";
            
            var oldSeedVersion = baseFilter.GetVersion();
            var newVersion = LocalConfiguration.GetInstance().YieldConfiguration().First(x => x.Key == "Version Number").Value;
            if (oldSeedVersion == newVersion)
            {
                errorMsg += "Version did not change! ";
            }
            else baseFilter.SetVersion(newVersion);
            
            // todo: style tags

            if (errorMsg != "")
            {
                var isStopping = !InfoPopUpMessageDisplay.DisplayQuestionMessageBox("Error: " + errorMsg + ". Do you want to continue the filter generation?");
                return isStopping;
            }

            return false;
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

            var seedFolder = Configuration.AppSettings["SeedFile Folder"];
            var ninjaUrl = Configuration.AppSettings["Ninja Request URL"];
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
                result.PerformRequest(league, variation, requestKey, prefix, this.RequestMode, seedFolder, ninjaUrl));

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

//        private void OnScreenTabSelect(object sender, RoutedEventArgs e)
//        {
//            GenerationOptions.Vo;
//            this.DataContext = ConfigurationView;
//            GenerationOptions.OpenSc
//            MainBorder.Child = new GenerationOptions();
//        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        private void GenerateAllFilterFiles(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, true, null, true, null));
        }

        private void OpenFilterFolder(object sender, RoutedEventArgs e)
        {
            var poePath = "%userprofile%/Documents/My Games/Path of Exile";
            poePath = Environment.ExpandEnvironmentVariables(poePath);
            Process.Start(poePath);
        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {
            Process.Start(Configuration.AppSettings["Output Folder"]);
        }

        private void CopyResultFilesToFilterFolder(object sender, RoutedEventArgs e)
        {
            var poeFolder = "%userprofile%/Documents/My Games/Path of Exile"; 
            poeFolder = Environment.ExpandEnvironmentVariables(poeFolder);
            
            foreach (var file in System.IO.Directory.EnumerateFiles(Configuration.AppSettings["Output Folder"]))
            {
                if (!file.EndsWith(".filter")) continue;
                var targetPath = poeFolder + "\\" + file.Split('/', '\\').Last();

                if (System.IO.File.Exists(targetPath))
                {
                    System.IO.File.Replace(file, targetPath, null);
                }
                else System.IO.File.Copy(file, targetPath);
            }
        }

        private void SelectSeedFilterFile(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var filePath = fd.FileName;
            var lineList = FileWork.ReadLinesFromFile(filePath);
            this.FilterAccessFacade.PrimaryFilter = new Filter(lineList);
        }

        private void GenerateFilterAsUnnamed(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false, null, false, "unnamed"));
        }

        private void GenerateFilterAs(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var filePath = fd.FileName;
            var name = filePath.Split('/', '\\').Last();
            filePath = filePath.Substring(0, filePath.LastIndexOf(name, StringComparison.Ordinal));
            
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false, filePath, false, name));
        }

        private void GenerateFilters_NoStyles(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false, null, null));
        }
    }
}
