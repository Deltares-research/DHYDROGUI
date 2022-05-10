using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class PavedDataValidator : IValidator<IRainfallRunoffModel, IEnumerable<PavedData>>
    {
        private const string pavedConcept = "Paved concept";
        
        public ValidationReport Validate(IRainfallRunoffModel rootObject, IEnumerable<PavedData> dataToValidate = null)
        {
            if (NothingToValidate(dataToValidate))
            {
                return ValidationReport.Empty(pavedConcept);
            }

            var issues = new List<ValidationIssue>();

            foreach (PavedData pavedData in dataToValidate)
            {
                issues.AddRange(ValidatePaved(pavedData));
            }

            return new ValidationReport(pavedConcept, issues);
        }

        private static bool NothingToValidate(IEnumerable<PavedData> dataToValidate)
        {
            return dataToValidate == null 
                   || !dataToValidate.Any();
        }

        private static IEnumerable<ValidationIssue> ValidatePaved(PavedData pavedData)
        {
            IHydroObject pavedMixedSewerLinkTarget = pavedData.MixedSewerTarget;
            IHydroObject pavedDwfSewerLinkTarget = pavedData.DwfSewerTarget;

            bool hasMixedLink = pavedMixedSewerLinkTarget != null;
            bool hasDwfLink = pavedDwfSewerLinkTarget != null;
            
            var issues = new List<ValidationIssue>();
            
            if(hasMixedLink)
            {
                issues.AddRange(EnsureThatPavedDataHasValidLinkTargetForMixedLink(pavedMixedSewerLinkTarget, pavedData));
            }
            else
            {
                issues.AddRange(EnsureThatPavedDataHasAMixedLink(pavedData));
            }

            issues.AddRange(EnsureThatPavedCatchmentDoesNotHaveMultipleLinksToSameType(pavedData));

            if (IsMixedSystem(pavedData.SewerType))
            {
                return issues;
            }

            if (!hasDwfLink)
            {
                issues.AddRange(EnsureThatPavedDataHasADryWeatherFlowLink(pavedData));
            }

            issues.AddRange(EnsureThatPavedDataHasValidLinkTargetForDryWeatherFlowLink(pavedDwfSewerLinkTarget, pavedData));

            return issues;
        }

        private static IEnumerable<ValidationIssue> EnsureThatPavedDataHasValidLinkTargetForMixedLink(
            IHydroObject target, PavedData pavedData)
        {
            if (!HasValidLink(target))
            {
                yield return new ValidationIssue(pavedData.Catchment, 
                                                 ValidationSeverity.Error, 
                                                 Resources.PavedDataValidation_Mixed_runoff_has_no_valid_target_for_link, 
                                                 new ValidatedFeatures(pavedData.Catchment.Basin, pavedData.Catchment));
            }
        }

        private static bool HasValidLink(IHydroObject target)
        {
            return IsConsideredBoundary(target) 
                   || IsOpenWaterCatchment(target) 
                   || IsWasteWaterTreatmentPlant(target);
        }

        private static bool IsConsideredBoundary(IHydroObject target)
        {
            return RainfallRunoffValidationHelper.IsConsideredAsBoundary(target);
        }

        private static bool IsOpenWaterCatchment(IHydroObject target)
        {
            return target is Catchment catchment && catchment.CatchmentType.Equals(CatchmentType.OpenWater);
        }
        
        private static bool IsWasteWaterTreatmentPlant(IHydroObject target)
        {
            return target is WasteWaterTreatmentPlant;
        }
        
        private static bool IsMixedSystem(PavedEnums.SewerType sewerType)
        {
            return sewerType == PavedEnums.SewerType.MixedSystem;
        }

        private static IEnumerable<ValidationIssue> EnsureThatPavedDataHasAMixedLink(PavedData pavedData)
        {
            yield return new ValidationIssue(pavedData.Catchment, 
                                             ValidationSeverity.Error,
                                             Resources.PavedDataValidation_No_target_for_mixed_flow, 
                                             new ValidatedFeatures(pavedData.Catchment.Basin, pavedData.Catchment));
        }

        private static IEnumerable<ValidationIssue> EnsureThatPavedDataHasADryWeatherFlowLink(PavedData pavedData)
        {
            yield return new ValidationIssue(pavedData.Catchment, 
                                             ValidationSeverity.Error,
                                             Resources.PavedDataValidation_No_target_for_dry_weather_flow,
                                             new ValidatedFeatures(pavedData.Catchment.Basin, pavedData.Catchment));
        }

        private static IEnumerable<ValidationIssue> EnsureThatPavedDataHasValidLinkTargetForDryWeatherFlowLink(
            IHydroObject target, PavedData pavedData)
        {
            if (!HasValidLink(target))
            {
                yield return new ValidationIssue(pavedData.Catchment, 
                                                 ValidationSeverity.Error, 
                                                 Resources.PavedDataValidation_Dry_weather_flow_link_has_no_valid_target, 
                                                 new ValidatedFeatures(pavedData.Catchment.Basin, pavedData.Catchment));
            }
        }
        
        private static IEnumerable<ValidationIssue> EnsureThatPavedCatchmentDoesNotHaveMultipleLinksToSameType(PavedData pavedData)
        {
            Catchment catchment = pavedData.Catchment;
            if (catchment == null)
            {
                yield break;
            }
            
            IEnumerable<HydroLink> links = catchment.Links;

            if (!links.Any())
            {
                yield break;
            }

            if (links.Count(link => IsWasteWaterTreatmentPlant(link.Target)) > 1)
            {
                yield return new ValidationIssue(pavedData.Catchment, 
                                                 ValidationSeverity.Error,
                                                 Resources.PavedDataValidation_Cant_link_to_multiple_waste_water_treatment_plants, 
                                                 new ValidatedFeatures(pavedData.Catchment.Basin, pavedData.Catchment));
            }
            
            if (links.Count(link => IsConsideredBoundary(link.Target)) > 1)
            {
                yield return new ValidationIssue(pavedData.Catchment, 
                                                 ValidationSeverity.Error,
                                                 Resources.PavedDataValidation_Cant_link_to_multiple_boundaries, 
                                                 new ValidatedFeatures(pavedData.Catchment.Basin, pavedData.Catchment));
            }
        }
    }
}