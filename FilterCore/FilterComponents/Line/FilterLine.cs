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

        public virtual FilterLine<T> Clone()
        {
            throw new NotImplementedException();
        }

        public virtual string Serialize()
        {
            return StringWork.CombinePieces(
                Ident, 
                Value?.Serialize() ?? string.Empty, 
                !string.IsNullOrEmpty(Comment) ? 
                    (Ident != string.Empty ? "#" : string.Empty) + Comment : 
                    string.Empty );
        }

        IFilterLine IFilterLine.Clone()
        {
            throw new NotImplementedException();
        }
    }
}
