﻿using System;
using FilterCore.Entry;
using FilterCore.Line.LineStrategy;
using FilterCore.Line.Parsing;
using FilterDomain.LineStrategy;
using FilterPolishUtil;

namespace FilterCore.Line
{
    public class FilterLine<T> : IFilterLine where T : ILineValueCore
    {
        public IFilterEntry Parent { get; set; }

        public bool IsActive { get; set; } = true;
        public string Ident { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        public ILineValueCore Value { get; set; }
        public bool identCommented { get; set; }

        private const int StyleCommentPad = 40;

        public virtual FilterLine<T> Clone()
        {
            return new FilterLine<T>
            {
                Value = this.Value.Clone(),
                Comment = this.Comment,
                Ident = this.Ident,
                IsActive = this.IsActive,
                identCommented = this.identCommented
            };
        }

        public virtual string Serialize()
        {
            if (!this.IsActive)
            {
                return string.Empty;
            }

            var res = StringWork.CombinePieces(
                string.Empty,
                Ident, 
                Value?.Serialize() ?? string.Empty
                );

            if (!string.IsNullOrEmpty(Comment) && !string.IsNullOrWhiteSpace(Comment))
            {
                if (Ident != string.Empty)
                {
                    res = res.PadRight(StyleCommentPad, ' ');
                    res += "#";
                }
                else if (!this.IsDividerComment()) res += " ";
                
                res += Comment;
            }

            return res;
        }

        private bool IsDividerComment()
        {
            return this.Comment.Length > 10 && this.Comment[0] == this.Comment[1] && this.Comment[1] == this.Comment[2];
        }

        IFilterLine IFilterLine.Clone()
        {
            return new FilterLine<T>
            {
                Value = this.Value.Clone(),
                Comment = this.Comment,
                Ident = this.Ident,
                identCommented = this.identCommented,
                IsActive = this.IsActive
            };
        }
    }
}
