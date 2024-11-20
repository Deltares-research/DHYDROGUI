using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Interaction logic for RefreshMainSectionWidthsDialog.xaml
    /// </summary>
    public partial class RemoveDuplicateCalculationPointsDialog : IView
    {
        public RemoveDuplicateCalculationPointsDialog()
        {
            InitializeComponent();
            ViewModel.AfterFix = Close;
        }

        object IView.Data
        {
            get { return Data; }
            set { Data = (IEnumerable<IGrouping<Coordinate, INetworkLocation>>)value; }
        }

        public IEnumerable<IGrouping<Coordinate, INetworkLocation>> Data
        {
            get { return ViewModel.DuplicateLocationsByCoordinate; }
            set { ViewModel.DuplicateLocationsByCoordinate = value; }
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
