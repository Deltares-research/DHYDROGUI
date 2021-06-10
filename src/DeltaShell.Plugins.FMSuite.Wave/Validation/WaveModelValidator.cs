using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public class WaveModelValidator : IValidator<WaveModel, WaveModel>
    {
        /// <summary>
        /// Validates the specified model.
        /// </summary>
        /// <param name="rootObject">The model.</param>
        /// <param name="target">The target, unused.</param>
        /// <returns>
        /// A <see cref="ValidationReport"/> containing any validation violations.
        /// </returns>
        /// <remarks>
        /// <paramref name="target"/> is currently unused.
        /// </remarks>
        public ValidationReport Validate(WaveModel rootObject, WaveModel target = null)
        {
            return new ValidationReport(rootObject.Name + " (Waves Model)", new[]
            {
                WaveDomainValidator.Validate(rootObject),
                WaveTimePointValidator.Validate(rootObject),
                WaveBoundariesValidator.Validate(rootObject.BoundaryContainer.Boundaries,
                                                 rootObject.TimeFrameData.TimePoints.FirstOrDefault()),
                WaveAreaValidator.Validate(rootObject),
                WaveCouplingValidator.Validate(rootObject),
                WavePropertiesValidator.Validate(rootObject),
                WaveOutputParametersValidator.Validate(rootObject)
            });
        }
    }

    public static class WaveAreaValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            IEnumerable<ValidationReport> subReports =
                model.FeatureContainer.ObservationCrossSections.Select(
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