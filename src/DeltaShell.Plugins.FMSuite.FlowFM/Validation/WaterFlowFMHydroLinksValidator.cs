using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Class for validating <see cref="HydroLink"/>.
    /// </summary>
    public static class WaterFlowFMHydroLinksValidator
    {
        /// <summary>
        /// Validates the <see cref="HydroLink"/> in the <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="IWaterFlowFMModel"/> to validate.</param>
        /// <returns>The validation report.</returns>
        public static ValidationReport Validate(IWaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();
            var report = new ValidationReport("HydroLinks", issues);
            
            var hydroRegion = model.Network?.Parent as HydroRegion;
            if (hydroRegion == null)
            {
                return report;
            }

            Dictionary<string, HydroLink> linksTargetLookup = CreateLinksTargetLookup(hydroRegion.Links);
            
            Dictionary<string, Model1DLateralSourceData> lateralSourceDataLookup = 
                model.LateralSourcesData.Distinct().ToDictionary(ld => ld.Feature.Name, StringComparer.InvariantCultureIgnoreCase);
            
            issues.AddRange(ValidateThatRealtimeLateralsHaveCorrectHydroLinks(model.LateralSourcesData, linksTargetLookup));
            issues.AddRange(ValidateThatHydroLinksBetweenCatchmentAndLateralAreRealtime(hydroRegion.Links, lateralSourceDataLookup));
            
            return report;
        }

        private static Dictionary<string, HydroLink> CreateLinksTargetLookup(IEnumerable<HydroLink> hydroRegionLinks)
        {
            var linksTargetLookup = new Dictionary<string, HydroLink>(StringComparer.InvariantCultureIgnoreCase);
            
            foreach (HydroLink hydroLink in hydroRegionLinks)
            {
                string targetName = hydroLink.Target.Name;
                if (linksTargetLookup.ContainsKey(targetName))
                {
                    continue;
                }
                linksTargetLookup.Add(targetName, hydroLink);
            }

            return linksTargetLookup;
        }

        /// <summary>
        /// Validates that laterals of type realtime have a hydrolink from a catchment to a lateral.
        /// </summary>
        /// <param name="lateralSourceDatas">The laterals to validate.</param>
        /// <param name="linksTargetLookup">Hydrolink lookup.</param>
        /// <returns>A collection of validation issues.</returns>
        private static IEnumerable<ValidationIssue> ValidateThatRealtimeLateralsHaveCorrectHydroLinks(
            IEnumerable<Model1DLateralSourceData> lateralSourceDatas, 
            IReadOnlyDictionary<string, HydroLink> linksTargetLookup)
        {
            foreach (Model1DLateralSourceData lateralSourceData in lateralSourceDatas)
            {
                if (lateralSourceData.DataType != Model1DLateralDataType.FlowRealTime)
                {
                    continue;
                }
                
                LateralSource lateralSource = lateralSourceData.Feature;

                if (linksTargetLookup.TryGetValue(lateralSource.Name, out HydroLink hydroLink) && hydroLink.Source is Catchment)
                {
                    continue;
                }

                const string errorMessage = "A lateral of type realtime must have a hydrolink between a catchment and the lateral.";
                yield return new ValidationIssue(lateralSource, ValidationSeverity.Error, errorMessage);
            }
        }

        /// <summary>
        /// Validates that all hydrolinks between catchments and laterals are of type realtime.
        /// </summary>
        /// <param name="hydroLinks">The hydrolinks to validate.</param>
        /// <param name="lateralSourceDataLookup">Lateral source data lookup.</param>
        /// <returns>A collection of validation issues.</returns>
        private static IEnumerable<ValidationIssue> ValidateThatHydroLinksBetweenCatchmentAndLateralAreRealtime(
            IEnumerable<HydroLink> hydroLinks, 
            Dictionary<string, Model1DLateralSourceData> lateralSourceDataLookup)
        {
            foreach (HydroLink hydroLink in hydroLinks)
            {
                if (!(hydroLink.Source is Catchment) 
                    || !(hydroLink.Target is LateralSource lateralSource)
                    || !lateralSourceDataLookup.TryGetValue(lateralSource.Name, out Model1DLateralSourceData lateralSourceData))
                {
                    continue;
                }
                
                if (lateralSourceData.DataType != Model1DLateralDataType.FlowRealTime)
                {
                    const string errorMessage = "A hydrolink between a catchment and a lateral must be of type realtime.";
                    yield return new ValidationIssue(hydroLink, ValidationSeverity.Error, errorMessage);
                }
            }
        }
    }
}