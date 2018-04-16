using System;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGrid1D2DLinksApi: RemoteGridApi, IUGrid1D2DLinksApi
    {
        public RemoteUGrid1D2DLinksApi()
        {
            var dimrDllAssembly = typeof(DimrRunner).Assembly;
            api = RemoteInstanceContainer.CreateInstance<IUGrid1D2DLinksApi, UGrid1D2DLinksApi>(Environment.Is64BitOperatingSystem, null, false, dimrDllAssembly);
        }
        public int Create1D2DLinks(int numberOf1D2DLinks)
        {
            var uGrid1D2DLinksApi = api as IUGrid1D2DLinksApi;
            return uGrid1D2DLinksApi != null
                ? uGrid1D2DLinksApi.Create1D2DLinks(numberOf1D2DLinks)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int Write1D2DLinks(int[] mesh1DPointIdx, int[] mesh2DFaceIdx, int[] linkType, string[] linkIds, string[] linkLongNames,
            int numberOf1D2DLinks)
        {
            var uGrid1D2DLinksApi = api as IUGrid1D2DLinksApi;
            return uGrid1D2DLinksApi != null
                ? uGrid1D2DLinksApi.Write1D2DLinks(mesh1DPointIdx, mesh2DFaceIdx, linkType, linkIds, linkLongNames, numberOf1D2DLinks)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNumberOf1D2DLinks(out int numberOf1D2DLinks)
        {
            numberOf1D2DLinks = -1;
            var uGrid1D2DLinksApi = api as IUGrid1D2DLinksApi;
            return uGrid1D2DLinksApi != null
                ? uGrid1D2DLinksApi.GetNumberOf1D2DLinks(out numberOf1D2DLinks)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int Read1D2DLinks(out int[] mesh1DPointIdx, out int[] mesh2DFaceIdx, out int[] linkTYpe, out string[] linkIds,
            out string[] linkLongNames)
        {
            mesh1DPointIdx = new int[] { };
            mesh2DFaceIdx = new int[] { };
            linkTYpe = new int[] { };
            linkIds = new string[]{ };
            linkLongNames = new string[] { };

            var uGrid1D2DLinksApi = api as IUGrid1D2DLinksApi;
            return uGrid1D2DLinksApi != null
                ? uGrid1D2DLinksApi.Read1D2DLinks(out mesh1DPointIdx, out mesh2DFaceIdx, out linkTYpe, out linkIds, out linkLongNames)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }
    }
}
