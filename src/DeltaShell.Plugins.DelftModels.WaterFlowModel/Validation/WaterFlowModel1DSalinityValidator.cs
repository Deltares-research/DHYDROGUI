using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
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
                .Where(bc => bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None
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
            if (!flowModel1D.SalinityValidNonConstantFormulation || flowModel1D.DispersionFormulationType != DispersionFormulationType.KuijperVanRijnPrismatic)
                return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateSalinity_Salinity, Enumerable.Empty<ValidationIssue>());

            var issues = new List<ValidationIssue>();
            
            if (string.IsNullOrEmpty(flowModel1D.SalinityEstuaryMouthNodeId))
            {
                issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, "No Estuary mouth node specified."));
            }
            else
            {
                var node = flowModel1D.Network?.HydroNodes.FirstOrDefault(n => n.Name == flowModel1D.SalinityEstuaryMouthNodeId);
                if (node == null)
                {
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, $"Can not find specified estuary mouth node {flowModel1D.SalinityEstuaryMouthNodeId}."));
                }
                else if (!node.IsValidSalinityEstuaryMouthNodeId())
                {
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, $"Estuary mouth node \"{flowModel1D.SalinityEstuaryMouthNodeId}\" is not a boundary node."));
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
