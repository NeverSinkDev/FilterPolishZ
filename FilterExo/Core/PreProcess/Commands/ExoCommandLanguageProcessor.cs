using System;
using System.Collections.Generic;
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
        public void Execute()
        {

        }
    }
}
