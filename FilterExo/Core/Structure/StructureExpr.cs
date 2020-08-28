using FilterExo.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FilterExo.FilterExoConfig;

namespace FilterExo.Core.Structure
{
    public class StructureExpr
    {
        public StructureExpr()
        {

        }

        public StructureExpr(ExoToken token, StructurizerMode mode = StructurizerMode.atom)
        {
            Mode = mode;
            Value = token.value;
            this.PrimitiveValue = token;
        }

        // Properties.
        public StructurizerMode Mode { get; set; } = StructurizerMode.atom;
        public StructurizerScopeType ScopeType { get; set; } = StructurizerScopeType.none;

        // Tree-Structure
        public StructureExpr Parent { get; set; }
        public List<StructureExpr> Children { get; set; } = new List<StructureExpr>();
        public List<StructureExpr> PropertyExpression { get; set; } = new List<StructureExpr>();

        // Value-Properties
        public string Value { get; set; }
        public ExoToken PrimitiveValue { get; set; }

        public StructureExpr GetParent()
        {
            if (this.Parent == null)
            {
                return this;
            }

            return this.Parent;
        }

        public bool IsSection()
        {
            if (this.PropertyExpression.Any(x => x.Value == "Section"))
            {
                return true;
            }
            return false;
        }

        public StructureExpr GetExplParent()
        {
            if (this.Mode == StructurizerMode.root || this.ScopeType == StructurizerScopeType.expl)
            {
                return this;
            }

            return this.Parent.GetExplParent();
        }

        public StructureExpr AddChild(StructureExpr child)
        {
            child.Parent = this;
            this.Children.Add(child);
            return this;
        }

        public string GetFirstPropertyDescriptor()
        {
            var property = this.PropertyExpression.FirstOrDefault();

            if (property == null)
            {
                return string.Empty;
            }

            return property.PrimitiveValue.value;
        }

        public StructureExpr PackageAtomicChildren()
        {
            var adoptableChildren = new List<StructureExpr>();
            var count = this.Children.Count;

            for (int t = count - 1; t >= 0; t--)
            {
                if (this.Children[t].Mode == StructurizerMode.atom)
                {
                    adoptableChildren.Add(this.Children[t]);
                    this.Children.RemoveAt(t);
                }
                else
                {
                    break;
                }
            }

            if (adoptableChildren.Count == 0)
            {
                return this;
            }

            var scope = StructureExpr.CreateScope(StructurizerScopeType.impl);
            adoptableChildren.ForEach(x => x.Parent = scope);
            scope.Children.AddRange(adoptableChildren);
            scope.Parent = this;
            this.Children.Add(scope);
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

        public StructureExpr ScopeOnLastChild()
        {
            if (this.Children.Count > 0)
            {
                return this.Children.Last();
            }

            return this;
        }


        public StructureExpr ScopeOnLastImplicit()
        {
            if (this.Children.Count > 0 && this.Children.Last().ScopeType == StructurizerScopeType.impl)
            {
                return this.Children.Last().ScopeOnLastImplicit();
            }
            else
            {
                return this;
            }
        }

        public void DestroyLastChild()
        {
            this.Children.RemoveAt(this.Children.Count - 1);
        }

        public StructureExpr GoToRoot()
        {
            while (this.Mode != StructurizerMode.root)
            {
                return Parent.GoToRoot();
            }

            return this;
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
