using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Model
{
    public interface IExoVariable
    {
        List<string> GetValue();
    }

    public class SimpleExoVariable : IExoVariable
    {
        public SimpleExoVariable(string value)
        {
            this.Value.Add(value);
        }

        public SimpleExoVariable(List<string> values)
        {
            this.Value.AddRange(values);
        }

        public List<string> Value { get; set; } = new List<string>();

        public List<string> GetValue()
        {
            return Value;
        }
    }
}
