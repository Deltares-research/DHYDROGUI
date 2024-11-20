using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator;
using DelftTools.Controls.Swf.DataEditorGenerator.FromType;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class HbvDataView : UserControl, IView, IRRUnitAwareView, IRRMeteoStationAwareView, IRRTemperatureStationAwareView
    {
        private HbvData data;
        private readonly DataEditor dataEditor;

        public HbvDataView()
        {
            dataEditor = DataEditorGeneratorSwf.GenerateView(
                ObjectDescriptionFromTypeExtractor.ExtractObjectDescription(typeof (HbvData)));
            dataEditor.Dock = DockStyle.Fill;
            Controls.Add(dataEditor);
        }

        public object Data
        {
            get { return data; }
            set
            {
                data = value as HbvData;               
                if (Controls.Count != 0)
                {
                    ((DataEditor) Controls[0]).Data = value;
                }
                if (data != null)
                {
                    Text = "HBV data: " + data.Name;
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        public RainfallRunoffEnums.AreaUnit AreaUnit { set; private get; }

        public bool UseMeteoStations
        {
            set { dataEditor.GetCustomControls().OfType<HbvMeteoStationSelection>().First().UseMeteoStations = value; }
        }

        public IEventedList<string> MeteoStations
        {
            set { dataEditor.GetCustomControls().OfType<HbvMeteoStationSelection>().First().MeteoStations = value; }
        }

        public bool UseTemperatureStations
        {
            set { dataEditor.GetCustomControls().OfType<HbvMeteoStationSelection>().First().UseTemperatureStations = value; }
        }

        public IEventedList<string> TemperatureStations
        {
            set { dataEditor.GetCustomControls().OfType<HbvMeteoStationSelection>().First().TemperatureStations = value; }
        }
    }

    //note: is used!
    public class TemperatureStationControlHelper : ICustomControlHelper
    {
        public Control CreateControl()
        {
            return new HbvMeteoStationSelection();
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            ((HbvMeteoStationSelection)control).Data = (HbvData)rootObject;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }
    }
}
