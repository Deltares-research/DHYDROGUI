using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    /// <summary>
    /// Validator for <see cref="IDrainageBasin"/>.
    /// </summary>
    public class RainfallRunoffBasinValidator : IValidator<RainfallRunoffModel, IDrainageBasin>
    {
        /// <summary>
        /// Validate a <see cref="IDrainageBasin"/>.
        /// </summary>
        /// <param name="rootObject">The model containing the provided drainage basin.</param>
        /// <param name="target">The drainage basin to validate.</param>
        /// <returns>A <see cref="ValidationReport"/> containing the results of the validation.</returns>
        public ValidationReport Validate(RainfallRunoffModel rootObject, IDrainageBasin target)
        {
            var issues = new List<ValidationIssue>();

            ValidateCatchments(target, issues);
            ValidateLinks(target, issues);
            ValidateWasteWaterTreatmentPlants(target, issues);
            ValidateRunoffBoundaries(target, issues);

            return new ValidationReport("Basin", issues);
        }

        private static void ValidateRunoffBoundaries(IDrainageBasin target, List<ValidationIssue> issues)
        {
            issues.AddRange(ValidationHelper.ValidateDuplicateNames(target.Boundaries, "runoff boundaries", target));
        }

        private static void ValidateWasteWaterTreatmentPlants(IDrainageBasin target, List<ValidationIssue> issues)
        {
            issues.AddRange(ValidationHelper.ValidateDuplicateNames(target.WasteWaterTreatmentPlants, "wastewater treatment plants", target));

            foreach (WasteWaterTreatmentPlant wwtp in target.WasteWaterTreatmentPlants)
            {
                if (wwtp.Links.Count(l => Equals(wwtp, l.Target)) == 0)
                {
                    issues.Add(new ValidationIssue(wwtp,
                                                   ValidationSeverity.Error,
                                                   Resources.RainfallRunoffBasinValidator_WWTP_has_no_incoming_runoff_links,
                                                   new ValidatedFeatures(target, wwtp)));
                }

                if (wwtp.Links.Count(l => Equals(wwtp, l.Source)) == 0)
                {
                    issues.Add(new ValidationIssue(wwtp,
                                                   ValidationSeverity.Error,
                                                   Resources.RainfallRunoffBasinValidator_WWTP_has_no_outgoing_runoff_links,
                                                   new ValidatedFeatures(target, wwtp)));
                }
                else if (wwtp.Links.Count(l => Equals(wwtp, l.Source)) > 1)
                {
                    issues.Add(new ValidationIssue(wwtp,
                                                   ValidationSeverity.Error,
                                                   Resources.RainfallRunoffBasinValidator_WWTP_has_more_than_one_outoging_runoff_links,
                                                   new ValidatedFeatures(target, wwtp)));
                }
            }
        }

        private static void ValidateLinks(IHydroRegion basin, List<ValidationIssue> issues)
        {
            List<HydroLink> links = basin.Links.ToList();

            if (basin.Parent is IHydroRegion hydroRegion)
            {
                links.AddRange(hydroRegion.Links);
            }

            ValidateDuplicateLinkNames(links, basin, issues);
            ValidateDuplicateLinkSourceAndTargetNames(links, issues);
        }

        private static void ValidateDuplicateLinkSourceAndTargetNames(List<HydroLink> links, List<ValidationIssue> issues)
        {
            IEnumerable<IHydroObject> objectsWithDuplicateNames = GetLinkSourcesAndTargetsWithDuplicateNames(links);
            issues.AddRange(objectsWithDuplicateNames.Select(hydroObject =>
                                                                 new ValidationIssue(hydroObject,
                                                                                     ValidationSeverity.Error,
                                                                                     Resources.RainfallRunoffBasinValidator_Multiple_objects_with_same_name,
                                                                                     hydroObject)));
        }

        private static void ValidateDuplicateLinkNames(IEnumerable<HydroLink> links, IHydroRegion basin, List<ValidationIssue> issues)
        {
            issues.AddRange(ValidationHelper.ValidateDuplicateNames(links, "links", basin));
        }

        private static IEnumerable<IHydroObject> GetLinkSourcesAndTargetsWithDuplicateNames(List<HydroLink> links)
        {
            var hydroObjectsByName = new Dictionary<string, HashSet<IHydroObject>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (HydroLink hydroLink in links)
            {
                if (IsExcludedFromDuplicateNameValidation(hydroLink.Source))
                {
                    continue;
                }
                
                ProcessHydroObject(hydroObjectsByName, hydroLink.Target);
                ProcessHydroObject(hydroObjectsByName, hydroLink.Source);
            }

            IEnumerable<IHydroObject> objectsWithDuplicateNames = GetHydroObjectsWithDuplicateNames(hydroObjectsByName);

            return objectsWithDuplicateNames;
        }

        private static bool IsExcludedFromDuplicateNameValidation(IHydroObject hydroObject)
        {
            return hydroObject is Catchment catchment && Equals(catchment.CatchmentType, CatchmentType.NWRW);
        }

        private static IEnumerable<IHydroObject> GetHydroObjectsWithDuplicateNames(Dictionary<string, HashSet<IHydroObject>> hydroObjectsByName)
        {
            var objectsWithDuplicateNames = new List<IHydroObject>();

            foreach (HashSet<IHydroObject> hydroObjectCollection in hydroObjectsByName.Values)
            {
                if (hydroObjectCollection.Count > 1)
                {
                    objectsWithDuplicateNames.AddRange(hydroObjectCollection);
                }
            }

            return objectsWithDuplicateNames;
        }

        private static void ProcessHydroObject(
            IDictionary<string, HashSet<IHydroObject>> hydroObjectsByName,
            IHydroObject hydroObject)
        {
            if (!hydroObjectsByName.ContainsKey(hydroObject.Name))
            {
                hydroObjectsByName.Add(hydroObject.Name, new HashSet<IHydroObject> { hydroObject });
            }
            else
            {
                HashSet<IHydroObject> alreadyProcessedHydroObjects = hydroObjectsByName[hydroObject.Name];
                if (!alreadyProcessedHydroObjects.Contains(hydroObject))
                {
                    alreadyProcessedHydroObjects.Add(hydroObject);
                }
            }
        }

        private static void ValidateCatchments(IDrainageBasin target, List<ValidationIssue> issues)
        {
            List<Catchment> allCatchments = target.AllCatchments.ToList();

            if (allCatchments.Count == 0)
            {
                issues.Add(new ValidationIssue(target,
                                               ValidationSeverity.Error,
                                               Resources.RainfallRunoffBasinValidator_Basin_contains_no_catchments,
                                               target));
            }

            issues.AddRange(ValidationHelper.ValidateDuplicateNames(allCatchments, "catchments",
                                                                    target));
        }
    }
}