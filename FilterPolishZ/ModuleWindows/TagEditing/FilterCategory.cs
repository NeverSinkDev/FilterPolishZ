using FilterCore.Entry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FilterPolishZ.ModuleWindows.TagEditing
{
    public class FilterCategory : IFilterCategoryEntity
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<IFilterCategoryEntity> FilterTree { get; set; } = new ObservableCollection<IFilterCategoryEntity>();
        public string Name { get; set; }
        public bool IsFinal { get; } = false;
        public ObservableCollection<IFilterCategoryEntity> Parent { get; set; }

        public IEnumerable<FilterEntry> GetEntries()
        {
            foreach (var item in FilterTree)
            {
                var results = item.GetEntries();
                foreach (var innerItem in results)
                {
                    yield return innerItem;
                }
            }
        }
    }

    public class FilterFinalCategory : IFilterCategoryEntity
    {
        public FilterEntry Entry { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; } = new Random().Next(0, 100000000).ToString();
        public bool IsFinal { get; } = true;
        public ObservableCollection<IFilterCategoryEntity> Parent { get; set; }

        public IEnumerable<FilterEntry> GetEntries()
        {
            yield return this.Entry;
        }
    }

    public interface IFilterCategoryEntity : INotifyPropertyChanged
    {
        string Name { get; set; }
        bool IsFinal { get; }
        ObservableCollection<IFilterCategoryEntity> Parent { get; set; }
        IEnumerable<FilterEntry> GetEntries();
    }

    public static class EFilterCategory
    {
        public static IEnumerable<T> Select<T>(this IFilterCategoryEntity me, Func<FilterFinalCategory,T> onLeaf)
        {
            switch (me)
            {
                case FilterCategory cat:
                    {
                        var catLeafs = cat.FilterTree.SelectMany(x => x.Select(onLeaf));
                        foreach (var item in catLeafs)
                        {
                            yield return item;
                        }
                    }
                    break;
                case FilterFinalCategory leaf:
                    yield return onLeaf(leaf);
                    break;
                default:
                    break;
            }
        }
    }
}
