using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using FilterCore.Entry;
using FilterCore.Line;
using FilterPolishUtil;
using FilterPolishUtil.Collections;

namespace FilterCore.FilterComponents.Entry
{
    public class FilterEntryBuilder
    {
        private class WipLine
        {
            public string Value = string.Empty;
            public string ExplicitIdent = string.Empty;
            public string Comment = string.Empty;
            public ContextOperations Context = ContextOperations.def;
        }

        private string Header = string.Empty;
        private bool IsComment = false;
        private bool Enabled = true;
        private bool IsContinue = false;
        private AutoDictionaryList<string,WipLine> tags = new AutoDictionaryList<string, WipLine>();
        private AutoDictionaryList<string,WipLine> lines = new AutoDictionaryList<string,WipLine>();
        private List<string> comments = new List<string>();

        public void Reset()
        {
            this.LineCommands = new List<FilterBuilderPiece>();
        }

        public void RestoreInitialValues()
        {
            this.Header = string.Empty;
            this.IsComment = false;
            this.Enabled = true;
            this.IsContinue = false;
            tags = new AutoDictionaryList<string, WipLine>();
            lines = new AutoDictionaryList<string, WipLine>();
            comments = new List<string>();
        }

        public static HashSet<string> HeaderDescriptors = new HashSet<string>()
        {
            "Show", "Hide", "Cont", "Conh", "Disable", "Enable"
        };

        public List<FilterBuilderPiece> LineCommands = new List<FilterBuilderPiece>();

        public FilterEntryBuilder AddCommand(List<string> command, ContextOperations context = ContextOperations.def)
        {
            return this.AddCommand(string.Join(" ", command), context);
        }

        public FilterEntryBuilder AddCommand(string command, ContextOperations context = ContextOperations.def)
        {
            var piece = new FilterBuilderPiece()
            {
                Command = command,
                Context = context,
            };

            if (HeaderDescriptors.Contains(command))
            {
                piece.Type = FilterBuilderPieceType.headeredit;
            }
            else if (command.IsFilterLine())
            {
                piece.Type = FilterBuilderPieceType.line;
                piece.Ident = command.GetFilterLineIdent();
            }
            else if (command.IsTag())
            {
                piece.Ident = command.Trim()[0].ToString();
                piece.Type = FilterBuilderPieceType.tag;
            }
            else if (command.IsComment())
            {
                piece.Type = FilterBuilderPieceType.comment;
            }
            else
            {
                throw new Exception("unknown piece type");
            }

            this.LineCommands.Add(piece);

            return this;
        }

        public FilterEntry Execute()
        {
            var entry = new FilterEntry();

            // preproces
            foreach (var piece in this.LineCommands)
            {
                switch (piece.Type)
                {
                    case FilterBuilderPieceType.headeredit:
                        this.HandleHeaderPiece(piece);
                        break;
                    case FilterBuilderPieceType.line:
                        this.HandleLinePiece(piece);
                        break;
                    case FilterBuilderPieceType.tag:
                        this.HandleTagPiece(piece);
                        break;
                    case FilterBuilderPieceType.comment:
                        this.HandleCommentPiece(piece);
                        break;
                    case FilterBuilderPieceType.meta:
                        this.HandleMetaPiece(piece);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (this.IsComment)
            {
                return FilterEntry.CreateCommentEntry(this.comments);
            }

            var header = this.ResolveHeader();

            if (HeaderDescriptors.Contains(this.Header))
            {
                entry = FilterEntry.CreateDataEntry(header);
                ResolveAllLines(entry);
            }
            else
            {
                // TraceUtility.Throw("Undetected comment entry!");
                return FilterEntry.CreateFillerEntry();
            }
            
            return entry;
        }

        private void ResolveAllLines(FilterEntry entry)
        {
            foreach (var linelist in this.lines.dict)
            {
                this.ResolveLines(linelist.Key, linelist.Value, entry);
            }

            if (this.IsContinue)
            {
                entry.Content.Add("Continue".ToFilterLine());
            }
        }

        private void ResolveLines(string ident, List<WipLine> lines, FilterEntry entry)
        {
            foreach (var wipLine in lines)
            {
                var sline = wipLine.ExplicitIdent + " " + wipLine.Value + " " + wipLine.Comment;
                entry.Content.Add(sline.ToFilterLine());
            }
        }

        private string ResolveHeader()
        {
            var resultingHeader = string.Empty;

            if (!this.Enabled)
            {
                resultingHeader += "# ";
                this.IsComment = true;
            }

            resultingHeader += this.Header;

            if (this.tags.dict.Count > 0)
            {
                resultingHeader += " # " + ResolveTags("$") + " " + ResolveTags("%");
            }

            return resultingHeader;
        }

        private string ResolveTags(string key)
        {
            var result = string.Empty;
            if (this.tags.dict.ContainsKey(key))
            {
                foreach (var tag in this.tags[key])
                {
                    result += key + tag.Value;
                    result += " ";
                }
            }

            return result;
        }

        private void HandleTagPiece(FilterBuilderPiece piece)
        {
            if (this.IsComment) TraceUtility.Throw("Conflicting commands!");

            WipLine wip = new WipLine()
            {
                Comment = null,
                ExplicitIdent = piece.Ident,
                Value = piece.Command.SubstringFrom(piece.Ident),
                Context = piece.Context
            };

            this.tags.Add(piece.Ident, wip);
        }

        private void HandleMetaPiece(FilterBuilderPiece piece)
        {
            throw new Exception("not implemented zomg!");
        }

        private void HandleLinePiece(FilterBuilderPiece piece)
        {
            if (this.IsComment) TraceUtility.Throw("Conflicting commands!");
            WipLine wip = new WipLine()
            {
                Comment = null,
                ExplicitIdent = piece.Ident,
                Value = piece.Command.SubstringFrom(piece.Ident),
                Context = piece.Context
            };

            this.lines.Add(piece.Ident, wip);
        }

        private void HandleCommentPiece(FilterBuilderPiece piece)
        {
            if (this.Header != string.Empty) TraceUtility.Throw("Conflicting commands!");
            this.IsComment = true;
            this.comments.Add(piece.Command);
        }

        private void HandleHeaderPiece(FilterBuilderPiece piece)
        {
            switch (piece.Command)
            {
                case "Hide":
                case "Show":
                    this.Header = piece.Command;
                    this.IsContinue = false;
                    break;
                case "Cont":
                    this.Header = "Show";
                    this.IsContinue = true;
                    break;
                case "Conh":
                    this.Header = "Hide";
                    this.IsContinue = false;
                    break;
                case "Enable":
                    this.Enabled = true;
                    break;
                case "Disable":
                    this.Enabled = false;
                    break;
                default:
                    TraceUtility.Throw("Unknown rule type?!");
                    break;
            }
        }
    }

    public class FilterBuilderPiece
    {
        public string Command;
        public string Ident;
        public ContextOperations Context;
        public FilterBuilderPieceType Type;
    }

    public enum FilterBuilderPieceType
    {
        line,
        tag,
        headeredit,
        comment,
        meta
    }

    public enum ContextOperations
    {
        def,
        add,
        replace,
        edit,
        delete,
        mask,
        comment,
        force
    }
}
