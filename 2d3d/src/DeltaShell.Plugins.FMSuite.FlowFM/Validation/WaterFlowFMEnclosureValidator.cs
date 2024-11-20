using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMEnclosureValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();
            var report = new ValidationReport("Enclosure", issues);

            IEventedList<GroupableFeature2DPolygon> enclosures = model.Area.Enclosures;
            if (enclosures.Count == 0)
            {
                return report;
            }

            /* CSSP 2017: If there are more than one enclosure, WE WILL ONLY display said error, 
             * the rest will appear after there is only one. To avoid displaying messages
             * for potentially removable enclosures. */
            if (enclosures.Count > 1)
            {
                /* This could happen while importing or loading a model, as during creation this functionality is disabled. */
                string enclosuresNames = string.Join(", ", enclosures.Select(e => e.Name));
                issues.Add(new ValidationIssue(
                               model.Area.Enclosures,
                               ValidationSeverity.Error,
                               string.Format(
                                   Resources
                                       .WaterFlowFMEnclosureValidator_Validate_Only_one_enclosure_per_model_is_allowed__Enclosures_in_model___0_,
                                   enclosuresNames),
                               model.Area.Enclosures));
            }
            else
            {
                CreateIssuesIfInvalidGeometry(issues, model, enclosures[0]);
            }

            return report;
        }

        private static void CreateIssuesIfInvalidGeometry(List<ValidationIssue> issues, WaterFlowFMModel model,
                                                          Feature2DPolygon feature)
        {
            var featureAsPolygon = feature.Geometry as Polygon;
            if (featureAsPolygon == null)
            {
                issues.Add(new ValidationIssue(
                               feature,
                               ValidationSeverity.Error,
                               string.Format(Resources.WaterFlowFMEnclosureValidator_Validate_GeometryNotValid,
                                             feature.Name),
                               model.Area.Enclosures));
                return;
            }

            if (!featureAsPolygon.IsValid)
            {
                /* If the polygon has not generated holes is because the method fail,
                 * which means the polygon is not valid to create a difference.
                 */
                issues.Add(new ValidationIssue(
                               feature,
                               ValidationSeverity.Error,
                               string.Format(
                                   Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid,
                                   feature.Name),
                               model.Area.Enclosures));
            }
        }
    }
}