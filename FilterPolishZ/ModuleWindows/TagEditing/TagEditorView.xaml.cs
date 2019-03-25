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

namespace FilterPolishZ.ModuleWindows.TagEditing
{
    /// <summary>
    /// Interaction logic for TagEditorView.xaml
    /// </summary>
    public partial class TagEditorView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FilterAccessFacade FilterAccessFacade { get; set; } = FilterAccessFacade.GetInstance();
        public ObservableCollection<IFilterCategoryEntity> FilterTree = new ObservableCollection<IFilterCategoryEntity>();
        //public ObservableCollection

        public TagEditorView()
        {
            this.InitializeTagLogic();
            InitializeComponent();
        }

        private void InitializeTagLogic()
        {
            foreach (var item in FilterTree)
            {

            }
        }
    }
}
