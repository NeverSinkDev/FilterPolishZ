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
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public FilterAccessFacade FilterAccessFacade { get; set; } = FilterAccessFacade.GetInstance();
        public ObservableCollection<IFilterCategoryEntity> FilterTree { get; set; } = new ObservableCollection<IFilterCategoryEntity>();
        public ObservableCollection<TierTag> TierTags { get; set; } = new ObservableCollection<TierTag>();
        public ObservableCollection<FilterEntry> SelectedEntries { get; set; } = new ObservableCollection<FilterEntry>();
        public Capsule SelectedTagCapsule => new Capsule((string s) => this.GetCurrentTagValue(s));

        public string UnknownTags { get; set; }

        //public ObservableCollection

        public TagEditorView()
        {
            this.InitializeTagLogic();
            this.InitializeTierTagsOnChange();
            this.DataContext = this;
            InitializeComponent();
        }

        private void InitializeTierTagsOnChange()
        {
            this.TierTags.Clear();
            FilterGenerationConfig.TierTagTypes.ForEach(x => TierTags.Add(new TierTag(x)));
        }

        private void InitializeTagLogic()
        {
            ObservableCollection<IFilterCategoryEntity> cursor = this.FilterTree;
            foreach (var item in FilterAccessFacade.PrimaryFilter.FilterEntries)
            {
                if (item.Header.Type == FilterGenerationConfig.FilterEntryType.Filler)
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

                var tagText = item.Header?.TierTags?.Serialize();
                if (string.IsNullOrEmpty(tagText))
                {
                    tagText = "untagged";
                }

                cursor.Add(new FilterFinalCategory()
                {
                    Name = tagText, // string.Join(" ", item.Serialize()),
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

            var tags = (selected as IFilterCategoryEntity).Select(x => x.Entry.Header.TierTags);
            var count = tags.Count();

            var relevantTags = tags
                .Where(x => x != null)
                .Where(x => x.ContainsKey(s))
                .Select(x => x[s].Serialize()).ToList();

            var emptyCount = count - relevantTags.Count;

            var results = relevantTags.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count()).OrderByDescending(x => x.Value).Select(x => $"[{x.Value}/{count}] {x.Key}").ToList();

            if (emptyCount > 0)
            {
                results.Add($"[{emptyCount}/{count}] EMPTY");
            }

            return string.Join(System.Environment.NewLine, results);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.InitializeTierTagsOnChange();
            this.InitializeUnknownTagsOnChange();
            this.InitializeSelectedEntriesOnChange();
        }

        private void InitializeUnknownTagsOnChange()
        {
            var selected = this.TreeView?.SelectedItem;

            if (selected == null)
            {
                this.UnknownTags = "none";
            }

            var tags = (selected as IFilterCategoryEntity).Select(x => x.Entry.Header.TierTags);
            var count = tags.Count();

            var irelevantTags = tags
                .Where(x => x != null)
                .SelectMany(x => x.TierTags).Where(x => !FilterGenerationConfig.TierTagTypes.Contains(x.Key))
                .Select(x => x.Value.Serialize())
                .ToList();

            var results = irelevantTags.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count()).OrderByDescending(x => x.Value).Select(x => $"[{x.Value}/{count}] {x.Key}").ToList();
            this.UnknownTags = string.Join(System.Environment.NewLine, results);
        }

        private void InitializeSelectedEntriesOnChange()
        {
            var selected = this.TreeView?.SelectedItem;
            if (selected is IFilterCategoryEntity ent)
            {
                this.SelectedEntries = new ObservableCollection<FilterEntry>(ent.GetEntries().Where(x => x.Header.Type == FilterGenerationConfig.FilterEntryType.Content));
            }
        }

        private void OnTagSetButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.Parent is Grid parent)) return;

            string tagType, newTagValue;

            if (parent.Children[0] is TextBlock tagTypeText)
            {
                tagType = tagTypeText.Text;
            }
            else return;

            if (parent.Children[3] is TextBox tagInputField)
            {
                newTagValue = tagInputField.Text;
            }
            else return;

            foreach (var entry in this.SelectedEntries)
            {
                var tags = entry.Header.TierTags;

                if (tags.ContainsKey(tagType))
                {
                    tags.TierTags.Remove(tagType);
                }

                tags.Add(tagType + "->" + newTagValue);
            }

            // todo: re-serialize filter files for current version
        }

        private void OnTagRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.Parent is Grid parent)) return;

            string tagType;

            if (parent.Children[0] is TextBlock tagTypeText)
            {
                tagType = tagTypeText.Text;
            }
            else return;

            foreach (var entry in this.SelectedEntries)
            {
                var tags = entry.Header.TierTags;

                if (tags.ContainsKey(tagType))
                {
                    tags.TierTags.Remove(tagType);
                }
            }
        }

        private void OnRemoveAllTagsButton(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("REMOVE ALL?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var source = this.FilterAccessFacade.PrimaryFilter.FilterEntries.Where(x => x.Header.Type == FilterGenerationConfig.FilterEntryType.Content);
                foreach (var entry in source)
                {
                    entry.Header.TierTags.TierTags.Clear();
                }
            }
        }

        private void OnRemoveUnknownTagsButton(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Remove ALL UNKNOWN?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var source = this.FilterAccessFacade.PrimaryFilter.FilterEntries.Where(x => x.Header.Type == FilterGenerationConfig.FilterEntryType.Content);
                foreach (var entry in source)
                {
                    var tags = entry.Header.TierTags;

                    List<string> toBeRemove = new List<string>();
                    foreach (var tag in tags.TierTags)
                    {
                        if (!FilterGenerationConfig.TierTagSort.ContainsKey(tag.Key))
                        {
                            toBeRemove.Add(tag.Key);
                        }
                    }

                    foreach (var item in toBeRemove)
                    {
                        tags.TierTags.Remove(item);
                    }
                }
            }
        }

        private void RemoveLocalUnknownTags(object sender, RoutedEventArgs e)
        {
            foreach (var entry in this.SelectedEntries)
            {
                var tags = entry.Header.TierTags;

                List<string> toBeRemove = new List<string>();
                foreach (var tag in tags.TierTags)
                {
                    if (!FilterGenerationConfig.TierTagSort.ContainsKey(tag.Key))
                    {
                        toBeRemove.Add(tag.Key);
                    }
                }

                foreach (var item in toBeRemove)
                {
                    tags.TierTags.Remove(item);
                }
            }
        }
    }
}
