using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections
{
    public class Capsule
    {
        public Capsule(Func<string, string> definition)
        {
            this.AccessCommand = definition;
        }

        public string this[string index]
        {
            get => this.AccessCommand(index);
        }

        public string this[int index]
        {
            get => this.AccessCommand(index.ToString());
        }

        public Func<string, string> AccessCommand { get; set; }

    }
}
