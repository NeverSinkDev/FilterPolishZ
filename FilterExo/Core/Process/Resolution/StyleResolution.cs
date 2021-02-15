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
    public static class StyleResolution
    {
        // find all relevant styles for the matching rule
        public static List<List<string>> Execute(ExoBlock block, ExoStyleDictionary style)
        {
            var result = new List<List<string>>();

            // Test if the FILTER part of the APPLY (style => filter) command matches, the current rule, find matching styles.
            var relevantRules = style.Rules.Where(x => x.IsMatchForRule(block)).ToList();

            // Style matching has 2 parts, in the second part we do the actual style matching and apply it onto the rule
            foreach (var styleRule in relevantRules)
            {
                return styleRule.Apply(block);
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
        public string OperationName;
        public ExoExpressionCommand Caller;

        // Test if the FILTER part of the APPLY (style => filter) command matches, the current rule
        public bool IsMatchForRule(ExoBlock currentRule)
        {
            var currentRuleName = currentRule.Name.ToLower();          // rulename: "t1"
            var parentSectionName = currentRule.Parent.Name.ToLower(); // sectionName: "IncubatorStyle"
            var parents = currentRule.YieldParentNames(parentSectionName);

            var parentMatch = parents.Any(x => Regex.IsMatch(x.ToLower(), this.FilterSection.SectionMatch));
            bool ruleMatch = true;

            if (this.FilterSection.Rule != "")
            {
                ruleMatch = Regex.IsMatch(currentRuleName, this.FilterSection.RuleMatch);
            }

            return parentMatch && ruleMatch;
        }

        public List<List<string>> Apply(ExoBlock block)
        {
            var globalStyles = ExoStyleProcessor.WorkedStyleFile;
            var results = new List<List<string>>();

            var filterRuleName = block.Name.ToLower();

            foreach (var style in this.StyleSection)
            {
                List<ExoFunction> functions = new List<ExoFunction>();

                    var mode = style.Mode;
                    // if the metafilter and the style both have a rulename specified perform a force application
                    if (style.Rule != "" && this.FilterSection.Rule != "")
                    {
                        mode = ExoStyleSearchMode.forced;
                    }

                    var exoBlock = style.Parent;
                    if (mode == ExoStyleSearchMode.match)
                    {
                        if (style.Rule == "")
                        {
                            // if the style section has no rule specified, perform an exact name match
                            functions = exoBlock.YieldLinkedFunctionsRegex(filterRuleName).ToList();
                        }
                        else
                        {
                            // if neither has a rule specified, apply styles to whatever filterchild matches
                            var relevantForStyle = exoBlock.YieldLinkedFunctionsRegex(style.RuleMatch);
                            var relevantForFilter = exoBlock.YieldLinkedFunctionsRegex(filterRuleName);

                            functions.AddRange(relevantForStyle.Intersect(relevantForFilter));

                            if (this.FilterSection.Rule == "")
                            {
                                functions.Where(x => Regex.IsMatch(filterRuleName, style.RuleMatch));
                            }
                        }
                    }
                    // if the mode is forced, we apply the/any style to the rule
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

                    results.AddRange(ApplyInner(block, functions, style.Parent));
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
