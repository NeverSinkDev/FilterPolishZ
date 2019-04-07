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
        private async Task WriteFilter(Filter baseFilter, bool isGeneratingStyles, bool isGeneratingSeedFilterOnly, string outputFolder, string filterName = "NeverSink's")
        {
            var isStopping = this.VerifyFilter(baseFilter);
            if (isStopping) return;
            
            if (outputFolder == null) outputFolder = Configuration.AppSettings["Output Folder"];
            var styleSheetFolderPath = Configuration.AppSettings["StyleSheet Folder"];
            var generationTasks = new List<Task>();
            
            var seedFilterString = baseFilter.Serialize();
            await FileWork.WriteTextAsync(outputFolder + "\\ADDITIONAL-FILES\\SeedFilter\\seed.filter", seedFilterString);
            if (isGeneratingSeedFilterOnly) return;

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
            var errorMsg = new List<string>();
            
            var oldSeedVersion = baseFilter.GetVersion();
            var newVersion = LocalConfiguration.GetInstance().YieldConfiguration().First(x => x.Key == "Version Number").Value;
            if (oldSeedVersion == newVersion)
            {
                errorMsg.Add("Version did not change!");
            }
            else baseFilter.SetVersion(newVersion);
            
            // todo: style tags -> autoFix if possible, else give user option for quick fix
            var styleDic = new Dictionary<string, Dictionary<string, string>>();
            var styleIdents = new List<string> { "SetTextColor" }; // todo: outsource to constants?
            var errorEntries = new Dictionary<string, HashSet<IFilterEntry>>();
            var unhandledLines = new HashSet<IFilterLine>();
            styleIdents.ForEach(ident => styleDic.Add(ident, new Dictionary<string, string>()));
            styleIdents.ForEach(ident => errorEntries.Add(ident, new HashSet<IFilterEntry>()));
            
            CheckStyleNames();
            CheckStyleNames(); // 2nd run in case an early entry is missing style names that were only found later

            void CheckStyleNames()
            {
                foreach (var entry in baseFilter.FilterEntries)
                {
                    if (entry.Header.Type != FilterConstants.FilterEntryType.Content) continue;

                    foreach (var ident in styleIdents)
                    {
                        if (!entry.Content.Content.ContainsKey(ident)) continue;
                        var line = entry.Content.Content[ident].First();
                        var value = line.Value.Serialize();
                        var name = line.Comment;
                        
                        // style exists -> compare names
                        if (styleDic[ident].ContainsValue(value))
                        {
                            var expKey = styleDic[ident].Single(x => x.Value.Equals(value)).Key;
                            if (name != expKey)
                            {
                                if (!errorEntries[ident].Contains(entry))
                                {
                                    errorMsg.Add("unexpected style name for : " + value + "\nexpected: " + expKey + "\nactual: " + name + "\nentry header: " + entry.Header.Serialize() + "\n");
                                    errorEntries[ident].Add(entry);
                                }
                            }
                        }
                        
                        else if (!string.IsNullOrEmpty(name))
                        {
                            // name exists -> compare values
                            if (styleDic[ident].ContainsKey(name))
                            {
                                if (styleDic[ident][name] != value)
                                {
                                    if (!errorEntries[ident].Contains(entry))
                                    {
                                        errorMsg.Add("Style name difference for: " + name + "\n" + styleDic[ident][name] + "\n" + value + "\nentry header: " + entry.Header.Serialize() + "\n");
                                        errorEntries[ident].Add(entry);
                                    }
                                }
                            }
                            else
                            {
                                styleDic[ident].Add(name, value);
                            }
                        }
                        
                        else
                        {
                            // only happens when the line was unhandled both times(?) -> info log
                            if (unhandledLines.Contains(line))
                            {
                                // unknown style without name -> ??? todo?
//                                errorMsg.Add("unhandled: " + line.Serialize());
                            }
                            
                            unhandledLines.Add(line);
                        }
                    }
                }
            }

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
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, true, false, null));
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
                System.IO.File.Copy(file, poeFolder + "\\" + file.Split('/', '\\').Last());
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

        private void SaveSeedFile_Unnamed(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false, true, null, "unnamed"));
        }

        private void SaveSeedFileAs(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var filePath = fd.FileName;
            
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false, true, filePath, "unnamed"));
        }

        private void GenerateFilters_NoStyles(object sender, RoutedEventArgs e)
        {
            Task.Run(() => WriteFilter(this.FilterAccessFacade.PrimaryFilter, false, false, null));
        }
    }
}
