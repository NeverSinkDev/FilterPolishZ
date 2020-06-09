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

            // cursor = cursor.AddAndScopeOnChild(StructureExpr.CreateScope(FilterExoConfig.StructurizerScopeType.expl));

            // ---
            for (tokenLine = 0; tokenLine < tokens.Count; tokenLine++)
            {
                var currentLine = tokens[tokenLine];

                // single line level
                for (tokenCol = 0; tokenCol < currentLine.Count; tokenCol++)
                {
                    DecideOnExpressionTreatment(currentLine[tokenCol]);
                }

                TreatLineEnd("eol");
            }

            void DecideOnExpressionTreatment(ExoToken token)
            {
                if (token.IsOperator)
                {
                    if (SpecialCharacterTreatment(token)) return;
                }

                if (cursor.Mode == StructurizerMode.root || cursor.ScopeType != StructurizerScopeType.impl)
                {
                    var child = StructureExpr.CreateScope(StructurizerScopeType.impl);
                    cursor = cursor.AddAndScopeOnChild(child);
                }
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
                    TreatLineEnd(";");
                    return true;
                }

                if (token.value == "{")
                {
                    cursor= cursor.ScopeOnLastImplicit();

                    // Detach children from parent
                    var children = cursor.Children.ToList();
                    cursor.Children = new List<StructureExpr>();

                    // Create new scope
                    var scope = StructureExpr.CreateScope(StructurizerScopeType.expl, token);
                    scope.Parent = cursor;
                    cursor.Children.Add(scope);

                    // Add children as descriptors
                    scope.Properties.Add("descriptor", children);
                    cursor = scope;

                    // Add an implicit listing
                    //var child = StructureExpr.CreateScope(StructurizerScopeType.impl);
                    //cursor = cursor.AddAndScopeOnChild(child);

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

            void TreatLineEnd(string trigger)
            {
                cursor = cursor.GetExplParent();
                // cursor = cursor.ScopeOnLastChild();
            }

            // TODO: go to root.
            return cursor.GoToRoot();
        }
    }
}
