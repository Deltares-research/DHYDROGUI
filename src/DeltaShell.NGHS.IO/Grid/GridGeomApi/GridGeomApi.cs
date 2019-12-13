using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    public class GridGeomApi
    {
        protected GridGeomWrapper geomWrapper;
        public const string LIB_DLL_NAME = "gridgeom.dll";
        private const string DFLOWFM_FOLDER_NAME = "share";
        private const string DFLOWFM_BINFOLDER_NAME = "bin";
        private const double missingValue = -999.0;

        static GridGeomApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(LIB_DLL_NAME, DllPath);
        }

        public GridGeomApi()
        {
            geomWrapper = new GridGeomWrapper();
            geomWrapper.DeallocateMemory();
        }

        public static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DFLOWFM_FOLDER_NAME, DFLOWFM_BINFOLDER_NAME); }
        }

        public int LastErrorCode { get; private set; } = GridApiDataSet.GridConstants.NOERR;

        #region 1d2dlinks logic

        public LinkInformation GetEmbedded1D2DLinks(DisposableMeshGeometryGridGeom disposableMeshGeometryGridGeom, IDiscretization networkDiscretization, IPolygon selectedArea = null, IList<bool> filter1DMesh = null, bool oneToMany = true)
        {
            if (filter1DMesh == null)
            {
                filter1DMesh = Enumerable.Repeat(true, networkDiscretization.Locations.Values.Count()).ToList();
            }

            var points = networkDiscretization.Locations.Values.Select(p => p.Geometry as IPoint).ToList();

            if (points.Count > 2)
            {
                if (selectedArea == null)
                {
                    selectedArea = GetSelectAllArea(points);
                }

                return oneToMany
                    ? SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(disposableMeshGeometryGridGeom,
                        networkDiscretization, MakeEmbeddedOneToMany1D2DLinks, selectedArea, LinkType.EmbeddedOneToOne,
                        filter1DMesh)
                    : SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(disposableMeshGeometryGridGeom,
                        networkDiscretization, MakeEmbeddedOneToOne1D2DLinks, selectedArea, LinkType.EmbeddedOneToOne,
                        filter1DMesh);
            }

            LastErrorCode = GridApiDataSet.GridConstants.NOERR;
            return null; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }

        public LinkInformation GetLateral1D2DLinks(DisposableMeshGeometryGridGeom disposableMeshGeometryGridGeom, IDiscretization networkDiscretization, IPolygon selectedArea = null, IList<bool> filter1DMesh = null)
        {
            if (filter1DMesh == null)
            {
                filter1DMesh = Enumerable.Repeat(true, networkDiscretization.Locations.Values.Count()).ToList();
            }

            var points = networkDiscretization.Locations.Values.Select(p => p.Geometry as IPoint).ToList();

            if (points.Count > 2)
            {
                if (selectedArea == null)
                {
                    selectedArea = GetSelectAllArea(points);
                }

                return SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(disposableMeshGeometryGridGeom, networkDiscretization, MakeLateral1D2DLinks, selectedArea, LinkType.EmbeddedOneToOne, filter1DMesh);
            }

            LastErrorCode = GridApiDataSet.GridConstants.NOERR;

            return null; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }

        public LinkInformation Get1D2DLinksFromGullies(DisposableMeshGeometryGridGeom disposableMeshGeometryGridGeom, IDiscretization networkDiscretization, List<bool> filter1DMesh, IEnumerable<IGeometry> geometryGullies)
        {
            if (filter1DMesh == null)
            {
                filter1DMesh = Enumerable.Repeat(true, networkDiscretization.Locations.Values.Count()).ToList();
            }

            if (filter1DMesh.Any(m => m.Equals(true)))
            {
                return SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(disposableMeshGeometryGridGeom, networkDiscretization, Make1D2DGullyLinks, null, LinkType.GullySewer, filter1DMesh, geometryGullies);
            }

            LastErrorCode = GridApiDataSet.GridConstants.NOERR;
            return null; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }

        private int Make1D2DGullyLinks(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> gullies)
        {
            IntPtr intPtrXValuesGullies = IntPtr.Zero;
            IntPtr intPtrYValuesGullies = IntPtr.Zero;
            IntPtr intPtrfilterMesh1DPoints = IntPtr.Zero;

            try
            {
                //gullies without seperator
                IList<Coordinate> coordinatesGullies = gullies.Select(g => g.Coordinate).ToList();
                int nCoordinatesGullies = coordinatesGullies.Count;

                int nFilterMesh1DPoints = filterMesh1DPoints.Count;
                intPtrXValuesGullies = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesGullies);
                intPtrYValuesGullies = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesGullies);
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFilterMesh1DPoints);

                var gullyXCoords = coordinatesGullies.Select(c => c.X).ToArray();
                var gullyYCoords = coordinatesGullies.Select(c => c.Y).ToArray();
                var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();

                Marshal.Copy(gullyXCoords, 0, intPtrXValuesGullies, nCoordinatesGullies);
                Marshal.Copy(gullyYCoords, 0, intPtrYValuesGullies, nCoordinatesGullies);
                Marshal.Copy(filterMesh1DPointsArray, 0, intPtrfilterMesh1DPoints, nFilterMesh1DPoints);

                var ierr = geomWrapper.Make1D2DGullyLinks(ref nCoordinatesGullies, ref intPtrXValuesGullies, ref intPtrYValuesGullies, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (intPtrXValuesGullies != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrXValuesGullies);
                intPtrXValuesGullies = IntPtr.Zero;
                if (intPtrYValuesGullies != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrYValuesGullies);
                intPtrYValuesGullies = IntPtr.Zero;
                if (intPtrfilterMesh1DPoints != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);
                intPtrfilterMesh1DPoints = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        private int MakeLateral1D2DLinks(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> dummy = null)
        {
            IntPtr intPtrXValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrYValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrZValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrfilterMesh1DPoints = IntPtr.Zero;

            try
            {

                var coordinates = selectedArea.Coordinates.ToList();
                coordinates.Add(new Coordinate(missingValue, missingValue, missingValue)); //add separator
                int nCoordinates = coordinates.Count;
                var selectedAreaXCoords = coordinates.Select(c => c.X).ToArray();
                var selectedAreaYCoords = coordinates.Select(c => c.Y).ToArray();
                var selectedAreaZCoords = Enumerable.Repeat<double>(0.0, nCoordinates).ToArray();

                intPtrXValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrYValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrZValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);

                var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();
                var nFilterMesh1DPointsArray = filterMesh1DPointsArray.Length;
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFilterMesh1DPointsArray);

                Marshal.Copy(selectedAreaXCoords, 0, intPtrXValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaYCoords, 0, intPtrYValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaZCoords, 0, intPtrZValuesSelectedArea, nCoordinates);
                Marshal.Copy(filterMesh1DPointsArray, 0, intPtrfilterMesh1DPoints, nFilterMesh1DPointsArray);

                var ierr = geomWrapper.Make1D2DLateralLinks(ref nCoordinates, ref intPtrXValuesSelectedArea, ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPointsArray, ref intPtrfilterMesh1DPoints);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (intPtrXValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrXValuesSelectedArea);
                intPtrXValuesSelectedArea = IntPtr.Zero;
                if (intPtrYValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrYValuesSelectedArea);
                intPtrYValuesSelectedArea = IntPtr.Zero;
                if (intPtrZValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrZValuesSelectedArea);
                intPtrZValuesSelectedArea = IntPtr.Zero;
                if (intPtrfilterMesh1DPoints != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);
                intPtrfilterMesh1DPoints = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        private int MakeEmbeddedOneToOne1D2DLinks(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> dummy = null)
        {
            IntPtr intPtrXValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrYValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrZValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrfilterMesh1DPoints = IntPtr.Zero;

            try
            {

                var coordinates = selectedArea.Coordinates.ToList();
                coordinates.Add(new Coordinate(missingValue, missingValue, missingValue)); //add separator
                int nCoordinates = coordinates.Count;
                var selectedAreaXCoords = coordinates.Select(c => c.X).ToArray();
                var selectedAreaYCoords = coordinates.Select(c => c.Y).ToArray();
                var selectedAreaZCoords = Enumerable.Repeat<double>(0.0, nCoordinates).ToArray();

                intPtrXValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (double))*nCoordinates);
                intPtrYValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (double))*nCoordinates);
                intPtrZValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (double))*nCoordinates);

                var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();
                var nFilterMesh1DPointsArray = filterMesh1DPointsArray.Length;
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFilterMesh1DPointsArray);

                Marshal.Copy(selectedAreaXCoords, 0, intPtrXValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaYCoords, 0, intPtrYValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaZCoords, 0, intPtrZValuesSelectedArea, nCoordinates);
                Marshal.Copy(filterMesh1DPointsArray, 0, intPtrfilterMesh1DPoints, nFilterMesh1DPointsArray);

                var ierr = geomWrapper.Make1D2DEmbeddedOneToOneLinks(ref nCoordinates, ref intPtrXValuesSelectedArea, ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPointsArray, ref intPtrfilterMesh1DPoints);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (intPtrXValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrXValuesSelectedArea);
                intPtrXValuesSelectedArea = IntPtr.Zero;
                if (intPtrYValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrYValuesSelectedArea);
                intPtrYValuesSelectedArea = IntPtr.Zero;
                if (intPtrZValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrZValuesSelectedArea);
                intPtrZValuesSelectedArea = IntPtr.Zero;
                if (intPtrfilterMesh1DPoints != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);
                intPtrfilterMesh1DPoints = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        private int MakeEmbeddedOneToMany1D2DLinks(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> dummy = null)
        {
            IntPtr intPtrXValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrYValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrZValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrfilterMesh1DPoints = IntPtr.Zero;

            try
            {

                var coordinates = selectedArea.Coordinates.ToList();
                coordinates.Add(new Coordinate(missingValue, missingValue, missingValue)); //add separator
                int nCoordinates = coordinates.Count;
                var selectedAreaXCoords = coordinates.Select(c => c.X).ToArray();
                var selectedAreaYCoords = coordinates.Select(c => c.Y).ToArray();
                var selectedAreaZCoords = Enumerable.Repeat<double>(0.0, nCoordinates).ToArray();

                intPtrXValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrYValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrZValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);

                var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();
                var nFilterMesh1DPointsArray = filterMesh1DPointsArray.Length;
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFilterMesh1DPointsArray);

                Marshal.Copy(selectedAreaXCoords, 0, intPtrXValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaYCoords, 0, intPtrYValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaZCoords, 0, intPtrZValuesSelectedArea, nCoordinates);
                Marshal.Copy(filterMesh1DPointsArray, 0, intPtrfilterMesh1DPoints, nFilterMesh1DPointsArray);

                var ierr = geomWrapper.Make1D2DEmbeddedOneToManyLinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
                        ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPointsArray, ref intPtrfilterMesh1DPoints);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (intPtrXValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrXValuesSelectedArea);
                intPtrXValuesSelectedArea = IntPtr.Zero;
                if (intPtrYValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrYValuesSelectedArea);
                intPtrYValuesSelectedArea = IntPtr.Zero;
                if (intPtrZValuesSelectedArea != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrZValuesSelectedArea);
                intPtrZValuesSelectedArea = IntPtr.Zero;
                if (intPtrfilterMesh1DPoints != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);
                intPtrfilterMesh1DPoints = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        private static IPolygon GetSelectAllArea(List<IPoint> points)
        {
            IPolygon selectedArea;
            var coordinates = new List<Coordinate>();
            var xMin = points.Select(p => p.X).Min();
            var yMin = points.Select(p => p.Y).Min();
            var xMax = points.Select(p => p.X).Max();
            var yMax = points.Select(p => p.Y).Max();

            coordinates.Add(new Coordinate(xMin, yMax));
            coordinates.Add(new Coordinate(xMin, yMin));
            coordinates.Add(new Coordinate(xMax, yMin));
            coordinates.Add(new Coordinate(xMax, yMin));
            coordinates.Add(new Coordinate(xMin, yMax));

            selectedArea = new Polygon(new LinearRing(coordinates.ToArray()));
            return selectedArea;
        }

        private IList<Coordinate> GetCoordinatesAndSeparators(IEnumerable<IGeometry> lstGeometry)
        {
            var coordinates = new List<Coordinate>();
            foreach (var geometry in lstGeometry)
            {
                foreach (var coordinate in geometry.Coordinates)
                {
                    coordinates.Add(new Coordinate(coordinate.X, coordinate.Y, 0.0));
                }
                coordinates.Add(new Coordinate(missingValue, missingValue, missingValue));
            }
            return coordinates;
        }

        // TODO: This method is huge.. Make smaller please :(
        private LinkInformation SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(DisposableMeshGeometryGridGeom mesh2d, IDiscretization networkDiscretization, Func<IPolygon, IList<bool>, IEnumerable<IGeometry>,int> make1D2DLinks, IPolygon selectedArea, LinkType linkType, IList<bool> filter1DMesh, IEnumerable<IGeometry> filter2DMesh = null)
        {
            IntPtr c_meshXCoords = IntPtr.Zero;
            IntPtr c_meshYCoords = IntPtr.Zero;
            IntPtr c_branchids = IntPtr.Zero;
            IntPtr c_branchoffset = IntPtr.Zero;
            IntPtr c_sourcenodeid = IntPtr.Zero;
            IntPtr c_targetnodeid = IntPtr.Zero;
            IntPtr c_branchlength = IntPtr.Zero;
            IntPtr c_arrayfrom = IntPtr.Zero;
            IntPtr c_arrayto = IntPtr.Zero;

            try
            {
                var ierr = geomWrapper.DeallocateMemory();
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    LastErrorCode = ierr;
                    return null;
                }

                //6. allocate the 1d arrays for storing the 1d coordinates and edge_nodes
                var discretisationPoints = networkDiscretization.Locations.AllValues.ToList();
                var branches = networkDiscretization.Network.Branches;
                var branchesIndexLookup = branches.ToIndexDictionary();
                var nodesIndexLookup = networkDiscretization.Network.Nodes.ToIndexDictionary();

                int nBranches = branches.Count;
                var branchLength = new double[nBranches];
                var sourceNodeId = new int[nBranches];
                var targetNodeId = new int[nBranches];

                for (int i = 0; i < nBranches; i++)
                {
                    var branch = branches[i];
                    branchLength[i] = branch.Length;
                    sourceNodeId[i] = nodesIndexLookup[branch.Source];
                    targetNodeId[i] = nodesIndexLookup[branch.Target];
                }
                
                int nMeshPoints = discretisationPoints.Count;
                
                var branchIds = new int[nMeshPoints];
                var meshXCoords = new double[nMeshPoints];
                var meshYCoords = new double[nMeshPoints];
                var branchOffset = new double[nMeshPoints];

                for (int i = 0; i < nMeshPoints; i++)
                {
                    var point = discretisationPoints[i];
                    branchIds[i] = branchesIndexLookup[point.Branch];
                    meshXCoords[i] = point.Geometry.Coordinate.X;
                    meshYCoords[i] = point.Geometry.Coordinate.Y;
                    branchOffset[i] = point.Chainage;
                }

                c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
                c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
                c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nMeshPoints);
                c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
                c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
                c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
                c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);

                Marshal.Copy(branchIds, 0, c_branchids, nMeshPoints);
                Marshal.Copy(meshXCoords, 0, c_meshXCoords, nMeshPoints);
                Marshal.Copy(meshYCoords, 0, c_meshYCoords, nMeshPoints);
                Marshal.Copy(sourceNodeId, 0, c_sourcenodeid, nBranches);
                Marshal.Copy(targetNodeId, 0, c_targetnodeid, nBranches);
                Marshal.Copy(branchOffset, 0, c_branchoffset, nMeshPoints);
                Marshal.Copy(branchLength, 0, c_branchlength, nBranches);

                //7. fill kn (Herman datastructure) for creating the links
                ierr = geomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset,
                    ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches,
                    ref nMeshPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    LastErrorCode = ierr;
                    return null;
                }

                var meshtwod = mesh2d.CreateMeshGeometry();
                var meshtwoddim = mesh2d.CreateMeshDimensions();

                try
                {
                    ierr = geomWrapper.Convert(ref meshtwod, ref meshtwoddim);
                }
                finally
                {
                    mesh2d.UnPinMemory();
                }

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    LastErrorCode = ierr;
                    return null;
                }

                //9. make the links
                ierr = make1D2DLinks.Invoke(selectedArea, filter1DMesh, filter2DMesh);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    LastErrorCode = ierr;
                    return null;
                }

                //10. get the number of links
                var intLinkType = (int)linkType;
                var linksCount = 0;
                ierr = geomWrapper.GetLinkCount(ref linksCount, ref intLinkType);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    LastErrorCode = ierr;
                    return null;
                }

                //11. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
                c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * linksCount); //2d cell number
                c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * linksCount); //1d node
                ierr = geomWrapper.Get1d2dLinks(ref c_arrayfrom, ref c_arrayto, ref linksCount, ref intLinkType);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    LastErrorCode = ierr;
                    return null;
                }

                int[] rcArrayFrom = new int[linksCount];
                int[] rcArrayTo = new int[linksCount];
                Marshal.Copy(c_arrayfrom, rcArrayFrom, 0, linksCount);
                Marshal.Copy(c_arrayto, rcArrayTo, 0, linksCount);
                //for writing the links look io_netcdf ionc_def_mesh_contact, ionc_put_mesh_contact 

                return new LinkInformation
                {
                    fromIndices = rcArrayFrom,
                    toIndices = rcArrayTo
                };
            }
            catch (Exception e)
            {
                LastErrorCode = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
                return null;
            }
            finally
            {
                //Free 1d arrays
                if (c_meshXCoords != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_meshXCoords);
                
                if (c_meshYCoords != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_meshYCoords);
                
                if (c_branchids != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_branchids);
                
                if (c_sourcenodeid != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_sourcenodeid);
                
                if (c_targetnodeid != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_targetnodeid);
                
                if (c_branchlength != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_branchlength);
                
                if (c_branchoffset != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_branchoffset);
                
                //Free from and to arrays describing the links 
                if (c_arrayfrom != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_arrayfrom);
                
                if (c_arrayto != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_arrayto);
            }
        }

        #endregion

    }
}