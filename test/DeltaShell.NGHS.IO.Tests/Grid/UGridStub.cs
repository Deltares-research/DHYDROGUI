using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    internal class UGridStub : UGrid
    {
        public UGridStub(string file) : base(file) {}

        public bool IsValidViaApi()
        {
            var ioncConvUgrid = GridApiDataSet.DataSetConventions.CONV_UGRID;
            bool isValidViaApi;
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                isValidViaApi = gridApi.AdheresToConventions(ioncConvUgrid);
            }

            return isValidViaApi;
        }

        public static bool TestWrite(string file)
        {
            int ierr;
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                ierr = gridApi.write_geom_ugrid(file);
            }

            return ierr == GridApiDataSet.GridConstants.NOERR;
        }

        public static bool TestWriteMap(string file)
        {
            int ierr;
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                ierr = gridApi.write_map_ugrid(file);
            }

            return ierr == GridApiDataSet.GridConstants.NOERR;
        }
    }
}