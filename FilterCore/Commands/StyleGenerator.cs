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
        private Filter filter;
        private Dictionary<string, Dictionary<string, string>> styleData;
        
        public StyleGenerator(Filter filter, string styleFilePath)
        {
            this.filter = filter;
            this.styleData = new StyleSheetParser(styleFilePath).Parse();
        }

        public StyleGenerator(Filter filter, IEnumerable<string> styleData)
        {
            this.filter = filter;
            this.styleData = new StyleSheetParser(styleData).Parse();
        }

        public void Apply()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (entry.Header.Type != FilterConstants.FilterEntryType.Content)
                {
                    continue;
                }

                foreach (var ident in this.styleData.Keys)
                {
                    var line = entry.Content.GetFirst(ident);
                    if (line == null) continue;
                    if (string.IsNullOrEmpty(line.Comment)) continue;
                    if (!this.styleData[ident].ContainsKey(line.Comment)) continue;

                    var newValue = this.styleData[ident][line.Comment];

                    switch (line.Value)
                    {
                        case ColorValueContainer colorVal:
                            var rgbVals = newValue.Split(' ').Select(x => short.Parse(x)).ToList();
                            colorVal.R = rgbVals[0];
                            colorVal.G = rgbVals[1];
                            colorVal.B = rgbVals[2];
                            if (rgbVals.Count == 4) colorVal.O = rgbVals[3];
                            else colorVal.O = -1;
                            break;
                        
                        case VariableValueContainer value: // todo: test sounds + add support for custom alert sounds
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

            public StyleSheetParser(IEnumerable<string> styleContent)
            {
                this.lineList = styleContent;
            }

            private readonly IEnumerable<string> lineList;
            private const string StartKey = "START: ";
            private const string CommentStartKey = "// #";
            private string currentIdent;

            public Dictionary<string, Dictionary<string, string>> Parse()
            {
                //                            ident            styleName  newValue
                var result = new Dictionary<string, Dictionary<string, string>>();

                foreach (var line in this.lineList)
                {
                    if (line.Trim() == "") continue;

                    if (line.StartsWith(StartKey))
                    {
                        var capsIdent = line.Substring(StartKey.Length);
                        var ident = FilterConstants.LineTypes.Keys.First(x => string.Equals(capsIdent, x, StringComparison.CurrentCultureIgnoreCase));
                        this.currentIdent = ident;
                        result.Add(ident, new Dictionary<string, string>());
                        continue;
                    }

                    var expectedLineStart = this.currentIdent + " ";
                    if (!line.StartsWith(expectedLineStart)) throw new Exception("styleSheet parse error");

                    var commentIndex = line.IndexOf(CommentStartKey, StringComparison.Ordinal);
                    var valueString = line.Substring(expectedLineStart.Length, commentIndex - expectedLineStart.Length).Trim();
                    var comment = line.Substring(commentIndex + CommentStartKey.Length);
//                    var styleName = comment.Substring(comment.IndexOf(":", StringComparison.Ordinal)+1).Trim();
                    
                    // todo: the comment in the line is actually the FULL comment, not just the styleName
                    // so we're gonna use/save that for now, because why not?
                    var styleName = comment;

                    if (result[this.currentIdent].ContainsKey(styleName))
                    {
                        Console.WriteLine("ERROR: duplicate style name!");
                        continue;
                    }
                    
                    result[this.currentIdent].Add(styleName, valueString);
                }
                
                // cleanUp unused idents
                result.Keys.Where(x => !result.ContainsKey(x)).ToList().ForEach(x => result.Remove(x));

                return result;
            }
        }
    }
}