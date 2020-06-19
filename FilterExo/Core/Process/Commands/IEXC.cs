using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Core.Process.Commands
{
    public interface IEXC
    {
        List<string> Execute(ExoFilterEntry entry);
    }
}
