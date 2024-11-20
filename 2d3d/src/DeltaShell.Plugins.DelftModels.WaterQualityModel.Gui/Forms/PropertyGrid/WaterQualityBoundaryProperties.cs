using System.ComponentModel;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Boundary")]
    public class WaterQualityBoundaryProperties : ObjectProperties<WaterQualityBoundary>
    {
        [Category("General")]
        [DisplayName("Location Aliases")]
        [Description("Comma separated list of location aliases. Example: bouy 1, bouy 2, factory")]
        public string LocationAliases
        {
            get => data.LocationAliases;
            set => data.LocationAliases = value;
        }
    }
}