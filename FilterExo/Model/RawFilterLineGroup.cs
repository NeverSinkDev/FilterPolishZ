using FilterCore.Line;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Model
{
    public class RawFilterLineGroup
    {

        public string Command;
        public List<List<string>> Value = new List<List<string>>();

        public List<IFilterLine> Serialize()
        {
            return null;
        }
    }
}
