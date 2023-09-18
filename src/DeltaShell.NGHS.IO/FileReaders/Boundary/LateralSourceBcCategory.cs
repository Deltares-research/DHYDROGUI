using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.NGHS.IO.FileReaders.Boundary
{
    /// <summary>
    /// Represents a boundary conditions category specific for lateral source discharge data from the boundary conditions file.
    /// </summary>
    public class LateralSourceBcCategory : ILateralSourceBcCategory
    {
        private readonly int lineNumber;
        private readonly IBcCategoryParser categoryParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateralSourceBcCategory"/> class.
        /// </summary>
        /// <param name="category"> The bc category to parse from. </param>
        /// <param name="categoryParser">Helper to parse bc category data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> or <paramref name="categoryParser"/>  is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="category"/> is not a lateral category.
        /// This means that the category should have name 'forcing' or 'LateralDischarge'.
        /// </exception>
        /// <remarks>
        /// The category is expected to have the following properties:
        /// - 'name'
        /// - 'function'
        /// If a property is missing, an error will be logged.
        /// Moreover, depending on the discharge data type, the quantities are expected to be in the correct order:
        /// First the argument, then the component.
        /// </remarks>
        public LateralSourceBcCategory(DelftBcCategory category, IBcCategoryParser categoryParser)
        {
            Ensure.NotNull(categoryParser, nameof(categoryParser));
            Ensure.NotNull(category, nameof(category));
            EnsureLateralCategory(category, nameof(category));
            
            this.categoryParser = categoryParser;
            lineNumber = category.Section.LineNumber;

            var name = category.Section.ReadProperty<string>(BoundaryRegion.Name.Key);
            var function = category.Section.ReadProperty<string>(BoundaryRegion.Function.Key);
            var periodic = category.Section.ReadProperty<string>(BoundaryRegion.Periodic.Key, true);

            Name = name;
            DataType = ToDataType(function);
            SetDischarge(category.Table, periodic);
        }

        /// <summary>
        /// The name of the lateral source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The discharge data type of the lateral source.
        /// </summary>
        public Model1DLateralDataType DataType { get; }

        /// <summary>
        /// The constant discharge value of the lateral source.
        /// </summary>
        public double Discharge { get; private set; }

        /// <summary>
        /// The variable discharge function of the lateral source.
        /// </summary>
        public IFunction DischargeFunction { get; private set; }

        private static void EnsureLateralCategory(DelftBcCategory category, string paramName)
        {
            if (!category.Section.Name.EqualsCaseInsensitive(BoundaryRegion.BcLateralHeader) &&
                !category.Section.Name.EqualsCaseInsensitive(BoundaryRegion.BcForcingHeader))
            {
                throw new ArgumentException($"{nameof(category)} should have header {BoundaryRegion.BcLateralHeader} or " +
                                            $"{BoundaryRegion.BcForcingHeader} for laterals.", paramName);
            }
        }

        private void SetDischarge(IList<IDelftBcQuantityData> table, string periodic)
        {
            switch (DataType)
            {
                case Model1DLateralDataType.FlowWaterLevelTable:
                    DischargeFunction = CreateFlowWaterLevelTable(table, periodic);
                    return;
                case Model1DLateralDataType.FlowTimeSeries:
                    DischargeFunction = CreateTimeSeries(table, periodic);
                    return;
                case Model1DLateralDataType.FlowConstant:
                    Discharge = categoryParser.CreateConstant(table, lineNumber);
                    return;
            }
        }

        private IFunction CreateTimeSeries(IList<IDelftBcQuantityData> table, string periodic)
        {
            if (!categoryParser.TryParseDateTimes(table[0].Values, table[0].Unit.Value, lineNumber, out IEnumerable<DateTime> argumentValues))
            {
                return null;
            }

            if (!categoryParser.TryParseDoubles(table[1].Values, lineNumber, out IEnumerable<double> functionValues))
            {
                return null;
            }

            TimeSeries function = HydroTimeSeriesFactory.CreateFlowTimeSeries();

            categoryParser.CompleteFunction(function, argumentValues, functionValues, periodic);

            return function;
        }

        private IFunction CreateFlowWaterLevelTable(IList<IDelftBcQuantityData> table, string periodic)
        {
            if (!categoryParser.TryParseDoubles(table[0].Values, lineNumber, out IEnumerable<double> argumentValues))
            {
                return null;
            }

            if (!categoryParser.TryParseDoubles(table[1].Values, lineNumber, out IEnumerable<double> functionValues))
            {
                return null;
            }

            var function = new FlowWaterLevelTable();
            categoryParser.CompleteFunction(function, argumentValues, functionValues, periodic);
            return function;
        }

        private static Model1DLateralDataType ToDataType(string function)
        {
            if (function.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.Constant))
            {
                return Model1DLateralDataType.FlowConstant;
            }

            if (function.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.QhTable))
            {
                return Model1DLateralDataType.FlowWaterLevelTable;
            }

            if (function.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.TimeSeries))
            {
                return Model1DLateralDataType.FlowTimeSeries;
            }

            return Model1DLateralDataType.FlowRealTime;
        }
    }
}