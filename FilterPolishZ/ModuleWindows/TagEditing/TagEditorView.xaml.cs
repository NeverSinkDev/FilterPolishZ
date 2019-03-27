using FilterCore;
using FilterCore.Entry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FilterCore.Commands;
using FilterCore.FilterComponents.Tags;
using FilterCore.Constants;
using FilterPolishUtil.Collections;

namespace FilterPolishZ.ModuleWindows.TagEditing
{
    /// <summary>
    /// Interaction logic for TagEditorView.xaml
    /// </summary>
    public partial class TagEditorView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FilterAccessFacade FilterAccessFacade { get; set; } = FilterAccessFacade.GetInstance();
        public ObservableCollection<IFilterCategoryEntity> FilterTree { get; set; } = new ObservableCollection<IFilterCategoryEntity>();

        public ObservableCollection<TierTag> TierTags { get; set; } = new ObservableCollection<TierTag>();

        public Capsule SelectedTagCapsule => new Capsule((string s) => this.GetCurrentTagValue(s));

        //public ObservableCollection

        public TagEditorView()
        {
            this.InitializeTagLogic();
            this.InitializeTierTags();
            this.DataContext = this;
            InitializeComponent();
        }

        private void InitializeTierTags()
        {
            this.TierTags.Clear();
            FilterConstants.TierTagTypes.ForEach(x => TierTags.Add(new TierTag(x)));
        }

        private void InitializeTagLogic()
        {
            ObservableCollection<IFilterCategoryEntity> cursor = this.FilterTree;
            foreach (var item in FilterAccessFacade.PrimaryFilter.FilterEntries)
            {
                if (item.Header.Type == FilterCore.Constants.FilterConstants.FilterEntryType.Filler)
                {
                    continue;
                }

                if (FilterTableOfContentsCreator.IsSectionTitleEntry(item))
                {
                    var depth = FilterTableOfContentsCreator.GetTitleDepth(item);
                    var title = FilterTableOfContentsCreator.GetTitle(item, depth);

                    if (depth == 2)
                    {
                        cursor = this.FilterTree;
                        var lastCategory = new FilterCategory()
                        {
                            Parent = this.FilterTree,
                            Name = title
                        };

                        cursor = lastCategory.FilterTree;
                        this.FilterTree.Add(lastCategory);
                    }
                    else
                    {
                        cursor = (this.FilterTree.Where(x => x.IsFinal == false)
                                .Last() as FilterCategory).FilterTree;

                        var lastCategory = new FilterCategory()
                        {
                            Parent = cursor,
                            Name = title
                        };

                        cursor.Add(lastCategory);
                        cursor = lastCategory.FilterTree;
                    }

                    continue;
                }

                cursor.Add(new FilterFinalCategory()
                {
                    Name = string.Join(" ", item.Serialize()),
                    Parent = cursor,
                    Entry = item as FilterEntry
                });
            }
        }

        private string GetCurrentTagValue(string s)
        {
            var selected = this.TreeView?.SelectedItem;

            if (selected == null)
            {
                return string.Empty;
            }

            switch (selected)
            {
                case FilterFinalCategory final:
                    var tag = final.Entry.Header.TierTags;
                    if (tag == null)
                    {
                        return string.Empty;
                    }

                    if (tag.ContainsKey(s))
                    {
                        return tag[s].Serialize();
                    }
                    return string.Empty;
                    break;
                case FilterCategory cat:
                    return string.Empty;
                    break;
                default:
                    return string.Empty;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.InitializeTierTags();

        }
    }
}
