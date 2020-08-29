using FilterCore.Line;
using FilterExo.Model;
using FilterPolishUtil.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FilterExo.Core.PreProcess.Commands
{
    public class ExoExpressionCommand : IExoCommand
    {
        public ExoExpressionCommand(List<string> values)
        {
            foreach (var item in values)
            {
                this.Values.Add(new ExoAtom(item));
            }
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
                results.Add(item.ResolveVariable(this.Parent));
            }

            return results.ToFilterLine();
        }

        public void PerformResolution()
        {
            var tree = this.CreateIndentationTree(this.Values);
            var cursor = tree.Tree;

            void ResolveBranch(Branch<ExoAtom> branch)
            {
                foreach (var item in branch.Leaves)
                {
                    ResolveBranch(item);
                }

                ResolveSelf(branch);
            }

            void ResolveSelf(Branch<ExoAtom> branch)
            {
                branch.RawValue.Value = branch.RawValue.ResolveVariable(this.Parent);

                if (branch.RawValue.CanBeVariable && branch.Leaves.Count > 0)
                {
                    ResolveChildrenExpression(branch);
                    var func = branch.RawValue.GetFunction(this.Parent);
                }
            }

            void ResolveChildrenExpression(Branch<ExoAtom> branch)
            {
                var children = branch.Leaves;
            }
        }

        // Time to get serious/serial
        public string SerializeDebug()
        {
            return this.Serialize().Serialize();
        }

        public BracesTree<ExoAtom> CreateIndentationTree(List<ExoAtom> list)
        {
            var tree = new BracesTree<ExoAtom>();
            var parent = new Branch<ExoAtom>();
            var child = new Branch<ExoAtom>();

            foreach (var item in list)
            {
                if (item.Value == "(")
                {
                    tree.Stack.Push(parent);
                    parent = child;
                }
                else if (item.Value == ")")
                {
                    child = parent;
                    parent = tree.Stack.Pop();
                }
                else
                {
                    child = new Branch<ExoAtom>() { RawValue = item };
                    parent.Leaves.Add(child);
                }
            }

            tree.Tree = parent;

            return tree;
        }
    }

    public class BracesTree<T>
    {
        public Stack<Branch<T>> Stack = new Stack<Branch<T>>();
        public Branch<T> Tree = new Branch<T>();
    }

    public class Branch<T>
    {
        public Branch<T> Parent;
        public List<Branch<T>> Leaves = new List<Branch<T>>();
        public T RawValue;
    }
}
