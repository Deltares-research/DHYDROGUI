using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="WavmFileFunctionStore"/> extends the <see cref="FMNetCdfFileFunctionStore"/>
    /// in order to support wave map files.
    /// </summary>
    /// <seealso cref="FMNetCdfFileFunctionStore"/>
    public class WavmFileFunctionStore : FMNetCdfFileFunctionStore, IWavmFileFunctionStore
    {
        private const string nSizeDimensionName = "nmax";
        private const string mSizeDimensionName = "mmax";
        private const string xCoordinateVariableName = "x";
        private const string yCoordinateVariableName = "y";

        private const string timeArgumentName = "Time";
        private const string columnArgumentName = "N";
        private const string rowArgumentName = "M";

        private const string fillValueAttributeName = "_FillValue";

        /// <summary>
        /// Creates a new <see cref="WavmFileFunctionStore"/>.
        /// </summary>
        /// <param name="ncPath">The nc path.</param>
        public WavmFileFunctionStore(string ncPath) : base(ncPath)
        {
            DisableCaching = true;
        }

        /// <summary>
        /// Gets the grid of this <see cref="WavmFileFunctionStore"/>.
        /// </summary>
        public CurvilinearGrid Grid { get; private set; }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            Grid = ReadGridFromFile();
            return HasValidNetCdfDimensions() ? ConstructCoveragesFromVariables(dataVariables) : Enumerable.Empty<IFunction>();
        }

        private bool HasValidNetCdfDimensions()
        {
            var dimensionNames =
                new HashSet<string>(netCdfFile.GetAllDimensions()
                                              .Select(d => netCdfFile.GetDimensionName(d)));

            return dimensionNames.Contains(TimeDimensionNames[0]) &&
                   dimensionNames.Contains(nSizeDimensionName) &&
                   dimensionNames.Contains(mSizeDimensionName);
        }

        private IEnumerable<CurvilinearCoverage> ConstructCoveragesFromVariables(IEnumerable<NetCdfVariableInfo> variables) =>
            variables.Where(IsValidVariable)
                     .Select(ConstructCoverage);

        private static bool IsValidVariable(NetCdfVariableInfo variableInfo) =>
            variableInfo.IsTimeDependent && variableInfo.NumDimensions == 3;

        private CurvilinearCoverage ConstructCoverage(NetCdfVariableInfo variableInfo)
        {
            var coverage = new CurvilinearCoverage(Grid)
            {
                IsTimeDependent = true,
                Store = this,
                Name = netCdfFile.GetVariableName(variableInfo.NetCdfDataVariable),
                IsEditable = false
            };

            coverage.Arguments[0].Name = timeArgumentName;
            coverage.Arguments[0].Attributes[NcNameAttribute] = TimeVariableNames[0];
            coverage.Arguments[0].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Arguments[0].Attributes[NcRefDateAttribute] = variableInfo.ReferenceDate;
            coverage.Arguments[0].IsEditable = false;

            List<NetCdfDimension> variableDims = netCdfFile.GetDimensions(variableInfo.NetCdfDataVariable).ToList();
            coverage.Arguments[1].Name = columnArgumentName;
            coverage.Arguments[1].Attributes[NcNameAttribute] = netCdfFile.GetDimensionName(variableDims[1]);
            coverage.Arguments[1].Attributes[NcUseVariableSizeAttribute] = "false";
            coverage.Arguments[1].IsEditable = false;

            coverage.Arguments[2].Name = rowArgumentName;
            coverage.Arguments[2].Attributes[NcNameAttribute] = netCdfFile.GetDimensionName(variableDims[2]);
            coverage.Arguments[2].Attributes[NcUseVariableSizeAttribute] = "false";
            coverage.Arguments[2].IsEditable = false;

            string variableName = netCdfFile.GetVariableName(variableInfo.NetCdfDataVariable);
            coverage.Components[0].Name = variableName;
            coverage.Components[0].Attributes[NcNameAttribute] = variableName;
            coverage.Components[0].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Components[0].IsEditable = false;

            return coverage;
        }

        private CurvilinearGrid ReadGridFromFile()
        {
            var grid = CurvilinearGrid.CreateDefault();

            int sizeN = netCdfFile.GetDimensionLength(nSizeDimensionName);
            int sizeM = netCdfFile.GetDimensionLength(mSizeDimensionName);

            IEnumerable<double> xCoordinates = GetCoordinates(xCoordinateVariableName);
            IEnumerable<double> yCoordinates = GetCoordinates(yCoordinateVariableName);

            grid.Resize(sizeN, sizeM, xCoordinates, yCoordinates);

            return grid;
        }

        private IEnumerable<double> GetCoordinates(string coordinateVariableName)
        {
            NetCdfVariable coordinateVariable = netCdfFile.GetVariableByName(coordinateVariableName);
            NetCdfAttribute fillValueAttribute = netCdfFile.GetAttribute(coordinateVariable, fillValueAttributeName);
        
            double fillValue = fillValueAttribute != null ? (double)fillValueAttribute.Value : double.NaN;

            using (var nc = new NetCdfFileWrapper(netCdfFile.Path))
            {
                return nc.GetValues1D<double>(coordinateVariableName)
                         .Select(val => val.Equals(fillValue) ? double.NaN : val)
                         .ToArray();
            }
        }
    }
}