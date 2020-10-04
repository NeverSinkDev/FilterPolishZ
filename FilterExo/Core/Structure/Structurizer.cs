using FilterExo.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static FilterExo.FilterExoConfig;

namespace FilterExo.Core.Structure
{
    public class Structurizer
    {
        public StructureExpr Execute(List<List<ExoToken>> tokens)
        {
            int tokenLine = 0;
            int tokenCol = 0;

            StructureExpr cursor = new StructureExpr();
            
            cursor.Mode = FilterExoConfig.StructurizerMode.root;
            cursor.ScopeType = FilterExoConfig.StructurizerScopeType.none;

            for (tokenLine = 0; tokenLine < tokens.Count; tokenLine++)
            {
                var currentLine = tokens[tokenLine];

                // single line level
                for (tokenCol = 0; tokenCol < currentLine.Count; tokenCol++)
                {
                    DecideOnExpressionTreatment(currentLine[tokenCol]);
                }

                TreatLineEnd();
            }

            // Single Token treatment main routine
            void DecideOnExpressionTreatment(ExoToken token)
            {
                // Handle special characters
                if (token.IsOperator)
                {
                    if (SpecialCharacterTreatment(token)) return;
                }

                else if (token.type == TokenizerMode.comment)
                {
                    cursor.AddChild(new StructureExpr(token, StructurizerMode.comm));
                    return;
                }

                // If we're in an explicit scope, we have to create a new scope before writing.
                else if (cursor.Mode == StructurizerMode.root || cursor.ScopeType == StructurizerScopeType.expl)
                {
                    var child = StructureExpr.CreateScope(StructurizerScopeType.impl);
                    cursor = cursor.AddAndScopeOnChild(child);
                }

                // currently never happens! keeping it around for safety purposes 
                else if (cursor.Children.Count > 0 && !cursor.Children.All(x => x.Mode == StructurizerMode.atom))
                {
                    cursor = cursor.PackageAtomicChildren();
                    cursor = cursor.ScopeOnLastChild();
                }

                // add token as atomic piece to the current cursor.
                cursor.AddChild(new StructureExpr(token));
            }

            bool SpecialCharacterTreatment(ExoToken token)
            {
                if (token.value == ";")
                {
                    TreatLineEnd();
                    return true;
                }

                if (token.value == "{")
                {
                    // finds the last child that contextualizes the explicit scope
                    cursor= cursor.ScopeOnLastImplicit();

                    // Detach children from parent
                    var children = cursor.Children.ToList();
                    cursor.Children = new List<StructureExpr>();

                    // Create new scope
                    var scope = StructureExpr.CreateScope(StructurizerScopeType.expl, token);
                    scope.Parent = cursor;
                    cursor.Children.Add(scope);

                    // Add children as descriptors
                    scope.PropertyExpression.AddRange(children);
                    cursor = scope;

                    return true;
                }

                if (token.value == "}")
                {
                    cursor = cursor.GetExplParent();
                    cursor = cursor.GetParent();
                    return true;
                }

                return false;
            }

            void TreatLineEnd()
            {
                cursor = cursor.GetExplParent();
            }

            // TODO: go to root.
            return cursor.GoToRoot();
        }
    }
}
