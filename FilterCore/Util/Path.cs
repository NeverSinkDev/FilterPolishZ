using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Util
{
    public class Path
    {
        public Path Parent { get; set; }
        public Path Next { get; set; }
        public string Value { get; set; } = string.Empty;

        public Path(string value)
        {
            this.Value = value;
        }

        public Path Attach(string value)
        {
            this.Next = new Path(value);
            this.Next.Parent = this;
            return this.Next;
        }

        public string Serialize()
        {
            return this.Value + "/" +  (this.Next != null ? this.Next.Serialize() : string.Empty);
        }
    }
}
