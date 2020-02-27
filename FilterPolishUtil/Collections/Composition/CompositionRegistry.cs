using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections.Composition
{
    public class CompositionRegistry<T> where T : class, IComposable<T>
    {
        private int counter = 0;
        public Dictionary<int, T> RegisteredCompositionNodes { get; set; } = new Dictionary<int, T>();
        public T Root { get; set; }

        public int Register(T newnode)
        {
            var id = counter;
            counter++;

            this.RegisteredCompositionNodes.Add(id, newnode);
            newnode.Composition.ID = id;

            if (newnode.Composition.Registry != null)
            {
                newnode.Composition.Registry.DeRegister(newnode);
            }

            newnode.Composition.Registry = this;

            return id;
        }

        public void DeRegister(T oldnode)
        {
            this.RegisteredCompositionNodes.Remove(oldnode.Composition.ID);
        }
    }
}
