using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo
{
    public class FilterExoFacade
    {
        private FilterExoFacade()
        {

        }

        private static FilterExoFacade instance;

        public static FilterExoFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new FilterExoFacade();
            }

            return instance;
        }
    }
}
