using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMEmbankmentValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            if (model.Area.Embankments.Count == 0)
            {
                return new ValidationReport("Embankment definitions", Enumerable.Empty<ValidationIssue>());
            }

            var issues = new List<ValidationIssue>();

            IEventedList<Embankment> embankments = model.Area.Embankments;

            // Check for Intersections between Embankments
            List<Embankment> intersectingEmbankments = embankments
                                                       .Where(b => embankments.Any(
                                                                  b2 => b2 != b && b2.Geometry.Intersects(b.Geometry)))
                                                       .ToList();
            if (intersectingEmbankments.Count != 0)
            {
                foreach (Embankment embankment in intersectingEmbankments)
                {
                    issues.Add(new ValidationIssue(model.GetDataItemByValue(model.Area), ValidationSeverity.Error,
                                                   $"Embankment {embankment.Name} intersects with other embankments"));
                }
            }

            // Check for Intersections in the Embankment itself
            intersectingEmbankments.Clear();
            foreach (Embankment embankment in embankments)
            {
                if (embankment.Geometry.Coordinates.Count() > 2)
                {
                    for (var iLine1 = 0; iLine1 < embankment.Geometry.Coordinates.Count() - 2; iLine1++)
                    {
                        for (int iLine2 = iLine1 + 2; iLine2 < embankment.Geometry.Coordinates.Count() - 1; iLine2++)
                        {
                            var line1 = new LineSegment();
                            var line2 = new LineSegment();

                            line1.P0 = embankment.Geometry.Coordinates[iLine1];
                            line1.P1 = embankment.Geometry.Coordinates[iLine1 + 1];

                            line2.P0 = embankment.Geometry.Coordinates[iLine2];
                            line2.P1 = embankment.Geometry.Coordinates[iLine2 + 1];

                            if (line1.Intersection(line2) != null)
                            {
                                intersectingEmbankments.Add(embankment);
                            }
                        }
                    }
                }
            }

            if (intersectingEmbankments.Count != 0)
            {
                foreach (Embankment embankment in intersectingEmbankments)
                {
                    issues.Add(new ValidationIssue(model.GetDataItemByValue(model.Area), ValidationSeverity.Error,
                                                   $"Embankment {embankment.Name} intersects with itself"));
                }
            }

            return new ValidationReport("Embankment definitions", issues);
        }

        private static IRegion GetTopRegion(IRegion region)
        {
            return region.Parent == null ? region : GetTopRegion(region.Parent);
        }
    }
}