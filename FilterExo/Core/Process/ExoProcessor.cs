using FilterCore.Entry;
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
                    DoWorkOnReadChild(readChild);

                    if (readChild.Scopes.Count > 0)
                    {
                        ProcessTreeStep(readChild);
                    }
                }
            }

            void DoWorkOnReadChild(ExoBlock readChild)
            {
                results.Add(readChild.ResolveAndSerialize());
            }

            return results;
        }
    }
}
