using FilterCore.Constants;
using FilterCore.Line.LineStrategy;
using FilterDomain.LineStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FilterCore.FilterGenerationConfig;

namespace FilterCore.Line.Parsing
{
    public static class LineParser
    {
        public static IFilterLine GenerateFilterLine(List<LineToken> tokens)
        {
            FilterLineIdentPhase phase = FilterLineIdentPhase.IdentScan;

            IFilterLine result;

            string ident = string.Empty;
            bool identCommented = false;
            List<LineToken> value = new List<LineToken>();
            string comment = string.Empty;

            foreach (var token in tokens)
            {
                switch (phase)
                {
                    case FilterLineIdentPhase.IdentScan:
                        {
                            if (token.isIdent)
                            {
                                ident = token.value;

                                if (token.isCommented)
                                {
                                    identCommented = true;
                                }
                            }
                            else
                            {
                                comment = token.value;
                                if (tokens.Count > 1) comment += " ";
                            }

                            phase = FilterLineIdentPhase.ValueScan;
                        }
                        break;

                    case FilterLineIdentPhase.ValueScan:
                        {
                            if (token.isFluffComment == true)
                            {
                                comment += token.value;
                            }
                            else
                            {
                                value.Add(token);
                            }
                        }
                        break;
                }
            }

            if (ident != string.Empty)
            {
                result = FilterGenerationConfig.LineTypes[ident].Construct(ident, value);
            }
            else
            {
                result = new FilterLine<EmptyValueContainer>();
            }

            result.Comment = comment;
            result.identCommented = identCommented;
            return result;
        }

        public static List<LineToken> TokenizeFilterLineString(string input)
        {
            // Initialization
            bool quoteMode = false;
            bool firstIdentTest = true;

            LineParsingCommentState CommentState = LineParsingCommentState.None;

            string currentString = "";

            LineToken currentToken = new LineToken();
            List<LineToken> tokens = new List<LineToken>();



            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];

                // Handle inComments
                if (CommentState == LineParsingCommentState.Comment)
                {
                    currentString += currentChar;
                    continue;
                }

                // Handle inQuotes
                if (quoteMode)
                {
                    if (currentChar == '"')
                    {
                        quoteMode = false;
                        AddCurrentToken();
                    }
                    else
                    {
                        currentString += currentChar;
                    }
                    continue;
                }

                switch(currentChar)
                {
                    case '#':
                        AddCurrentToken();
                        if (CommentState == LineParsingCommentState.None && firstIdentTest == true)
                        {
                            CommentState = LineParsingCommentState.Testing;
                        }
                        else
                        {
                            CommentState = LineParsingCommentState.Comment;
                        }
                        break;

                    case '"':
                        quoteMode = true;
                        currentToken.isQuoted = true;
                        break;

                    case ' ':
                    case '\t':
                        {
                            TestForIdentToken();
                            AddCurrentToken();
                        }
                        break;

                    default:
                        currentString += currentChar;
                        break;

                }

            }

            if (tokens.Count == 0)
            {
                TestForIdentToken();
            }

            AddCurrentToken();

            if (tokens.Count == 0 && CommentState != LineParsingCommentState.None)
            {
                tokens.Add(LineToken.CreateFluffComment(" "));
            }

            return tokens;

            // Local functions
            void TestForIdentToken()
            {
                if (firstIdentTest)
                {
                    var trimmed = currentString.Trim();
                    if (trimmed.Length > 0)
                    {
                        if (CommentState == LineParsingCommentState.Testing)
                        {
                            if (FilterGenerationConfig.LineTypes.ContainsKey(trimmed))
                            {
                                currentString = trimmed;
                                currentToken.isIdent = true;
                                CommentState = LineParsingCommentState.Value;
                            }
                            else
                            {
                                CommentState = LineParsingCommentState.Comment;
                            }
                        }
                        else
                        {
                            if (FilterGenerationConfig.LineTypes.ContainsKey(trimmed))
                            {
                                currentToken.isIdent = true;
                            }
                            else
                            {
                                throw new Exception("Unknown Ident during Check Phase: " + currentString);
                            }
                        }

                        firstIdentTest = false;
                    }
                }
            }

            void AddCurrentToken()
            {
                if (currentString != string.Empty)
                {
                    if (CommentState == LineParsingCommentState.Value)
                    {
                        currentToken.isCommented = true;
                    }

                    if (CommentState == LineParsingCommentState.Comment)
                    {
                        currentToken.isFluffComment = true;
                        currentToken.isCommented = true;
                    }

                    currentToken.value = currentString;
                    tokens.Add(currentToken);

                    currentString = string.Empty;
                    currentToken = new LineToken();
                }
            };
        }

        public enum LineParsingCommentState
        {
            None,
            Value,
            Testing,
            Comment,
        }
    }
}
