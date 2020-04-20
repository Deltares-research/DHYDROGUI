using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public class WaveModelValidator : IValidator<WaveModel, WaveModel>
    {
        public ValidationReport Validate(WaveModel model, WaveModel target = null)
        {
            return new ValidationReport(model.Name + " (Waves Model)", new[]
            {
                WaveDomainValidator.Validate(model),
                WaveTimePointValidator.Validate(model),
                WaveBoundaryConditionValidator.Validate(model.BoundaryConditions),
                WaveBoundariesValidator.Validate(model.BoundaryContainer.Boundaries),
                WaveAreaValidator.Validate(model),
                WaveCouplingValidator.Validate(model),
                WavePropertiesValidator.Validate(model),
                WaveOutputParametersValidator.Validate(model)
            });
        }
    }

    public static class WaveAreaValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            IEnumerable<ValidationReport> subReports =
                model.ObservationCrossSections.Select(
                    cs => new ValidationReport(cs.Name, ValidateObservationCrossSection(cs)));

            return new ValidationReport("Waves Model Area", subReports);
        }

        private static IEnumerable<ValidationIssue> ValidateObservationCrossSection(Feature2D cs)
        {
            if (cs.Name.Length > 8)
            {
                yield return new ValidationIssue(cs.Name, ValidationSeverity.Error,
                                                 "Name of the observation cross section is too long. Maximum: 8 characters.");
            }
        }
    }
}