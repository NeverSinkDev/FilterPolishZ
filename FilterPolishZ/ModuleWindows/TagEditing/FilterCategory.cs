using FilterCore.Entry;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FilterPolishZ.ModuleWindows.TagEditing
{
    public class FilterCategory : IFilterCategoryEntity
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<FilterFinalCategory> FilterTree { get; set; } = new ObservableCollection<FilterFinalCategory>();
        public string Name { get; set; }
        public bool IsFinal { get; } = false;
        public IFilterCategoryEntity Parent { get; }
    }

    public class FilterFinalCategory : IFilterCategoryEntity
    {
        public FilterEntry Entry { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; } = new Random().Next(0, 100000000).ToString();
        public bool IsFinal { get; } = true;
        public IFilterCategoryEntity Parent { get; }
    }

    public interface IFilterCategoryEntity : INotifyPropertyChanged
    {
        string Name { get; set; }
        bool IsFinal { get; }
        IFilterCategoryEntity Parent { get; }
    }
}
