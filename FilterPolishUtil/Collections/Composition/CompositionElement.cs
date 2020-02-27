using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections.Composition
{
    public class CompositionElement<T> : ICompositionElement<T> where T : class, IComposable<T>
    {
        public T Value { get; set; }
        public ICollection<T> Children { get; set; } = new List<T>();
        public ICollection<T> Parent { get; set; } = new List<T>();
        public int ID { get; set; }
        public CompositionRegistry<T> Registry { get; set; }
    }

    public class MetaCompositionElement<T> : CompositionElement<T> where T : class, IComposable<T>
    {
        public AutoDictionary<string, string> MetaData = new AutoDictionary<string, string>();
    }
}
