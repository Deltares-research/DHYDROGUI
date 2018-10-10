using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.NetworkEditor.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class OpenWaterDataView : UserControl, IView, IRRMeteoStationAwareView
    {
        private OpenWaterData data;

        public OpenWaterDataView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get { return data; }
            set 
            {
                if (data != null)
                {
                    catchmentMeteoStationSelection1.CatchmentModelData = null;
                    catchmentMeteoStationSelection1.MeteoStations = null;
                }
                data = (OpenWaterData) value;
                if (data != null)
                {
                    Text = "Open water data: " + data.Name;
                    catchmentMeteoStationSelection1.CatchmentModelData = data;
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }


        public bool UseMeteoStations { set { catchmentMeteoStationSelection1.UseMeteoStations = value; } }
        public IEventedList<string> MeteoStations { set { catchmentMeteoStationSelection1.MeteoStations = value; } }
    }
}