using FilterExo.Model;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Core.PreProcess.Commands
{
    public class ExoExpressionCombineBuilder
    {
        public List<ExoAtom> Results = new List<ExoAtom>();
        public List<ExoAtom> Stack = new List<ExoAtom>();

        public ExoBlock ResolutionParent;

        public static List<IExoAtomMergeStrategy> Patterns = new List<IExoAtomMergeStrategy>()
        {
            new DictPassiveMergeStrategy(),
            new DictAddUpStrategy(),
            new DictRemoveMergeStrategy()
        };

        public ExoExpressionCombineBuilder(ExoBlock parent)
        {
            this.ResolutionParent = parent;
        }

        public bool Add(ExoAtom input)
        {
            if (input.IdentifiedType == ExoAtomType.prim)
            {
                var result = ResolveStack();
                Results.Add(input);
                return result;
            }
            else
            {
                Stack.Add(input);

                if (CanResolve())
                {
                    return ResolveStack();
                }

                return false;
            }
        }

        public bool Finish()
        {
            return ResolveStack();
        }

        private bool CanResolve()
        {
            foreach (var pattern in Patterns)
            {
                if (pattern.Match(Stack))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ResolveStack()
        {
            if (Stack.Count == 0)
            {
                return false;
            }

            foreach (var pattern in Patterns)
            {
                if (pattern.Match(Stack))
                {
                    Results.AddRange(pattern.Execute(Stack));
                    Stack.Clear();
                    return true;
                }
            }

            Results.AddRange(Stack);
            Stack.Clear();
            return false;
        }
    }

    public interface IExoAtomMergeStrategy
    {
        bool Match(List<ExoAtom> input);
        List<ExoAtom> Execute(List<ExoAtom> input);
    }

    public class DictAddUpStrategy : IExoAtomMergeStrategy
    {
        public List<ExoAtom> Execute(List<ExoAtom> input)
        {
            var hs1 = (input[0].ValueCore as HashSetValueCore).Values;
            hs1.UnionWith((input[2].ValueCore as HashSetValueCore).Values);
            var merge = new ExoAtom(hs1);
            return new List<ExoAtom>() { merge };
        }

        public bool Match(List<ExoAtom> input)
        {
            var match = input.ConfirmPattern(
                    x => x.IdentifiedType == ExoAtomType.dict,
                    x => x.GetRawValue() == "+",
                    x => x.IdentifiedType == ExoAtomType.dict
                );

            return match;
        }
    }

    public class DictRemoveMergeStrategy : IExoAtomMergeStrategy
    {
        public List<ExoAtom> Execute(List<ExoAtom> input)
        {
            var hs1 = (input[0].ValueCore as HashSetValueCore).Values;
            var hs2 = (input[2].ValueCore as HashSetValueCore).Values;
            

            foreach (var item in hs2)
            {
                if (hs1.Contains(item))
                {
                    hs1.Remove(item);
                }
            }
            
            return new List<ExoAtom>() { input[0] };
        }

        public bool Match(List<ExoAtom> input)
        {
            var match = input.ConfirmPattern(
                    x => x.IdentifiedType == ExoAtomType.dict,
                    x => x.GetRawValue() == "-",
                    x => x.IdentifiedType == ExoAtomType.dict
                );

            return match;
        }
    }

    public class DictPassiveMergeStrategy : IExoAtomMergeStrategy
    {
        public List<ExoAtom> Execute(List<ExoAtom> input)
        {
            var hs1 = (input[0].ValueCore as HashSetValueCore).Values;
            hs1.UnionWith((input[1].ValueCore as HashSetValueCore).Values);
            var merge = new ExoAtom(hs1);
            return new List<ExoAtom>() { merge };
        }

        public bool Match(List<ExoAtom> input)
        {
            var match = input.ConfirmPattern(
                    x => x.IdentifiedType == ExoAtomType.dict,
                    x => x.IdentifiedType == ExoAtomType.dict
                );

            return match;
        }
    }
}
