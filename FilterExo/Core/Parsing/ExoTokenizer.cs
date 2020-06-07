using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using static FilterExo.FilterExoConfig;

namespace FilterExo.Core.Parsing
{
    public class ExoTokenizer
    {
        public void Execute(List<string> input)
        {
            var mode = TokenizerMode.normal;

            int lineNumber = 0;
            int charNumber = 0;

            var results = new List<List<ExoToken>>();
            var tokenLine = new List<ExoToken>();
            var isLastWordOperator = false;

            string currentWord = string.Empty;

            // Line Parsing Section
            for (lineNumber = 0; lineNumber < input.Count; lineNumber++)
            {
                // start of new line
                var currentLine = input[lineNumber];
                tokenLine = new List<ExoToken>();

                // Character Parsing Section
                for (charNumber = 0; charNumber < currentLine.Length; charNumber++)
                {
                    char c = currentLine[charNumber];
                    DecideCharacterHandling(c);
                }

                if (mode == TokenizerMode.quoted)
                {
                    // ERROR: forgot to close quote!;
                    Debug.WriteLine($"Missing Quotes: {lineNumber}");
                }

                FinishLastWord();
                mode = TokenizerMode.normal;
                isLastWordOperator = false;
                results.Add(tokenLine);
            }
            
            // Character handling logic, abstracted to keep internal loop clean
            void DecideCharacterHandling(char c)
            {
                CheckForModeSwitch(c);

                if (Separators.Contains(c) && mode == TokenizerMode.normal)
                {
                    FinishLastWord();
                } 
                else
                {
                    HandleNormalCharacters(c);
                }
            }

            void CheckForModeSwitch(char c)
            {
                if (mode == TokenizerMode.comment)
                {
                    return;
                }

                if (c == CommentCharacter && mode != TokenizerMode.quoted)
                {
                    mode = TokenizerMode.comment;
                }

                if (c == QuoteCharacter)
                {
                    if (mode == TokenizerMode.quoted)
                    {
                        mode = TokenizerMode.normal;
                        return;
                    }

                    mode = TokenizerMode.quoted;
                    return;
                }
            }

            // handling spaces and tabulators
            void HandleNormalCharacters(char c)
            {
                if (SimpleOperators.Contains(c))
                {
                    if (isLastWordOperator)
                    {
                        if (tokenLine.Last().TryExpandOperator(c))
                        {
                            // work performed in TryExpand
                            return;
                        }
                    }

                    FinishLastWord();
                    currentWord += c;
                    FinishLastWord();

                    isLastWordOperator = true;

                    return;
                }

                isLastWordOperator = false;
                currentWord += c;
            }

            // finish the current word and add it to the CURRENT token handling list
            void FinishLastWord()
            {
                if (currentWord == string.Empty)
                {
                    return;
                }

                var token = new ExoToken()
                {
                    value = currentWord,
                    line = lineNumber,
                    type = mode
                };

                tokenLine.Add(token);
                currentWord = string.Empty;
            }
        }
    }

    [DebuggerDisplay("{type} {value}")]
    public class ExoToken
    {
        public string value;
        public int line;
        public TokenizerMode type;

        public bool TryExpandOperator(char c)
        {
            if (CombinedOperators.Contains(this.value + c))
            {
                this.value += c;
                return true;
            }

            return false;
        }
    }
}
