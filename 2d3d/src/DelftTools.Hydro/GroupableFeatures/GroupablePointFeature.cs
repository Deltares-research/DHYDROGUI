using System.ComponentModel;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro.GroupableFeatures
{
    [Entity]
    public class GroupablePointFeature : PointFeature, IGroupableFeature
    {
        private string groupName;

        /// <summary>
        /// Name used to group features with the same group name together
        /// </summary>
        [FeatureAttribute]
        [DisplayName("Group name")]
        public string GroupName
        {
            get => groupName;
            set => groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value);
        }

        public bool IsDefaultGroup { get; set; }

        public override object Clone()
        {
            var instance = new GroupablePointFeature()
            {
                Geometry = Geometry,
                Attributes = Attributes?.Clone() as IFeatureAttributeCollection
            };
            return this.CloneGroupableFeature(instance);
        }
    }
}