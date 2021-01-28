using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters
{
    public class HydroRegionFeatureCoverageFromNetworkCoverageValueConverter : HydroRegionCoverageValueConverterBase<IFeatureCoverage, INetworkCoverage>, IExplicitValueConverter
    {
        readonly Dictionary<INetworkFeature, int> networkFeatureLocationIndex = new Dictionary<INetworkFeature, int>();

        //todo: add different aggregation types
        //todo: if required, add knowledge of link types (sewer, etc)
        protected override void Convert(DateTime dateTimeToUpdate = default(DateTime))
        {
            // Performance optimization simular to HydroLinksFeatureCoverageValueConverter
        }

        private IFeatureCoverage RRInputWaterLevel
        {
            get { return OriginalValue; }
        }

        private INetworkCoverage FlowOutputWaterLevel
        {
            get { return ConvertedValue; }
        }

        public void Update(DateTime time, object value = null)
        {
            var catchments = RRInputWaterLevel.FeatureVariable.Values.OfType<IHydroObject>().ToList();
            if (catchments.Count == 0)
                return;

            var timeIndex = GetActualOrPreviousTimeIndex(FlowOutputWaterLevel.Time, time);
            if (timeIndex < 0)
                return;

            var linkedFlowFeatures = catchments.Select(f =>
            {
                var networkFeatures = f.Links.Select(l => OtherSide(l, f)).OfType<INetworkFeature>().ToList();

                if (networkFeatures.Count > 1)
                {
                    throw new NotSupportedException("Converting value from two linked features is not supported yet");
                }

                return networkFeatures.FirstOrDefault();
            }).ToList();

            // Throw away features without links; They should not propagate values.
            catchments = catchments.Where((f, i) => linkedFlowFeatures[i] != null).ToList();
            linkedFlowFeatures = linkedFlowFeatures.Where(f => f != null).ToList();

            foreach (var networkFeature in linkedFlowFeatures.Where(f => !networkFeatureLocationIndex.ContainsKey(f)))
                networkFeatureLocationIndex[networkFeature] = FindIndexOfUpstreamLocation(networkFeature);
            
            var inputWaterLevels = new double[catchments.Count];
            for (var i = 0; i < catchments.Count; i++)
            {
                var locationIndex = networkFeatureLocationIndex[linkedFlowFeatures[i]];
                inputWaterLevels[i] = FlowOutputWaterLevel.GetValues<double>(
                        new VariableIndexRangeFilter(FlowOutputWaterLevel.Time, timeIndex),
                        new VariableIndexRangeFilter(FlowOutputWaterLevel.Locations, locationIndex))[0];
            }

            var oldValue = RRInputWaterLevel.Store.FireEvents;
            try
            {
                RRInputWaterLevel.Store.FireEvents = false;
                RRInputWaterLevel.Time.Clear();
                RRInputWaterLevel[time] = inputWaterLevels;
            }
            finally
            {
                RRInputWaterLevel.Store.FireEvents = oldValue;
            }
        }

        private int FindIndexOfUpstreamLocation(INetworkFeature networkFeature)
        {
            var locations = ConvertedValue.Locations.Values;
            if (locations.Count == 0)
            {
                return -1;
            }

            var networkLocation = networkFeature.ToNetworkLocation();

            for (var i = 0; i < locations.Count; ++i)
            {
                if (!Equals(locations[i].Branch, networkLocation.Branch))
                {
                    continue;
                }

                if (locations[i].Chainage > networkLocation.Chainage)
                {
                    return i == 0 ? 0 : i - 1;
                }
            }

            return locations.Count - 1;
        }

        protected override void ConvertedValueSecondArgumentValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            networkFeatureLocationIndex.Clear(); // clear cache
        }

        protected override void Initialize()
        {
            base.Initialize();

            networkFeatureLocationIndex.Clear();
        }

        public override object DeepClone()
        {
            return new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
                {
                    OriginalValue = OriginalValue,
                    ConvertedValue = ConvertedValue,
                    HydroRegion = HydroRegion
                };
        }
    }
}