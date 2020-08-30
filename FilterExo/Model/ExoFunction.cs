using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Model
{
    public class ExoFunction
    {
        public string Name;
        public ExoBlock Content;
        public Dictionary<string, ExoAtom> Variables = new Dictionary<string, ExoAtom>();
    }
}
