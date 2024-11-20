using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Validator class for restart settings.
    /// </summary>
    public static class WaterFlowFMRestartInputValidator
    {
        /// <summary>
        /// Validates the given restart model.
        /// </summary>
        /// <param name="model"> The restart model. </param>
        /// <returns>
        /// A <see cref="ValidationReport"/> containing the validation issues.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public static ValidationReport Validate(IRestartModel<WaterFlowFMRestartFile> model)
        {
            Ensure.NotNull(model, nameof(model));
            
            if (!model.UseRestart)
            {
                return new ValidationReport(Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state,
                                            Enumerable.Empty<ValidationReport>());
            }

            IList<ValidationIssue> issues = new List<ValidationIssue>();

            if (!model.RestartInput.Exists)
            {
                issues.Add(new ValidationIssue(Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state,
                                               ValidationSeverity.Error,
                                               Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_file_does_not_exist_cannot_restart));
            }

            return new ValidationReport(Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state, issues);
        }
    }
}