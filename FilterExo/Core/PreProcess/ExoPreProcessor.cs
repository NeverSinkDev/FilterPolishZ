﻿using FilterCore;
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
            WriteCursor = new ExoBlock {Type = FilterExoConfig.ExoFilterType.root, Name = "ROOT"};

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

            
            void DoWorkOnReadChild(StructureExpr readChild)
            {
                // LOCAL: We skip the lowest level and instead treat them within sections.
                if (readChild.Mode == FilterExoConfig.StructurizerMode.atom)
                {
                    return;
                }

                // LOCAL: We skip the lowest level and instead treat them within sections.
                if (readChild?.PrimitiveValue?.type == FilterExoConfig.TokenizerMode.comment)
                {
                    return;
                }

                // explicit scope handling
                if (readChild.ScopeType == FilterExoConfig.StructurizerScopeType.expl)
                {
                    if (!readChild.IsSection()) return;

                    var child = new ExoBlock();
                    child.Parent = this.WriteCursor;
                    WriteCursor.Scopes.Add(child);
                    WriteCursor = child;
                    WriteCursor.Name = readChild.PropertyExpression[1].Value;

                    ExpressionMutatorUtil.ExpandBlockWithMutators(child, readChild.PropertyExpression, "mutator");

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

                        if (item.Mode == FilterExoConfig.StructurizerMode.comm)
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
