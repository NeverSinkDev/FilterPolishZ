using FilterEconomy.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy
{
    public class TieringSuggestionFacade
    {
        private static TieringSuggestionFacade instance;
        private TieringSuggestionFacade()
        {
        }

        public Dictionary<string, List<TieringCommand>> Suggestions = new Dictionary<string, List<TieringCommand>>();

        public static TieringSuggestionFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new TieringSuggestionFacade();
            }
            return instance;
        }
    }
}
