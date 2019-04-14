using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Util
{
    public class EventGridFacade
    {
        private static EventGridFacade instance;

        public event EventHandler FilterChangeEvent;

        public void Publish()
        {
            FilterChangeEvent?.Invoke(null, EventArgs.Empty);
        }

        private EventGridFacade()
        {

        }

        public static EventGridFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new EventGridFacade();
            }
            return instance;
        }
    }
}
