using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class GroupableFeature2D : Feature2D, IGroupableFeature
    {
        private string groupName;

        /// <summary>
        /// Name used to group features with the same group name together
        /// </summary>
        [FeatureAttribute]
        [DisplayName("Group name")]
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set { groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value); }
        }

        public bool IsDefaultGroup { get; set; }

        public override object Clone()
        {
            var instance = (GroupableFeature2D) base.Clone();
            return this.CloneGroupableFeature(instance);
        }
    }
}