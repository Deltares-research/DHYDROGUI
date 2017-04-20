using System;
using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    internal class UGridStub : UGrid
    {
        public UGridStub(string file) : base(file) { }

        public bool IsValidViaApi()
        {
            var ioncConvUgrid = GridApiDataSet.DataSetConventions.IONC_CONV_UGRID;
            bool isValidViaApi;
            using (var gridApi = GridApiFactory.CreateNew())
            {
                isValidViaApi = gridApi.adherestoConventions(ioncConvUgrid);
            }
            return isValidViaApi;
        }
        
        public static bool TestWrite(string file)
        {
            int ierr;
            using (var gridApi = GridApiFactory.CreateNew())
            {
                ierr = gridApi.ionc_write_geom_ugrid(file);
            }
            return ierr == GridApiDataSet.GridConstants.IONC_NOERR;
        }
        
        public static bool TestWriteMap(string file)
        {
            int ierr;
            using (var gridApi = GridApiFactory.CreateNew())
            {
                ierr = gridApi.ionc_write_map_ugrid(file);
            }
            return ierr == GridApiDataSet.GridConstants.IONC_NOERR;
        }
    }
}