using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FilterCore;
using FilterCore.Commands;
using FilterCore.Constants;
using FilterCore.Utility;
using FilterPolishUtil;
using FilterPolishUtil.Model;

namespace FilterPolishZ.Util
{
    public static class FilterWriter
    {
        public static async Task WriteFilter(Filter baseFilter, bool isGeneratingStylesAndSeed, string outputFolder, string styleSheetFolderPath)
        {
            new FilterTableOfContentsCreator(baseFilter).Run();

            const string filterName = "NeverSink's";
            var generationTasks = new List<Task>();
            var seedFilterString = baseFilter.Serialize();

            if (isGeneratingStylesAndSeed)
            {
                var seedPath = outputFolder + "\\ADDITIONAL-FILES\\SeedFilter\\";
                if (!Directory.Exists(seedPath)) Directory.CreateDirectory(seedPath);
                seedPath += filterName + " filter - SEED (SeedFilter) .filter";
                generationTasks.Add(FileWork.WriteTextAsync(seedPath, seedFilterString));
            }
            
            baseFilter = new Filter(seedFilterString); // we do not want to edit the seedFilter directly and execute its tag commands
            try { baseFilter.ExecuteCommandTags(); } catch (Exception e) { throw e; }
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

            await Task.WhenAll(generationTasks).ConfigureAwait(false);
            LoggingFacade.LogInfo("Filter generation successfully done!", true);

            // local func
            async Task GenerateFilter_Inner(string style, int strictnessIndex, int? consoleStrictness = null, string explicitName = null)
            {
                var filePath = outputFolder;
                var fileName = filterName + " filter - " + strictnessIndex + "-" + FilterGenerationConfig.FilterStrictnessLevels[strictnessIndex].ToUpper();
                var filter = new Filter(baseFilterString);

                LoggingFacade.LogDebug($"GENERATING: {fileName}:{style}");

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
                LoggingFacade.LogDebug($"DONE GENERATING: {fileName}");
            }
        }
    }
}