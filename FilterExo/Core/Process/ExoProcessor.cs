using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Core.Process
{
    public class ExoProcessor
    {
        public List<string> Execute(ExoFilter exoFilter)
        {
            List<string> results = new List<string>();

            ProcessTreeStep(exoFilter.RootEntry);

            // DO WORK;
            void ProcessTreeStep(ExoFilterEntry cursor)
            {
                foreach (var readChild in cursor.Entries)
                {
                    DoWorkOnReadChild(readChild);

                    if (readChild.Entries.Count > 0)
                    {
                        ProcessTreeStep(readChild);
                    }
                }
            }

            void DoWorkOnReadChild(ExoFilterEntry readChild)
            {
                results.AddRange(readChild.FormattedValue.Serialize());
                // FIND RULES. EXECUTE THEM
                if  (false) // NOT RULE
                {
                    return;
                }

                // var localResults = rule.command.execute();
                // results.AddRange(localResults);
            }

            return results;
        }
    }
}
