using System;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for PipeView.xaml
    /// </summary>
    public partial class PipeView : UserControl, IView
    {
        public PipeView()
        {
            InitializeComponent();
            
        }
        
        #region IView implementation

        public void Dispose()
        {
        }

        public void EnsureVisible(object item)
        {
        }

        public object Data
        {
            get { return ViewModel.Pipe; }
            set
            {
                ViewModel.Pipe = (IPipe)value;
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
