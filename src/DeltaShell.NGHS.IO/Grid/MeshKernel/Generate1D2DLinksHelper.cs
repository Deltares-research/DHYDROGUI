using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using MeshKernelNETCore.Api;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid.MeshKernel
{
    public static class Generate1D2DLinksHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Generate1D2DLinksHelper));

        public static IEnumerable<ILink1D2D> Generate1D2DLinks(IPolygon selectedArea, LinkGeneratingType linkType, UnstructuredGrid grid, IEventedList<Gully> gullies, IDiscretization discretization)
        {
            using (new TimedAction(ts => log.Debug($"Link generation took: {ts.TotalSeconds} sec")))
            {
                var mask1DMesh = GetValidMesh1DFilter(discretization, linkType, out var isValid, selectedArea);
                if (!isValid)
                {
                    return Enumerable.Empty<ILink1D2D>();
                }

                selectedArea = selectedArea ?? GetSelectAllArea(discretization.Locations.Values.Select(p => p.Geometry as IPoint).ToArray());

                using (var mesh2d = grid.CreateDisposableMesh2D())
                using (var mesh1d = discretization.CreateDisposableMesh1D())
                using (var api = new MeshKernelApi())
                {
                    var id = api.AllocateState(0);

                    // setup 1d/2d meshes
                    var success = api.Mesh1dSet(id, mesh1d);
                    success = success && api.Mesh2dSet(id, mesh2d);

                    // generate contacts (links)
                    success = success && ComputeContacts(api, id, linkType, mask1DMesh, selectedArea, gullies);

                    var contacts = success ? api.ContactsGetData(id) : new DisposableContacts(0);

                    api.DeallocateState(id);

                    return Creates1d2dLinks(contacts, grid, discretization, linkType);
                }
            }
        }

        private static bool ComputeContacts(IMeshKernelApi api, int id, LinkGeneratingType linkType, bool[] mask1DMesh, IPolygon selectedArea, IEventedList<Gully> gullies)
        {
            var intMask1dMesh = mask1DMesh.Select(m => m ? 1 : 0).ToArray();
            var maskHandle = GCHandle.Alloc(intMask1dMesh, GCHandleType.Pinned);
            var mask1DMeshPtr = maskHandle.AddrOfPinnedObject();

            var selectedAreaGeometry = selectedArea.CreateDisposableGeometryList();
            DisposableGeometryList gulliesData = null;

            try
            {
                switch (linkType)
                {
                    case LinkGeneratingType.EmbeddedOneToOne:
                        return api.ContactsComputeSingle(id, ref mask1DMeshPtr, ref selectedAreaGeometry);
                    case LinkGeneratingType.EmbeddedOneToMany:
                        return api.ContactsComputeMultiple(id, ref mask1DMeshPtr);
                    case LinkGeneratingType.Lateral:
                        return api.ContactsComputeBoundary(id, ref mask1DMeshPtr, ref selectedAreaGeometry, 5000);
                    case LinkGeneratingType.GullySewer:
                        var geometryGullies = gullies
                                              .Where(r => r.Geometry.Intersects(selectedArea))
                                              .Select(r => r.Geometry).ToList();

                        gulliesData = geometryGullies.CreateDisposableGeometryList();
                        return api.ContactsComputeWithPoints(id, ref mask1DMeshPtr, ref gulliesData);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(linkType), linkType, null);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                maskHandle.Free();
                selectedAreaGeometry.Dispose();
                gulliesData?.Dispose();
            }
        }

        private static bool[] GetValidMesh1DFilter(IDiscretization discretization, LinkGeneratingType linkType, out bool isValid, IPolygon selectedArea = null)
        {
            isValid = true;
            var mesh1DFilter = new bool[0];
            if (linkType != LinkGeneratingType.GullySewer && discretization.Locations.Values.Count < 2)
            {
                isValid = false;
                return mesh1DFilter;
            }

            mesh1DFilter = GetMesh1DFilter(discretization, linkType, selectedArea);

            if (!mesh1DFilter.Any(m => m.Equals(true)))
            {
                isValid = false;
            }
            
            return mesh1DFilter;
        }

        private static IList<Link1D2D> Creates1d2dLinks(DisposableContacts linkInformation, UnstructuredGrid grid, IDiscretization networkDiscretization, LinkGeneratingType linkType)
        {
            var lstNewLinks = new List<Link1D2D>();
            for (int i = 0; i < linkInformation.NumContacts; i++)
            {
                //seems lists are swapt  
                var pointIndex = linkInformation.Mesh1dIndices[i];
                var cellIndex = linkInformation.Mesh2dIndices[i];

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

        public static bool[] GetMesh1DFilter(IDiscretization networkDiscretization, LinkGeneratingType linkType, IPolygon selectedArea = null, bool generatedByUser = false)
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
                                                     sewerConnection.WaterType == SewerConnectionWaterType.None ||
                                                     sewerConnection.WaterType == SewerConnectionWaterType.Combined ||
                                                     sewerConnection.WaterType == SewerConnectionWaterType.StormWater;
                            
                            if (!(sewerConnection is null) && isAvailableMesh1DPoint)
                            {
                                if (discretisationPoint.Chainage == 0)
                                {
                                    isAvailableMesh1DPoint &= !(sewerConnection.SourceCompartment is OutletCompartment);
                                }

                                if (Math.Abs(discretisationPoint.Chainage - sewerConnection.Length) < 1e-10)
                                {
                                    isAvailableMesh1DPoint &= !(sewerConnection.TargetCompartment is OutletCompartment);
                                }
                            }
                            
                            break;
                        case LinkGeneratingType.GullySewer:
                            if (sewerConnection != null)
                            {
                                isAvailableMesh1DPoint = sewerConnection.WaterType == SewerConnectionWaterType.Combined ||
                                                         sewerConnection.WaterType == SewerConnectionWaterType.StormWater;
                                
                                if (generatedByUser)
                                {
                                    isAvailableMesh1DPoint = isAvailableMesh1DPoint ||
                                                             sewerConnection.WaterType == SewerConnectionWaterType.None;
                                }
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(linkType), linkType, null);
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