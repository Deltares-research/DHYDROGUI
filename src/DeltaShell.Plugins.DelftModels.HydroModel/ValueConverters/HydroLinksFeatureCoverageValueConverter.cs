using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters
{
    public class HydroLinksFeatureCoverageValueConverter : HydroRegionCoverageValueConverterBase<IFeatureCoverage, IFeatureCoverage>, IExplicitValueConverter
    {
        /// <summary>
        /// Grabs output from RR and places it in Inflows input for flow for requested timestep.
        /// </summary>
        /// <param name="time">The time to update</param>
        /// <param name="value">The value, currently unused</param>
        /// <remarks>
        /// <paramref name="value"/> is currently unused.
        /// </remarks>
        public void Update(DateTime time, object value = null)
        {
            // TODO: D3DFMIQ-2083
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

        protected override void Convert(DateTime dateTimeToUpdate = new DateTime())
        {
            // do nothing.. I hate this implicit convert stuff, and it's very error prone,
            // so I made it explicit in RR, booyah!
        }

        protected override void OnOriginalValueModified()
        {
            // do nothing implicit
        }

        private IFeatureCoverage FlowInflows
        {
            get
            {
                return OriginalValue;
            }
        }
    }
}