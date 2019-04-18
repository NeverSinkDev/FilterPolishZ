using FilterCore.Constants;
using FilterCore.FilterComponents.Tags;
using FilterCore.Line;
using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterCore.Commands.EntryCommands;
using static FilterCore.Constants.FilterConstants;

namespace FilterCore.Entry
{
    public class FilterEntryHeader
    {
        public FilterEntryType Type { get; set; }
        public bool IsFrozen { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public string ID { get; set; } = "";
        private static float cloneID = 1;

        public bool IsGenerated { get; set; } = false;

        public string HeaderValue { get; set; }

        public string HeaderComment { get; set; }
        public List<GenerationTag> GenerationTags { get; set; }
        public TierTagSet TierTags { get; set; }

        public string Serialize()
        {
            switch (this.Type)
            {
                case FilterEntryType.Content:

                    var comment = string.Join(" ",
                        string.Join(" ", GenerationTags.Select(x => x.Serialize()).ToList()),
                        this.TierTags?.Serialize(),
                        HeaderComment).Trim();

                    if (!string.IsNullOrEmpty(comment))
                    {
                        comment = $"# {comment}";
                    }

                    return StringWork.CombinePieces(string.Empty,HeaderValue, comment);

                case FilterEntryType.Filler:
                case FilterEntryType.Comment:
                default:
                    return string.Empty;
            }
        }

        public void ExtractTagsFromLine(IFilterLine line, FilterEntry entry)
        {
            GenerationTags = new List<GenerationTag>();
            TierTags = new TierTagSet();

            bool firstComment = true;
            StringBuilder builder = new StringBuilder();
            var strings = line.Comment.ToLower().Split(FilterConstants.WhiteLineChars);

            foreach (var s in strings)
            {
                if (s.Length == 0)
                {
                    continue;
                }

                if (s[0] == '%')
                {
                    GenerationTag tag; 
                    var split = s.Substring(1).ToUpper();
                    var lastPos = split.Length - 1;
                    var command = split.Substring(0, lastPos);
                    short digit = -1;

                    // in case the command is something without digit at the end (like %UP, unlike %h3)
                    if (FilterConstants.EntryCommand.ContainsKey(split))
                    {
                        command = split;
                    }

                    // checking if the last char is a digit wont work correctly in cases of e.g. "crafting-83"
                    // which will save the "3" as strictness, so we instead check if the command is in the EntryCommand list
                    else if (FilterConstants.EntryCommand.ContainsKey(command))
                    {
                        digit = short.Parse(split.Substring(lastPos));
                    }
                    else
                    {
                        throw new Exception();
                    }
                    
                    var tagType = FilterConstants.EntryCommand[command];
                    tag = tagType.GetConstructors().Single().Invoke(new object[] {entry}) as GenerationTag;
                    tag.Strictness = digit;
                    tag.Value = command;

                    this.GenerationTags.Add(tag);
                }
                else if (s[0] == '$')
                {
                    this.TierTags.Add(s);
                }
                else
                {
                    if (!firstComment)
                    {
                        builder.Append(" ");
                    }
                    else
                    {
                        firstComment = false;
                    }

                    builder.Append(s);
                }
            }

            HeaderComment = builder.ToString();
        }

        public FilterEntryHeader Clone()
        {
            return new FilterEntryHeader
            {
                GenerationTags = this.GenerationTags.Where(x => !(x is IEntryGenerationCommand)).Select(x => x.Clone()).ToList(), // todo
                Type = this.Type,
                HeaderValue = this.HeaderValue,
                HeaderComment = this.HeaderComment,
                IsFrozen = this.IsFrozen,
                TierTags = this.TierTags.Clone(),
                IsActive = this.IsActive,
                ID = this.ID + "clone" + cloneID++,
                IsGenerated = true
            };
        }
    }

    public interface IFilterEntryContent
    {
        bool Add(IFilterLine line);
        List<string> Serialize();
        void Revert();
    }
}
