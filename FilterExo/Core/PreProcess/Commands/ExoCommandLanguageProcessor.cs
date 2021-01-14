using FilterExo.Model;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
            new DictArrayAccessRangeStrategy(),
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
            if (input.IdentifiedType == ExoAtomType.prim && !int.TryParse(input.GetRawValue(), out _))
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

    public class DictArrayAccessRangeStrategy : IExoAtomMergeStrategy
    {
        public List<ExoAtom> Execute(List<ExoAtom> input)
        {
            var hs1 = (input[0].ValueCore as HashSetValueCore).Values;

            var results = new List<ExoAtom>();
            var lowerBounds = int.Parse(input[2].GetRawValue());
            var upperbounds = int.Parse(input[4].GetRawValue());

            var i = 0;
            foreach (var item in hs1)
            {
                i++;
                if (i >= lowerBounds && i < upperbounds)
                {
                    results.Add(new ExoAtom(item));
                }
            }

            return results;
        }

        public bool Match(List<ExoAtom> input)
        {
            if (input.Count > 3)
            {
                var y = "asd";
            }

            var match = input.ConfirmPattern(
                x => x.IdentifiedType == ExoAtomType.dict,
                x => x.GetRawValue() == "[",
                x => x.IdentifiedType == ExoAtomType.prim && int.TryParse(x.GetRawValue(), out _),
                x => x.GetRawValue() == "-",
                x => x.IdentifiedType == ExoAtomType.prim && int.TryParse(x.GetRawValue(), out _),
                x => x.GetRawValue() == "]"
            );

            return match;
        }
    }
}
