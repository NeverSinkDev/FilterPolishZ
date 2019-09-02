using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Entry
{
    public interface IFilterEntry
    {
        FilterEntryHeader Header { get; set; }
        FilterEntryDataContent Content { get; set; }
        IFilterEntry Clone();
        List<string> Serialize();
    }
}
