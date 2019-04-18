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
using FilterCore.Entry;
using FilterCore.Line;
using FilterCore.Tests;
using FilterPolishUtil.Constants;
using MethodTimer;
using MessageBox = System.Windows.Forms.MessageBox;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using FilterPolishZ.Util;

namespace FilterPolishZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public EconomyRequestFacade.RequestType RequestMode { get; set; } = EconomyRequestFacade.RequestType.Dynamic;

        // Components
        public EventGridFacade EventGrid = EventGridFacade.GetInstance();

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
            LoadAllComponents();

            // Initialize Settings
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void LoadAllComponents()
        {
            this.EconomyData = this.LoadEconomyOverviewData();
            this.ItemInfoData = this.LoadItemInformationOverview();
            this.TierListFacade = this.LoadTierLists(this.FilterAccessFacade.PrimaryFilter);
            
            this.EconomyData.EnrichAll();
            this.TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());
        }

        private void PerformEconomyTiering()
        {
            var economyTieringSystem = new ConcreteEconomyRules();
            economyTieringSystem.Execute();
            this.TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());
            this.EventGrid.Publish();
        }

        private void ResetAllComponents()
        {
            this.EconomyData.Reset();
            this.ItemInfoData.Reset();
            this.TierListFacade.Reset();
        }

        [Time]
        private Filter PerformFilterWork()
        {
            var outputFolder = Configuration.AppSettings["Output Folder"];
            var defaultPath = outputFolder + "\\Unnamed filter - SEED (SeedFilter) .filter";
            string filePath;

            if (System.IO.File.Exists(defaultPath))
            {
//                InfoPopUpMessageDisplay.ShowInfoMessageBox("unnamed seed used");
                filePath = defaultPath;
            }
            else
            {
                this.SelectSeedFilterFile(null, null);
                return this.FilterAccessFacade.PrimaryFilter;
            }
            
            this.FilterRawString = FileWork.ReadLinesFromFile(filePath);
            if (this.FilterRawString == null || this.FilterRawString.Count < 4500)
            {
                InfoPopUpMessageDisplay.ShowError("Warning: (seed) filter result line count: " + this.FilterRawString?.Count);
            }
            return new Filter(this.FilterRawString);
        }

        [Time]
        private async Task WriteFilter(Filter baseFilter, bool isGeneratingStylesAndSeed)
        {
            var isStopping = this.VerifyFilter(baseFilter);
            if (isStopping) return;
            
            new FilterTableOfContentsCreator(baseFilter).Run();

            const string filterName = "NeverSink's";
            var outputFolder = Configuration.AppSettings["Output Folder"];
            var styleSheetFolderPath = Configuration.AppSettings["StyleSheet Folder"];
            var generationTasks = new List<Task>();
            var seedFilterString = baseFilter.Serialize();

            if (isGeneratingStylesAndSeed)
            {
                var seedPath = outputFolder + "\\ADDITIONAL-FILES\\SeedFilter\\" + filterName + " filter - SEED (SeedFilter) .filter";
                generationTasks.Add(FileWork.WriteTextAsync(seedPath, seedFilterString));
            }
            
            baseFilter.ExecuteCommandTags();
            var baseFilterString = baseFilter.Serialize();
            if (baseFilterString == null || baseFilterString.Count < 4500) InfoPopUpMessageDisplay.ShowError("Warning: (seed) filter result line count: " + baseFilterString?.Count);
            
            for (var strictnessIndex = 0; strictnessIndex < FilterConstants.FilterStrictnessLevels.Count; strictnessIndex++)
            {
                if (isGeneratingStylesAndSeed)
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
                filter.ExecuteStrictnessCommands(strictnessIndex);

                if (style != "")
                {
                    new StyleGenerator(filter, styleSheetFolderPath + style + ".fsty", style).Apply();
                    filePath += "(STYLE) " + style.ToUpper() + "\\";
                    fileName += " (" + style + ") ";
                }

                if (!System.IO.Directory.Exists(filePath)) System.IO.Directory.CreateDirectory(filePath);
                
                var result = filter.Serialize();

                if (result.Count <= seedFilterString?.Count)
                {
                    // todo: temporarily disabled due to missing "up" tags that make the result filter longer
                    // while things like %hs make the result filter shorter, triggering this error
                    InfoPopUpMessageDisplay.ShowError("Error: style/strictness variant is smaller size than seed");
                }
                
                await FileWork.WriteTextAsync(filePath + "\\" + fileName + ".filter", result);
            }
        }

        private async Task WriteSeedFilter(Filter baseFilter, string filePath)
        {
            var seedFilterString = baseFilter.Serialize();
            await FileWork.WriteTextAsync(filePath, seedFilterString);
        }

        private bool VerifyFilter(Filter baseFilter)
        {
            var errorMsg = new List<string>();
            
            var oldSeedVersion = baseFilter.GetHeaderMetaData("version:");
            var newVersion = LocalConfiguration.GetInstance().YieldConfiguration().First(x => x.Key == "Version Number").Value;
            if (oldSeedVersion == newVersion)
            {
                errorMsg.Add("Version did not change!");
            }
            else baseFilter.SetHeaderMetaData("version:", newVersion);
            
//            FilterStyleVerifyer.Run(baseFilter); // todo: re-enable this when the filter doesnt have the tons of errors anymore

            if (errorMsg.Count > 0)
            {
                var isStopping = !InfoPopUpMessageDisplay.DisplayQuestionMessageBox("Error: \n\n" + string.Join("\n", errorMsg) + "\n\nDo you want to continue the filter generation?");
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
            var task = this.LoadEconomyOverviewData_Inner();
            Task.WaitAll(task);
            return task.Result;
        }
        
        private async Task<EconomyRequestFacade> LoadEconomyOverviewData_Inner()
        {
            var result = EconomyRequestFacade.GetInstance();
            var seedFolder = Configuration.AppSettings["SeedFile Folder"];
            var ninjaUrl = Configuration.AppSettings["Ninja Request URL"];
            var variation = Configuration.AppSettings["Ninja League"];
            var league = Configuration.AppSettings["betrayal"];
            
            var requestData = new List<Tuple<string, string, string>>
            {
                new Tuple<string, string, string>("divination", "divination", "?"),
                new Tuple<string, string, string>("maps->uniques", "uniqueMaps", "?"),
                new Tuple<string, string, string>("uniques", "uniqueWeapons", "?"),
                new Tuple<string, string, string>("uniques", "uniqueFlasks", "?"),
                new Tuple<string, string, string>("uniques", "uniqueArmours", "?"),
                new Tuple<string, string, string>("uniques", "uniqueAccessory", "?"),
                new Tuple<string, string, string>("basetypes", "basetypes", "&")
            };
            
            foreach (var tuple in requestData)
            {
                PerformEcoRequest(tuple.Item1, tuple.Item2, tuple.Item3);
            }

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
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, true));
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
            if (lineList == null || lineList.Count < 4500) InfoPopUpMessageDisplay.ShowError("Warning: (seed) filter result line count: " + lineList?.Count);
            this.FilterAccessFacade.PrimaryFilter = new Filter(lineList);

            this.ResetAllComponents();
            this.LoadAllComponents();
            this.EventGrid.Publish();
        }

        private void SaveSeedFileAsUnnamed(object sender, RoutedEventArgs e)
        {
            var outputFolder = Configuration.AppSettings["Output Folder"];
            var seedPath = outputFolder + "\\Unnamed filter - SEED (SeedFilter) .filter";
            Task.Run(() => WriteSeedFilter(this.FilterAccessFacade.PrimaryFilter, seedPath));
        }

        private void SaveSeedFilterAs(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var filePath = fd.FileName;
            Task.Run(() => WriteSeedFilter(this.FilterAccessFacade.PrimaryFilter, filePath));
        }

        private void GenerateFilters_NoStyles(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false));
        }

        private void PerformAutomatedTiering(object sender, RoutedEventArgs e)
        {
            PerformEconomyTiering();
        }
    }
}
