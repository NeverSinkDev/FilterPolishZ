using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FilterCore.FilterComponents.Entry;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Process.StyleResoluton;
using FilterExo.Model;
using FilterPolishUtil;

namespace FilterExo.Core.Process
{
    public class ExoStyleDictionary
    {
        public List<ExoStylePiece> Rules = new List<ExoStylePiece>();
        public ExoFilter StyleExoFilter;
    }

    public class ExoStyleProcessor
    {
        public static ExoStyleDictionary ConstructedDictionary = new ExoStyleDictionary();
        public static ExoFilter WorkedStyleFile;

        public ExoStyleDictionary Execute(ExoFilter styleFile)
        {
            WorkedStyleFile = styleFile;

            ProcessTreeStep(styleFile.RootEntry);

            // DO WORK;
            void ProcessTreeStep(ExoBlock cursor)
            {
                foreach (var readChild in cursor.Scopes)
                {
                    // Commands
                    if (readChild.Commands.Count > 0)
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

            void HandleChild(ExoBlock readChild)
            {
                var resolvedTokens = readChild.ResolveAndSerializeStyle();
            }

            return ConstructedDictionary;
        }
    }
}
