using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Validation
{
    public class RealTimeControlModelValidator : IValidator<RealTimeControlModel, RealTimeControlModel>
    {
        public ValidationReport Validate(RealTimeControlModel rootObject, RealTimeControlModel target = null)
        {
            var validationReports = new List<ValidationReport>
            {
                ValidateRealTimeControlModel(rootObject),
                RestartTimeRangeValidator.ValidateRestartTimeRangeSettings(true,
                    rootObject.SaveStateStartTime,
                    rootObject.SaveStateStopTime,
                    rootObject.SaveStateTimeStep,
                    rootObject),
                ValidateRestartInputState(rootObject),
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
            var result = ObjectValidation.Validate(model);
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
                return;

            var actualControlledModels = GetActuallyControlledModels(model).ToList();
            var simultaneousRunningModels = compositeModel.CurrentWorkflow.GetActivitiesOfType<IActivity>().ToList();
            var controlledModelsNotRunningSimultaneous = actualControlledModels.Except(simultaneousRunningModels);

            if (!simultaneousRunningModels.Contains(model) && actualControlledModels.Any())
            {
                foreach (var actualControlledModel in actualControlledModels)
                {
                    issues.Add(new ValidationIssue(actualControlledModel, ValidationSeverity.Error,
                                                   "This model is being controlled by RTC, but the RTC model is not running simultaneous according to the workflow of the composite model. This is a requirement."));
                }
                
            }
            foreach (var problemModel in controlledModelsNotRunningSimultaneous)
            {
                issues.Add(new ValidationIssue(problemModel, ValidationSeverity.Error,
                                               "This model is being controlled by RTC, but they are not running simultaneous according to the workflow of the composite model. This is a requirement."));
            }
        }

        private static IEnumerable<IActivity> GetActuallyControlledModels(RealTimeControlModel model)
        {
            //todo: get this from model service somehow?!

            var controlledModels = new List<IActivity>();

            foreach (var item in model.AllDataItems)
            {
                var relatedDataItems = item.LinkedBy.Concat(item.LinkedTo != null ? new[] {item.LinkedTo} : new IDataItem[0]);
                foreach (var consumer in relatedDataItems)
                {
                    var linkedModel = GetOwner(consumer);

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
                return null;
            
            var owner = dataItem.Owner as IActivity;
            return owner ?? GetOwner(dataItem.Parent);
        }

        private static void RtcBaseObjectCheckForUniqueness(IEnumerable<INameable> nameables,
                                                            IList<ValidationIssue> issueList, string typeObject)
        {
            var ruleNames = new HashSet<string>();
            foreach (var nameable in nameables)
            {
                if (ruleNames.Contains(nameable.Name))
                {
                    issueList.Add(new ValidationIssue(nameable,
                                                      ValidationSeverity.Error,
                                                      String.Format("The name '{0}' is used by {1} {2}s.",
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

        private ValidationReport ValidateRestartInputState(RealTimeControlModel model)
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