using FilterCore.Constants;
using FilterCore.FilterComponents.Tags;
using FilterCore.Line;
using FilterCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FilterCore.Constants.FilterConstants;

namespace FilterCore.Entry
{
    public class FilterEntryHeader
    {
        public FilterEntryType Type { get; set; }
        public bool IsFrozen { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public float ID { get; set; } = -1;

        public bool IsGenerated { get; set; } = false;

        public string HeaderValue { get; set; }

        public string HeaderComment { get; set; }
        public List<GenerationTag> GenerationTags { get; set; }
        public List<StyleTag> StyleTags { get; set; }
        public List<TierTag> TierTag { get; set; }

        public string Serialize()
        {
            switch (this.Type)
            {
                case FilterEntryType.Content:

                    var comment = string.Join(" ",
                        string.Join(" ", GenerationTags.Select(x => x.Serialize()).ToList()),
                        string.Join(" ", StyleTags.Select(x => x.Serialize()).ToList()),
                        HeaderComment).Trim();

                    if (!string.IsNullOrEmpty(comment))
                    {
                        comment = $"# {comment}";
                    }

                    return StringWork.CombinePieces(HeaderValue, comment);

                case FilterEntryType.Filler:
                case FilterEntryType.Comment:
                default:
                    return string.Empty;
            }
        }

        public void ExtractTagsFromLine(IFilterLine line)
        {
            GenerationTags = new List<GenerationTag>();
            StyleTags = new List<StyleTag>();

            bool firstComment = true;
            StringBuilder builder = new StringBuilder();
            var strings = line.Comment.Split(FilterConstants.WhiteLineChars);

            foreach (var s in strings)
            {
                if (s.Length == 0)
                {
                    continue;
                }

                if (s[0] == '%')
                {
                    GenerationTag tag; 
                    var split = s.Substring(1);

                    if (char.IsDigit(split[split.Length-1]))
                    {
                        var lastPos = split.Length - 1;
                        var digit = short.Parse(split.Substring(lastPos));
                        var command = split.Substring(0, lastPos);
                        tag = new GenerationTag()
                        {
                            Strictness = digit,
                            Value = command
                        };
                    }
                    else
                    {
                        tag = new GenerationTag()
                        {
                            Value = split
                        };
                    }

                    this.GenerationTags.Add(tag);
                }
                else if (s[0] == '$')
                {
                    var split = s.Substring(1);
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
    }

    public interface IFilterEntryContent
    {
        bool Add(IFilterLine line);
        List<string> Serialize();
        void Revert();
    }
}
