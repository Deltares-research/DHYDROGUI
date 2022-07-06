using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro.Roughness
{
    /// <summary>
    /// Custom network coverage to store roughness and roughness type
    /// extra limitation is only roughness type supported per branch
    /// </summary>
    public class RoughnessNetworkCoverage : NetworkCoverage
    {
        internal void UpdateCoverageName(string newName)
        {
            Name = newName;
            Components[0].Name = newName;
        }

        private const int RoughnessValueComponentIndex = 0;
        private const int RoughnessTypeComponentIndex = 1;

        //parameterless constructor needed for (IFunction) Activator.CreateInstance(GetType())
        public RoughnessNetworkCoverage()
        {
        }

        public RoughnessNetworkCoverage(string name, bool isTimeDependent, string outputUnit = "-") 
            : base(name, isTimeDependent, name, outputUnit)
        {
            Components.Add(new Variable<int>("roughnessType"));
            DefaultRoughnessType = RoughnessType.Chezy;
            DefaultValue = 45.0;
            InterpolateAcrossNodes = false;
        }


        public double EvaluateRoughnessValue(INetworkLocation networkLocation)
        {
            return Evaluate(networkLocation);
        }

        public virtual RoughnessType DefaultRoughnessType
        {
            get { return (RoughnessType)Components[RoughnessTypeComponentIndex].DefaultValue; }
            set { Components[RoughnessTypeComponentIndex].DefaultValue = (int)value; }
        }

        public RoughnessType EvaluateRoughnessType(INetworkLocation networkLocation)
        {
            //find a location on the same branch
            var location = Locations.Values.FirstOrDefault(l => l.Branch == networkLocation.Branch);
            if (location == null)
            {
                return DefaultRoughnessType;
            }

            var value = RoughnessTypeComponent[location];
            return (RoughnessType)value;
        }

        public override void SetValues(IEnumerable values, params IVariableFilter[] filters)
        {
            if (filters.Length > 1)
            {
                throw new ArgumentException("SetValues only supports 1 filter");
            }

            // In the first implementation it was thought that only one RoughnessType was allowed per branch 
            // but this not true for YZ cross sections. They are completely unrelated and can ony be 
            // interpolated as block. Chech should be done when initializing ModelApi.
            base.SetValues(values, filters);
        }

        public IVariable RoughnessTypeComponent
        {
            get { return Components[RoughnessTypeComponentIndex]; }
        }

        public IVariable RoughnessValueComponent
        {
            get { return Components[RoughnessValueComponentIndex]; }
        }

        protected override void UpdateValuesForBranchSplit(BranchSplitAction currentEditAction)
        {
            double splitAtOffset = currentEditAction.SplittedBranch.Length;
            var splitLocation = new NetworkLocation(currentEditAction.SplittedBranch, splitAtOffset);

            var roughnessValue = EvaluateRoughnessValue(splitLocation);
            int roughnessType = (int)EvaluateRoughnessType(splitLocation);

            IEnumerable<INetworkLocation> networkLocationsToMove = Locations.Values.Where(
                nl => nl.Branch == currentEditAction.SplittedBranch && nl.Chainage >= splitAtOffset)
                .ToList();

            //a move of location moves the component values along
            foreach (var location in networkLocationsToMove)
            {
                location.Branch = currentEditAction.NewBranch;
                location.Chainage = BranchFeature.SnapChainage(currentEditAction.NewBranch.Length, location.Chainage - splitAtOffset);
            }

            //add a point at the end of the original branch 
            var startLocation = new NetworkLocation(currentEditAction.NewBranch, 0);
            RoughnessTypeComponent[startLocation] = roughnessType;
            RoughnessTypeComponent[splitLocation] = roughnessType;

            RoughnessValueComponent[startLocation] = roughnessValue;
            RoughnessValueComponent[splitLocation] = roughnessValue;
        }

        protected override void LocationsValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            // Add a value based on other values on the branch
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                InterpolateValuesForNewLocation(e);
            }
            base.LocationsValuesChanged(sender, e); // <- This call should not be wrapped in EditAction!
        }

        public bool SkipInterpolationForNewLocation { get; set; } = false;

        [EditAction]
        private void InterpolateValuesForNewLocation(FunctionValuesChangingEventArgs e)
        {
            // in case just a location is added (without values), this method makes sure the corresponding 
            // values are quite sane.

            if (SkipInterpolationForNewLocation)
                return;

            //assumes a single value
            var networkLocation = (NetworkLocation) e.Items[0];

            var branch = networkLocation.Branch;
            var otherLocation = Locations.Values.FirstOrDefault(nl => nl.Branch == branch);
            if (otherLocation != null)
            {
                this[networkLocation.Clone()] =
                    new[]
                        {
                            EvaluateRoughnessValue(otherLocation),
                            RoughnessTypeComponent[otherLocation]
                        };
            }
        }
    }
}