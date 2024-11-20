using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.Gui.ObjectProperties;
using GisResources = DeltaShell.Plugins.SharpMapGis.Gui.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "SetLabelOperationProperties_DisplayName")]
    public class SetLabelOperationProperties : SpatialOperationProperties<SetLabelOperation>
    {
        [PropertyOrder(3)]
        [ResourcesCategory(typeof(GisResources), "Categories_Operation_Parameters")]
        [ResourcesDisplayName(typeof(GisResources), "ValueOperation_Operation_DisplayName")]
        [ResourcesDescription(typeof(GisResources), "ValueOperation_Operation_DisplayName")]
        public PointwiseOperationType Operation
        {
            get => data.OperationType;
            set => data.OperationType = value;
        }

        [PropertyOrder(4)]
        [ResourcesCategory(typeof(GisResources), "Categories_Operation_Parameters")]
        [ResourcesDisplayName(typeof(Resources), "SetLabelOperation_Label_DisplayName")]
        [ResourcesDescription(typeof(Resources), "SetLabelOperation_Label_Description")]
        public string Label
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