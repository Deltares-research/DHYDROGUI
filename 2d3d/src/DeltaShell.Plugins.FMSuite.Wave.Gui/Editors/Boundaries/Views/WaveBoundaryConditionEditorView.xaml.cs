using System.Windows.Controls;
using DelftTools.Controls;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views
{
    /// <summary>
    /// Interaction logic for WaveBoundaryConditionEditorView.xaml
    /// </summary>
    public sealed partial class WaveBoundaryConditionEditorView : UserControl, IView
    {
        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorView"/>.
        /// </summary>
        public WaveBoundaryConditionEditorView()
        {
            InitializeComponent();
        }

        public object Data { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public void Dispose()
        {
            BoundaryGeometryView.Dispose();
        }

        public void EnsureVisible(object item)
        {
            // No specific object requires focus.
        }
    }
}