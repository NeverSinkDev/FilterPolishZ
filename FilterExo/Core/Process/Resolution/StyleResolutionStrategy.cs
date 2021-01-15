using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;
using FilterPolishUtil;

namespace FilterExo.Core.Process.StyleResoluton
{
    public static class StyleResolutionStrategy
    {
        public static List<List<string>> Execute(ExoBlock currentRule, ExoStyleDictionary style)
        {
            var result = new List<List<string>>();

            // match the filter-rules (such as T1) to the style rules. Handle different application strategies here later
            var relevantRules = style.Rules.Where(x => x.IsMatch(currentRule)).ToList();

            foreach (var styleRule in relevantRules)
            {
                return styleRule.Apply(currentRule);
            }

            return result;
        }
    }

    public enum ExoStyleSearchMode
    {
        match,
        forced
    }

    public class ExoStylePieceToken
    {
        public ExoBlock Parent;

        public ExoStylePieceToken(string expression)
        {
            this.RawTarget = expression;

            if (this.RawTarget.Contains("!"))
            {
                this.Mode = ExoStyleSearchMode.forced;
                this.RawTarget = this.RawTarget.Replace("!", "");
            }

            if (this.RawTarget.Contains("."))
            {
                var pieces = this.RawTarget.Split('.');
                this.Section = pieces[0];
                this.Rule = pieces[1];

                SectionMatch = StringWork.WildCardToRegular(Section);
                RuleMatch = StringWork.WildCardToRegular(Rule);
            }
            else
            {
                this.Section = this.RawTarget;
                SectionMatch = StringWork.WildCardToRegular(Section);

                this.Rule = "";
            }
        }

        public void InitializeStylePieceParent()
        {
            this.Parent = ExoStyleProcessor.WorkedStyleFile.RootEntry.FindChildSection(this.Section).First();
        }

        public string SectionMatch;
        public string RuleMatch;

        public string Section;
        public string Rule;
        public string RawTarget;
        public List<string> CachedTargets = new List<string>();
        public ExoStyleSearchMode Mode = ExoStyleSearchMode.match;
    }

    public class ExoStylePiece
    {
        public ExoStylePieceToken FilterSection;
        public List<ExoStylePieceToken> StyleSection;
        public bool Enabled = true;
        public string ApplyRuleName = ""; // TODO
        
        public string OperationName;

        public ExoBlock Block;
        public ExoExpressionCommand Caller;

        public bool IsMatch(ExoBlock currentRule)
        {
            var currentRuleName = currentRule.Name.ToLower();          // rulename: "t1"
            var parentSectionName = currentRule.Parent.Name.ToLower(); // sectionName: "Incubators"

            var parentMatch = Regex.IsMatch(this.FilterSection.SectionMatch, parentSectionName);
            bool ruleMatch = true;

            if (this.FilterSection.Rule != "")
            {
                ruleMatch = Regex.IsMatch(this.FilterSection.RuleMatch, currentRuleName);
            }

            return parentMatch && ruleMatch;
        }

        public List<List<string>> Apply(ExoBlock currentRule)
        {
            var globalStyles = ExoStyleProcessor.WorkedStyleFile;
            var results = new List<List<string>>();

            var filterRuleName = currentRule.Name.ToLower();

            foreach (var style in this.StyleSection)
            {
                var sections = globalStyles.RootEntry.FindChildSectionRegex(style.SectionMatch).ToList();

                foreach (var exoBlock in sections)
                {
                    List<ExoFunction> functions = new List<ExoFunction>();

                    var mode = style.Mode;
                    if (style.Rule != "" && this.FilterSection.Rule != "")
                    {
                        mode = ExoStyleSearchMode.forced;
                    }

                    if (mode == ExoStyleSearchMode.match)
                    {
                        if (style.Rule == "")
                        {
                            // if the style section has no rule specified, perform an exact name match
                            functions = exoBlock.YieldLinkedFunctionsRegex(filterRuleName).ToList();
                        }
                        else
                        {
                            var relevantForStyle = exoBlock.YieldLinkedFunctionsRegex(style.RuleMatch);
                            var relevantForFilter = exoBlock.YieldLinkedFunctionsRegex(filterRuleName);

                            functions.AddRange(relevantForStyle.Intersect(relevantForFilter));

                            if (this.FilterSection.Rule == "")
                            {
                                functions.Where(x => Regex.IsMatch(filterRuleName, style.RuleMatch));
                            }

                            // Apply (IncubatorStyle.T1 =>    IncubatorFilter)
                            // Apply (IncubatorStyle.T1 => IncubatorFilter.T1)
                        }
                    }
                    else if (mode == ExoStyleSearchMode.forced)
                    {
                        if (style.Rule == "")
                        {
                            functions = exoBlock.YieldLinkedFunctionsRegex(StringWork.WildCardToRegular("*")).ToList();
                        }
                        else
                        {
                            functions = exoBlock.YieldLinkedFunctionsRegex(style.RuleMatch).ToList();
                        }
                    }

                    results.AddRange(ApplyInner(currentRule, functions, style.Parent));
                }
            }

            return results;
        }

        private IEnumerable<List<string>> ApplyInner(ExoBlock currentRule, List<ExoFunction> functions, ExoBlock parent)
        {
            var parameters = new PreProcess.Commands.Branch<ExoAtom>();
            var caller = this.Caller;

            var funcResults = new List<List<ExoAtom>>();
            foreach (var function in functions)
            {
                caller.Executor = parent;
                caller.Parent = parent;
                function.Content.Parent = parent;
                funcResults.AddRange(function.Execute(parameters, caller).ToList());
            }

            foreach (var item in funcResults)
            {
                yield return new ExoExpressionCommand(item) { Parent = parent, Executor = parent }.Serialize();
            }
        }

        public static List<ExoStylePiece> FromExpression(string stylePart, string filterPart, string op, ExoExpressionCommand caller)
        {
            var results = new List<ExoStylePiece>();

            var filterSections = filterPart.ToLower().Split(',').Select(x => x.Trim()).ToList();
            var styleSections = stylePart.ToLower().Split(',').Select(x =>
            {
                var token = new ExoStylePieceToken(x.Trim());
                token.InitializeStylePieceParent();
                return token;
            }).ToList();

            foreach (var fs in filterSections)
            {
                var piece = new ExoStylePiece()
                {
                    FilterSection = new ExoStylePieceToken(fs),
                    StyleSection = styleSections,
                    OperationName = op,
                    Caller = caller
                };

                results.Add(piece);
            }

            return results;
        }
    }
}
