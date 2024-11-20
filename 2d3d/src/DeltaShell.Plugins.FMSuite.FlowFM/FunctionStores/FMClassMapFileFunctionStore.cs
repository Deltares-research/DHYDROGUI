using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Function store for Class Map Files.
    /// </summary>
    /// <seealso cref="FMNetCdfFileFunctionStore"/>
    public class FMClassMapFileFunctionStore : FMNetCdfFileFunctionStore, IFMClassMapFileFunctionStore
    {
        private const string LocationAttributeName = "location";
        private const string StandardNameAttributeName = "standard_name";
        private const string LongNameAttributeName = "long_name";
        private const string UnitsAttributeName = "units";
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMMapFileFunctionStore));

        private UnstructuredGrid grid;

        /// <summary>
        /// Initializes a new instance of the <see cref="FMClassMapFileFunctionStore"/> class.
        /// </summary>
        /// <param name="classMapFilePath"> The class map file path. </param>
        public FMClassMapFileFunctionStore(string classMapFilePath) : base(classMapFilePath) {}

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
        /// <param name="dataVariables"> The data variables. </param>
        /// <returns> </returns>
        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            grid = UnstructuredGridFileHelper.LoadFromFile(netCdfFile.Path, true);
            IEnumerable<NetCdfVariableInfo> timeDepVariables =
                dataVariables.Where(v => v.IsTimeDependent && v.NumDimensions > 1);
            IEnumerable<UnstructuredGridCoverage> functions =
                timeDepVariables.Select(CreateCoverageForTimeDependentVariable).Where(c => c != null);

            return functions;
        }

        /// <summary>
        /// Gets the variable values.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="function"> The function. </param>
        /// <param name="filters"> The variable filters. </param>
        /// <returns> </returns>
        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(
            IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new [] { size });
            }

            return base.GetVariableValuesCore<T>(function, filters);
        }

        private UnstructuredGridCoverage CreateCoverageForTimeDependentVariable(
            NetCdfVariableInfo timeDependentVariable)
        {
            NetCdfVariable netCdfVariable = timeDependentVariable.NetCdfDataVariable;
            string netCdfVariableName = netCdfFile.GetVariableName(netCdfVariable);
            NetCdfDataType netCdfVariableType = netCdfFile.GetVariableDataType(netCdfVariable);

            if (netCdfVariableType != NetCdfDataType.NcByte)
            {
                Log.Warn(
                    $"Time dependent functions in the class map file are expected to be of type Byte. Please check the value type for variable '{netCdfVariableName}'.");
                return null;
            }

            NetCdfDimension secondDimension = netCdfFile.GetDimensions(netCdfVariable).ElementAt(1);
            string secondDimensionName = netCdfFile.GetDimensionName(secondDimension);
            string location = netCdfFile.GetAttributeValue(netCdfVariable, LocationAttributeName);
            string longName = netCdfFile.GetAttributeValue(netCdfVariable, LongNameAttributeName) ??
                              netCdfFile.GetAttributeValue(netCdfVariable, StandardNameAttributeName);
            string unit = netCdfFile.GetAttributeValue(netCdfVariable, UnitsAttributeName);
            string coverageLongName = longName != null ? $"{longName} ({netCdfVariableName})" : netCdfVariableName;

            Type dotNetType = NetCdfConstants.GetClrDataType(netCdfVariableType);
            UnstructuredGridCoverage coverage = CreateCoverage(location, coverageLongName, dotNetType);
            if (coverage != null)
            {
                InitializeCoverage(coverage, secondDimensionName, netCdfVariableName, unit,
                                   timeDependentVariable.ReferenceDate);
            }

            return coverage;
        }

        private UnstructuredGridCoverage CreateCoverage(string location, string coverageLongName, Type outputType)
        {
            if (location != GridApiDataSet.UGridAttributeConstants.LocationValues.Face)
            {
                Log.WarnFormat(
                    $"Cannot create coverage: can only create coverages for cell faces. See '{coverageLongName}' in the class map file: {Path}.");
                return null;
            }

            var coverage = new UnstructuredGridCellCoverage(grid, true) {Name = coverageLongName};

            coverage.Components.RemoveAt(0);
            coverage.Components.Add((IVariable) TypeUtils.CreateGeneric(typeof(Variable<>), outputType));
            coverage.Components[0].Name = "value";

            return coverage;
        }

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName,
                                        string unitSymbol, string refDate)
        {
            coverage.Store = this;

            IVariable timeDimension = coverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            IVariable secondDimension = coverage.Arguments[1];
            secondDimension.Name = secondDimensionName;
            secondDimension.Attributes[NcNameAttribute] = secondDimensionName;
            secondDimension.Attributes[NcUseVariableSizeAttribute] = "false";
            secondDimension.IsEditable = false;

            IVariable coverageComponent = coverage.Components[0];
            coverageComponent.Name = variableName;
            coverageComponent.Attributes[NcNameAttribute] = variableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";

            if (coverageComponent.ValueType == typeof(double))
            {
                coverageComponent.NoDataValue = MissingValue;
            }

            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);
            coverage.IsEditable = false;
        }
    }
}