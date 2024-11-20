using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.Gui.ObjectProperties;
using GisResources = DeltaShell.Plugins.SharpMapGis.Gui.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui
{
    [ResourcesDisplayName(typeof(Resources), "OverwriteLabelOperationProperties_DisplayName")]
    public class OverwriteLabelOperationProperties : SpatialOperationProperties<OverwriteLabelOperation>
    {
        [PropertyOrder(3)]
        [ResourcesCategory(typeof(GisResources), "Categories_Operation_Parameters")]
        [ResourcesDisplayName(typeof(GisResources), "OverwriteValueOperation_X_DisplayName")]
        [ResourcesDescription(typeof(GisResources), "OverwriteValueOperation_X_Description")]
        public double X
        {
            get => data.X;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    return;
                }

                data.X = value;
            }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(GisResources), "Categories_Operation_Parameters")]
        [ResourcesDisplayName(typeof(GisResources), "OverwriteValueOperation_Y_DisplayName")]
        [ResourcesDescription(typeof(GisResources), "OverwriteValueOperation_Y_Description")]
        public double Y
        {
            get => data.Y;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    return;
                }

                data.Y = value;
            }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(GisResources), "Categories_Operation_Parameters")]
        [ResourcesDisplayName(typeof(Resources), "OverwriteLabelOperation_Label_DisplayName")]
        [ResourcesDescription(typeof(Resources), "OverwriteLabelOperation_Label_Description")]
        public string Value
        {
            get => data.Label;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                data.Label = value;
            }
        }
    }
}