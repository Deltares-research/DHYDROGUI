using System;
using DelftTools.Utils.Data;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class ImportedFMNetFile : Unique<long>
    {
        private UnstructuredGrid grid;

        // nhib
        public ImportedFMNetFile() {}

        public ImportedFMNetFile(string path)
        {
            Path = path;
        }

        public UnstructuredGrid Grid
        {
            get
            {
                if (grid == null)
                {
                    LoadGrid();
                }

                return grid;
            }
        }

        public string Path { get; set; }

        private void LoadGrid()
        {
            try
            {
                grid = UnstructuredGridFileHelper.LoadFromFile(Path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}