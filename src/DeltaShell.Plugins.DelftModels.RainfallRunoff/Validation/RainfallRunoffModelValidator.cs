using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.Restart;
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
                    ValidateRestartTimeRangeSettingsDimr(rootObject),
                    RestartTimeRangeValidator.ValidateRestartTimeRangeSettings(rootObject.UseSaveStateTimeRange,
                                                                               rootObject.SaveStateStartTime,
                                                                               rootObject.SaveStateStopTime,
                                                                               rootObject.SaveStateTimeStep,
                                                                               rootObject),
                    ValidateRestartInput(rootObject)
                });
        }

        private ValidationReport ValidateRestartTimeRangeSettingsDimr(RainfallRunoffModel model)
        {
            var issues = new List<ValidationIssue>();
            if (!(model.WriteRestart && model.UseSaveStateTimeRange)) return new ValidationReport("Dimr intermediate restart files", issues);
            issues.Add(new ValidationIssue("Dimr restart files", ValidationSeverity.Error, "Currently, Rainfall Runoff models cannot create intermediate restart files. At the moment, a single restart file may only be written for the final time-step after a complete run."));
            return new ValidationReport("Dimr intermediate restart files", issues);
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

            if (runoffs.Count == 0)
            {
                issues.Add(new ValidationIssue(conceptData.Catchment, ValidationSeverity.Warning,
                                               string.Format("No runoff target has been defined (concept: {0}); an implicit boundary will be used.",conceptData.GetType().Name),
                                               conceptData.Catchment.Basin));
            }
            else if (runoff != null && !RainfallRunoffValidationHelper.IsConnectedToBoundary(runoff.Target))
            {
                issues.Add(new ValidationIssue(conceptData.Catchment, ValidationSeverity.Error,
                                               string.Format(
                                                   "A {0} node can only be connected (downstream) to a boundary or lateral",
                                                   conceptData.GetType().Name),
                                               conceptData.Catchment.Basin));
            }
        }

        private ValidationReport ValidateRestartInput(RainfallRunoffModel model)
        {
            if (!model.UseRestart) return new ValidationReport("Input restart state", Enumerable.Empty<ValidationReport>());

            IEnumerable<string> errors, warnings;
            model.ValidateInputState(out errors, out warnings);

            var issues = errors.Select(error => new ValidationIssue("Input restart state", ValidationSeverity.Error, error)).ToList();
            issues.AddRange(warnings.Select(warning => new ValidationIssue("Input restart state", ValidationSeverity.Warning, warning)));

            return new ValidationReport("Input restart state", issues);
        }
    }
}