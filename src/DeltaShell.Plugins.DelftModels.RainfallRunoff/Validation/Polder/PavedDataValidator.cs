using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation.Polder
{
    public class PavedDataValidator : IValidator<RainfallRunoffModel, IEnumerable<PavedData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<PavedData> target = null)
        {
            if (!target.Any())
            {
                return ValidationReport.Empty("Paved concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var pavedData in target)
            {
                issues.AddRange(ValidatePaved(pavedData));
            }

            return new ValidationReport("Paved concept", issues);
        }

        private static bool IsOpenWater(IHydroObject hydroObject) => 
            hydroObject is Catchment catchment && catchment.CatchmentType.Equals(CatchmentType.OpenWater);

        private static IEnumerable<ValidationIssue> ValidatePaved(PavedData pavedData)
        {
            var pavedMixedSewerLink = pavedData.MixedSewerTarget; //favor boundaries
            var pavedDwfSewerLink = pavedData.DwfSewerTarget; //favor wwtp?

            bool hasMixedLink = pavedMixedSewerLink != null;
            bool hasDwfLink = pavedDwfSewerLink != null;
            bool mixedToBoundary = RainfallRunoffValidationHelper.IsConnectedToBoundary(pavedMixedSewerLink);
            bool mixedToOpenWater = IsOpenWater(pavedMixedSewerLink);
            bool mixedToWwtp = pavedMixedSewerLink is WasteWaterTreatmentPlant;

            var issues = new List<ValidationIssue>();
            if (!hasMixedLink)
            {
                issues.Add(new ValidationIssue(pavedData.Catchment, ValidationSeverity.Error,
                                              "No runoff target has been defined for the paved rainfall/mixed flow, or the selected runoff type does not match any of the linked features", pavedData.Catchment.Basin));
            }

            if (hasMixedLink && !(mixedToBoundary || mixedToOpenWater || mixedToWwtp))
            {
                issues.Add(new ValidationIssue(pavedData.Catchment, ValidationSeverity.Error,
                                              "A paved node mixed sewer link can only be connected (downstream) to a boundary, open water, lateral or waste water treatment plant", pavedData.Catchment.Basin));
            }

            if (pavedData.SewerType == PavedEnums.SewerType.MixedSystem)
                return issues; //done

            if (!hasDwfLink)
            {
                issues.Add(new ValidationIssue(pavedData.Catchment, ValidationSeverity.Error,
                                              "No runoff target has been defined for the paved dry water flow", pavedData.Catchment.Basin));
                return issues;
            }
            if (!hasMixedLink)
                return issues; //don't go further, enough errors now

            bool dwfToBoundary = RainfallRunoffValidationHelper.IsConnectedToBoundary(pavedDwfSewerLink);
            bool dwfToOpenWater = IsOpenWater(pavedDwfSewerLink);
            bool dwfToWwtp = pavedDwfSewerLink is WasteWaterTreatmentPlant;

            if (!(dwfToBoundary || dwfToOpenWater || dwfToWwtp))
            {
                issues.Add(new ValidationIssue(pavedData.Catchment, ValidationSeverity.Error,
                                              "A paved node dry water flow sewer link can only be connected (downstream) to a boundary, lateral or waste water treatment plant", pavedData.Catchment.Basin));
            }

            if (((dwfToBoundary && mixedToBoundary) || (dwfToOpenWater && mixedToOpenWater) || (dwfToWwtp && mixedToWwtp)) && pavedDwfSewerLink != pavedMixedSewerLink)
            {
                //if same type, must be same object
                issues.Add(new ValidationIssue(pavedData.Catchment, ValidationSeverity.Error,
                    GetMessage(dwfToBoundary, dwfToOpenWater, dwfToWwtp), pavedData.Catchment.Basin));
            }
            return issues;
        }

        private static string GetMessage(bool toBoundary, bool toOpenWater, bool toWwtp)
        {
            const string start = "A paved node cannot be connected to multiple ";
            if (toBoundary)
            {
                return start + "boundaries (both pumps must discharge to the same, or to a waste water treatment plant)";
            }
            if (toOpenWater)
            {
                return start + "open waters";
            }
            if (toWwtp)
            {
                return start + "waste water treatment plants";
            }

            return "targets of the same type";
        }
    }
}