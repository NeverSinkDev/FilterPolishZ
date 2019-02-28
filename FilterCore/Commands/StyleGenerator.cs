using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using FilterCore.Constants;
using FilterCore.Line;
using FilterDomain.LineStrategy;

namespace FilterCore.Commands
{
    public class StyleGenerator
    {
        private readonly Filter filter;
        private readonly Dictionary<string, Tuple<string, string>> styleData;
        
        public StyleGenerator(Filter filter, string styleFilePath)
        {
            this.filter = filter;
            this.styleData = new StyleSheetParser(styleFilePath).Parse();
        }

        public void Apply()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (entry.Header.Type != FilterConstants.FilterEntryType.Content)
                {
                    continue;
                }

                foreach (var styleName in this.styleData.Keys)
                {
                    var line = entry.Content.GetFirstLineWhere(x => x.Comment == styleName);
                    if (line == null) continue;

                    var newIdent = this.styleData[styleName].Item1;
                    var newValue = this.styleData[styleName].Item2;

                    line.Ident = newIdent;

                    switch (line.Value)
                    {
                        case ColorValueContainer colorVal:
                            var rgbVals = newValue.Split(' ').Select(short.Parse).ToList();
                            colorVal.R = rgbVals[0];
                            colorVal.G = rgbVals[1];
                            colorVal.B = rgbVals[2];
                            if (rgbVals.Count == 4) colorVal.O = rgbVals[3];
                            else colorVal.O = -1;
                            break;
                        
                        case VariableValueContainer value:
                            value.Value = newValue.Split(' ').Select(x => new LineToken {value = x}).ToList();
                            break;
                        
                        default:
                            throw new Exception("unexpected style type");
                    }
                }
            }
        }
        
        private class StyleSheetParser
        {
            public StyleSheetParser(string filePath)
            {
                if (!System.IO.File.Exists(filePath))
                {
                    throw new Exception("style file not found: " + filePath);
                }

                this.lineList = System.IO.File.ReadAllLines(filePath);
            }

            private readonly IEnumerable<string> lineList;
            private const string StartKey = "START: ";
            private const string CommentStartKey = "// #";

            public Dictionary<string, Tuple<string, string>> Parse()
            {
                //                         styleName     newIdent newValue
                var result = new Dictionary<string, Tuple<string, string>>();

                foreach (var line in this.lineList)
                {
                    if (line == "") continue;
                    
                    if (line.StartsWith(StartKey))
                    {
                        continue;
                    }

                    var newIdent = line.Split(' ').First();
                    var commentIndex = line.IndexOf(CommentStartKey, StringComparison.Ordinal);
                    var valueString = line.Substring(newIdent.Length + 1, commentIndex - (newIdent.Length + 1)).Trim();
                    var styleName = line.Substring(commentIndex + CommentStartKey.Length);
                    
                    if (result.ContainsKey(styleName))
                    {
                        // todo: error case!
                        continue;
                    }
                    
                    result.Add(styleName, new Tuple<string, string>(newIdent, valueString));
                }

                return result;
            }
        }
    }
}