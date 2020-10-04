using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Core.PreProcess
{
    public class ExoPreProcessorDebugger
    {
        public List<string> Execute(ExoFilter exo)
        {
            var results = new List<string>();
            ProcessBranch(exo.RootEntry);
            return results;

            void ProcessBranch(ExoBlock entry)
            {
                ProcessBlock(entry);
                foreach (var item in entry.Scopes)
                {
                    ProcessBranch(item);
                }
            }

            void ProcessBlock(ExoBlock entry)
            {
                results.AddRange(entry.Debug_GetSummary());
                results.AddRange(new List<string>());
            }
        }
    }
}
