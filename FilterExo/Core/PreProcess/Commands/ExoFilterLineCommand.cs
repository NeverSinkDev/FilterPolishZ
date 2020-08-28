using FilterCore.Line;
using FilterExo.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FilterExo.Core.PreProcess.Commands
{
    public class ExoFilterLineCommand : IExoCommand
    {
        public ExoFilterLineCommand(string name, List<string> values)
        {
            foreach (var item in values)
            {
                this.Values.Add(new ExoAtom(item));
            }

            this.Name = name;
        }

        public string Name { get; set; }
        public ExoBlock Parent { get; set; }

        public IExoCommandType Type => IExoCommandType.filter;

        public List<ExoAtom> Values = new List<ExoAtom>();

        public IFilterLine Serialize()
        {
            var results = new List<string>();
            foreach (var item in this.Values)
            {
                // here we attempt to resolve every value using the parents variables.
                // if no values will be found, we just return the basic value.
                results.Add(item.ResolveValue(this.Parent));
            }

            return results.ToFilterLine(this.Name);
        }

        public void IsResolvable()
        {
            
        }

        // Time to get serious/serial
        public string SerializeDebug()
        {
            return this.Serialize().Serialize();
        }
    }
}
