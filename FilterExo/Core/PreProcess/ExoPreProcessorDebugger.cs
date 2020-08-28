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
            ProcessChildren(exo.RootEntry);
            return results;

            void ProcessChildren(ExoBlock entry)
            {
                foreach (var item in entry.Scopes)
                {
                    ProcessSingleChild(item);
                    ProcessChildren(item);
                }
            }

            void ProcessSingleChild(ExoBlock entry)
            {
                results.AddRange(entry.Debug_GetSummary());
            }
        }
    }
}
