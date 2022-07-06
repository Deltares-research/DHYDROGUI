using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Roughness
{
    public class RoughnessSectionChangeBranchFunction : EditActionBase
    {
        public RoughnessSectionChangeBranchFunction(IBranch branch, RoughnessSection roughnessSection, RoughnessFunction from, RoughnessFunction to)
            : base("Branch roughness dependency type changed")
        {
            Branch = branch;
            RoughnessSection = roughnessSection;
            RoughnessFunctionFrom = from;
            RoughnessFunctionTo = to;
            Name = string.Format("Roughness dependency ({0}): {1} -> {2}", branch, RoughnessFunctionFrom, RoughnessFunctionTo);
        }

        public IBranch Branch { get; set; }

        public RoughnessSection RoughnessSection { get; set; }

        public RoughnessFunction RoughnessFunctionFrom { get; set; }

        public RoughnessFunction RoughnessFunctionTo { get; set; }

        public override bool HandlesRestore
        {
            get { return true; }
        }

        public override void Restore()
        {
            // create reverse action
            RoughnessSection.ChangeBranchFunction(Branch, RoughnessFunctionFrom);
        }

        public override bool SuppressEventBasedRestore
        {
            get { return true; }
        }
    }
}