using FilterCore.Entry;
using System.Collections.ObjectModel;

namespace FilterPolishZ.ModuleWindows.TagEditing
{
    public class FilterCategory : IFilterCategoryEntity
    {
        public string Name { get; set; }
        public bool IsFinal { get; } = false;

        public ObservableCollection<IFilterCategoryEntity> FilterTree = new ObservableCollection<IFilterCategoryEntity>();
    }

    public class FilterFinalCategory : IFilterCategoryEntity
    {
        public string Name { get; set; }
        public bool IsFinal { get; } = true;

        public FilterEntry Entry { get; set; }
    }

    public interface IFilterCategoryEntity
    {
        string Name { get; set; }
        bool IsFinal { get; }
    }
}
