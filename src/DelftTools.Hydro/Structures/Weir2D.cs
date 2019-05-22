using System.ComponentModel;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class Weir2D : Weir, IGroupableFeature
    {
        private string groupName;

        public Weir2D() : this(true)
        {
        }

        // The default name "Structure" will be overwritten due to the initialization of a
        // HydroAreaFeature2DCollection object for weirs2D in the NetworkEditorMapLayerProvider class. 
        public Weir2D(bool allowTimeVaryingData = true) :this("Structure", allowTimeVaryingData) { }


        public Weir2D(string name, bool allowTimeVaryingData = true) : base(name, allowTimeVaryingData)
        {
            CrestWidth = double.NaN;
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
    }
}