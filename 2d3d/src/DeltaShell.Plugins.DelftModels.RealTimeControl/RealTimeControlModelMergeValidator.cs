using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public class RealTimeControlModelMergeValidator
    {
        public ValidationReport Validate(RealTimeControlModel destinationModel, RealTimeControlModel sourceModel)
        {
            return new ValidationReport(destinationModel.Name + " (Real Time Control)", new[]
            {
                ValidateControlGroups(destinationModel, sourceModel)
            });
        }

        public static ValidationReport ValidateControlGroups(RealTimeControlModel destinationModel, RealTimeControlModel sourceModel)
        {
            IEventedList<ControlGroup> destControlGroups = destinationModel.ControlGroups;
            IEventedList<ControlGroup> srcControlGroups = sourceModel.ControlGroups;
            var issues = new List<ValidationIssue>();
            foreach (ControlGroup srcControlGroup in srcControlGroups)
            {
                foreach (ControlGroup destControlGroup in destControlGroups)
                {
                    if (srcControlGroup.Name == destControlGroup.Name)
                    {
                        issues.Add(new ValidationIssue(srcControlGroup, ValidationSeverity.Warning, string.Format("Control group : {0} in source model : {1} has the same name as in the destination model : {2}", srcControlGroup.Name, sourceModel.Name, destinationModel.Name)));
                    }
                }
            }

            return issues.Count == 0 ? new ValidationReport("Control group names", Enumerable.Empty<ValidationIssue>()) : new ValidationReport("Control group names", issues);
        }
    }
}