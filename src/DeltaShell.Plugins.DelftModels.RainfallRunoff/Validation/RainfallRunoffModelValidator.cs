using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffModelValidator : IValidator<RainfallRunoffModel, RainfallRunoffModel>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, RainfallRunoffModel target = null)
        {
            return new ValidationReport(rootObject.Name + " (Rainfall Runoff Model)", new[]
                {
                    new RainfallRunoffBasinValidator().Validate(rootObject, rootObject.Basin),
                    new RainfallRunoffMeteoValidator().Validate(rootObject, rootObject),
                    new RainfallRunoffCatchmentDataValidator().Validate(rootObject, rootObject.GetAllModelData()),
                    ValidateTimers(rootObject),
                    new RainfallRunoffSettingsValidator().Validate(rootObject, rootObject),
                });
        }

        public static ValidationReport ValidateTimers(RainfallRunoffModel model)
        {
            //time setting
            var validator = new ModelTimersValidator();
            return new ValidationReport("Timers", validator.ValidateModelTimers(model, model.OutputTimeStep));
        }

        public static void ValidateRunoffs(CatchmentModelData conceptData, List<ValidationIssue> issues)
        {
            var runoffs = conceptData.Catchment.Links;
            var runoff = runoffs.FirstOrDefault();

            if (runoff?.Target == conceptData.Catchment)
            {
                return;
            }

            if (runoffs.Count == 0)
            {
                // TODO: check if this validation still is correct after the fix for FM1D2D-1629
                // issues.Add(new ValidationIssue(conceptData.Catchment, ValidationSeverity.Error,
                //                                string.Format("No runoff target has been defined (concept: {0}); an implicit boundary will be used.",conceptData.GetType().Name),
                //                                conceptData.Catchment.Basin));
            }
            else if (runoff != null && !RainfallRunoffValidationHelper.IsConsideredAsBoundary(runoff.Target))
            {
                issues.Add(new ValidationIssue(conceptData.Catchment, ValidationSeverity.Error,
                                               string.Format(
                                                   "A {0} node can only be connected (downstream) to a boundary or lateral",
                                                   conceptData.GetType().Name),
                                               new ValidatedFeatures(conceptData.Catchment.Basin, conceptData.Catchment)));
            }
        }
    }
}