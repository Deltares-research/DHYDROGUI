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

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class SacramentoDataView : UserControl, IView, IRRUnitAwareView, IRRMeteoStationAwareView
    {
        private SacramentoData data;
        private readonly DataEditor dataEditor;

        public SacramentoDataView()
        {
            dataEditor = DataEditorGeneratorSwf.GenerateView(
                ObjectDescriptionFromTypeExtractor.ExtractObjectDescription(typeof (SacramentoData)));
            dataEditor.Dock = DockStyle.Fill;
            Controls.Add(dataEditor);
        }

        public object Data
        {
            get { return data; }
            set 
            {
                data = (SacramentoData) value;               
                if (Controls.Count != 0)
                {
                    ((DataEditor) Controls[0]).Data = value;
                }
                if (data != null)
                {
                    Text = "Sacramento data: " + data.Name;
                }
            }
        }
        public Image Image { get; set; }
        public void EnsureVisible(object item){}
        public ViewInfo ViewInfo { get; set; }

        public RainfallRunoffEnums.AreaUnit AreaUnit { set; private get; }

        public bool UseMeteoStations
        {
            set { dataEditor.GetCustomControls().OfType<CatchmentMeteoStationSelection>().First().UseMeteoStations = value; }
        }

        public IEventedList<string> MeteoStations
        {
            set { dataEditor.GetCustomControls().OfType<CatchmentMeteoStationSelection>().First().MeteoStations = value; }
        }
    }

    //note: is used!
    public class MeteoStationControlHelper : ICustomControlHelper
    {
        public Control CreateControl()
        {
            return new CatchmentMeteoStationSelection();
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            ((CatchmentMeteoStationSelection)control).CatchmentModelData = (CatchmentModelData)rootObject;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }
    }

    //note: is used!
    public class SacramentoCapacitiesControlHelper: ICustomControlHelper
    {
        public Control CreateControl()
        {
            return new SacramentoCapacitiesControl();
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            ((SacramentoCapacitiesControl) control).Data = (SacramentoData) rootObject;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }
    }

    //note: is used!
    public class SacramentoUnitHydrographControlHelper : ICustomControlHelper
    {
        public Control CreateControl()
        {
            return new SacramentoUnitHydrographControl();
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            ((SacramentoUnitHydrographControl)control).Data = (SacramentoData)rootObject;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }
    }
}
