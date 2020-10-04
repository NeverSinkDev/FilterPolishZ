using FilterCore.Entry;
using FilterCore.Line;
using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FilterExo.Core.Process
{
    public class ExoProcessor
    {
        public List<FilterEntry> Execute(ExoFilter exoFilter)
        {
            List<FilterEntry> results = new List<FilterEntry>();

            ProcessTreeStep(exoFilter.RootEntry);

            // DO WORK;
            void ProcessTreeStep(ExoBlock cursor)
            {
                foreach (var readChild in cursor.Scopes)
                {
                    if (readChild.Commands.Count > 0)
                    {
                        DoWorkOnReadChild(readChild);
                    }

                    if (readChild.Scopes.Count > 0)
                    {
                        ProcessTreeStep(readChild);
                    }
                }
            }

            void DoWorkOnReadChild(ExoBlock readChild)
            {
                var entry = FilterEntry.CreateDataEntry("Show");

                foreach (var comm in readChild.ResolveAndSerialize())
                {
                    var line = comm.ToFilterLine();
                    entry.Content.Add(line);
                }

                results.Add(entry);
            }

            return results;
        }
    }
}
