using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Validation;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Validation
{
    public class RealTimeControlModelValidator : IValidator<RealTimeControlModel, RealTimeControlModel>
    {
        /// <summary>
        /// Validation method for real time control settings. Since the bool useSaveStateTimeRange is removed
        /// from the plugin the first argument of the ValidateRestartTimeRangeSettings is always true.
        /// </summary>
        /// <param name="rootObject">RTC model</param>
        /// <param name="target">Optional</param>
        /// <returns>This method will return a validation report</returns>
        public ValidationReport Validate(RealTimeControlModel rootObject, RealTimeControlModel target = null)
        {
            var validationReports = new List<ValidationReport>
            {
                ValidateRealTimeControlModel(rootObject),
                RestartTimeRangeValidator.ValidateWriteRestartSettings(rootObject.WriteRestart,
                                                                       rootObject.SaveStateStartTime, rootObject.SaveStateStopTime, rootObject.SaveStateTimeStep,
                                                                       rootObject.StartTime, rootObject.TimeStep)
            };
            validationReports.AddRange(
                rootObject.ControlGroups.Select(cg => new ControlGroupValidator().Validate(rootObject, cg)));
            return new ValidationReport(rootObject.Name + " (Real Time Control)",
                                        validationReports);
        }

        private static ValidationReport ValidateRealTimeControlModel(RealTimeControlModel model)
        {
            var issues = new List<ValidationIssue>();

            // Should have at least 1 control group:
            if (!model.ControlGroups.Any())
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                               "There must be at least 1 control group defined"));
            }

            // Controlled models must run simultaneously
            ValidateControlledModels(model, issues);

            // Control Group names must be unique:
            RtcBaseObjectCheckForUniqueness(model.ControlGroups, issues, "Control group");

            // PostSharp validation:
            ValidationResult result = ObjectValidation.Validate(model);
            if (!result.IsValid)
            {
                issues.AddRange(result.Messages.Select(m => new ValidationIssue(model, ValidationSeverity.Error, m)));
            }

            return new ValidationReport("Model configuration", issues);
        }

        private static void ValidateControlledModels(RealTimeControlModel model, List<ValidationIssue> issues)
        {
            var compositeModel = model.Owner as ICompositeActivity;
            if (compositeModel == null)
            {
                return;
            }

            List<IActivity> actualControlledModels = GetActuallyControlledModels(model).ToList();
            List<IActivity> simultaneousRunningModels = compositeModel.CurrentWorkflow.GetActivitiesOfType<IActivity>().ToList();
            IEnumerable<IActivity> controlledModelsNotRunningSimultaneous = actualControlledModels.Except(simultaneousRunningModels);

            if (!simultaneousRunningModels.Contains(model) && actualControlledModels.Any())
            {
                foreach (IActivity actualControlledModel in actualControlledModels)
                {
                    issues.Add(new ValidationIssue(actualControlledModel, ValidationSeverity.Error,
                                                   "This model is being controlled by RTC, but the RTC model is not running simultaneous according to the workflow of the composite model. This is a requirement."));
                }
            }

            foreach (IActivity problemModel in controlledModelsNotRunningSimultaneous)
            {
                issues.Add(new ValidationIssue(problemModel, ValidationSeverity.Error,
                                               "This model is being controlled by RTC, but they are not running simultaneous according to the workflow of the composite model. This is a requirement."));
            }
        }

        private static IEnumerable<IActivity> GetActuallyControlledModels(RealTimeControlModel model)
        {
            var controlledModels = new List<IActivity>();

            foreach (IDataItem item in model.AllDataItems)
            {
                IEnumerable<IDataItem> relatedDataItems = item.LinkedBy.Concat(item.LinkedTo != null
                                                                                   ? new[]
                                                                                   {
                                                                                       item.LinkedTo
                                                                                   }
                                                                                   : new IDataItem[0]);
                foreach (IDataItem consumer in relatedDataItems)
                {
                    IActivity linkedModel = GetOwner(consumer);

                    if (linkedModel != null && !linkedModel.Equals(model))
                    {
                        controlledModels.Add(linkedModel);
                    }
                }
            }

            return controlledModels.Distinct().ToArray();
        }

        private static IActivity GetOwner(IDataItem dataItem)
        {
            if (dataItem == null)
            {
                return null;
            }

            var owner = dataItem.Owner as IActivity;
            return owner ?? GetOwner(dataItem.Parent);
        }

        private static void RtcBaseObjectCheckForUniqueness(IEnumerable<INameable> nameables,
                                                            IList<ValidationIssue> issueList, string typeObject)
        {
            var ruleNames = new HashSet<string>();
            foreach (INameable nameable in nameables)
            {
                if (ruleNames.Contains(nameable.Name))
                {
                    issueList.Add(new ValidationIssue(nameable,
                                                      ValidationSeverity.Error,
                                                      string.Format("The name '{0}' is used by {1} {2}s.",
                                                                    nameable.Name,
                                                                    nameables.Count(bo => bo.Name == nameable.Name),
                                                                    typeObject)));
                }
                else
                {
                    ruleNames.Add(nameable.Name);
                }
            }
        }
    }
}