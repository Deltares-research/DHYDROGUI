using System.Windows.Controls;
using DelftTools.Controls;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views
{
    /// <summary>
    /// Interaction logic for WaveBoundaryConditionEditorView.xaml
    /// </summary>
    public partial class WaveBoundaryConditionEditorView : UserControl, IView
    {
        public WaveBoundaryConditionEditorView()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
        }

        public void EnsureVisible(object item)
        {
        }

        public object Data { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }
    }
}
