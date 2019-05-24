using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public class WavmFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private const string NSizeDimensionName = "nmax";
        private const string MSizeDimensionName = "mmax";
        private const string XCoordinateVariableName = "x";
        private const string YCoordinateVariableName = "y";

        public CurvilinearGrid Grid { get; private set; }

        //nhib
        protected WavmFileFunctionStore() : base() {}

        public WavmFileFunctionStore(string ncPath) : base(ncPath)
        {
            DisableCaching = true;
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            Grid = ReadGridFromFile();

            IEnumerable<string> dimNames = netCdfFile.GetAllDimensions().Select(d => netCdfFile.GetDimensionName(d));
            if (dimNames.Intersect(new[]
            {
                TimeDimensionNames[0],
                NSizeDimensionName,
                MSizeDimensionName
            }).Count() != 3)
            {
                yield break;
            }

            foreach (NetCdfVariableInfo varInfo in dataVariables)
            {
                if (!varInfo.IsTimeDependent || varInfo.NumDimensions != 3)
                {
                    continue;
                }

                var coverage = new CurvilinearCoverage(Grid) {IsTimeDependent = true};
                coverage.Store = this;

                coverage.Arguments[0].Name = "Time";
                coverage.Arguments[0].Attributes[NcNameAttribute] = TimeVariableNames[0];
                coverage.Arguments[0].Attributes[NcUseVariableSizeAttribute] = "true";
                coverage.Arguments[0].Attributes[NcRefDateAttribute] = varInfo.ReferenceDate;
                coverage.Arguments[0].IsEditable = false;

                List<NetCdfDimension> variableDims = netCdfFile.GetDimensions(varInfo.NetCdfDataVariable).ToList();
                coverage.Arguments[1].Name = "N";
                coverage.Arguments[1].Attributes[NcNameAttribute] = netCdfFile.GetDimensionName(variableDims[1]);
                coverage.Arguments[1].Attributes[NcUseVariableSizeAttribute] = "false";
                coverage.Arguments[1].IsEditable = false;

                coverage.Arguments[2].Name = "M";
                coverage.Arguments[2].Attributes[NcNameAttribute] = netCdfFile.GetDimensionName(variableDims[2]);
                coverage.Arguments[2].Attributes[NcUseVariableSizeAttribute] = "false";
                coverage.Arguments[2].IsEditable = false;

                string variableName = netCdfFile.GetVariableName(varInfo.NetCdfDataVariable);
                coverage.Components[0].Name = variableName;
                coverage.Components[0].Attributes[NcNameAttribute] = variableName;
                coverage.Components[0].Attributes[NcUseVariableSizeAttribute] = "true";
                coverage.Components[0].IsEditable = false;

                coverage.Name = netCdfFile.GetVariableName(varInfo.NetCdfDataVariable);
                coverage.IsEditable = false;

                yield return coverage;
            }
        }

        private CurvilinearGrid ReadGridFromFile()
        {
            CurvilinearGrid grid = CurvilinearGrid.CreateDefault();

            int sizeN = netCdfFile.GetDimensionLength(NSizeDimensionName);
            int sizeM = netCdfFile.GetDimensionLength(MSizeDimensionName);

            using (var nc = new NetCdfFileWrapper(netCdfFile.Path))
            {
                IList<double> x = nc.GetValues1D<double>(XCoordinateVariableName);
                IList<double> y = nc.GetValues1D<double>(YCoordinateVariableName);
                grid.Resize(sizeN, sizeM, x, y);
            }

            return grid;
        }
    }
}