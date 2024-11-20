using System.ComponentModel;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class Gate2D : Gate, IGroupableFeature, IStructure2D
    {
        private string groupName;

        public Gate2D()
        {
        }

        public Gate2D(string name) : base(name)
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
            var instance = (Gate2D) base.Clone();
            return this.CloneGroupableFeature(instance);
        }

        public Structure2DType Structure2DType
        {
            get { return Structure2DType.Gate; }
        }
    }
}