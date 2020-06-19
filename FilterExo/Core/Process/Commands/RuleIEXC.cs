using FilterCore.Entry;
using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Core.Process.Commands
{
    public class RuleIEXC : IEXC
    {
        public List<string> Execute(ExoFilterEntry entry)
        {
            var result = new List<string>();
            // First ask parent to give us any matchign or important information from it's tempalte
            var info = new FilterEntry();

            info = entry.CollectEntryDataFromParent(info);
            // Then apply whatever information the atomic children have.
            info = entry.CollectEntryDataFromChildren(info);
            // Then match style (whatever fits)

            return result;
        }
    }
}
