using System;
using System.Runtime.InteropServices;
using System.Text;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid1D2DLinksApi: GridApi, IUGrid1D2DLinksApi
    {
        protected int meshLinks1D2DIdx;

        public UGrid1D2DLinksApi()
        {
            meshLinks1D2DIdx = -1;
        }

        public int Create1D2DLinks(int numberOf1D2DLinks, int mesh1Idx, int mesh2Idx)
        {
            meshLinks1D2DIdx = -1;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
                var ierr = wrapper.Create1D2DLinks(ioncId, ref meshLinks1D2DIdx, GridApiDataSet.DataSetNames.Links1D2D, numberOf1D2DLinks, mesh1Idx, mesh2Idx, (int)GridApiDataSet.LocationType.UG_LOC_NODE, (int)GridApiDataSet.LocationType.UG_LOC_FACE);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                return GridApiDataSet.GridConstants.NOERR;
            }
            catch(Exception e)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int Write1D2DLinks(int[] mesh1DPointIdx, int[] mesh2DFaceIdx, int[] linkType, string[] linkIds, string[] linkLongNames, int numberOf1D2DLinks)
        {
            if (!Initialized || !Links1D2DReadyForWriting) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            var err = CheckArrayFormatWithFileDeclaration(mesh1DPointIdx, mesh2DFaceIdx, linkType, linkIds, linkLongNames, numberOf1D2DLinks);
            if( err != GridApiDataSet.GridConstants.NOERR) return err;

            IntPtr intPtrMesh1Dindexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOf1D2DLinks);
            IntPtr intPtrMesh2Dindexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOf1D2DLinks);
            IntPtr intPtrContactType = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOf1D2DLinks);

            try
            {
                Marshal.Copy(mesh1DPointIdx, 0, intPtrMesh1Dindexes, numberOf1D2DLinks);
                Marshal.Copy(mesh2DFaceIdx, 0, intPtrMesh2Dindexes, numberOf1D2DLinks);
                Marshal.Copy(linkType, 0, intPtrContactType, numberOf1D2DLinks);

                GridWrapper.interop_charinfo[] linkInfo = new GridWrapper.interop_charinfo[numberOf1D2DLinks];

                for (int i = 0; i < numberOf1D2DLinks; i++)
                {
                    string tmpstring;
                    tmpstring = linkIds[i] ?? string.Empty;
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    linkInfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = linkLongNames[i] ?? string.Empty;
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    linkInfo[i].longnames = tmpstring.ToCharArray();
                }
                var ierr = wrapper.Write1D2DLinks(ioncId, meshLinks1D2DIdx, intPtrMesh1Dindexes, intPtrMesh2Dindexes, intPtrContactType, linkInfo, numberOf1D2DLinks);

                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                Marshal.FreeCoTaskMem(intPtrMesh1Dindexes);
                Marshal.FreeCoTaskMem(intPtrMesh2Dindexes);
                Marshal.FreeCoTaskMem(intPtrContactType);
            }
        }

        public int GetNumberOf1D2DLinks(out int numberOf1D2DLinks)
        {
            numberOf1D2DLinks = -1;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
                return wrapper.GetNumberOf1D2DLinks(ref ioncId, ref meshLinks1D2DIdx , ref numberOf1D2DLinks);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int Read1D2DLinks(out int[] mesh1DPointIdx, out int[] mesh2DFaceIdx, out int[] linkType, out string[] linkIds, out string[] linkLongNames)
        {
            mesh1DPointIdx = new int[0];
            mesh2DFaceIdx = new int[0];
            linkType = new int[0];
            linkIds = new string[0];
            linkLongNames = new string[0];

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOf1D2DLinks = -1;
            try
            {
                var ierr = GetNumberOf1D2DLinks(out numberOf1D2DLinks);
                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
                if (numberOf1D2DLinks < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            IntPtr mesh1DPointIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOf1D2DLinks);
            IntPtr mesh2DFaceIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOf1D2DLinks);
            IntPtr linkTypePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOf1D2DLinks);

            try
            {
                var linksInfo = new GridWrapper.interop_charinfo[numberOf1D2DLinks];
                var ierr = wrapper.Read1D2DLinks(ioncId, meshLinks1D2DIdx, ref mesh1DPointIdxPtr, ref mesh2DFaceIdxPtr, ref linkTypePtr, ref linksInfo, ref numberOf1D2DLinks);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                mesh1DPointIdx = new int[numberOf1D2DLinks];
                mesh2DFaceIdx = new int[numberOf1D2DLinks];
                linkType = new int[numberOf1D2DLinks];
                linkIds = new string[numberOf1D2DLinks];
                linkLongNames = new string[numberOf1D2DLinks];

                Marshal.Copy(mesh1DPointIdxPtr, mesh1DPointIdx, 0, numberOf1D2DLinks);
                Marshal.Copy(mesh2DFaceIdxPtr, mesh2DFaceIdx, 0, numberOf1D2DLinks);
                Marshal.Copy(linkTypePtr, linkType, 0, numberOf1D2DLinks);

                for (int i = 0; i< numberOf1D2DLinks; ++i)
                {
                    linkIds[i] = new string(linksInfo[i].ids).Trim();
                    linkLongNames[i] = new string(linksInfo[i].longnames).Trim(); 
                }

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (mesh1DPointIdxPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(mesh1DPointIdxPtr);
                mesh1DPointIdxPtr = IntPtr.Zero;
                if (mesh2DFaceIdxPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(mesh2DFaceIdxPtr);
                mesh2DFaceIdxPtr = IntPtr.Zero;
                if (linkTypePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(linkTypePtr);
                linkTypePtr = IntPtr.Zero;
            }
        }
        public virtual bool Links1D2DReadyForWriting
        {
            get { return meshLinks1D2DIdx > 0; }
        }

        private int CheckArrayFormatWithFileDeclaration(int[] mesh1DPointIdx, int[] mesh2DFaceIdx, int[] linkType, string[] linkIds, string[] linkLongNames, int numberOf1D2DLinks)
        {
            int numberOfLinks;
            if (GetNumberOf1D2DLinks(out numberOfLinks) != GridApiDataSet.GridConstants.NOERR)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            if (numberOfLinks < 0 ||
                numberOfLinks != numberOf1D2DLinks ||
                numberOfLinks != linkIds.Length ||
                numberOfLinks != linkLongNames.Length ||
                numberOfLinks != linkType.Length ||
                numberOfLinks != mesh1DPointIdx.Length ||
                numberOfLinks != mesh2DFaceIdx.Length
                )
            {
                return GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR;
            }
            return GridApiDataSet.GridConstants.NOERR;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
