using FilterCore;
using FilterExo.Core.PreProcess.Strategies;
using FilterExo.Core.Structure;
using FilterExo.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Core.PreProcess
{
    public class ExoPreProcessor
    {
        public StructureExpr ReadCursor { get; set; }
        public ExoBlock WriteCursor { get; set; }

        public ExoFilter Execute(StructureExpr tree)
        {
            var result = new ExoFilter();

            // transformation process definition
            ReadCursor = tree.GoToRoot();
            WriteCursor = new ExoBlock() { Type = FilterExoConfig.ExoFilterType.root };

            // builder information
            var builder = new ExpressionBuilder(this);

            // call
            ProcessTreeStep(ReadCursor);
            result.RootEntry = WriteCursor;

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

                this.ReadCursor = cursor;
                PerformClosingScopeResolution(cursor);
            }

            void PerformClosingScopeResolution(StructureExpr cursor)
            {
                var success = builder.Execute();

                if (cursor.IsSection())
                {
                    WriteCursor = WriteCursor.GetParent();
                }

                if (success)
                {
                    builder = new ExpressionBuilder(this);
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
                    if (readChild.IsSection())
                    {
                        var child = new ExoBlock();
                        ExpressionMutatorUtil.ExpandBlockWithMutators(child, readChild.PropertyExpression, "mutator");

                        child.Parent = this.WriteCursor;
                        WriteCursor.Scopes.Add(child);
                        WriteCursor = child;
                    }

                    return;
                }

                // implicit scope handling
                if (readChild.ScopeType == FilterExoConfig.StructurizerScopeType.impl)
                {
                    builder.AddNewPage();

                    foreach (var item in readChild.Children)
                    {
                        if (item.Mode == FilterExoConfig.StructurizerMode.atom)
                        {
                            builder.AddKeyWord(item);
                        }
                    }
                }
            }

            return result;
        }
    }
}
