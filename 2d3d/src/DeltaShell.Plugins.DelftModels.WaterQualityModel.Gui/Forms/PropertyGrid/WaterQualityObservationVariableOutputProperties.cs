using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Output observation point / area")]
    public class
        WaterQualityObservationVariableOutputProperties : ObjectProperties<WaterQualityObservationVariableOutput>
    {
        [Category("General")]
        [DisplayName("Observation point / area")]
        [Description("Name of the observation point / area")]
        public string Name => data.Name;

        [Category("General")]
        [DisplayName("Number of output variables")]
        [Description("Number of output variables (substances and output parameters) for the observation point / area")]
        public int NoOutputVariables => data.TimeSeriesList.Count();
    }
}