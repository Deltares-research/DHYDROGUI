using System.ComponentModel;
using DelftTools.Shell.Gui;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Spatial data")]
    public class WaterQualityModelUnstructuredGridCellCoverageProperties : ObjectProperties<UnstructuredGridCellCoverage>
    {
        [Category("General")]
        [DisplayName("Name")]
        [Description("Name of the time series")]
        public string Name
        {
            get { return data.Name; }
        }

        [Category("General")]
        [DisplayName("Default value")]
        [Description("Default value when no data is available")]
        public double DefaultValue
        {
            get { return (double) data.Components[0].DefaultValue; }
        }

        [Category("General")]
        [DisplayName("Unit")]
        [Description("Unit of the time series values")]
        public string Unit
        {
            get { return data.Components[0].Unit.Symbol; }
        }
    }
}