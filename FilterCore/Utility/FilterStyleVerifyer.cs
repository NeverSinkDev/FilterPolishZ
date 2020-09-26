using System;
using System.Collections.Generic;
using System.Linq;
using FilterCore.Constants;
using FilterCore.Line;
using FilterCore.Line.Parsing;
using FilterPolishUtil.Extensions;

namespace FilterCore.Utility
{
    /// <summary>
    /// This class scans and verifies the different styles of the filter.
    /// a style is e.g. SetBackgroundColor 255 255 255 255 and its style name is T0 Drop
    /// This class will detect inconsistencies of style names/values and try to fix them (in case a name is simply missing)
    /// or let the user decide on the actual correct name/value of a style
    /// </summary>
    public class FilterStyleVerifyer
    {
        public static void Run(Filter filter)
        {
            FilterGenerationConfig.StyleIdents.ToList().ForEach(ident => new FilterStyleVerifyer_SingleIdent(ident).Run(filter));
        }
        
        private class FilterStyleVerifyer_SingleIdent
        {
            private readonly string ident;
            private readonly Dictionary<string, Dictionary<string, int>> valueToNamesDic = new Dictionary<string, Dictionary<string, int>>();
            private readonly Dictionary<string, Dictionary<string, int>> nameToValuesDic = new Dictionary<string, Dictionary<string, int>>();
            
            public FilterStyleVerifyer_SingleIdent(string ident)
            {
                this.ident = ident;
            }

            public void Run(Filter baseFilter)
            {
                this.ForEachLineDo(baseFilter, SaveStyleData);
                this.BuildNameToValueDic();
                
                this.FixStyleNames();
                this.FixStyleValues();
                
                this.ForEachLineDo(baseFilter, VerifyStyleValues);
                this.ForEachLineDo(baseFilter, VerifyStyleNames);
            }

            private void FixStyleNames()
            {
                foreach (var pair in valueToNamesDic.Where(x => x.Value.Count > 1))
                {
                    var index = 0;

                    while (true)
                    {
                        var msg = ident + " " + pair.Key + " has multiple style names:\n";
                        var i = 0;
                        string selectedName = null;
                        foreach (var nameData in pair.Value)
                        {
                            msg += "#" + i + ": " + nameData.Key + " (used " + nameData.Value + " times) ";
                            if (index == i)
                            {
                                msg += "<--";
                                selectedName = nameData.Key;
                            }
                            msg += " \n";
                            i++;
                        }
                        
                        // TODO: Add: "Do you want to select the name #" + index + " as style name?";
                        return;
                    }
                }
            }

            private void FixStyleValues()
            {
                foreach (var pair in nameToValuesDic.Where(x => x.Value.Count > 1))
                {
                    var index = 0;

                    while (true)
                    {
                        var msg = pair.Key + " has multiple values:\n";
                        var i = 0;
                        string selectedValue = null;
                        foreach (var valueData in pair.Value)
                        {
                            msg += "#" + i + ": " + valueData.Key + " (used " + valueData.Value + " times) ";
                            if (index == i)
                            {
                                msg += "<--";
                                selectedValue = valueData.Key;
                            }
                            msg += " \n";
                            i++;
                        }

                        // TODO: Make it conditional
                        msg += "Do you want to select value #" + index + " as style value?";
                        return;
                    }
                }
            }

            private void ForEachLineDo(Filter baseFilter, Action<string, string, IFilterLine> func)
            {
                foreach (var entry in baseFilter.FilterEntries)
                {
                    if (entry.Header.Type != FilterGenerationConfig.FilterEntryType.Content) continue;
                    if (!entry.Content.Content.ContainsKey(ident)) continue;
                    var line = entry.Content.Content[ident].Single();
                    var value = line.Value.Serialize();
                    var name = line.Comment;
                    func(value, name, line);
                }
            }

            private void SaveStyleData(string value, string name, IFilterLine line)
            {   
                if (string.IsNullOrEmpty(name)) return;
                
                if (valueToNamesDic.ContainsKey(value))
                {
                    if (valueToNamesDic[value].ContainsKey(name)) valueToNamesDic[value][name]++;
                    else valueToNamesDic[value].Add(name, 1);
                }
                else valueToNamesDic.Add(value, new Dictionary<string, int> { {name, 1} });
            }
            
            private void BuildNameToValueDic()
            {
                foreach (var pair in valueToNamesDic)
                {
                    var value = pair.Key;
                    foreach (var nameData in pair.Value)
                    {
                        var name = nameData.Key;
                        if (string.IsNullOrEmpty(name)) continue;

                        if (nameToValuesDic.ContainsKey(name))
                        {
                            if (nameToValuesDic[name].ContainsKey(value))
                            {
                                nameToValuesDic[name][value]++;
                            }
                            else
                            {
                                nameToValuesDic[name].Add(value, 1);
                            }
                        }
                        else
                        {
                            nameToValuesDic.Add(name, new Dictionary<string, int>() {{value, 1}});
                        }
                    }
                }
            }

            private void VerifyStyleNames(string value, string name, IFilterLine line)
            {
                if (this.valueToNamesDic.ContainsKey(value))
                {
                    var styleNames = this.valueToNamesDic[value];
                    var newName = styleNames.OrderByDescending(x => x.Value).First().Key;
                    if (newName == name) return;
                    line.Comment = newName;
                }

                else if (this.nameToValuesDic.ContainsKey(name))
                {
                    Console.WriteLine("todo");
//                    throw new Exception("unexpected care for styleVerifier");
                }
            }
            
            private void VerifyStyleValues(string value, string name, IFilterLine line)
            {
//                if (this.nameToValuesDic.ContainsKey(name))
//                {
//                    var newValue = this.nameToValuesDic[name].OrderByDescending(x => x.Value).First().Key;
////                    if (newValue == value) return;
//
//                    var newLine = LineParser.GenerateFilterLine(LineParser.TokenizeFilterLineString(line.Ident + " " + newValue));
//                    line.Value = newLine.Value;
//                }

                if (this.valueToNamesDic.ContainsKey(value))
                {
                    // style name was removed -> check dic for new name
                    var newName = this.valueToNamesDic[value].OrderByDescending(x => x.Value).First().Key;
                    if (newName == name) return;

                    line.Comment = newName;
                }
            
            }
        }
    }
}