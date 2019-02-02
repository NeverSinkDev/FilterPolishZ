using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Line
{
    public class LineToken
    {
        public string value = "";
        public bool isQuoted = false;
        public bool isCommented = false;
        public bool isUtility = false;
        public bool isIdent = false;
        public bool isFluffComment = false;

        public static LineToken CreateValueToken(string value)
        {
            return new LineToken()
            {
                value = value,
            };
        }

        public static LineToken CreateQuotedValueToken(string value)
        {
            return new LineToken()
            {
                value = value,
                isQuoted = true
            };
        }

        public static LineToken CreateFluffComment(string value)
        {
            return new LineToken()
            {
                value = value,
                isCommented = true
            };
        }

        public string Serialize()
        {
            return $"{(isQuoted ? "\"" : "")}{value}{(isQuoted ? "\"" : "")}";
        }
    }
}
