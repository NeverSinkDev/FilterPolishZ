using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FilterExo.Core.Structure
{
    public class StructurizerDebugger
    {
        public string Execute(StructureExpr tree)
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
                    props += string.Join(" ", level.Properties["descriptor"].Select(x => x.PrimitiveValue.value));
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
