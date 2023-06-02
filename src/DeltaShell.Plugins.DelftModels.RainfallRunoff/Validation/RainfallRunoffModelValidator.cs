using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    /// <summary>
    /// Validator for <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public class RainfallRunoffModelValidator
    {
        /// <summary>
        /// Validate the given <see cref="RainfallRunoffModel"/>
        /// </summary>
        /// <param name="rainfallRunoffModel">The model to validate.</param>
        /// <returns>A <see cref="ValidationReport"/> containing the results of the validation.</returns>
        public static ValidationReport Validate(RainfallRunoffModel rainfallRunoffModel)
        {
            return new ValidationReport(rainfallRunoffModel.Name + " (Rainfall Runoff Model)", new[]
                {
                    new RainfallRunoffBasinValidator().Validate(rainfallRunoffModel, rainfallRunoffModel.Basin),
                    RainfallRunoffMeteoValidator.Validate(rainfallRunoffModel),
                    new RainfallRunoffCatchmentDataValidator().Validate(rainfallRunoffModel, rainfallRunoffModel.GetAllModelData()),
                    ValidateTimers(rainfallRunoffModel),
                    new RainfallRunoffSettingsValidator().Validate(rainfallRunoffModel, rainfallRunoffModel),
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

            if (runoff != null && !RainfallRunoffValidationHelper.IsConsideredAsBoundary(runoff.Target))
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
