using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridGeomApi
    {
        protected GridGeomWrapper geomWrapper;
        public const string LIB_DLL_NAME = "gridgeom.dll";
        private const string DFLOWFM_FOLDER_NAME = "share";
        private const string DFLOWFM_BINFOLDER_NAME = "bin";
        private const double missingValue = -999.0;

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

        #region 1d2dlinks logic

        public int Get1D2DLinksFrom1DTo2D(string gridFilePath, IDiscretization networkDiscretization,
            ref List<int> linksFrom, ref List<int> linksTo, ref int startIndex, ref int linksCount, IPolygon selectedArea = null, IList<bool> filter1DMesh = null)
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

                return SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(gridFilePath, networkDiscretization, ref linksFrom, ref linksTo,
                    ref startIndex, ref linksCount, Make1D2DLinksFromMesh1D, selectedArea, LinkType.Embedded, filter1DMesh);
            }
            return GridApiDataSet.GridConstants.NOERR; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }

        public int Get1D2DLinksFromRoofs(string netFilePath, IDiscretization networkDiscretization, ref List<int> linksFrom, ref List<int> linksTo, ref int startIndex, ref int linksCount, List<bool> filter1DMesh, IEnumerable<IGeometry> geometryRoofs)
        {
            if (filter1DMesh == null)
            {
                filter1DMesh = Enumerable.Repeat(true, networkDiscretization.Locations.Values.Count()).ToList();
            }

            if (filter1DMesh.Any(m => m.Equals(true)))
            {
                return SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(netFilePath, networkDiscretization, ref linksFrom, ref linksTo,
                    ref startIndex, ref linksCount, Make1D2DRoofLinks, null, LinkType.RoofSewer, filter1DMesh, geometryRoofs);
            }
            return GridApiDataSet.GridConstants.NOERR; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }

        public int Get1D2DLinksFromGullies(string netFilePath, IDiscretization networkDiscretization, ref List<int> linksFrom, ref List<int> linksTo, ref int startIndex, ref int linksCount, List<bool> filter1DMesh, IEnumerable<IGeometry> geometryGullies)
        {
            if (filter1DMesh == null)
            {
                filter1DMesh = Enumerable.Repeat(true, networkDiscretization.Locations.Values.Count()).ToList();
            }

            if (filter1DMesh.Any(m => m.Equals(true)))
            {
                return SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(netFilePath, networkDiscretization, ref linksFrom, ref linksTo,
                    ref startIndex, ref linksCount, Make1D2DGullyLinks, null, LinkType.GullySewer, filter1DMesh, geometryGullies);
            }
            return GridApiDataSet.GridConstants.NOERR; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }

        public int Get1D2DLinksFrom2DBoundaryCellsTo1D(string gridFilePath, IDiscretization networkDiscretization, ref List<int> linksFrom, ref List<int> linksTo, ref int startIndex, ref int linksCount, IPolygon selectedArea, IList<bool> filter1DMesh)
        {
            var points = networkDiscretization.Locations.Values.Select(p => p.Geometry as IPoint).ToList();

            if (points.Count > 2)
            {
                if (selectedArea == null)
                {
                    selectedArea = GetSelectAllArea(points);
                }

                return SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(gridFilePath, networkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, Make1D2DLateralLinks, selectedArea, LinkType.Lateral, filter1DMesh);
            }
            return GridApiDataSet.GridConstants.NOERR; //no selected area possible, no discretization points available. result will be no 1d2d links anyway -> no error
        }


        private int Make1D2DRoofLinks(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> roofs)
        {
            IntPtr intPtrXValuesRoofs = IntPtr.Zero;
            IntPtr intPtrYValuesRoofs = IntPtr.Zero;
            IntPtr intPtrZValuesRoofs = IntPtr.Zero;
            IntPtr intPtrfilterMesh1DPoints = IntPtr.Zero;

            try
            {
                IList<Coordinate> coordinatesRoofs = GetCoordinatesAndSeparators(roofs);
                var nCoordinatesRoofs = coordinatesRoofs.Count;

                int nFilterMesh1DPoints = filterMesh1DPoints.Count;
                intPtrXValuesRoofs = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesRoofs);
                intPtrYValuesRoofs = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesRoofs);
                intPtrZValuesRoofs = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesRoofs);
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFilterMesh1DPoints);

                var roofXCoords = coordinatesRoofs.Select(c => c.X).ToArray();
                var roofYCoords = coordinatesRoofs.Select(c => c.Y).ToArray();
                var roofZCoords = Enumerable.Repeat<double>(0.0, nCoordinatesRoofs).ToArray();
                var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();

                Marshal.Copy(roofXCoords, 0, intPtrXValuesRoofs, nCoordinatesRoofs);
                Marshal.Copy(roofYCoords, 0, intPtrYValuesRoofs, nCoordinatesRoofs);
                Marshal.Copy(roofZCoords, 0, intPtrZValuesRoofs, nCoordinatesRoofs);
                Marshal.Copy(filterMesh1DPointsArray, 0, intPtrfilterMesh1DPoints, nFilterMesh1DPoints);

                var ierr = geomWrapper.Make1D2DRoofLinks(ref nCoordinatesRoofs, ref intPtrXValuesRoofs,
                    ref intPtrYValuesRoofs, ref intPtrZValuesRoofs, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
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
                if (intPtrXValuesRoofs != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrXValuesRoofs);
                intPtrXValuesRoofs = IntPtr.Zero;
                if (intPtrYValuesRoofs != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrYValuesRoofs);
                intPtrYValuesRoofs = IntPtr.Zero;
                if (intPtrZValuesRoofs != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrZValuesRoofs);
                intPtrZValuesRoofs = IntPtr.Zero;
                if (intPtrfilterMesh1DPoints != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);
                intPtrfilterMesh1DPoints = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
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

        private int Make1D2DLateralLinks(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> filterMesh2D)
        {
            IntPtr intPtrXValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrYValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrZValuesSelectedArea = IntPtr.Zero;
            IntPtr intPtrfilterMesh1DPoints = IntPtr.Zero;
            IntPtr intPtrXValuesFilterMesh2D = IntPtr.Zero;
            IntPtr intPtrYValuesFilterMesh2D = IntPtr.Zero;
            IntPtr intPtrZValuesFilterMesh2D = IntPtr.Zero;

            try
            {
                var coordinatesSelectedArea = selectedArea.Coordinates.ToList();
                coordinatesSelectedArea.Add(new Coordinate(missingValue, missingValue, missingValue)); //add separator
                int nCoordinatesSelectedArea = coordinatesSelectedArea.Count;

                IList<Coordinate> coordinatesFilterMesh2D = GetCoordinatesAndSeparators(filterMesh2D);
                var nCoordinatesFilterMesh2D = coordinatesFilterMesh2D.Count;

                int nFilterMesh1DPoints = filterMesh1DPoints.Count;
                intPtrXValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesSelectedArea);
                intPtrYValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesSelectedArea);
                intPtrZValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesSelectedArea);
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFilterMesh1DPoints);
                intPtrXValuesFilterMesh2D = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesFilterMesh2D);
                intPtrYValuesFilterMesh2D = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesFilterMesh2D);
                intPtrZValuesFilterMesh2D = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinatesFilterMesh2D);

                var selectedAreaXCoords = coordinatesSelectedArea.Select(c => c.X).ToArray();
                var selectedAreaYCoords = coordinatesSelectedArea.Select(c => c.Y).ToArray();
                var selectedAreaZCoords = Enumerable.Repeat<double>(0.0, nCoordinatesSelectedArea).ToArray();
                var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();
                var filterMesh2DXCoords = coordinatesFilterMesh2D.Select(c => c.X).ToArray();
                var filterMesh2DYCoords = coordinatesFilterMesh2D.Select(c => c.Y).ToArray();
                var filterMesh2DZCoords = coordinatesFilterMesh2D.Select(c => c.Z).ToArray();

                Marshal.Copy(selectedAreaXCoords, 0, intPtrXValuesSelectedArea, nCoordinatesSelectedArea);
                Marshal.Copy(selectedAreaYCoords, 0, intPtrYValuesSelectedArea, nCoordinatesSelectedArea);
                Marshal.Copy(selectedAreaZCoords, 0, intPtrZValuesSelectedArea, nCoordinatesSelectedArea);
                Marshal.Copy(filterMesh1DPointsArray, 0, intPtrfilterMesh1DPoints, nFilterMesh1DPoints);
                Marshal.Copy(filterMesh2DXCoords, 0, intPtrXValuesSelectedArea, nCoordinatesFilterMesh2D);
                Marshal.Copy(filterMesh2DYCoords, 0, intPtrYValuesSelectedArea, nCoordinatesFilterMesh2D);
                Marshal.Copy(filterMesh2DZCoords, 0, intPtrZValuesSelectedArea, nCoordinatesFilterMesh2D);

                var ierr = geomWrapper.Make1D2DLateralInternalNetlinks(ref nCoordinatesSelectedArea, ref intPtrXValuesSelectedArea,
                    ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
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
                if (intPtrXValuesFilterMesh2D != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrXValuesFilterMesh2D);
                intPtrXValuesFilterMesh2D = IntPtr.Zero;
                if (intPtrYValuesFilterMesh2D != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrYValuesFilterMesh2D);
                intPtrYValuesFilterMesh2D = IntPtr.Zero;
                if (intPtrZValuesFilterMesh2D != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtrZValuesFilterMesh2D);
                intPtrZValuesFilterMesh2D = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }



        private int Make1D2DLinksFromMesh1D(IPolygon selectedArea, IList<bool> filterMesh1DPoints, IEnumerable<IGeometry> dummy = null)
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

                var ierr = geomWrapper.Make1D2DInternalNetlinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
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
        private int SetUpGridGeomConnectionAndInvokeFunctionMake1D2DLink(string gridFilePath, IDiscretization networkDiscretization, ref List<int> linksFrom, ref List<int> linksTo, ref int startIndex, ref int linksCount, Func<IPolygon, IList<bool>, IEnumerable<IGeometry>,int> make1D2DLinks, IPolygon selectedArea, LinkType linkType, IList<bool> filter1DMesh, IEnumerable<IGeometry> filter2DMesh = null)
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
            GridWrapper.meshgeom meshtwod = new GridWrapper.meshgeom();
            GridWrapper.meshgeomdim meshtwoddim = new GridWrapper.meshgeomdim();
            try
            {
                var ierr = geomWrapper.DeallocateMemory();
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                var gridWrapper = new GridWrapper();

                //1. open the file with the 2d mesh
                int ioncId = 0; //file variable 
                int mode = 0; //create in read mode
                int iConvType = 2;
                double convVersion = 0.0;

                ierr = gridWrapper.Open(gridFilePath, mode, ref ioncId, ref iConvType, ref convVersion);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //2. get the 2d mesh Id
                int meshId = 1;
                ierr = gridWrapper.Get2DMeshId(ref ioncId, ref meshId);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //2.1. Fill in the data related to the 2dMesh
                int num2dNodes = 0;
                ierr = gridWrapper.GetNodeCount(ioncId, meshId, ref num2dNodes);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                int num2dEdges = 0;
                ierr = gridWrapper.GetEdgeCount(ioncId, meshId, ref num2dEdges);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //3. get the dimensions of the 2d mesh
                ierr = gridWrapper.get_meshgeom_dim(ref ioncId, ref meshId, ref meshtwoddim);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //4. allocate the arrays in meshgeom for storing the 2d mesh coordinates, edge_nodes
                meshtwod.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
                meshtwod.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
                meshtwod.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
                meshtwod.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * num2dEdges * 2);

                //5. get the meshgeom arrays
                bool includeArrays = true;
                startIndex = 0;
                ierr = gridWrapper.get_meshgeom(ref ioncId, ref meshId, ref meshtwod, ref startIndex, includeArrays);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //Close file
                ierr = gridWrapper.Close(ioncId);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                double[] rc_twodnodex = new double[num2dNodes];
                double[] rc_twodnodey = new double[num2dNodes];
                double[] rc_twodnodez = new double[num2dNodes];
                Marshal.Copy(meshtwod.nodex, rc_twodnodex, 0, num2dNodes);
                Marshal.Copy(meshtwod.nodey, rc_twodnodey, 0, num2dNodes);
                Marshal.Copy(meshtwod.nodez, rc_twodnodez, 0, num2dNodes);

                //6. allocate the 1d arrays for storing the 1d coordinates and edge_nodes
                var discretisationPoints = networkDiscretization.Locations.AllValues.ToList();

                int nBranches = networkDiscretization.Network.Branches.Count;
                int[] branchIds = networkDiscretization.Locations.AllValues
                    .Select(dp => networkDiscretization.Network.Branches.IndexOf(dp.Branch)).ToArray();

                int nMeshPoints = discretisationPoints.Count;

                double[] meshXCoords = discretisationPoints.Select(dPoint => dPoint.Geometry.Coordinate.X).ToArray();
                double[] meshYCoords = discretisationPoints.Select(dPoint => dPoint.Geometry.Coordinate.Y).ToArray();
                double[] branchOffset = discretisationPoints.Select(dPoint => dPoint.Chainage).ToArray();
                double[] branchLength = networkDiscretization.Network.Branches.Select(b => b.Length).ToArray();

                int[] sourceNodeId = networkDiscretization.Network.Branches
                    .Select(b => b.Network.Nodes.IndexOf(b.Source)).ToArray();
                int[] targetNodeId = networkDiscretization.Network.Branches
                    .Select(b => b.Network.Nodes.IndexOf(b.Target)).ToArray();

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
                int start_index = 0;
                ierr = geomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset,
                    ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches,
                    ref nMeshPoints, ref start_index);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                ierr = geomWrapper.Convert(ref meshtwod, ref meshtwoddim, ref start_index);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                //9. make the links
                ierr = make1D2DLinks.Invoke(selectedArea, filter1DMesh, filter2DMesh);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //10. get the number of links
                var intLinkType = (int)linkType;
                linksCount = 0;
                ierr = geomWrapper.GetLinkCount(ref linksCount, ref intLinkType);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //11. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
                c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * linksCount); //2d cell number
                c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * linksCount); //1d node
                ierr = geomWrapper.Get1d2dLinks(ref c_arrayfrom, ref c_arrayto, ref linksCount, ref intLinkType, ref start_index);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                int[] rcArrayFrom = new int[linksCount];
                int[] rcArrayTo = new int[linksCount];
                Marshal.Copy(c_arrayfrom, rcArrayFrom, 0, linksCount);
                Marshal.Copy(c_arrayto, rcArrayTo, 0, linksCount);
                //for writing the links look io_netcdf ionc_def_mesh_contact, ionc_put_mesh_contact 

                linksFrom = rcArrayFrom.ToList();
                linksTo = rcArrayTo.ToList();
            }
            catch (Exception e)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                //Free 2d arrays
                if (meshtwod.nodex != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(meshtwod.nodex);
                meshtwod.nodex = IntPtr.Zero;
                if (meshtwod.nodey != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(meshtwod.nodey);
                meshtwod.nodey = IntPtr.Zero;
                if (meshtwod.nodez != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(meshtwod.nodez);
                meshtwod.nodez = IntPtr.Zero;
                if (meshtwod.edge_nodes != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(meshtwod.edge_nodes);

                //Free 1d arrays
                if (c_meshXCoords != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_meshXCoords);
                c_meshXCoords = IntPtr.Zero;
                if (c_meshYCoords != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_meshYCoords);
                c_meshYCoords = IntPtr.Zero;
                if (c_branchids != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_branchids);
                c_branchids = IntPtr.Zero;
                if (c_sourcenodeid != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_sourcenodeid);
                c_sourcenodeid = IntPtr.Zero;
                if (c_targetnodeid != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_targetnodeid);
                c_targetnodeid = IntPtr.Zero;
                if (c_branchlength != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_branchlength);
                c_branchlength = IntPtr.Zero;
                if (c_branchoffset != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_branchoffset);
                c_branchoffset = IntPtr.Zero;

                //Free from and to arrays describing the links 
                if (c_arrayfrom != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_arrayfrom);
                c_arrayfrom = IntPtr.Zero;
                if (c_arrayto != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(c_arrayto);
                c_arrayto = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        #endregion

    }
}