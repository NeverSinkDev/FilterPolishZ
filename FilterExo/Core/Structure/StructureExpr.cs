using FilterExo.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using static FilterExo.FilterExoConfig;

namespace FilterExo.Core.Structure
{
    public class StructureExpr
    {
        public StructureExpr()
        {

        }

        public StructureExpr(ExoToken token)
        {
            Mode = StructurizerMode.atom;
            Value = token.value;
            this.PrimitiveValue = token;
        }

        // Properties.
        public StructurizerMode Mode { get; set; } = StructurizerMode.atom;
        public StructurizerScopeType ScopeType { get; set; } = StructurizerScopeType.none;

        // Tree-Structure
        public StructureExpr Parent { get; set; }
        public List<StructureExpr> Children { get; set; } = new List<StructureExpr>();
        public Dictionary<string, List<StructureExpr>> Properties { get; set; } = new Dictionary<string, List<StructureExpr>>();

        // Value-Properties
        public string Value { get; set; }
        public ExoToken PrimitiveValue { get; set; }

        public StructureExpr GetParent()
        {
            return this.Parent;
        }

        public StructureExpr GetExplParent()
        {
            if (this.Parent.Mode == StructurizerMode.root)
            {
                return this;
            }

            if (this.Parent.ScopeType != StructurizerScopeType.impl)
            {
                return this.Parent;
            }

            return this.Parent.GetExplParent();
        }

        public StructureExpr AddChild(StructureExpr child)
        {
            child.Parent = this;
            this.Children.Add(child);
            return this;
        }

        public int CalculateDepth(int depth = 0)
        {
            while (this.Mode != StructurizerMode.root)
            {
                return this.GetParent().CalculateDepth(depth + 1);
            }
            return depth;
        }

        public StructureExpr AddAndScopeOnChild(StructureExpr child)
        {
            child.Parent = this;
            this.Children.Add(child);
            return child;
        }

        public void DestroyLastChild()
        {
            this.Children.RemoveAt(this.Children.Count - 1);
        }

        public StructureExpr GoToRoot()
        {
            while (this.Parent.Mode != StructurizerMode.root)
            {
                return Parent.GoToRoot();
            }

            return Parent;
        }

        public static StructureExpr CreateScope(StructurizerScopeType scopeType, ExoToken token = null)
        {
            var child = new StructureExpr()
            {
                ScopeType = scopeType,
                Mode = StructurizerMode.scop,
                PrimitiveValue = token,
            };
            return child;
        }
    }
}
