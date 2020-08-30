using FilterExo.Model;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Core.PreProcess.Commands
{
    public class ExoCommandLanguageProcessor
    {
        /*
         * we have a list of objects, these objects represent our commands or the important syntax for the language
         * 1) we parse through every command
         * 2) we try simplify/resolve variables/commands
         * 3) we execute functions
         * 4) we honor the order
         * 5) if actions were performed, we try again, until we can't simplify anything
         * 6) we serialize in the end
         * 7) we respect patterns
         * Pattern examples. 
         * We build a "bracket tree". Things inside brackets get executed first, then the surrounding bracket action
         * Brackets mean functions and commas represent parameter splitting
         * We try to resolve every function
         * We rely on operators and prefixes, such as + -
         * After no changes are performed in every simplification step, we return the results.
         * Every bracket needs to be resolved.
         */
    }

    public class ExoExpressionCombineBuilder
    {
        public List<ExoAtom> Results = new List<ExoAtom>();
        public List<ExoAtom> Stack = new List<ExoAtom>();

        public static List<IExoAtomMergeStrategy> Patterns = new List<IExoAtomMergeStrategy>()
        {
            new DictAddUpStrategy()
        };

        public void Add(ExoAtom input)
        {
            if (input.IdentifiedType == ExoAtomType.prim)
            {
                Results.Add(input);
                ResolveStack();

                return;
            }
            else
            {
                Stack.Add(input);

            }
        }

        public List<ExoAtom> Finish()
        {
            ResolveStack();
            return Results;
        }

        private void ResolveStack()
        {
            throw new NotImplementedException();
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
}
