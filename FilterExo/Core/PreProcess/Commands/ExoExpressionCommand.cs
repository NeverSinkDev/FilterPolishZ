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
    public class ExoExpressionCommand
    {
        public ExoExpressionCommand(List<string> values)
        {
            foreach (var item in values)
            {
                this.Values.Add(new ExoAtom(item));
            }
        }

        public ExoBlock Parent { get; set; }

        public List<ExoAtom> Values = new List<ExoAtom>();

        public IFilterLine Serialize()
        {
            var results = new List<string>();
            foreach (var item in this.Values)
            {
                // here we attempt to resolve every value using the parents variables.
                // if no values will be found, we just return the basic value.
                results.Add(item.Serialize(this.Parent));
            }

            return results.ToFilterLine();
        }

        public void ResolveExpression()
        {
            // create bracket tree
            var tree = this.CreateIndentationTree(this.Values);
            var cursor = tree.Tree;

            ResolveBranch(tree.Tree);

            // perform actions starting with deepest child
            void ResolveBranch(Branch<ExoAtom> branch)
            {
                foreach (var item in branch.Leaves)
                {
                    ResolveBranch(item);
                }

                ResolveSelf(branch);
            }

            // attempt to resolve variables and expressions of children
            void ResolveSelf(Branch<ExoAtom> branch)
            {
                // branch.RawValue.Value = branch.RawValue.ResolveVariable(this.Parent);

                if (branch.Content.CanBeVariable && branch.Leaves.Count > 0)
                {
                    ResolveChildrenExpression(branch);
                }
            }

            // resolve child expression
            void ResolveChildrenExpression(Branch<ExoAtom> branch)
            {
                branch.Leaves = ResolveBranchExpression(branch.Leaves);
            }
        }

        public List<Branch<ExoAtom>> ResolveBranchExpression(List<Branch<ExoAtom>> children)
        {
            // Split by komma

            var splitChildren = children.SplitDivide(x => x.Content.GetRawValue() == ",");
            // Foreach values

            foreach (var subexpression in splitChildren)
            {
                foreach (var atom in subexpression)
                {
                    // Execute functions
                    var func = atom.Content.GetFunction(this.Parent);
                    // TODO: execute here
                }

                foreach (var atom in subexpression)
                {
                    // Combine if possible
                    if (atom.Content.IdentifiedType == ExoAtomType.dict)
                    {
                        // TODO MERGE OPERATORS HERE
                    }
                }
            }


            return null;
        }

        // Time to get serious/serial
        public string SerializeDebug()
        {
            return this.Serialize().Serialize();
        }

        public void SetParent(ExoBlock parent)
        {
            this.Parent = parent;
        }

        public BracesTree<ExoAtom> CreateIndentationTree(List<ExoAtom> list)
        {
            var tree = new BracesTree<ExoAtom>();
            var parent = new Branch<ExoAtom>();
            var child = new Branch<ExoAtom>();

            foreach (var item in list)
            {
                if (item.IdentifiedType == ExoAtomType.oper && item.GetRawValue() == "(")
                {
                    tree.Stack.Push(parent);
                    parent = child;
                }
                else if (item.IdentifiedType == ExoAtomType.oper && item.GetRawValue() == ")")
                {
                    child = parent;
                    parent = tree.Stack.Pop();
                }
                else
                {
                    child = new Branch<ExoAtom>() { Content = item };
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
        public T Content;
    }
}
