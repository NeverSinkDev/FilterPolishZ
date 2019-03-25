using FilterCore.Entry;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FilterPolishZ.ModuleWindows.TagEditing
{
    public class FilterCategory : IFilterCategoryEntity
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<IFilterCategoryEntity> FilterTree { get; set; } = new ObservableCollection<IFilterCategoryEntity>();
        public string Name { get; set; }
        public bool IsFinal { get; } = false;
        public ObservableCollection<IFilterCategoryEntity> Parent { get; set; }
    }

    public class FilterFinalCategory : IFilterCategoryEntity
    {
        public FilterEntry Entry { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; } = new Random().Next(0, 100000000).ToString();
        public bool IsFinal { get; } = true;
        public ObservableCollection<IFilterCategoryEntity> Parent { get; set; }
    }

    public interface IFilterCategoryEntity : INotifyPropertyChanged
    {
        string Name { get; set; }
        bool IsFinal { get; }
        ObservableCollection<IFilterCategoryEntity> Parent { get; set; }
    }
}
