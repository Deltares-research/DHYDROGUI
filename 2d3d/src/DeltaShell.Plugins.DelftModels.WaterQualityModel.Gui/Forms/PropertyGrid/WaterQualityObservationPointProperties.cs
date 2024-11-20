using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Observation point")]
    public class WaterQualityObservationPointProperties : NameblePointFeatureProperties
    {
        [Category("General")]
        [DisplayName("Observation point type")]
        [PropertyOrder(2)]
        public ObservationPointType ObservationPointType
        {
            get => ObservationPoint.ObservationPointType;
            set => ObservationPoint.ObservationPointType = value;
        }

        [DynamicVisibleValidationMethod]
        public override bool IsPropertyVisible(string propertyName)
        {
            if (propertyName == nameof(Z))
            {
                return ObservationPointType == ObservationPointType.SinglePoint;
            }

            return true;
        }

        private WaterQualityObservationPoint ObservationPoint => (WaterQualityObservationPoint) data;
    }
}