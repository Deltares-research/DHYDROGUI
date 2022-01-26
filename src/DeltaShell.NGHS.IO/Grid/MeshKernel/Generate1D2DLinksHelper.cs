using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
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

        /// <summary>
        /// Generates links between 1d and 2d mesh
        /// </summary>
        /// <param name="selectedArea">Area of effect (1d mesh nodes within are connected)</param>
        /// <param name="linkType">Type of connection</param>
        /// <param name="grid">2d mesh</param>
        /// <param name="gullies">List of gullies to connect (<see cref="LinkGeneratingType.GullySewer"/>)</param>
        /// <param name="discretization">1d mesh</param>
        /// <param name="nodesToExclude">Nodes to exclude from linking (nodes with boundary conditions)</param>
        /// <returns>Generated links</returns>
        public static IEnumerable<ILink1D2D> Generate1D2DLinks(IPolygon selectedArea, LinkGeneratingType linkType, UnstructuredGrid grid, IEnumerable<Gully> gullies, IDiscretization discretization, INode[] nodesToExclude = null)
        {
            using (new TimedAction(ts => log.Debug($"Link generation took: {ts.TotalSeconds} sec")))
            {
                if (!GetValidMesh1DFilter(discretization.Locations.Values, linkType, nodesToExclude, selectedArea, out var mask1DMesh))
                {
                    return Enumerable.Empty<ILink1D2D>();
                }

                var filterArea = selectedArea ?? GetSelectAllArea(discretization.Locations.Values.Select(p => p.Geometry as IPoint).ToArray());

                using (var mesh2d = grid.CreateDisposableMesh2D())
                using (var mesh1d = discretization.CreateDisposableMesh1D())
                using (var api = new MeshKernelApi())
                {
                    var id = api.AllocateState(0);

                    // setup 1d/2d meshes
                    var success = api.Mesh1dSet(id, mesh1d);
                    success = success && api.Mesh2dSet(id, mesh2d);

                    // generate contacts (links)
                    success = success && ComputeContacts(api, id, linkType, mask1DMesh, filterArea, gullies);

                    var contacts = success ? api.ContactsGetData(id) : new DisposableContacts(0);

                    api.DeallocateState(id);

                    return Creates1d2dLinks(contacts, grid, discretization, linkType);
                }
            }
        }

        public static bool[] GetMesh1DFilter(IList<INetworkLocation> discretisationPoints, LinkGeneratingType linkType, IPolygon selectedArea = null, INode[] nodesToExclude = null, bool generatedByUser = false)
        {
            var filterList = new bool[discretisationPoints.Count];
            var nodesToExcludeHash = new HashSet<INode>(nodesToExclude ?? Enumerable.Empty<INode>());

            for (int i = 0; i < discretisationPoints.Count; i++)
            {
                if (selectedArea != null && !selectedArea.Intersects(discretisationPoints[i].Geometry))
                {
                    filterList[i] = false;
                    continue;
                }

                filterList[i] = IsMesh1DPointValid(discretisationPoints[i], linkType, generatedByUser, nodesToExcludeHash);
            }
            return filterList;
        }

        private static bool IsMesh1DPointValid(INetworkLocation discretisationPoint, LinkGeneratingType linkType, bool generatedByUser, HashSet<INode> nodesToExcludeHash)
        {
            var sewerConnection = discretisationPoint.Branch as SewerConnection;

            switch (linkType)
            {
                case LinkGeneratingType.Lateral: 
                    return sewerConnection == null;
                case LinkGeneratingType.EmbeddedOneToOne: //go to next case
                case LinkGeneratingType.EmbeddedOneToMany:
                    if (sewerConnection != null)
                    {
                        var isCorrectWaterTypeEmbedded = sewerConnection.WaterType == SewerConnectionWaterType.None ||
                                                 sewerConnection.WaterType == SewerConnectionWaterType.Combined ||
                                                 sewerConnection.WaterType == SewerConnectionWaterType.StormWater;

                        return isCorrectWaterTypeEmbedded && !IsOnOutletCompartment(discretisationPoint, sewerConnection);
                    }
                    
                    if (nodesToExcludeHash.Count != 0)
                    {
                        return !IsExcludedNode(nodesToExcludeHash, discretisationPoint);
                    }

                    return true;
                case LinkGeneratingType.GullySewer:
                    if (sewerConnection == null)
                    {
                        return true;
                    }

                    return sewerConnection.WaterType == SewerConnectionWaterType.Combined ||
                           sewerConnection.WaterType == SewerConnectionWaterType.StormWater || 
                           (sewerConnection.WaterType == SewerConnectionWaterType.None && generatedByUser);

                default:
                    throw new ArgumentOutOfRangeException(nameof(linkType), linkType, null);
            }
        }

        private static bool ComputeContacts(IMeshKernelApi api, int id, LinkGeneratingType linkType, bool[] mask1DMesh, IPolygon selectedArea, IEnumerable<Gully> gullies)
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

        private static bool GetValidMesh1DFilter(IList<INetworkLocation> discretisationPoints, LinkGeneratingType linkType, INode[] nodesToExclude, IPolygon selectedArea, out bool[] mesh1dFilter)
        {
            if (linkType != LinkGeneratingType.GullySewer && discretisationPoints.Count < 2)
            {
                mesh1dFilter = Array.Empty<bool>();
                return false;
            }

            mesh1dFilter = GetMesh1DFilter(discretisationPoints, linkType, selectedArea, nodesToExclude);
            return mesh1dFilter.Any(m => m.Equals(true));
        }

        private static IEnumerable<Link1D2D> Creates1d2dLinks(DisposableContacts linkInformation, UnstructuredGrid grid, IDiscretization networkDiscretization, LinkGeneratingType linkType)
        {
            var lstNewLinks = new List<Link1D2D>();
            for (int i = 0; i < linkInformation.NumContacts; i++)
            {
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

        private static bool IsOnOutletCompartment(INetworkLocation discretisationPoint, SewerConnection sewerConnection)
        {
            if (discretisationPoint.Chainage == 0)
            {
                return sewerConnection.SourceCompartment is OutletCompartment;
            }

            if (discretisationPoint.IsOnEndOfBranch())
            {
                return sewerConnection.TargetCompartment is OutletCompartment;
            }

            return false;
        }

        private static bool IsExcludedNode(HashSet<INode> nodesToExclude, INetworkLocation discretisationPoint)
        {
            if (discretisationPoint.Chainage == 0)
            {
                return nodesToExclude.Contains(discretisationPoint.Branch.Source);
            }
            
            if (discretisationPoint.IsOnEndOfBranch())
            {
                return nodesToExclude.Contains(discretisationPoint.Branch.Target);
            }

            return false;
        }

        private static IPolygon GetSelectAllArea(IReadOnlyCollection<IPoint> points)
        {
            var xMin = points.Select(p => p.X).Min();
            var yMin = points.Select(p => p.Y).Min();
            var xMax = points.Select(p => p.X).Max();
            var yMax = points.Select(p => p.Y).Max();

            var envelope = new Envelope(xMin, xMax, yMin, yMax);

            envelope.ExpandBy((envelope.Width/100) * 2, (envelope.Height/ 100) * 2);

            var coordinates = new[]
            {
                new Coordinate(envelope.MinX, envelope.MaxY),
                new Coordinate(envelope.MinX, envelope.MinY),
                new Coordinate(envelope.MaxX, envelope.MinY),
                new Coordinate(envelope.MaxX, envelope.MaxY),
                new Coordinate(envelope.MinX, envelope.MaxY)
            };

            return new Polygon(new LinearRing(coordinates));
        }
    }
}