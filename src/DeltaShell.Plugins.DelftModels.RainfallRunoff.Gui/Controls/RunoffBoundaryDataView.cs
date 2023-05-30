using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class RunoffBoundaryDataView : UserControl, IView
    {
        private RunoffBoundaryData data;

        public RunoffBoundaryDataView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get { return data; }
            set
            {
                data = (RunoffBoundaryData) value;
                
                if (data != null)
                {
                    Text = data.Boundary.Name + " water level";
                    rrBoundarySeriesView1.Data = data.Series;
                }
                else
                {
                    rrBoundarySeriesView1.Data = null;
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

    }
}
