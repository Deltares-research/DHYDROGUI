using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class ThinDamValidator
    {
        /// <summary>
        /// Validate the thin dams and return any issues encountered.
        /// </summary>
        /// <param name="model">The model to which the thinDams belong.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(WaterFlowFMModel model)
        {
            var thinDams = model.Area.ThinDams;
            var thinDamsNotSnappingToGrid = thinDams.Where(td => !td.Geometry.SnapsToFlowFmGrid(model.GridExtent));

            foreach (var thinDam in thinDamsNotSnappingToGrid)
            {
                yield return new ValidationIssue(thinDam, ValidationSeverity.Warning,
                    string.Format(Resources.WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent, thinDam.Name),
                    thinDams);
            }
        }
    }
}