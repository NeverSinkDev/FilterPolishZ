using FilterExo.Core.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Core.Process;
using FilterExo.Core.Process.StyleResoluton;

namespace FilterExo.Model
{
    public class ExoFilter
    {
        public string FileName;

        public ExoBlock RootEntry;
        public List<ExoFilter> AttachedSections = new List<ExoFilter>();
    }
}
