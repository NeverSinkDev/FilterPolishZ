using FilterPolishZ.Domain.WindowType;
using FilterPolishZ.ModuleWindows.BaseTypeMigrate;
using FilterPolishZ.ModuleWindows.BaseTypeTiering;
using FilterPolishZ.ModuleWindows.Configuration;
using FilterPolishZ.ModuleWindows.GenerationOptions;
using FilterPolishZ.ModuleWindows.ItemInfo;
using FilterPolishZ.ModuleWindows.Logging;
using FilterPolishZ.ModuleWindows.TagEditing;
using FilterPolishZ.ModuleWindows.TieringSuggestions;

namespace FilterPolishZ.Domain
{
    public class MainWindowViewModel
    {
        public ToolWindow[] ToolWindows { get; }

        public MainWindowViewModel()
        {
            this.ToolWindows = new[]
            {
                new ToolWindow("Configuration", new ConfigurationView()),
                new ToolWindow("Logs", new LoggingScreenView()),
                new ToolWindow("ItemInfo", new ItemInfoView()),
                new ToolWindow("Tag Editing",new TagEditorView()),
                new ToolWindow("Tiering Suggestions", new TieringSuggestionView()),
                new ToolWindow("BaseType Matrix", new BaseTypeTieringView()),
                new ToolWindow("BaseType Migration", new BaseTypeMigrateView()),
                new ToolWindow("Generation Options", new GenerationOptions())
            };
        }
    }
}
