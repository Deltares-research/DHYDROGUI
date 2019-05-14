using System.ComponentModel;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity]
    public class LateralSourceGridCell : BranchFeatureHydroObject
    {
        public LateralSourceGridCell()
        {
            Name = "sourcelocation";
        }

        public static LateralSourceGridCell CreateDefault(IBranch branch, double chainage, ILateralSource lateralSource)
        {
            var lateralSourceLocation = new LateralSourceGridCell()
            {
                Branch = branch,
                Chainage = chainage,
                LateralSource = lateralSource
            };
            lateralSourceLocation.Name =
                HydroNetworkHelper.GetUniqueFeatureName(lateralSourceLocation.Network as HydroNetwork,
                                                        lateralSourceLocation);
            return lateralSourceLocation;
        }

        [Aggregation]
        public virtual ILateralSource LateralSource { get; set; }

        public override object Clone()
        {
            var clone = (LateralSourceGridCell) base.Clone();
            clone.LateralSource = LateralSource;
            clone.Branch = Branch;
            return clone;
        }

        public virtual IHydroNetwork HydroNetwork => Network as IHydroNetwork;

        [DisplayName("LongName")]
        [FeatureAttribute]
        public virtual string LongName
        {
            get => Description;
            set => SetLongNameToDescription(value);
        }

        [EditAction]
        private void SetLongNameToDescription(string value)
        {
            Description = value;
        }

        public override bool CanBeLinkTarget => false;
    }
}