using System;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for PipeView.xaml
    /// </summary>
    public partial class SewerConnectionView : UserControl, IView
    {
        public SewerConnectionView()
        {
            InitializeComponent();
            
        }

        public Action<object> OpenView
        {
            set
            {
                CrossSectionPipeView.EditClickedAction = (o, e) =>
                {
                    value?.Invoke((e as SelectedItemChangedEventArgs)?.Item);
                };
            }
        }

        #region IView implementation

        public void Dispose()
        {
            OpenView = null;
        }

        public void EnsureVisible(object item)
        {
        }

        public object Data
        {
            get { return ViewModel.SewerConnection; }
            set
            {
                ViewModel.SewerConnection = (ISewerConnection)value;
            }
        }

        public RoughnessSection PipeRoughnessSection
        {
            get { return ViewModel.PipeRoughnessSection; }
            set { ViewModel.PipeRoughnessSection = value; }
        }

        public string Text { get; set; }
        public Image Image { get; set; }
        public bool Visible { get; }
        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}
