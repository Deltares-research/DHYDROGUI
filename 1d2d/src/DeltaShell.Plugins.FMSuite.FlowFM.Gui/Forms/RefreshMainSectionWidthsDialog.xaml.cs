using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Interaction logic for RefreshMainSectionWidthsDialog.xaml
    /// </summary>
    public partial class RefreshMainSectionWidthsDialog : IView
    {
        public RefreshMainSectionWidthsDialog()
        {
            InitializeComponent();
            ViewModel.AfterFix = Close;
        }

        object IView.Data
        {
            get { return Data; }
            set { Data = (IEnumerable<ICrossSection>)value; }
        }

        public IEnumerable<ICrossSection> Data
        {
            get { return ViewModel.CrossSections; }
            set { ViewModel.CrossSections = value; }
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible { get; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public void Dispose()
        {

        }
    }
}
