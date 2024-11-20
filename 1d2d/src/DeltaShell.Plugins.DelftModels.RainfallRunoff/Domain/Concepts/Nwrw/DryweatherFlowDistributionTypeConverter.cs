using System;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Converts a dryweather flow distribution type
    /// string as found in GWSW files to a valid
    /// <see cref="DryweatherFlowDistributionType"/>.
    /// </summary>
    public static class DryweatherFlowDistributionTypeConverter
    {
        /// <summary>
        /// Converts a string to a <see cref="DryweatherFlowDistributionType"/>. />
        /// </summary>
        /// <param name="distributionTypeString">The distribution type as a string.</param>
        /// <returns>The corresponding <see cref="DryweatherFlowDistributionType"/>.</returns>
        /// <exception cref="InvalidOperationException">When unknown string is provided.</exception>
        public static DryweatherFlowDistributionType ConvertStringToDryweatherFlowDistributionType(string distributionTypeString)
        {
            switch (distributionTypeString.ToLower())
            {
                case "cst":
                    return DryweatherFlowDistributionType.Constant;
                case "dag":
                    return DryweatherFlowDistributionType.Daily;
                case "var":
                    return DryweatherFlowDistributionType.Variable;
                default:
                   throw new InvalidOperationException(
                       Properties.Resources._0__is_not_a_valid_dryweather_flow_distribution_type);
            }
        }
    }
}