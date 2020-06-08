using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    public static class Generate1D2DLinksHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Generate1D2DLinksHelper));

        public static IEnumerable<ILink1D2D> Generate1D2DLinks(IPolygon selectedArea, LinkGeneratingType linkType, UnstructuredGrid grid, IEventedList<Gully> gullies, IDiscretization discretization)
        {
            var generate1D2DLinks = Enumerable.Empty<ILink1D2D>();

            var mustHaveTwoPoints = linkType != LinkGeneratingType.GullySewer;
            if (mustHaveTwoPoints && discretization.Locations.Values.Count < 2)
            {
                return generate1D2DLinks;
            }

            var mask1DMesh = GetMesh1DFilter(discretization, linkType, selectedArea);
            if (!mask1DMesh.Any(m => m.Equals(true)))
            {
                return generate1D2DLinks;
            }

            if (selectedArea == null)
            {
                var points = discretization.Locations.Values.Select(p => p.Geometry as IPoint).ToList();
                selectedArea = GetSelectAllArea(points);
            }

            using (var disposableMeshGeometry = new DisposableMeshGeometryGridGeom(grid))
            using (var mesh1D = new Mesh1DGeometry(discretization))
            using (var selectedAreaGeometry = new GeometriesData(new List<IGeometry> { selectedArea }))
            using (var gGeomApi = new GridGeomApi())
            {
                LinkInformation linkInformation = null;
                if (linkType == LinkGeneratingType.GullySewer)
                {
                    var geometryGullies = gullies
                        .Where(r => r.Geometry.Intersects(selectedArea))
                        .Select(r => r.Geometry).ToList();

                    if (geometryGullies.Count == 0)
                    {
                        return generate1D2DLinks;
                    }

                    using (var gulliesData = new GeometriesData(geometryGullies))
                    {
                        linkInformation = gGeomApi.GetLinkInformation(disposableMeshGeometry, mesh1D, selectedAreaGeometry, mask1DMesh, linkType, gulliesData);
                    }
                }
                else
                {
                    linkInformation = gGeomApi.GetLinkInformation(disposableMeshGeometry, mesh1D, selectedAreaGeometry, mask1DMesh, linkType);
                }

                if (gGeomApi.LastErrorCode != UGridConstants.NoErrorCode)
                {
                    var format =
                        $"1D2D Links were not generated between the grid and the network of WaterFlowFMModel." +
                        $" Please make sure the grid and network are correct.";
                    log.ErrorFormat(format);
                    return generate1D2DLinks;
                }

                return Creates1d2dLinks(linkInformation, grid, discretization, linkType);
            }
        }

        private static IList<Link1D2D> Creates1d2dLinks(LinkInformation linkInformation, UnstructuredGrid grid, IDiscretization networkDiscretization, LinkGeneratingType linkType)
        {
            var lstNewLinks = new List<Link1D2D>();
            for (int i = 0; i < linkInformation.FromIndices.Length; i++)
            {
                //seems lists are swapt  
                var pointIndex = linkInformation.ToIndices[i];
                var cellIndex = linkInformation.FromIndices[i];

                var cell = grid.Cells[cellIndex];
                var node = networkDiscretization.Locations.Values[pointIndex];
                var link = new Link1D2D(pointIndex, cellIndex)
                {
                    Geometry = new LineString(new[] { node.Geometry.Coordinate, cell.Center }),
                    TypeOfLink = linkType.GetLinkStorageType()
                };
                lstNewLinks.Add(link);
            }
            return lstNewLinks;
        }

        public static bool[] GetMesh1DFilter(IDiscretization networkDiscretization, LinkGeneratingType linkType, IPolygon selectedArea = null)
        {
            var discretisationPoints = networkDiscretization.Locations.Values;
            var filterList = new bool[discretisationPoints.Count];

            for (int i = 0; i < discretisationPoints.Count; i++)
            {
                var discretisationPoint = discretisationPoints[i];
                var isAvailableMesh1DPoint = false;
                if (selectedArea == null || selectedArea.Intersects(discretisationPoint.Geometry))
                {
                    var sewerConnection = discretisationPoint.Branch as SewerConnection;

                    switch (linkType)
                    {
                        case LinkGeneratingType.Lateral:
                            isAvailableMesh1DPoint = sewerConnection == null;
                            break;
                        case LinkGeneratingType.EmbeddedOneToOne: //go to next case
                        case LinkGeneratingType.EmbeddedOneToMany:
                            isAvailableMesh1DPoint = sewerConnection == null ||
                                                     sewerConnection.WaterType == SewerConnectionWaterType.Combined ||
                                                     sewerConnection.WaterType == SewerConnectionWaterType.StormWater;
                            break;
                        case LinkGeneratingType.GullySewer:
                            isAvailableMesh1DPoint = sewerConnection != null &&
                                                     (sewerConnection.WaterType == SewerConnectionWaterType.Combined ||
                                                      sewerConnection.WaterType == SewerConnectionWaterType.StormWater);
                            break;
                    }
                }

                filterList[i] = isAvailableMesh1DPoint;
            }
            return filterList;
        }

        private static IPolygon GetSelectAllArea(IReadOnlyCollection<IPoint> points)
        {
            var xMin = points.Select(p => p.X).Min();
            var yMin = points.Select(p => p.Y).Min();
            var xMax = points.Select(p => p.X).Max();
            var yMax = points.Select(p => p.Y).Max();

            var coordinates = new[]
            {
                new Coordinate(xMin, yMax),
                new Coordinate(xMin, yMin),
                new Coordinate(xMax, yMin),
                new Coordinate(xMax, yMin),
                new Coordinate(xMin, yMax)
            };

            return new Polygon(new LinearRing(coordinates));
        }
    }
}