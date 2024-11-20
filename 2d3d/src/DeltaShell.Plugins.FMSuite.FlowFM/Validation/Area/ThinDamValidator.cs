using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class ThinDamValidator
    {
        /// <summary>
        /// Validates the thin dams and return any issues encountered.
        /// </summary>
        /// <param name="thinDams"> The <see cref="ThinDam2D"/> objects that are being validated. </param>
        /// <param name="gridExtent"> The <see cref="Envelope"/> object that describes the extent of the FM model grid. </param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<ThinDam2D> thinDams, Envelope gridExtent)
        {
            IEnumerable<ThinDam2D> thinDamsNotSnappingToGrid =
                thinDams.Where(td => !td.Geometry.SnapsToFlowFmGrid(gridExtent));

            foreach (ThinDam2D thinDam in thinDamsNotSnappingToGrid)
            {
                yield return new ValidationIssue(thinDam, ValidationSeverity.Warning,
                                                 string.Format(
                                                     Resources
                                                         .WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent,
                                                     thinDam.Name),
                                                 thinDams);
            }
        }
    }
}