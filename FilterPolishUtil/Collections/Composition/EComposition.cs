using FilterPolishUtil.Extensions;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections.Composition
{
    public interface IComposable<T> where T : class, IComposable<T>
    {
        ICompositionElement<T> Composition { get; set; }
    }

    public interface ICompositionElement<T> where T : class, IComposable<T>
    {
        CompositionRegistry<T> Registry { get; set; }
        T Value { get; set; }
        ICollection<T> Children { get; set; }
        ICollection<T> Parent { get; set; }
        int ID { get; set; }

    }

    public static class ECompositionElement
    {
        public static T ToCompositionRoot<T>(this T current) where T : class, IComposable<T>
        {
            current.Composition.Parent = null;

            var registry = new CompositionRegistry<T>()
            {
                Root = current
            };

            current.PropagateAction(x =>
            {
                registry.Register(x);
            });

            return current;
        }

        public static void PropagateAction<T>(this T current, Action<T> action) where T : class, IComposable<T>
        {
            action(current);
            current.Composition.Children.ForEach(x => x.PropagateAction(action));
        }

        public static IEnumerable<T> GetAllNodesOnBranch<T>(this T current, bool includeSelf = true) where T : class, IComposable<T>
        {
            if (includeSelf)
            {
                yield return current;
            }

            foreach (var child in current.Composition.Children)
            {
                var childchildren = GetAllNodesOnBranch(child);
                foreach (var childchild in childchildren)
                {
                    yield return childchild;
                }
            }
        }

        public static T AdoptChild<T>(this T current, T child) where T : class, IComposable<T>
        {
            current.Composition.Children.Add(child);

            if (current.Composition.Registry != current.Composition.Registry)
            {
                child.Composition.Registry.Register(child);
            }

            child.Composition.Parent.Clear();
            child.Composition.Parent.Add(current);

            return child;
        }

        public static T LinkChild<T>(this T current, T child) where T : class, IComposable<T>
        {
            current.Composition.Children.Add(child);

            if (current.Composition.Registry != current.Composition.Registry)
            {
                child.Composition.Registry.Register(child);
            }

            child.Composition.Parent.Add(current);

            return child;
        }

        public static void UnregisterBranch<T>(this T current) where T : class, IComposable<T>
        {
            var nodes = current.GetAllNodesOnBranch();
            foreach (var item in nodes)
            {
                item.DeleteNode();
            }
        }

        public static T GoToRoot<T>(this T current) where T : class, IComposable<T>
        {
            return current.Composition.Registry.Root;
        }

        public static T GoToID<T>(this T current, int id) where T : class, IComposable<T>
        {
            return current.Composition.Registry.RegisteredCompositionNodes[id];
        }

        public static IEnumerable<T> GoToParents<T>(this T current) where T : class, IComposable<T>
        {
            if (current.Composition.Parent != null)
            {
                var parent = current.Composition.Parent;

                foreach (var item in parent)
                {
                    yield return item;
                }
            }
        }

        private static void DeleteNode<T>(this T current) where T : class, IComposable<T>
        {
            current.Composition.Parent = null;
            current.Composition.Registry.DeRegister(current);
            current.Composition.Children = null;
            current.Composition = null;
            current = null;
        }
    }
}
