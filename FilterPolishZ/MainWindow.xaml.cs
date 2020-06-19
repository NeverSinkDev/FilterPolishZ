﻿using System;
using FilterCore;
using FilterPolishZ.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using FilterPolishUtil;
using FilterEconomy.Facades;
using FilterEconomy.Model.ItemInformationData;
using System.Linq;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishZ.Domain;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using MethodTimer;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using FilterPolishZ.Util;
using Newtonsoft.Json;
using FilterPolishUtil.Model;
using System.IO.Compression;
using FilterPolishWindowUtils;
using FilterPolishZ.Economy;
using FilterEconomyProcessor;
using FilterEconomyProcessor.ClassAbstraction;
using FilterCore.Constants;
using Application = System.Windows.Application;
using FilterExo;
using FilterPolishZ.ModuleWindows.GenerationOptions;

namespace FilterPolishZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        // Components
        public EventGridFacade EventGrid = EventGridFacade.GetInstance();

        public LocalConfiguration Configuration { get; set; } = LocalConfiguration.GetInstance();
        public EconomyRequestFacade EconomyData { get; set; }
        public ItemInformationFacade ItemInfoData { get; set; }
        public TierListFacade TierListFacade { get; set; }
        public FilterAccessFacade FilterAccessFacade { get; set; } = FilterAccessFacade.GetInstance();

        public FilterExoFacade FilterExoFacade { get; set; } = FilterExoFacade.GetInstance();

        public List<string> FilterRawString { get; set; }

        public MainWindow()
        {
            InfoPopUpMessageDisplay.InitExceptionHandling();
            ConcreteEnrichmentProcedures.Initialize();

            // LoadMetaFilter();

            // Loads and Parses filter, then loads economy, tierlists, aspects, suggestions
            this.FilterAccessFacade.PrimaryFilter = this.PerformFilterWork();
            LoadAllComponents();

            // Initialize Settings
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void LoadAllComponents()
        {
            // request ninja-economy info
            this.EconomyData = this.LoadEconomyOverviewData();
            this.EconomyData.RequestPoeLeagueInfo();

            if (!this.EconomyData.IsLeagueActive())
            {
                LoggingFacade.LogWarning("No Active League detected!");
            }

            // load aspects
            this.ItemInfoData = this.LoadItemInformationOverview();

            // load filter tierlists
            this.TierListFacade = this.LoadTierLists(this.FilterAccessFacade.PrimaryFilter);

            // add derived tiers (Shaper, Elder)
            this.EconomyData.CreateSubEconomyTiers();

            BaseTypeDataProvider.Initialize();

            // run all the enrichment procedures (calculate confidence, min price, max price etc)
            this.EconomyData.EnrichAll(EnrichmentProcedureConfiguration.PriorityEnrichmentProcedures);
            FilterPolishUtil.FilterPolishConfig.AdjustPricingInformation();
            this.EconomyData.EnrichAll(EnrichmentProcedureConfiguration.EnrichmentProcedures);

            // experimental basetype->class abstraction
            var abstractions = new InfluencedBasesAbstractionOverview();
            abstractions.Execute();
            var abstractionConclusons = new InfluencedBasesAbstractionConclusions();
            abstractionConclusons.Execute();

            // run tiering
            this.TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());

            LoggingFacade.LogInfo($"FilterBlade main subroutine loaded succesfully");
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
                filePath = defaultPath;
            }
            else
            {
                this.SelectSeedFilterFile(null, null);
                return this.FilterAccessFacade.PrimaryFilter;
            }

            LoggingFacade.LogInfo($"Loading Filter: {filePath}");

            this.FilterRawString = FileWork.ReadLinesFromFile(filePath);

            if (this.FilterRawString == null || this.FilterRawString.Count < 4500)
            {
                LoggingFacade.LogWarning($"Loading Filter: Filter Content Suspiciously Short");
            }
            return new Filter(this.FilterRawString);
        }

        [Time]
        private TierListFacade LoadTierLists(Filter filter)
        {
            LoggingFacade.LogInfo($"Loading Tierlists...");

            TierListFacade tierList = TierListFacade.GetInstance();
            tierList.WriteFolder = Configuration.AppSettings["EcoFile Folder"];

            var workTiers = FilterPolishConfig.FilterTierLists;
            var tiers = filter.ExtractTiers(workTiers);
            tierList.TierListData = tiers;

            foreach (var item in tierList.TierListData.Values)
            {
                LoggingFacade.LogDebug($"Loading Tierlist: {item.Category}");
                item.ReEvaluate();
            }

            LoggingFacade.LogInfo($"Done Loading Tierlists...");
            return tierList;
        }

        [Time]
        private EconomyRequestFacade LoadEconomyOverviewData()
        {
            LoggingFacade.LogDebug("Loading Economy Data...");

            var result = EconomyRequestFacade.GetInstance();
            var seedFolder = Configuration.AppSettings["EcoFile Folder"];
            var variation = Configuration.AppSettings["leagueType"];
            var league = Configuration.AppSettings["currentLeague"];
            
            foreach (var tuple in FilterPolishConfig.FileRequestData)
            {
                PerformEcoRequest(tuple.Item1, tuple.Item2, tuple.Item3);
                LoggingFacade.LogDebug($"Loading Economy: {tuple.Item1} + {tuple.Item2} + {tuple.Item3}");
            }

            void PerformEcoRequest(string dictionaryKey, string requestKey, string url) =>
                result.AddToDictionary(dictionaryKey,
                    result.PerformRequest(league, variation, requestKey, url, seedFolder));

            LoggingFacade.LogInfo("Economy Data Loaded...");

            return result;
        }

        [Time]
        private ItemInformationFacade LoadItemInformationOverview()
        {
            ItemInformationFacade result = ItemInformationFacade.GetInstance();

            LoggingFacade.LogDebug("Economy Item Information Loaded...");

            var leagueType = Configuration.AppSettings["leagueType"];
            var baseStoragePath = Configuration.AppSettings["Aspect Folder"];

            result.LeagueType = leagueType;
            result.BaseStoragePath = baseStoragePath;

            var branchKeys = FilterPolishConfig.TierableEconomySections;
            branchKeys.ForEach(key => result.EconomyTierListOverview.Add(key, new Dictionary<string, List<ItemInformationData>>()));

            result.LoadFromSaveFile();

            LoggingFacade.LogDebug("Item Information Loaded...");

            return result;
        }

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
            if (File.Exists(outputFolder + "RESULTS.zip"))
            {
                File.Delete(outputFolder + "RESULTS.zip");
            }

            var oldSeedVersion = baseFilter.GetHeaderMetaData("version:");
            var newVersion = LocalConfiguration.GetInstance().YieldConfiguration().First(x => x.Key == "Version Number").Value;
            if (oldSeedVersion == newVersion)
            {
                var isStopping = !InfoPopUpMessageDisplay.DisplayQuestionMessageBox("Error: \n\nVersion did not change!\nDo you want to continue the filter generation?");
                if (isStopping) return;
            }
            else baseFilter.SetHeaderMetaData("version:", newVersion);

            await FilterWriter.WriteFilter(baseFilter, isGeneratingStylesAndSeed, outputFolder, Configuration.AppSettings["StyleSheet Folder"]);
        }

        private async Task WriteSeedFilter(Filter baseFilter, string filePath)
        {
            LoggingFacade.LogInfo("Writing SeedFilter!");
            var seedFilterString = baseFilter.Serialize();
            await FileWork.WriteTextAsync(filePath, seedFilterString);
            LoggingFacade.LogInfo("DONE: Writing SeedFilter!");
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

        private void GenerateAllFilterFiles(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, true, Configuration.AppSettings["Output Folder"]));
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

            LoggingFacade.LogInfo($"Copying filter files from: {poeFolder}");
            LoggingFacade.LogInfo($"Copying filter files to: {Configuration.AppSettings["Output Folder"]}");

            foreach (var file in System.IO.Directory.EnumerateFiles(Configuration.AppSettings["Output Folder"]))
            {
                if (!file.EndsWith(".filter")) continue;
                var targetPath = poeFolder + "\\" + file.Split('/', '\\').Last();
                System.IO.File.Copy(file, targetPath, true);
            }
        }

        private void SelectSeedFilterFile(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var filePath = fd.FileName;
            var lineList = FileWork.ReadLinesFromFile(filePath);
            if (lineList == null || lineList.Count < 4500) LoggingFacade.LogError("Warning: (seed) filter result line count: " + lineList?.Count);
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

            LoggingFacade.LogInfo($"Writing Changelog! Tiers Logged: {this.TierListFacade.Changelog.Select(x => x.Value.Count).Sum()}");

            var json = JsonConvert.SerializeObject(this.TierListFacade.Changelog).Replace("->", "_");
            var changeLogPath = LocalConfiguration.GetInstance().AppSettings["Output Folder"] + "/Changelog/changelog.json";
            FileWork.WriteTextAsync(changeLogPath, json);
            
            this.EventGrid.Publish();
        }

        private void CopyResultsToGitHubFolder(object sender, RoutedEventArgs e)
        {
            var gitFolder = Configuration.AppSettings["Git Folder"];

            LoggingFacade.LogInfo($"Copying filter files to: {gitFolder}");

            foreach (var file in System.IO.Directory.EnumerateFiles(Configuration.AppSettings["Output Folder"], "*", SearchOption.AllDirectories))
            {
                var valid = false;

                if (file.EndsWith(".filter") || file.EndsWith(".json") || file.EndsWith(".fsty"))
                {
                    valid = true;
                }

                if (file.ToLower().Contains("unnamed") || file.ToLower().Contains("copy"))
                {
                    continue;
                }

                if (!valid)
                {
                    continue;
                }

                var targetPath = gitFolder + file.SkipSegment(Configuration.AppSettings["Output Folder"]);

                if (!Directory.Exists(System.IO.Path.GetDirectoryName(targetPath)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(targetPath));
                }
                
                System.IO.File.Copy(file, targetPath, true);
            }

            LoggingFacade.LogInfo($"Done Copying filter files to: {Configuration.AppSettings["Git Folder"]}");
            Process.Start(Configuration.AppSettings["Git Folder"]);
        }

        private void CreateZipFiles(object sender, RoutedEventArgs e)
        {
            using (var zipFileStream = new FileStream(Configuration.AppSettings["Output Folder"]+"RESULTS.zip", FileMode.OpenOrCreate))
            {
                using (var zipArch = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    zipArch.ZipDirectory(Configuration.AppSettings["Output Folder"],
                        x => (x.Contains(".filter") && !x.ToLower().Contains("unnamed") && !x.ToLower().Contains("copy") && !x.ToLower().Contains("seed")), 
                        x => (x.Contains("STYLE") || x.Contains("Console")));
                }
            }

            Process.Start(Configuration.AppSettings["Output Folder"]);
            LoggingFacade.LogInfo("Succesfully Archived Files!");
        }

        private void LoadMetaFilter(object sender, RoutedEventArgs e)
        {
            var outputFolder = Configuration.AppSettings["Meta Filter Path"];

            LoggingFacade.LogInfo($"Loading Meta Filter: {outputFolder}");

            this.FilterExoFacade.RawMetaFilterText = FileWork.ReadLinesFromFile(outputFolder);
            var output = this.FilterExoFacade.Execute();

            GenerationOptions.TextSources = output;
            EventGrid.Publish();

            if (this.FilterRawString == null || this.FilterRawString.Count < 4500)
            {
                LoggingFacade.LogWarning($"Loading Filter: Meta-Filter Content Suspiciously Short");
            }
        }
    }
}
