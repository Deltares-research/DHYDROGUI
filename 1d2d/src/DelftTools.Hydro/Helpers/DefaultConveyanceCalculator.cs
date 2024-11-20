using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Helpers
{
    public class DefaultConveyanceCalculator : ConveyanceCalculatorBase
    {
        public override IFunction GetConveyance(ICrossSection crossSection)
        {
            var crossSectionDefinition = crossSection.Definition;
            var yzValues = crossSectionDefinition.GetProfile();
            double[] y = yzValues.Select(yz => yz.X).ToArray();
            double[] z = yzValues.Select(yz => yz.Y).ToArray();

            double[] zValuesOrdered = z.OrderBy(v => v).ToArray();
            double maxZ = zValuesOrdered.Max();

            var vertices = new List<Coordinate> { new Coordinate(y[0] - 1, maxZ + 1) };

            // In order to avoid NTS throw exceptions (polygon with internal maximum)
            // set an extra 'top' on the polygon
            // Since we will only use intersections at valid z values these will be filtered out.
            // extra value at start
            for (var i = 0; i < zValuesOrdered.Length; i++)
            {
                vertices.Add(new Coordinate(y[i], zValuesOrdered[i]));
            }
            // extra value at end.
            vertices.Add(new Coordinate(vertices[vertices.Count - 1].X + 1, maxZ + 1));
            vertices.Add(new Coordinate(vertices[0].X, vertices[0].Y));

            IGeometry crossSectionPolygon = new Polygon(new LinearRing(vertices.ToArray()));

            var verticesRectangle = new List<Coordinate>
                                        {
                                            new Coordinate(y[0], zValuesOrdered[0]),
                                            new Coordinate(y[0],zValuesOrdered[zValuesOrdered.Length - 1]),
                                            new Coordinate(y[y.Length - 1],zValuesOrdered[zValuesOrdered.Length - 1]),
                                            new Coordinate(y[y.Length - 1],zValuesOrdered[0]),
                                            new Coordinate(y[0], zValuesOrdered[0])
                                        };
            var result = GetEmptyConveyanceFunction();
            for (int i = 0; i < zValuesOrdered.Length; i++)
            {
                verticesRectangle[1].Y = zValuesOrdered[i];
                verticesRectangle[2].Y = zValuesOrdered[i];
                IGeometry rectangle = new Polygon(new LinearRing(verticesRectangle.ToArray()));

                IGeometry intersection = crossSectionPolygon.Intersection(rectangle);
                const double chezy = 45.0;
                double width = 0;
                double storageWidth;
                double area = 0;
                double wettedPerimeter = 0;
                double hydraulicRadius;
                double conveyance;
                if (intersection is IGeometryCollection collection)
                {
                    for (int j = 0; j < collection.NumGeometries; j++)
                    {
                        IGeometry smallPolygon = collection.Geometries[j];
                        width += smallPolygon.EnvelopeInternal.Width;
                        area += smallPolygon.Area;
                        wettedPerimeter += smallPolygon.Boundary.Length;
                    }
                }
                else
                {
                    width = intersection.EnvelopeInternal.Width;
                    area = intersection.EnvelopeInternal.Area;
                    wettedPerimeter = intersection.Boundary.Length;
                }

                hydraulicRadius = area / wettedPerimeter;
                if (hydraulicRadius == 0 || wettedPerimeter == 0)
                {
                    conveyance = 0;
                    hydraulicRadius = 0;
                }
                else
                {
                    // use Manning's equation to compute conveyance
                    conveyance = area * chezy * Math.Sqrt(hydraulicRadius);
                }
                storageWidth = width;

                // flowArea, flowWidth, perimeter, hydraulicRadius, totalWidth, conveyancePos, conveyanceNeg
                result[zValuesOrdered[i]] = new[] { conveyance, area,  (width-storageWidth), wettedPerimeter, hydraulicRadius, width, conveyance };
            }
            return result;
        }
    }
}