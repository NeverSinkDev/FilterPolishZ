using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FilterCore;
using FilterCore.Commands;
using FilterCore.Constants;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using FilterPolishWindowUtils;
using FilterPolishZ.Configuration;

namespace FilterPolishZ.Util
{
    public static class FilterWriter
    {
        public static LocalConfiguration Configuration { get; set; } = LocalConfiguration.GetInstance();
        
        public static async Task WriteFilter(Filter baseFilter, bool isGeneratingStylesAndSeed, string outputFolder = null)
        {
            LoggingFacade.LogInfo($"STARTING: FILTER GENERATION");

            var isStopping = VerifyFilter(baseFilter);
            if (isStopping) return;
            
            new FilterTableOfContentsCreator(baseFilter).Run();

            const string filterName = "NeverSink's";
            if (outputFolder == null) outputFolder = Configuration.AppSettings["Output Folder"];
            var styleSheetFolderPath = Configuration.AppSettings["StyleSheet Folder"];
            var generationTasks = new List<Task>();
            var seedFilterString = baseFilter.Serialize();

            if (isGeneratingStylesAndSeed)
            {
                var seedPath = outputFolder + "ADDITIONAL-FILES\\SeedFilter\\";
                if (!Directory.Exists(seedPath)) Directory.CreateDirectory(seedPath);
                seedPath += filterName + " filter - SEED (SeedFilter) .filter";
                generationTasks.Add(FileWork.WriteTextAsync(seedPath, seedFilterString));
            }
            
            baseFilter = new Filter(seedFilterString); // we do not want to edit the seedFilter directly and execute its tag commands
            baseFilter.ExecuteCommandTags();
            var baseFilterString = baseFilter.Serialize();
            if (baseFilterString == null || baseFilterString.Count < 4500) LoggingFacade.LogError("Warning: (seed) filter result line count: " + baseFilterString?.Count);
            
            for (var strictnessIndex = 0; strictnessIndex < FilterGenerationConfig.FilterStrictnessLevels.Count; strictnessIndex++)
            {
                if (isGeneratingStylesAndSeed)
                {
                    foreach (var style in FilterGenerationConfig.FilterStyles)
                    {
                        if (style.ToLower() == "default" || style.ToLower() == "backup" || style.ToLower() == "streamsound") continue;
                        generationTasks.Add(GenerateFilter_Inner(style, strictnessIndex));
                    }
                }

                // default style
                generationTasks.Add(GenerateFilter_Inner("", strictnessIndex));
            }

            if (isGeneratingStylesAndSeed)
            {
                generationTasks.Add(GenerateFilter_Inner("", 3, 2, explicitName: "NeverSink's filter - 1X-ConStrict"));
                generationTasks.Add(GenerateFilter_Inner("", 4, 2, explicitName: "NeverSink's filter - 2X-ConStrict"));
                generationTasks.Add(GenerateFilter_Inner("", 4, 3, explicitName: "NeverSink's filter - 3X-ConStrict"));
                generationTasks.Add(GenerateFilter_Inner("", 5, 3, explicitName: "NeverSink's filter - 4X-ConStrict"));
                generationTasks.Add(GenerateFilter_Inner("", 6, 3, explicitName: "NeverSink's filter - 5X-ConStrict"));
            }

            await Task.WhenAll(generationTasks);
            LoggingFacade.LogInfo("Filter generation successfully done!", true);

            // local func
            async Task GenerateFilter_Inner(string style, int strictnessIndex, int? consoleStrictness = null, string explicitName = null)
            {
                var filePath = outputFolder;
                var fileName = filterName + " filter - " + strictnessIndex + "-" + FilterGenerationConfig.FilterStrictnessLevels[strictnessIndex].ToUpper();
                var filter = new Filter(baseFilterString);

                LoggingFacade.LogDebug($"GENERATING: {fileName}");

                new FilterTableOfContentsCreator(filter).Run();
                filter.ExecuteStrictnessCommands(strictnessIndex, consoleStrictness);

                if (style != "")
                {
                    new StyleGenerator(filter, styleSheetFolderPath + style + ".fsty", style).Apply();
                    filePath += "(STYLE) " + style.ToUpper() + "\\";
                    fileName += " (" + style + ") ";
                }

                if (consoleStrictness.HasValue)
                {
                    // little dirty fix
                    filePath += "(Console-Strictness)\\";
                    fileName += " Console-Strictness ";
                }

                if (explicitName != null)
                {
                    fileName = explicitName;
                }

                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                
                var result = filter.Serialize();

                if (result.Count <= seedFilterString?.Count)
                {
                    LoggingFacade.LogError("Error: style/strictness variant is smaller size than seed");
                }

                await FileWork.WriteTextAsync(filePath + "\\" + fileName + ".filter", result);
                LoggingFacade.LogInfo($"DONE GENERATING: {fileName}");
            }
        }
        
        private static bool VerifyFilter(Filter baseFilter)
        {
            var errorMsg = new List<string>();
            
            var oldSeedVersion = baseFilter.GetHeaderMetaData("version:");
            var newVersion = LocalConfiguration.GetInstance().YieldConfiguration().First(x => x.Key == "Version Number").Value;
            if (oldSeedVersion == newVersion)
            {
                errorMsg.Add("Version did not change!");
            }
            else baseFilter.SetHeaderMetaData("version:", newVersion);

            // add missing UP command tags // currently unused/unnecessary plus bug: trinkets/amulets/... should not be affected by this!!
//            foreach (var entry in baseFilter.FilterEntries)
//            {
//                if (entry.Header.Type != FilterConstants.FilterEntryType.Content) continue;
//                
//                if (!(entry?.Content?.Content?.ContainsKey("ItemLevel") ?? false)) continue;
//                if (entry.Content.Content["ItemLevel"]?.Count != 1) continue;
//                var ilvl = entry.Content.Content["ItemLevel"].Single().Value as NumericValueContainer;
//                if (ilvl == null) continue;
//                if (ilvl.Value != "65" || ilvl.Operator != ">=") continue;
//                
//                if (!(entry?.Content?.Content?.ContainsKey("SetTextColor") ?? false)) continue;
//                
//                if (entry.Header.HeaderValue == "Hide") continue;
//                
//                if (!entry.Content.Content.ContainsKey("Rarity")) continue;
//                var rarity = entry.Content.Content["Rarity"].Single().Value as NumericValueContainer;
//                if (rarity.Value != "Rare") continue;
//                if (!string.IsNullOrEmpty(rarity.Operator))
//                {
//                    if (rarity.Operator.Contains("<") || rarity.Operator.Contains(">")) continue;                    
//                }
//                
//                if (entry.Header.GenerationTags.Any(tag => tag is RaresUpEntryCommand)) continue;
//                
//                InfoPopUpMessageDisplay.ShowInfoMessageBox("Adding UP tag to this entry:\n\n\n" + string.Join("\n", entry.Serialize()));
//                entry.Header.GenerationTags.Add(new RaresUpEntryCommand(entry as FilterEntry) { Value = "UP", Strictness = -1});
//            }
            
//            FilterStyleVerifyer.Run(baseFilter); // todo: re-enable this when the filter doesnt have the tons of errors anymore

            if (errorMsg.Count > 0)
            {
                var isStopping = !InfoPopUpMessageDisplay.DisplayQuestionMessageBox("Error: \n\n" + string.Join("\n", errorMsg) + "\n\nDo you want to continue the filter generation?");
                return isStopping;
            }

            return false;
        }
    }
}