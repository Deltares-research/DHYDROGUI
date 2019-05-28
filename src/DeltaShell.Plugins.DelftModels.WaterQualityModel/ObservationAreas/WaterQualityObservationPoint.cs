using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas
{
    [Entity]
    public class WaterQualityObservationPoint : NameablePointFeature
    {
        public const string ObservationPointTypeAttributeName = "PointType";

        [DynamicReadOnly]
        [FeatureAttribute(Order = 3)]
        public override double Z
        {
            get => base.Z;
            set => base.Z = value;
        }

        // TODO: UI logic on Domain class; Extract view model wrapper when more data is added.
        [DisplayName("Observation point type")]
        [FeatureAttribute(Order = 4, ExportName = ObservationPointTypeAttributeName)]
        public virtual ObservationPointType ObservationPointType { get; set; }

        [DynamicReadOnlyValidationMethod]
        public virtual bool CheckReadOnly(string propertyName)
        {
            if (propertyName == TypeUtils.GetMemberDescription(() => Z))
            {
                return ObservationPointType != ObservationPointType.SinglePoint;
            }

            return false;
        }
    }
}