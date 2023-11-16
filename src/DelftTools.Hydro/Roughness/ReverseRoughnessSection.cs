using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Roughness
{
    [Entity(FireOnCollectionChange = false)]
    public class ReverseRoughnessSection : RoughnessSection
    {
        //public for clone etc
        public ReverseRoughnessSection()
        {
            
        }

        public ReverseRoughnessSection(RoughnessSection normalSection): base(normalSection.CrossSectionSectionType, normalSection.Network)
        {
            NormalSection = normalSection;
            UseNormalRoughness = true;
        }

        public override bool Reversed
        {
            get { return true; }
        }

        public virtual bool UseNormalRoughness { get; set; }

        private RoughnessSection normalSection;

        [Aggregation]
        public virtual RoughnessSection NormalSection
        {
            get { return normalSection; }
            set
            {
                if (normalSection != null)
                {
                    normalSection.RoughnessTypeChanged -= NormalSectionRoughnessTypeChanged;
                }

                normalSection = value;

                if (normalSection != null)
                {
                    normalSection.RoughnessTypeChanged += NormalSectionRoughnessTypeChanged;
                }
            }
        }

        public override void CopyFrom(object source)
        {
            var sourceSection = (ReverseRoughnessSection)source;
            UseNormalRoughness = sourceSection.UseNormalRoughness;
            base.CopyFrom(source);
            NormalSection = sourceSection.NormalSection;
        }

        private bool internalChange = false;

        protected override void RoughnessNetworkCoverageValueChanged(object sender, DelftTools.Functions.MultiDimensionalArrayChangingEventArgs e)
        {
            if (internalChange)
                return;

            if (isInKnownEditAction)
                return;

            //make sure that when a new location is added / a type is set, it takes the roughness type from the normal roughness section
            var replaceAction = e.Action == NotifyCollectionChangeAction.Replace;
            if ((e.Action == NotifyCollectionChangeAction.Add ||
                replaceAction) 
                && Equals(sender, ReverseRoughnessNetworkCoverage.RoughnessTypeComponent))
            {
                var location = ReverseRoughnessNetworkCoverage.Locations.Values[e.Index];
                var normalType = NormalSection.RoughnessNetworkCoverage.EvaluateRoughnessType(location);
                var setType = (RoughnessType)e.Items[0];
                if (normalType != setType)
                {
                    var value = replaceAction
                                    ? RoughnessNetworkCoverage.EvaluateRoughnessValue(location)
                                    : NormalSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(location);
                    internalChange = true;
                    ReverseRoughnessNetworkCoverage[location] = new object[] { value, normalType };
                    internalChange = false;
                }
                return;
            }
            base.RoughnessNetworkCoverageValueChanged(sender, e);
        }

        void NormalSectionRoughnessTypeChanged(IBranch branch, RoughnessType newType)
        {
            var relevantLocations =
                ReverseRoughnessNetworkCoverage.Locations.Values.Where(loc => loc.Branch == branch).ToList();

            if (relevantLocations.Count > 0) //if we don't have any locations for that branch, don't bother
            {
                foreach(var location in relevantLocations)
                {
                    //don't change the value, however senseless it may become
                    var currentValue = ReverseRoughnessNetworkCoverage.EvaluateRoughnessValue(location); 
                    ReverseRoughnessNetworkCoverage[location] = new object[] { currentValue, newType };
                }
            }

            //sync while were at it
            if (ReverseRoughnessNetworkCoverage.DefaultRoughnessType != NormalSection.GetDefaultRoughnessType()) // prevent events
                ReverseRoughnessNetworkCoverage.DefaultRoughnessType = NormalSection.GetDefaultRoughnessType();
            if (ReverseRoughnessNetworkCoverage.DefaultValue != NormalSection.GetDefaultRoughnessValue()) // prevent events
                ReverseRoughnessNetworkCoverage.DefaultValue = NormalSection.GetDefaultRoughnessValue();
        }

        public override DelftTools.Functions.IFunction FunctionOfH(IBranch branch)
        {
            return ShouldUseReverseRoughnessDefinitionForBranch(branch) ?
                base.FunctionOfH(branch) :
                normalSection.FunctionOfH(branch);
        }

        public override DelftTools.Functions.IFunction FunctionOfQ(IBranch branch)
        {
            return ShouldUseReverseRoughnessDefinitionForBranch(branch) ? 
                base.FunctionOfQ(branch) : 
                normalSection.FunctionOfQ(branch);
        }

        public override void SetDefaults(RoughnessType defaultType, double defaultValue)
        {
            ReverseRoughnessNetworkCoverage.DefaultRoughnessType = defaultType;
            ReverseRoughnessNetworkCoverage.DefaultValue = defaultValue;

            if (UseNormalRoughness)
            {
                if (normalSection.RoughnessNetworkCoverage.DefaultValue.Equals(defaultValue)
                    && normalSection.RoughnessNetworkCoverage.DefaultRoughnessType.Equals(defaultType))
                {
                    //nothing to do
                    return;
                }
                UseNormalRoughness = false; //we are deviating from our normal section
            }
        }

        public override double GetDefaultRoughnessValue()
        {
            return UseNormalRoughness
                ? normalSection.RoughnessNetworkCoverage.DefaultValue
                : base.GetDefaultRoughnessValue();
        }

        public override RoughnessType GetDefaultRoughnessType()
        {
            return UseNormalRoughness
                ? normalSection.RoughnessNetworkCoverage.DefaultRoughnessType
                : base.GetDefaultRoughnessType();
        }

        public override RoughnessNetworkCoverage RoughnessNetworkCoverage
        {
            get
            {
                if (UseNormalRoughness)
                {
                    return normalSection.RoughnessNetworkCoverage;
                }
                return ReverseRoughnessNetworkCoverage;
            }
        }

        private RoughnessNetworkCoverage ReverseRoughnessNetworkCoverage
        {
            get { return base.RoughnessNetworkCoverage; }
        }

        public override RoughnessType EvaluateRoughnessType(INetworkLocation location)
        {
            return ShouldUseReverseRoughnessDefinitionForBranch(location.Branch) ? 
                ReverseRoughnessNetworkCoverage.EvaluateRoughnessType(location) : 
                normalSection.EvaluateRoughnessType(location);
        }

        public override double EvaluateRoughnessValue(INetworkLocation location)
        {
            return ShouldUseReverseRoughnessDefinitionForBranch(location.Branch) ? 
                ReverseRoughnessNetworkCoverage.EvaluateRoughnessValue(location) : 
                normalSection.EvaluateRoughnessValue(location);
        }

        private bool ShouldUseReverseRoughnessDefinitionForBranch(IBranch branch)
        {
            return !UseNormalRoughness &&
                   ReverseRoughnessNetworkCoverage.Locations.Values.Any(loc => loc.Branch == branch);
        }

        public override RoughnessFunction GetRoughnessFunctionType(IBranch branch)
        {
            return ShouldUseReverseRoughnessDefinitionForBranch(branch) ? 
                base.GetRoughnessFunctionType(branch) : 
                normalSection.GetRoughnessFunctionType(branch);
        }
    }
}