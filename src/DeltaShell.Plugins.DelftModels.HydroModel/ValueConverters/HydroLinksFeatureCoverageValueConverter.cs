using System;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters
{
    public class HydroLinksFeatureCoverageValueConverter : HydroRegionCoverageValueConverterBase<IFeatureCoverage, IFeatureCoverage>, IExplicitValueConverter
    {
        protected override void Convert(DateTime dateTimeToUpdate = new DateTime())
        {
            // do nothing.. I hate this implicit convert stuff, and it's very error prone,
            // so I made it explicit in RR, booyah!
        }

        private IFeatureCoverage FlowInflows 
        {
            get { return OriginalValue; }
        }

        private IFeatureCoverage RRDischarges
        {
            get { return ConvertedValue; }
        }

        /// <summary>
        /// Grabs output from RR and places it in Inflows input for flow for requested timestep.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void Update(DateTime time, object value = null)
        {
            var flowBoundaries = FlowInflows.FeatureVariable.Values.OfType<IHydroObject>().ToList();
            if (flowBoundaries.Count == 0)
                return;

            // initialize the slices & other data
            var rrFeatureIndexLookup = RRDischarges.FeatureVariable.Values.Cast<IFeature>()
                                                   .Select((f, i) => new {Feat = f, Index = i})
                                                   .ToDictionary(a => a.Feat, a => a.Index);
            
            var numRRFeatures = rrFeatureIndexLookup.Count;
            if (numRRFeatures == 0)
                return;

            var flowValues = new double[flowBoundaries.Count];

            // copy the rr output values for the timeToUpdate
            var rrValues = GatherRRDischargeValues(time, numRRFeatures);

            var iFeature = 0;
            // gather the results based on the linked features
            foreach (var flowBoundary in flowBoundaries)
            {
                var valueForTarget = double.NaN; //NaN in case we have no source at all (shouldn't happen)

                // use hydro links to find the matching feature in the other coverage
                foreach (var rrFeature in flowBoundary.Links.Select(l => OtherSide(l, flowBoundary)))
                {
                    if (double.IsNaN(valueForTarget))
                        valueForTarget = 0.0; //we have at least one source: init at 0.0

                    var index = rrFeatureIndexLookup[rrFeature];
                    valueForTarget += rrValues[index];
                }

                flowValues[iFeature] = valueForTarget; //store in temp lists
                iFeature++;
            }

            var oldValue = FlowInflows.Store.FireEvents;
            try
            {
                FlowInflows.Store.FireEvents = false;
                FlowInflows.Time.Clear();
                FlowInflows[time] = flowValues;
            }
            finally
            {
                FlowInflows.Store.FireEvents = oldValue;
            }
        }

        private double[] GatherRRDischargeValues(DateTime timeToUpdate, int numRRFeatures)
        {
            var rrValues = new double[numRRFeatures];
            var diskFilteredArray = RRDischarges.Components[0]
                .GetValues<double>(new VariableValueFilter<DateTime>(RRDischarges.Time, timeToUpdate));
            diskFilteredArray.CopyTo(rrValues, 0);
            return rrValues;
        }

        protected override void OnOriginalValueModified()
        {
            // do nothing implicit
        }

        public override object DeepClone()
        {
            return new HydroLinksFeatureCoverageValueConverter
                {
                    OriginalValue = OriginalValue,
                    ConvertedValue = ConvertedValue,
                    HydroRegion = HydroRegion
                };
        }
    }
}