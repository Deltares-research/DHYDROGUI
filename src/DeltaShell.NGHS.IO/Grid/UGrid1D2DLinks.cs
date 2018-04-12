using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid1D2DLinks : AGrid<IUGrid1D2DLinksApi>
    { 
        public UGrid1D2DLinks(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, mode)
        {
            GridApi = GridApiFactory.CreateNew1D2DLinks();
        }

        public UGrid1D2DLinks(string file, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, globalMetaData, mode)
        {
            GridApi = GridApiFactory.CreateNew1D2DLinks();
        }

        #region Write links

        public void Create1D2DLinksInFile(int numberOf1D2DLinks, int mesh1DIdx, int mesh2DIdx)
        {
            string errorMessage =
                string.Format(Resources.UGrid1D2DLinks_Create1D2DLinksInFile_Couldn_t_create_new_link1D2Dmesh__with_0_links_from_1d_mesh1DId_to_mesh2Id_2, numberOf1D2DLinks, mesh1DIdx, mesh2DIdx);

            var uGrid1D2DLinksApi = GetValidGridApi(errorMessage);

            var ierr = uGrid1D2DLinksApi.Create1D2DLinks(numberOf1D2DLinks, mesh1DIdx, mesh2DIdx);

            ThrowIfError(ierr, errorMessage);
        }

        public void Write1D2DLinks(int[] mesh1DPointIdx, int[] mesh2DFaceIdx, int[] linkType, string[] linkIds, string[] linkLongNames)
        {
            int numberOf1D2DLinks = -1;

            DoWithValidGridApi(uGrid1D2DLinksApi => uGrid1D2DLinksApi.GetNumberOf1D2DLinks(out numberOf1D2DLinks),
                Resources.UGrid1D2DLinks_Couldn_t_get_number_of_1D2DLinks);

            DoWithValidGridApi(uGrid1D2DLinksApi => uGrid1D2DLinksApi.Write1D2DLinks(mesh1DPointIdx, mesh2DFaceIdx, linkType, linkIds, linkLongNames, numberOf1D2DLinks),
                Resources.UGrid1D2DLinks_Write1D2DLinks_Couldn_t_write_1D2DLinks);
        }

        #endregion

        #region Read links

        public void Read1D2DLinks(out int[] mesh1DPointIdx, out int[] mesh2DFaceIdx, out int[] linkTYpe, out string[] linkIds, out string[] linkLongNames)
        {
            var uGrid1D2DLinksApi = GetValidGridApi(Resources.UGrid1D2DLinks_Read1D2DLinks_Couldn_t_read_links);

            var ierr = uGrid1D2DLinksApi.Read1D2DLinks(out mesh1DPointIdx, out mesh2DFaceIdx, out linkTYpe, out linkIds, out linkLongNames);

            ThrowIfError(ierr, Resources.UGrid1D2DLinks_Read1D2DLinks_Couldn_t_read_links);
        }

        #endregion
    }
}
