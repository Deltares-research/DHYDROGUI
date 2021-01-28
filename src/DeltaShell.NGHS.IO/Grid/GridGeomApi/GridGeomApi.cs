using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    public class GridGeomApi : IGridGeomApi
    {
        private GridGeomWrapper geomWrapper;

        public const string LIB_DLL_NAME = "gridgeom.dll";
        private const string DFLOWFM_FOLDER_NAME = "share";
        private const string DFLOWFM_BINFOLDER_NAME = "bin";

        static GridGeomApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(LIB_DLL_NAME, DllPath);
        }

        public GridGeomApi()
        {
            geomWrapper = new GridGeomWrapper();
        }

        ~GridGeomApi()  
        {
            ReleaseUnmanagedResources();
        }

        private static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        private static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DFLOWFM_FOLDER_NAME, DFLOWFM_BINFOLDER_NAME); }
        }

        public int LastErrorCode { get; private set; } = UGridConstants.NoErrorCode;

        /// <inheritdoc/>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public LinkInformation GetLinkInformation(DisposableMeshGeometryGridGeom mesh2D, Mesh1DGeometry mesh1D, GeometriesData selectedArea, bool[] filter1DMesh, LinkGeneratingType linkType, GeometriesData geometryGullies = null)
        {
            try
            {
                if (DoWithApi(() => Set1DMesh(mesh1D)))
                    return null;

                if (DoWithApi(() => Set2DMesh(mesh2D)))
                    return null;

                if (DoWithApi(() => Make1D2DLinks(linkType, selectedArea, filter1DMesh, geometryGullies)))
                    return null;

                return GetLinkInformation(linkType);

            }
            catch (Exception)
            {
                LastErrorCode = UGridConstants.GeneralFatalErrorCode;
                return null;
            }
        }

        private int Make1D2DLinks(LinkGeneratingType linkType, GeometriesData selectedArea, bool[] filterMesh1DPoints, GeometriesData gullies)
        {
            var filterMesh1DPointsArray = filterMesh1DPoints.Select(b => b ? 1 : 0).ToArray();
            var pinnedFilterMesh1DPointsArray = GCHandle.Alloc(filterMesh1DPointsArray, GCHandleType.Pinned);

            var intPtrXValuesSelectedArea = selectedArea.GetPinnedObjectPointer(selectedArea.XValues);
            var intPtrYValuesSelectedArea = selectedArea.GetPinnedObjectPointer(selectedArea.YValues);
            var intPtrZValuesSelectedArea = selectedArea.GetPinnedObjectPointer(selectedArea.ZValues);

            try
            {
                int nCoordinates = selectedArea.XValues.Length;
                int nFilterMesh1DPoints = filterMesh1DPoints.Length;
                var intPtrfilterMesh1DPoints = pinnedFilterMesh1DPointsArray.AddrOfPinnedObject();

                int ierr;

                switch (linkType)
                {
                    case LinkGeneratingType.EmbeddedOneToOne:
                        ierr = geomWrapper.Make1D2DEmbeddedOneToOneLinks(ref nCoordinates, ref intPtrXValuesSelectedArea, ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                        break;
                    case LinkGeneratingType.EmbeddedOneToMany:
                        ierr = geomWrapper.Make1D2DEmbeddedOneToManyLinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
                            ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                        break;
                    case LinkGeneratingType.Lateral:
                        ierr = geomWrapper.Make1D2DLateralLinks(ref nCoordinates, ref intPtrXValuesSelectedArea, ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                        break;
                    case LinkGeneratingType.GullySewer:
                        int nCoordinatesGullies = gullies.XValues.Length;
                        var intPtrXValuesGullies = gullies.GetPinnedObjectPointer(gullies.XValues);
                        var intPtrYValuesGullies = gullies.GetPinnedObjectPointer(gullies.YValues);
                        
                        ierr = geomWrapper.Make1D2DGullyLinks(ref nCoordinatesGullies, ref intPtrXValuesGullies, ref intPtrYValuesGullies, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(linkType), linkType, null);
                }

                if (ierr != UGridConstants.NoErrorCode)
                {
                    return ierr;
                }
            }
            catch
            {
                pinnedFilterMesh1DPointsArray.Free();
                return UGridConstants.GeneralFatalErrorCode;
            }

            return UGridConstants.NoErrorCode;
        }

        private bool DoWithApi(Func<int> func)
        {
            var errorCode = func();
            var hasError = errorCode != UGridConstants.NoErrorCode;

            if (hasError)
            {
                LastErrorCode = errorCode;
            }

            return hasError;
        }

        private int Set2DMesh(DisposableMeshGeometryGridGeom mesh2d)
        {
            var meshtwod = mesh2d.CreateMeshGeometry();
            var meshtwoddim = mesh2d.CreateMeshDimensions();

            return geomWrapper.Convert(ref meshtwod, ref meshtwoddim);
        }

        private int Set1DMesh(Mesh1DGeometry mesh1D)
        {
            var native = mesh1D.GetNative();

            return geomWrapper.Convert1dArray(ref native.meshXCoords, ref native.meshYCoords, ref native.branchOffset,
                ref native.branchLength, ref native.branchIds, ref native.sourcenodeid, ref native.targetnodeid,
                ref native.nBranches, ref native.nMeshPoints);
        }

        private LinkInformation GetLinkInformation(LinkGeneratingType linkType)
        {
            var linkInformation = new LinkInformation();

            var linksCount = 0;
            var linkTypeNumber = (int) linkType.GetLinkStorageType();
            
            if (DoWithApi(() => geomWrapper.GetLinkCount(ref linksCount, ref linkTypeNumber)))
                return linkInformation;

            var fromArrayHandle = GCHandle.Alloc(new int[linksCount], GCHandleType.Pinned);
            var toArrayHandle = GCHandle.Alloc(new int[linksCount], GCHandleType.Pinned);

            var fromPointer = fromArrayHandle.AddrOfPinnedObject();
            var toPointer = toArrayHandle.AddrOfPinnedObject();

            if (DoWithApi(() => geomWrapper.Get1d2dLinks(ref fromPointer, ref toPointer, ref linksCount, ref linkTypeNumber)))
            {
                return linkInformation;
            }

            linkInformation.FromIndices = CreateValueArray(fromPointer, linksCount);
            linkInformation.ToIndices = CreateValueArray(toPointer, linksCount);

            fromArrayHandle.Free();
            toArrayHandle.Free();

            return linkInformation;
        }

        private static int[] CreateValueArray(IntPtr pointer, int size)
        {
            var array = new int[size];
            Marshal.Copy(pointer, array, 0, size);

            return array;
        }

        private void ReleaseUnmanagedResources()
        {
            geomWrapper?.DeallocateMemory();
        }
    }
}