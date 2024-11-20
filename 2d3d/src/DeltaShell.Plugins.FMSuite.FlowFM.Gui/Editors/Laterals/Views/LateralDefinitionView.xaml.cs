using System.Drawing;
using DelftTools.Controls;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.Views
{
    /// <summary>
    /// Interaction logic for LateralDefinitionView.xaml
    /// </summary>
    public sealed partial class LateralDefinitionView : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LateralDefinitionView"/> class.
        /// </summary>
        public LateralDefinitionView()
        {
            InitializeComponent();
            MultipleFunctionView.TableView.ColumnAutoWidth = true;
        }

        /// <summary>
        /// The data; a <see cref="DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals.Lateral"/>.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The view title; the name of the <see cref="DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals.Lateral"/>.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Not used; <c>null</c>.
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// The <see cref="ViewInfo"/> object corresponding with this view.
        /// </summary>
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            MultipleFunctionView.Dispose();
            WindowsFormsHost.Dispose();
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void EnsureVisible(object item)
        {
            // No specific object requires focus.
        }
    }
}