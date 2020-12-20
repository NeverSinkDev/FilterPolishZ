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
    public class ExoProcessor
    {
        FilterEntryBuilder builder = new FilterEntryBuilder();
        List<FilterEntry> results = new List<FilterEntry>();

        public List<FilterEntry> Execute(ExoFilter exoFilter)
        {
            results.Clear();
            builder = new FilterEntryBuilder();

            ProcessTreeStep(exoFilter.RootEntry);

            // DO WORK;
            void ProcessTreeStep(ExoBlock cursor)
            {
                foreach (var readChild in cursor.Scopes)
                {
                    // Comments
                    if (readChild.SimpleComments.Count > 0 || readChild.Commands.Count > 0)
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

            //FilterEntry CreateEntryFromChild(ExoBlock readChild)
            //{
            //    FilterEntry entry = null;
                
            //    var lines = new List<IFilterLine>();
            //    foreach (var comm in readChild.ResolveAndSerialize())
            //    {
            //        var line = comm.ToFilterLine();
            //        lines.Add(line);
            //    }

            //    var metaExpression = string.Join(" ", readChild.YieldMetaTags().Select(x => x.GetRawValue()));

            //    if (metaExpression != string.Empty)
            //    {
            //        metaExpression = " # " + metaExpression;
            //    }

            //    switch (readChild.DescriptorCommand)
            //    {
            //        case "Hide":
            //        case "Show":
            //            entry = FilterEntry.CreateDataEntry(readChild.DescriptorCommand + metaExpression);
            //            break;
            //        case "Cont":
            //            entry = FilterEntry.CreateDataEntry("Show" + metaExpression);
            //            entry.Content.Add("Continue".ToFilterLine());
            //            break;
            //        case "Conh":
            //            entry = FilterEntry.CreateDataEntry("Hide" + metaExpression);
            //            entry.Content.Add("Continue".ToFilterLine());
            //            break;
            //        default:
            //            TraceUtility.Throw("Unknown rule type?!");
            //            break;
            //    }

            //    foreach (var item in lines)
            //    {
            //        entry.Content.Add(item);
            //    }

            //    return entry;
            //}

            //FilterEntry CreateCommentFromChild(ExoBlock readChild)
            //{
            //    var entry = FilterEntry.CreateCommentEntry();

            //    foreach (var comm in readChild.ResolveAndSerialize())
            //    {
            //        var line = comm.ToFilterLine();
            //        entry.Content.Add(line);
            //    }

            //    return entry;
            //}

            return results;
        }

        private void HandleChild(ExoBlock readChild)
        {
            builder.Reset();
            builder.RestoreInitialValues();

            if (readChild.Commands.Count > 0 || readChild.Mutators.Count > 0)
            {
                readChild.ResolveAndSerialize().ForEach(x => builder.AddCommand(x));
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
