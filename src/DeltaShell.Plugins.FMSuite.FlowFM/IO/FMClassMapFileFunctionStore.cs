using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{

    /// <summary>
    /// Function store for Class Map Files.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.Common.IO.FMNetCdfFileFunctionStore" />
    public class FMClassMapFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMMapFileFunctionStore));

        private const string LocationAttributeName = "location";
        private const string StandardNameAttributeName = "standard_name";
        private const string LongNameAttributeName = "long_name";
        private const string UnitsAttributeName = "units";

        private UnstructuredGrid grid;

        /// <summary>
        /// Initializes a new instance of the <see cref="FMClassMapFileFunctionStore"/> class.
        /// </summary>
        /// <param name="classMapFilePath">The class map file path.</param>
        public FMClassMapFileFunctionStore(string classMapFilePath) : base (classMapFilePath)
        {
        }

        /// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>
        /// The grid.
        /// </value>
        public UnstructuredGrid Grid => grid;

        /// <summary>
        /// Constructs the functions for netCdf variables that are time dependent.
        /// </summary>
        /// <param name="dataVariables">The data variables.</param>
        /// <returns></returns>
        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            grid = UnstructuredGridFileHelper.LoadFromFile(netCdfFile.Path, true);
            var timeDepVariables = dataVariables.Where(v => v.IsTimeDependent && v.NumDimensions > 1);
            var functions = timeDepVariables.SelectMany(ProcessTimeDependentVariable).Where(c => c != null).ToList();

            return functions;
        }

        private IEnumerable<UnstructuredGridCoverage> ProcessTimeDependentVariable(NetCdfVariableInfo timeDependentVariable)
        {
            var netcdfVariable = timeDependentVariable.NetCdfDataVariable;
            var netCdfVariableName = netCdfFile.GetVariableName(netcdfVariable);
            var netCdfVariableType = netCdfFile.GetVariableDataType(netcdfVariable);

            if (netCdfVariableType != NetCdfDataType.NcByte)
            {
                Log.Info($"Time dependent functions in the class map file are expected to be of type Byte. Please check the value type for variable '{netCdfVariableName}'.");
            }

            var secondDimension = netCdfFile.GetDimensions(netcdfVariable).ElementAt(1);
            var secondDimensionName = netCdfFile.GetDimensionName(secondDimension);
            var location = netCdfFile.GetAttributeValue(netcdfVariable, LocationAttributeName);
            var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttributeName) ??
                           netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttributeName);
            var unit = netCdfFile.GetAttributeValue(netcdfVariable, UnitsAttributeName);
            var coverageLongName = longName != null? $"{longName} ({netCdfVariableName})" : netCdfVariableName;

            var dotNetType = NetCdfConstants.GetClrDataType(netCdfVariableType);
            var coverage = CreateCoverage(location, coverageLongName, dotNetType);
            if (coverage != null)
            {
                InitializeCoverage(coverage, secondDimensionName, netCdfVariableName, unit, timeDependentVariable.ReferenceDate);
            }

            yield return coverage;
        }

        /// <summary>
        /// Gets the variable values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">The function.</param>
        /// <param name="filters">The variable filters.</param>
        /// <returns></returns>
        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), size);
            }

            return base.GetVariableValuesCore<T>(function, filters);
        }

        private UnstructuredGridCoverage CreateCoverage(string location, string coverageLongName, Type outputType, int number = -1)
        {
            UnstructuredGridCoverage coverage;

            switch (location)
            {
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Face:
                    coverage = new UnstructuredGridCellCoverage(grid, true) { Name = coverageLongName };
                    break;
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Edge:
                    coverage = new UnstructuredGridEdgeCoverage(grid, true) { Name = coverageLongName };
                    break;
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Node:
                    coverage = new UnstructuredGridVertexCoverage(grid, true) { Name = coverageLongName };
                    break;
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Volume:
                    Log.WarnFormat($"Cannot create coverage on a volume location. See '{coverageLongName}' in the class map file: {Path}.");
                    return null;
                default:
                    Log.WarnFormat($"Cannot create coverage: did not recognize location type. See '{coverageLongName}' in the class map file: {Path}.");
                    return null;
            }

            coverage.Components.RemoveAt(0);
            coverage.Components.Add((IVariable) TypeUtils.CreateGeneric(typeof(Variable<>), outputType));
            coverage.Components[0].Name = "value";

            return coverage;
        }

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName, string unitSymbol, string refDate)
        {
            coverage.Store = this;

            var timeDimension = coverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            var secondDimension = coverage.Arguments[1];
            secondDimension.Name = secondDimensionName;
            secondDimension.Attributes[NcNameAttribute] = secondDimensionName;
            secondDimension.Attributes[NcUseVariableSizeAttribute] = "false";
            secondDimension.IsEditable = false;

            var coverageComponent = coverage.Components[0];
            coverageComponent.Name = variableName;
            coverageComponent.Attributes[NcNameAttribute] = variableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";

            if (coverageComponent.ValueType == typeof(Double)) coverageComponent.NoDataValue = MissingValue;
            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);
            coverage.IsEditable = false;
        }
    }
}
