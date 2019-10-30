using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DSalinityValidator
    {
        public static ValidationReport Validate(WaterFlowModel1D model)
        {
            if (!model.UseSalt) return new ValidationReport("Salinity", Enumerable.Empty<ValidationIssue>());

            return new ValidationReport("Salinity", new[]
            {
                ValidateSalinityBoundaryConditions(model),
                ValidateSalinityForKuijperVanRijnPrismaticIsValid(model)
            });
        }

        private static ValidationReport ValidateSalinityBoundaryConditions(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();
            
            var invalidSaltBoundaryConditions = model.BoundaryConditions
                .Where(bc => bc.DataType != Model1DBoundaryNodeDataType.None
                    && bc.SaltConditionType == SaltBoundaryConditionType.None);

            foreach (var bc in invalidSaltBoundaryConditions)
            {
                issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, string.Format(
                    Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_a_salinity_type_of_None__All_open_boundaries_must_specify_salinity_values_, bc.Name)));
            }
            
            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_Boundary_conditions, issues);
        }
       
        private static ValidationReport ValidateSalinityForKuijperVanRijnPrismaticIsValid(WaterFlowModel1D flowModel1D)
        {
            var issues = new List<ValidationIssue>();

            if (flowModel1D.DispersionFormulationType != DispersionFormulationType.Constant && 
                flowModel1D.DispersionFormulationType == DispersionFormulationType.KuijperVanRijnPrismatic)
            {
                //Check if the mouth node Id is correct.
                if (string.IsNullOrEmpty(flowModel1D.SalinityEstuaryMouthNodeId))
                {
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_No_Estuary_mouth_node_specified_));
                }
                else
                {
                    var node = flowModel1D.Network?.HydroNodes.FirstOrDefault(n => n.Name == flowModel1D.SalinityEstuaryMouthNodeId);
                    if (node == null)
                    {
                        issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, String.Format(Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_Can_not_find_specified_estuary_mouth_node__0__,flowModel1D.SalinityEstuaryMouthNodeId)));
                    }
                    else if (!node.IsValidSalinityEstuaryMouthNodeId())
                    {
                        issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, String.Format(Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_Estuary_mouth_node__0__is_not_a_boundary_node_, flowModel1D.SalinityEstuaryMouthNodeId)));
                    }
                }

                //Check if F4 Dispersion values are all 0.
                var f4CoverageValues = flowModel1D.DispersionF4Coverage.GetValues<double>().ToList();
                if (f4CoverageValues.Any() && f4CoverageValues.All(v => v.Equals(0)))
                {
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_F4_Coverage_values_cannot_all_be_set_to_0__Either_remove_them_or_set_a_valid_value_) );
                }
            }

            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateSalinity_Salinity, issues);
        }

        public static bool IsValidSalinityEstuaryMouthNodeId(this IHydroNode node)
        {
            return node.IncomingBranches.Concat(node.OutgoingBranches).Count() == 1;
        }
    }
}
