using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Validation
{
    public class HydroModelValidator : IValidator<HydroModel, HydroModel>
    {
        /// <summary>
        /// Performs the relevant checks for the <paramref name="rootObject"/>
        /// <see cref="HydroModel"/> object and returns a resulting validation report.
        /// </summary>
        /// <param name="rootObject">The model being validated.</param>
        /// <param name="target">The target model, unused.</param>
        /// <remarks>
        /// The target is currently unused.
        /// </remarks>
        public ValidationReport Validate(HydroModel rootObject, HydroModel target = null)
        {
            string validationReportName = rootObject.Name + " (Hydro Model)";

            // null-check of current workflow
            if (rootObject.CurrentWorkflow == null)
            {
                return new ValidationReport(validationReportName, new List<ValidationIssue> {new ValidationIssue(rootObject, ValidationSeverity.Error, Resources.HydroModelValidator_Validate_Current_Workflow_cannot_be_empty)});
            }

            var hydroModelReports = new List<ValidationReport>
            {
                ConstructCurrentWorkflowReport(),
                ConstructModelStructureReport(rootObject),
                ConstructModelGridReport(rootObject)
            };

            var hydroModelSpecificReports = new ValidationReport(Resources.HydroModelValidator_Validate_HydroModel_Specific, hydroModelReports);
            IEnumerable<ValidationReport> subModelReports = ConstructSubmodelReports(rootObject);

            var reports = new List<ValidationReport> {hydroModelSpecificReports};
            reports.AddRange(subModelReports);

            return new ValidationReport(validationReportName, reports);
        }

        #region ModelGrid

        private static ValidationReport ConstructModelGridReport(ICompositeActivity model)
        {
            var gridCoordinatesIssues = new List<ValidationIssue>();
            List<IHasCoordinateSystem> activitiesWithCoordSyst = model.CurrentWorkflow.Activities.GetActivitiesOfType<IHasCoordinateSystem>().Where(act => act.CoordinateSystem != null).ToList();

            if (activitiesWithCoordSyst.Count > 1 && activitiesWithCoordSyst.GroupBy(act => act.CoordinateSystem.IsGeographic).Count() > 1)
            {
                gridCoordinatesIssues.Add(
                    new ValidationIssue(
                        model,
                        ValidationSeverity.Error,
                        Resources.HydroModelValidator_ConstructModelGridReport_Wave_and_WaterFlowFM_Grids_need_to_be_of_the_same_type__either_Spherical_or_Cartesian__));
            }

            return new ValidationReport(Resources.HydroModelValidator_ConstructModelGridReport_Grid_Coordinate_System_type, gridCoordinatesIssues);
        }

        private static IEnumerable<ValidationReport> ConstructSubmodelReports(ICompositeActivity model)
        {
            var subModelReports = new List<ValidationReport>();
            if (model.CurrentWorkflow != null)
            {
                List<IDimrModel> dimrModels = model.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                                   .Where(dm => dm != null).ToList();
                foreach (IDimrModel dimrModel in dimrModels)
                {
                    subModelReports.Add(dimrModel.Validate());
                }
            }

            return subModelReports;
        }

        #endregion

        #region ModelStructure

        private static ValidationReport ConstructModelStructureReport(ICompositeActivity model)
        {
            IEnumerable<ValidationIssue> modelNameIssues = ValidateIfModelNamesAreUnique(model.CurrentWorkflow.Activities.GetActivitiesOfType<IActivity>().ToArray());
            return new ValidationReport(Resources.HydroModelValidator_ConstructModelStructureReport_Model_structure, modelNameIssues);
        }

        private static IEnumerable<ValidationIssue> ValidateIfModelNamesAreUnique(IEnumerable<IActivity> activities)
        {
            var modelStructureIssues = new List<ValidationIssue>();
            string[] lowercaseNames = activities.Select(a => a.Name).Where(n => n != null).Select(n => n.ToLowerInvariant()).ToArray();
            IEnumerable<string> duplicateNames = lowercaseNames.GroupBy(x => x)
                                                               .Where(group => group.Count() > 1)
                                                               .Select(group => group.Key);

            foreach (string duplicateName in duplicateNames)
            {
                modelStructureIssues.Add(new ValidationIssue(duplicateName, ValidationSeverity.Error,
                                                             string.Format(Resources.HydroModelValidator_ValidateIfModelNamesAreUnique_Two_or_more_activities_in_the_current_workflow_have_the_same_name___0____possibly_only_differing_by_uppercase_letters__Please_make_sure_that_these_activity_names_are_uniquely_named_, duplicateName.ToLower())));
            }

            return modelStructureIssues;
        }

        #endregion

        #region CurrentWorkflow

        private static ValidationReport ConstructCurrentWorkflowReport() => new ValidationReport(Resources.HydroModelValidator_ConstructCurrentWorkflowReport_Workflow, Enumerable.Empty<ValidationIssue>());

        #endregion
    }
}