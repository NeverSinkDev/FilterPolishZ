using FilterExo.Core.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Model
{
    public class ExoFilter
    {
        public string FileName;

        public ExoBlock RootEntry;
        public List<ExoFilter> AttachedSections = new List<ExoFilter>();
    }
}
