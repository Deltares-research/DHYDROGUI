using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class RRBoundaryLinkPanel : UserControl, IView
    {
        private UnpavedDataViewModel viewModel;

        public RRBoundaryLinkPanel()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// UnpavedDataViewModel for data binding
        /// </summary>
        public object Data
        {
            get => viewModel;
            set
            {
                viewModel = (UnpavedDataViewModel)value;
                bindingSourceViewModel.DataSource = viewModel;
            }
        }

        public void EnsureVisible(object item)
        {
            // Nothing to implement here. Required because this type implements the IView interface.
        }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }
    }
}