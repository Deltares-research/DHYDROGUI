using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

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

            issues.AddRange(ValidateThatRealtimeLateralsHaveCorrectHydroLinks(hydroRegion, model.LateralSourcesData, hydroRegion.Links.ToList()));
            issues.AddRange(ValidateThatHydroLinksBetweenCatchmentAndLateralHaveRealtimeLaterals(hydroRegion, model.LateralSourcesData.ToList()));

            return report;
        }

        private static Dictionary<string, List<HydroLink>> CreateLinksTargetLookup(IEnumerable<HydroLink> hydroRegionLinks)
        {
            var linksTargetLookup = new Dictionary<string, List<HydroLink>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (HydroLink hydroLink in hydroRegionLinks)
            {
                string targetName = hydroLink.Target.Name;
                if (linksTargetLookup.ContainsKey(targetName))
                {
                    linksTargetLookup[targetName].Add(hydroLink);
                    continue;
                }

                var hydroLinks = new List<HydroLink> { hydroLink };
                linksTargetLookup.Add(targetName, hydroLinks);
            }

            return linksTargetLookup;
        }

        /// <summary>
        /// Validates that laterals of type realtime have a hydrolink from a catchment to a lateral.
        /// </summary>
        /// <param name="region">The region for which to validate the laterals.</param>
        /// <param name="lateralSourceDatas">The laterals to validate.</param>
        /// <param name="links">The hydro links in the region that are required to perform validation.</param>
        /// <returns>A collection of validation issues.</returns>
        private static IEnumerable<ValidationIssue> ValidateThatRealtimeLateralsHaveCorrectHydroLinks(
            IHydroRegion region,
            IEnumerable<Model1DLateralSourceData> lateralSourceDatas,
            IEnumerable<HydroLink> links)
        {
            Dictionary<string, List<HydroLink>> linksTargetLookup = CreateLinksTargetLookup(links);

            foreach (Model1DLateralSourceData lateralSourceData in lateralSourceDatas)
            {
                if (lateralSourceData.DataType != Model1DLateralDataType.FlowRealTime)
                {
                    continue;
                }

                LateralSource lateralSource = lateralSourceData.Feature;

                if (linksTargetLookup.TryGetValue(lateralSource.Name, out List<HydroLink> hydroLinks) &&
                    hydroLinks.Exists(hydroLink => hydroLink.Source is Catchment))
                {
                    continue;
                }
                        
                yield return new ValidationIssue(lateralSource,
                                                 ValidationSeverity.Error,
                                                 Resources.WaterFlowFMHydroLinksValidator_Realtime_lateral_must_have_link_between_catchment_and_lateral,
                                                 new ValidatedFeatures(region, lateralSource));
            }
        }

        /// <summary>
        /// Validates that all hydrolinks between catchments and laterals are of type realtime.
        /// </summary>
        /// <param name="region"> The region of the hydro links. </param>
        /// <param name="lateralSourceDatas">The lateral source data from the model.</param>
        /// <returns>A collection of validation issues.</returns>
        private static IEnumerable<ValidationIssue> ValidateThatHydroLinksBetweenCatchmentAndLateralHaveRealtimeLaterals(
            IHydroRegion region,
            IEnumerable<Model1DLateralSourceData> lateralSourceDatas)
        {
            var lateralSourceDataLookup = new Dictionary<string, Model1DLateralSourceData>(StringComparer.InvariantCultureIgnoreCase);
            foreach (Model1DLateralSourceData model1DLateralSourceData in lateralSourceDatas.Distinct())
            {
                string featureName = model1DLateralSourceData.Feature.Name;
                if (lateralSourceDataLookup.ContainsKey(featureName))
                {
                    continue;
                }

                lateralSourceDataLookup.Add(featureName, model1DLateralSourceData);
            }

            foreach (HydroLink hydroLink in region.Links)
            {
                if (!(hydroLink.Source is Catchment)
                    || !(hydroLink.Target is LateralSource lateralSource)
                    || !lateralSourceDataLookup.TryGetValue(lateralSource.Name, out Model1DLateralSourceData lateralSourceData))
                {
                    continue;
                }

                if (lateralSourceData.DataType != Model1DLateralDataType.FlowRealTime)
                {
                    yield return new ValidationIssue(hydroLink,
                                                     ValidationSeverity.Error,
                                                     Resources.WaterFlowFMHydroLinksValidator_Hydrolink_between_catchment_and_lateral_must_be_realtime,
                                                     new ValidatedFeatures(region, hydroLink));
                }
            }
        }
    }
}