using FilterPolishZ.Domain.WindowType;
using FilterPolishZ.ModuleWindows.Configuration;
using FilterPolishZ.ModuleWindows.GenerationOptions;
using FilterPolishZ.ModuleWindows.ItemInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new ToolWindow("ItemInfo", new ItemInfoView()),
                new ToolWindow("Generation Options", new GenerationOptions())
            };
        }
    }
}
