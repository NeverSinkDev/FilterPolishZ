using FilterPolishZ.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Windows.Controls;

namespace FilterPolishZ.ModuleWindows.GenerationOptions
{
    public partial class GenerationOptions : UserControl, INotifyPropertyChanged
    {
        public static string CurrentKey = "empty";
        public static Dictionary<string, string> TextSources = new Dictionary<string, string>();
        public ObservableCollection<string> DebugKeys = new ObservableCollection<string>();

        public EventGridFacade EventGridFacade { get; }

        public GenerationOptions()
        {
            InitializeComponent();
            this.DataContext = this;

            if (!TextSources.ContainsKey("empty"))
            {
                TextSources.Add("empty", "");
            }

            this.EventGridFacade = EventGridFacade.GetInstance();
            this.EventGridFacade.FilterChangeEvent += EventGridFacade_UpdateDebugText;
            this.Update();
        }

        private void EventGridFacade_UpdateDebugText(object sender, EventArgs e)
        {
            this.Update();
        }

        public void Update()
        {
            if (!TextSources.ContainsKey("empty"))
            {
                TextSources.Add("empty", "");
                CurrentKey = "empty";
            }

            DebugKeys = new ObservableCollection<string>(TextSources.Keys.ToList());

            this.StructureText.Document.Blocks.Clear();
            this.StructureText.AppendText(TextSources[CurrentKey]);

            this.SelectionKeys.Items.Clear();

            foreach (var item in DebugKeys)
            {
                this.SelectionKeys.Items.Add(item);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void Refresh(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectionKeys?.SelectedItem == null)
            {
                return;
            }

            CurrentKey = this.SelectionKeys.SelectedItem.ToString();
            this.SelectionKeys.SelectedValue = CurrentKey;
            Update();
        }
    }
}
