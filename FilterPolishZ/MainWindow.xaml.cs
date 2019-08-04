using System;
using FilterCore;
using FilterCore.FilterComponents.Tier;
using FilterPolishZ.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using FilterCore.Commands.EntryCommands;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.Line;
using FilterCore.Tests;
using FilterDomain.LineStrategy;
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
            Closing += this.OnWindowClose;
        }

        private void OnWindowClose(object sender, CancelEventArgs e)
        {
//            var doSave = InfoPopUpMessageDisplay.DisplayQuestionMessageBox("QuickSave as unnamed seedFilter?");
//            if (doSave)
//            {
//                this.SaveSeedFileAsUnnamed(null, null);
//            }
        }

        private void LoadAllComponents()
        {
            // request ninja-economy info
            this.EconomyData = this.LoadEconomyOverviewData();

            // load aspects
            this.ItemInfoData = this.LoadItemInformationOverview();

            // load filter tierlists
            this.TierListFacade = this.LoadTierLists(this.FilterAccessFacade.PrimaryFilter);

            // add derived tiers (Shaper, Elder)
            this.CreateSubEconomyTiers();

            // run all the enrichment procedures (calculate confidence, min price, max price etc)
            this.EconomyData.EnrichAll();

            // run tiering
            this.TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());
        }

        private void CreateSubEconomyTiers()
        {
            var shaperbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();
            var elderbases =  new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();
            var otherbases =  new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();

            foreach (var items in this.EconomyData.EconomyTierlistOverview["basetypes"])
            {
                var shapergroup = items.Value.Where(x => x.Variant == "Shaper").ToList();
                if (shapergroup.Count != 0)
                {
                    shaperbases.Add(items.Key, new ItemList<NinjaItem>());
                    shaperbases[items.Key].AddRange((shapergroup));
                }

                var eldegroup = items.Value.Where(x => x.Variant == "Elder").ToList();
                if (eldegroup.Count != 0)
                {
                    elderbases.Add(items.Key, new ItemList<NinjaItem>());
                    elderbases[items.Key].AddRange((eldegroup));
                }

                var othergroup = items.Value.Where(x => x.Variant != "Shaper" && x.Variant != "Elder").ToList();
                if (othergroup.Count != 0)
                {
                    otherbases.Add(items.Key, new ItemList<NinjaItem>());
                    otherbases[items.Key].AddRange((othergroup));
                }
            }

            this.EconomyData.AddToDictionary("rare->shaper", shaperbases);
            this.EconomyData.AddToDictionary("rare->elder", elderbases);
            this.EconomyData.AddToDictionary("rare->normal", otherbases);
        }

        private void PerformEconomyTiering()
        {
            var economyTieringSystem = new ConcreteEconomyRules();
            economyTieringSystem.GenerateSuggestions();
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
        private TierListFacade LoadTierLists(Filter filter)
        {
            TierListFacade tierList = TierListFacade.GetInstance();
            tierList.WriteFolder = Configuration.AppSettings["SeedFile Folder"];

            var workTiers = FilterPolishConstants.FilterTierLists;
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
            
            foreach (var tuple in FilterPolishConstants.FileRequestData)
            {
                PerformEcoRequest(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
            }

            void PerformEcoRequest(string dictionaryKey, string requestKey, string url, string prefix) =>
                result.AddToDictionary(dictionaryKey,
                    result.PerformRequest(league, variation, requestKey, url, prefix, this.RequestMode, seedFolder, ninjaUrl));

            return result;
        }

        [Time]
        private ItemInformationFacade LoadItemInformationOverview()
        {
            ItemInformationFacade result = ItemInformationFacade.GetInstance();
            
            var leagueType = Configuration.AppSettings["Ninja League"];
            var baseStoragePath = Configuration.AppSettings["Aspect Folder"];

            result.LeagueType = leagueType;
            result.BaseStoragePath = baseStoragePath;

            var branchKeys = FilterPolishConstants.TierableEconomySections;
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
        
        [Time]
        private async Task WriteFilter(Filter baseFilter, bool isGeneratingStylesAndSeed, string outputFolder = null)
        {
            await FilterWriter.WriteFilter(baseFilter, isGeneratingStylesAndSeed, outputFolder);
        }

        private async Task WriteSeedFilter(Filter baseFilter, string filePath)
        {
            var seedFilterString = baseFilter.Serialize();
            await FileWork.WriteTextAsync(filePath, seedFilterString);
        }

        private void GenerateAllFilterFiles(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, true));
        }
        
        private void GenerateAllFilterFilesTo(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, true, fbd.SelectedPath + "\\"));
                }
            }
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

        private void ApplyAllSuggestions(object sender, RoutedEventArgs e)
        {
            this.TierListFacade.ApplyAllSuggestions();
            this.TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());
            this.EventGrid.Publish();
        }
    }
}
