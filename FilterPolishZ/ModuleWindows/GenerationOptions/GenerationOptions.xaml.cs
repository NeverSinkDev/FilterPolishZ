using FilterPolishZ.Util;
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Windows.Controls;

namespace FilterPolishZ.ModuleWindows.GenerationOptions
{
    public partial class GenerationOptions : UserControl, INotifyPropertyChanged
    {
        public static string DebugText = string.Empty;

        public EventGridFacade EventGridFacade { get; }

        public GenerationOptions()
        {
            InitializeComponent();
            this.DataContext = this;

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
            this.StructureText.AppendText(DebugText);
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
