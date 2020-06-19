using FilterCore;
using FilterExo.Core.PreProcess.FilterExtractedCommands;
using FilterExo.Core.Structure;
using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Core.PreProcess
{
    public class ExoPreProcessor
    {
        public ExoFilter Execute(StructureExpr tree)
        {
            var result = new ExoFilter();

            // transformation process definition
            StructureExpr readCursor = tree.GoToRoot();
            ExoFilterEntry writeCursor = new ExoFilterEntry();

            // call
            ProcessTreeStep(readCursor);

            result.RootEntry = writeCursor;

            // LOCAL: process the tree
            void ProcessTreeStep(StructureExpr cursor)
            {
                foreach (var readChild in cursor.Children)
                {
                    DoWorkOnReadChild(readChild);

                    if (readChild.Children.Count > 0)
                    {
                        ProcessTreeStep(readChild);
                    }
                }
            }

            // LOCAL: Perform work on write branch, by reading current step
            void DoWorkOnReadChild(StructureExpr readChild)
            {
                if (readChild.Mode == FilterExoConfig.StructurizerMode.atom)
                {
                    return;
                }

                // identify the line type
                if (readChild?.PrimitiveValue?.type == FilterExoConfig.TokenizerMode.comment)
                {
                    // treat it as comment
                    return;
                }

                // explicit scope handling
                if (readChild.ScopeType == FilterExoConfig.StructurizerScopeType.expl)
                {
                    // handle scope based on properties
                    return;
                }

                // implicit scope handling
                if (readChild.ScopeType == FilterExoConfig.StructurizerScopeType.impl)
                {
                    var builder = new CommandExtractionBuilder();
                    foreach (var item in readChild.Children)
                    {
                        builder.AddKeyWord(item);
                    }

                    var filterentry = builder.Execute();

                    if (filterentry.Content.Content.Keys.Count == 0)
                    {
                        return;
                    }

                    // perform writecursor work
                    var writeentry = new ExoFilterEntry();
                    writeentry.Parent = writeCursor;

                    writeCursor.Entries.Add(writeentry);


                    writeentry.FormattedValue = filterentry;

                    // writeCursor = writeentry.GetParent();
                }
            }

            void EvaluateExplicitScopeCommands(StructureExpr readExpr)
            {
                var properties = readExpr.Properties["descriptor"];
                var rule = properties.FirstOrDefault();
                // Func
                // Section
                // Rule
            }

            return result;
        }
    }
}
