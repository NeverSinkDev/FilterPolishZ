using FilterCore.Entry;
using FilterCore.Line;
using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FilterCore;
using FilterCore.FilterComponents.Entry;
using FilterPolishUtil;
using FilterPolishUtil.Extensions;

namespace FilterExo.Core.Process
{
    public class ExoFilterProcessor
    {
        FilterEntryBuilder builder = new FilterEntryBuilder();
        List<FilterEntry> results = new List<FilterEntry>();

        ExoStyleDictionary styleFile;

        public List<FilterEntry> Execute(ExoFilter exoFilter, ExoStyleDictionary styleFile)
        {
            results.Clear();
            builder = new FilterEntryBuilder();

            ProcessTreeStep(exoFilter.RootEntry);

            // DO WORK;
            void ProcessTreeStep(ExoBlock cursor)
            {
                foreach (var readChild in cursor.Scopes)
                {
                    // Commands
                    if (readChild.SimpleComments.Count > 0 || readChild.Commands.Count > 0 || FilterEntryBuilder.HeaderDescriptors.Contains(readChild.DescriptorCommand))
                    {
                        HandleChild(readChild);
                    }

                    // Recurse
                    if (readChild.Scopes.Count > 0)
                    {
                        ProcessTreeStep(readChild);
                    }
                }
            }

            return results;
        }

        private void HandleChild(ExoBlock readChild)
        {
            builder.Reset();
            builder.RestoreInitialValues();

            if (readChild.Commands.Count > 0 || readChild.YieldMutators().Any())
            {
                var resolvedTokens = readChild.ResolveAndSerialize(this.styleFile).ToList();
                resolvedTokens.ForEach(x => builder.AddCommand(x));
            }
            
            if (readChild.SimpleComments.Count > 0)
            {
                readChild.SimpleComments.ForEach(x => builder.AddCommand(x));
            }

            if (FilterEntryBuilder.HeaderDescriptors.Contains(readChild.DescriptorCommand))
            {
                builder.AddCommand(readChild.DescriptorCommand);
            }

            var entry = builder.Execute();
            results.Add(entry);
        }
    }
}
