using System;
using FilterCore.Entry;
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

        private const int StyleCommentPad = 37;

        public virtual FilterLine<T> Clone()
        {
            throw new NotImplementedException();
        }

        public virtual string Serialize()
        {
            var res = StringWork.CombinePieces(
                string.Empty,
                Ident, 
                Value?.Serialize() ?? string.Empty
                );

            if (!string.IsNullOrEmpty(Comment))
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
            throw new NotImplementedException();
        }
    }
}
