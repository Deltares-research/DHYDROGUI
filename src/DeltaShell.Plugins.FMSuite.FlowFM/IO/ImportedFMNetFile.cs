using System;
using DelftTools.Utils.Data;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class ImportedFMNetFile : Unique<long>
    {
        // nhib
        protected ImportedFMNetFile()
        {
        }

        public ImportedFMNetFile(string path)
        {
            Path = path;
        }

        private UnstructuredGrid grid;
        public UnstructuredGrid Grid
        {
            get
            {
                if (grid == null)
                    LoadGrid();
                return grid;
            }
        }

        private void LoadGrid()
        {
            try
            {
                grid = new UnstructuredGrid();
                using (var ugridFile = new UGridFile(Path))
                    ugridFile.SetUnstructuredGrid(grid, recreateCells: false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string Path { get; set; }
    }
}