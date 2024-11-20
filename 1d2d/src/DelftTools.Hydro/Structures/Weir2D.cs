using System.ComponentModel;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class Weir2D : Weir, IGroupableFeature, IStructure2D
    {
        private string groupName;

        public Weir2D(): this(false)
        {
        }

        public Weir2D(bool allowTimeVaryingData) :this("Weir", allowTimeVaryingData) { }


        public Weir2D(string name, bool allowTimeVaryingData = false) : base(name, allowTimeVaryingData)
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
            var instance = (Weir2D) base.Clone();
            return this.CloneGroupableFeature(instance);
        }

        public Structure2DType Structure2DType
        {
            get
            {
                return WeirFormula is GeneralStructureWeirFormula
                    ? Structure2DType.GeneralStructure
                    : Structure2DType.Weir;
            }
        }
    }
}