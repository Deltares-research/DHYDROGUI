using System.ComponentModel;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class Pump2D : Pump, IGroupableFeature, IStructure2D
    {
        private string groupName;

        public Pump2D()
        {
        }

        public Pump2D(bool canBeTimeDependent) : base(canBeTimeDependent)
        {
        }

        public Pump2D(string name, bool canBeTimeDependent = false) : base(name, canBeTimeDependent)
        {
        }

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
            var instance = (Pump2D) base.Clone();
            return this.CloneGroupableFeature(instance);
        }

        public Structure2DType Structure2DType
        {
            get { return Structure2DType.Pump; }
        }
    }
}