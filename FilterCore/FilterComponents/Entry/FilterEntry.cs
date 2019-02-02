using FilterCore.Line;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterCore.Entry
{
    public class FilterEntry : IFilterEntry
    {
        public FilterEntryHeader Header { get; set; }
        public FilterEntryDataContent Content { get; set; }

        public bool IsFrozen
        {
            get
            {
                return this.Header.IsFrozen;
            }

            set
            {
                this.Header.IsFrozen = value;
            }
        }

        public IFilterEntry Clone()
        {
            throw new NotImplementedException();
        }

        public List<string> Serialize()
        {
            var result = new List<string>();

            if (!this.Header.IsActive)
            {
                return result;
            }

            var headerText = this.Header.Serialize();
            var contentText = this.Content?.Serialize() ?? new List<string>() { string.Empty };

            if (!string.IsNullOrEmpty(headerText))
            {
                result.Add(headerText);
            }

            if (contentText != null)
            {
                if (this.Header.Type == Constants.FilterConstants.FilterEntryType.Content)
                {
                    contentText = contentText.Select(x => $"\t{x}").ToList();
                }

                result.AddRange(contentText);
            }

            if (this.Header.IsFrozen)
            {
                result = result.Select(x => $"#{x}").ToList();
            }

            return result;
        }

        public static FilterEntry CreateDataEntry(IFilterLine line)
        {
            var entry = new FilterEntry();

            entry.Header = new FilterEntryHeader();
            entry.Header.HeaderValue = line.Ident;
            entry.Header.IsFrozen = line.identCommented;
            entry.Header.IsActive = true;
            entry.Header.Type = Constants.FilterConstants.FilterEntryType.Content;
            entry.Header.ExtractTagsFromLine(line);

            entry.Content = new FilterEntryDataContent();
            entry.Content.Content = new Dictionary<string, List<IFilterLine>>();
            return entry;
        }

        public static FilterEntry CreateCommentEntry(IFilterLine line)
        {
            var entry = new FilterEntry();

            entry.Header = new FilterEntryHeader();
            entry.Header.Type = Constants.FilterConstants.FilterEntryType.Comment;
            entry.Header.IsFrozen = true;
            entry.Header.IsActive = true;

            entry.Content = new FilterEntryDataContent();
            entry.Content.Content = new Dictionary<string, List<IFilterLine>>();
            entry.Content.AddComment(line);

            return entry;
        }

        public static FilterEntry CreateFillerEntry()
        {
            var entry = new FilterEntry();

            entry.Header = new FilterEntryHeader();
            entry.Header.Type = Constants.FilterConstants.FilterEntryType.Filler;
            entry.Header.IsFrozen = false;
            entry.Header.IsActive = true;

            return entry;
        }
    }
}
