using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Core.Structure
{
    public class StructurizerDebugger
    {
        public Dictionary<string,T> SelectOnTree<T>(StructureExpr tree, Func<StructureExpr, T> transform)
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            // propagation function

            RecurseInto(tree, string.Empty);

            void RecurseInto(StructureExpr branch, string loc)
            {
                // get location identification function
                if (branch.Mode == FilterExoConfig.StructurizerMode.root)
                {
                    loc = "r";
                }
                else
                {
                    loc += "." + branch.Parent.Children.IndexOf(branch);
                }

                var res = transform(branch);
                dict.Add(loc, res);

                // recurse into children
                foreach (var leaf in branch.Children)
                {
                    RecurseInto(leaf, loc);
                }
            }

            return dict;
        }

        public string CreateTreeString(StructureExpr tree)
        {
            var result = new List<string>();

            TreatProps(tree);

            void TreatProps(StructureExpr level)
            {
                var meta = BuildMetaString(level);
                bool atomFound = false;

                var lvlstr = "";
                int depth = level.CalculateDepth();

                for (int i = 0; i < depth; i++)
                {
                    lvlstr += "\t";
                }

                string props = " ";
                if (level.Properties.ContainsKey("descriptor"))
                {
                    props += string.Join(" ", level.Properties["descriptor"].Select(x => x.PrimitiveValue?.value ?? "NO PRIM VALUE"));
                }

                result.Add(lvlstr + meta + " " + level?.PrimitiveValue?.value + props);

                foreach (var item in level.Children)
                {
                    if (item.Parent != level)
                    {
                        result[result.Count - 1] += "WRONG PARENT!!!";
                    }

                    if (item.Mode == FilterExoConfig.StructurizerMode.atom)
                    {
                        if (!atomFound)
                        {
                            var metaString2 = lvlstr + "\t" + BuildMetaString(item);
                            result.Add(metaString2);
                        }
                        atomFound = true;
                        result[result.Count - 1] += " " + item.Value;
                    }
                    else
                    {
                        TreatProps(item);
                    }


                }
            }

            string BuildMetaString(StructureExpr level)
            {
                return $"{level.Mode.ToString()} {level.ScopeType}";
            }

            return string.Join(System.Environment.NewLine, result);
        }

        
    }
}
