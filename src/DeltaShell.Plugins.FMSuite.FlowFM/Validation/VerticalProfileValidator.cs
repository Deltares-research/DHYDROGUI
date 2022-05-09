using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class VerticalProfileValidator
    {
        public static IEnumerable<ValidationIssue> ValidateVerticalProfile(string subject,
            VerticalProfileDefinition depthProfile, object viewData, string locationName)
        {
            if (!depthProfile.PointDepths.AllUnique())
            {
                yield return new ValidationIssue(subject, ValidationSeverity.Error,
                    ValidationMessage("Duplicate profile depths detected", subject, locationName),
                    viewData);
            }
            switch (depthProfile.Type)
            {
                case VerticalProfileType.PercentageFromBed:
                    if (depthProfile.PointDepths.Any(pd => pd < 0))
                    {
                        yield return new ValidationIssue(subject, ValidationSeverity.Warning,
                            ValidationMessage("Vertical profile point below bed detected", subject,
                                locationName), viewData);
                    }
                    if (depthProfile.PointDepths.Any(pd => pd > 100))
                    {
                        yield return new ValidationIssue(subject, ValidationSeverity.Warning,
                            ValidationMessage("Vertical profile point above surface detected", subject,
                                locationName), viewData);
                    }
                    break;

                case VerticalProfileType.PercentageFromSurface:
                    if (depthProfile.PointDepths.Any(pd => pd < 0))
                    {
                        yield return new ValidationIssue(subject, ValidationSeverity.Warning,
                            ValidationMessage("Vertical profile point above surface detected", subject,
                                locationName), viewData);
                    }
                    if (depthProfile.PointDepths.Any(pd => pd > 100))
                    {
                        yield return new ValidationIssue(subject, ValidationSeverity.Warning,
                            ValidationMessage("Vertical profile point below bed detected", subject,
                                locationName), viewData);
                    }
                    break;

                case VerticalProfileType.ZFromBed:
                    if (depthProfile.PointDepths.Any(pd => pd < 0))
                    {
                        yield return new ValidationIssue(subject, ValidationSeverity.Warning,
                            ValidationMessage("Vertical profile point below bed detected", subject,
                                locationName), viewData);
                    }
                    break;
                case VerticalProfileType.ZFromSurface:
                    if (depthProfile.PointDepths.Any(pd => pd < 0))
                    {
                        yield return new ValidationIssue(subject, ValidationSeverity.Warning,
                            ValidationMessage("Vertical profile point above surface detected", subject,
                                locationName), viewData);
                    }
                    break;
            }
        }

        private static string ValidationMessage(string baseMessage, string issueTitle, string messageKey)
        {
            return messageKey == null
                ? String.Format("{0} for {1}", baseMessage, issueTitle)
                : String.Format("{0} for {1} at {2}", baseMessage, issueTitle, messageKey);
        }
    }
}
