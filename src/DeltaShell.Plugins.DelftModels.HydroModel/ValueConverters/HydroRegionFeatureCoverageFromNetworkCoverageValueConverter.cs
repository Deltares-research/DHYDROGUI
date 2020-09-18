using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters
{
    public class HydroRegionFeatureCoverageFromNetworkCoverageValueConverter : HydroRegionCoverageValueConverterBase<IFeatureCoverage, INetworkCoverage>, IExplicitValueConverter
    {
        private readonly Dictionary<INetworkFeature, int> networkFeatureLocationIndex = new Dictionary<INetworkFeature, int>();

        public void Update(DateTime time, object value = null)
        {
            // TODO D3DFMIQ-2083
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

        //todo: add different aggregation types
        //todo: if required, add knowledge of link types (sewer, etc)
        protected override void Convert(DateTime dateTimeToUpdate = default(DateTime))
        {
            // Performance optimization simular to HydroLinksFeatureCoverageValueConverter
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

        private IFeatureCoverage RRInputWaterLevel
        {
            get
            {
                return OriginalValue;
            }
        }

        private INetworkCoverage FlowOutputWaterLevel
        {
            get
            {
                return ConvertedValue;
            }
        }

        private int FindIndexOfUpstreamLocation(INetworkFeature networkFeature)
        {
            IMultiDimensionalArray<INetworkLocation> locations = ConvertedValue.Locations.Values;
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
    }
}