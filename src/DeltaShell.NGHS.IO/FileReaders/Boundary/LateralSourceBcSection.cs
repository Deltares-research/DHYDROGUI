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
    /// Represents a boundary conditions section specific for lateral source discharge data from the boundary conditions file.
    /// </summary>
    public class LateralSourceBcSection : ILateralSourceBcSection
    {
        private readonly int lineNumber;
        private readonly IBcSectionParser sectionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateralSourceBcSection"/> class.
        /// </summary>
        /// <param name="iniSection"> The bc section to parse from. </param>
        /// <param name="sectionParser">Helper to parse bc section data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iniSection"/> or <paramref name="sectionParser"/>  is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="iniSection"/> is not a lateral section.
        /// This means that the section should have name 'forcing' or 'LateralDischarge'.
        /// </exception>
        /// <remarks>
        /// The section is expected to have the following properties:
        /// - 'name'
        /// - 'function'
        /// If a property is missing, an error will be logged.
        /// Moreover, depending on the discharge data type, the quantities are expected to be in the correct order:
        /// First the argument, then the component.
        /// </remarks>
        public LateralSourceBcSection(BcIniSection iniSection, IBcSectionParser sectionParser)
        {
            Ensure.NotNull(sectionParser, nameof(sectionParser));
            Ensure.NotNull(iniSection, nameof(iniSection));
            EnsureLateralSection(iniSection, nameof(iniSection));
            
            this.sectionParser = sectionParser;
            lineNumber = iniSection.Section.LineNumber;

            var name = iniSection.Section.ReadProperty<string>(BoundaryRegion.Name.Key);
            var function = iniSection.Section.ReadProperty<string>(BoundaryRegion.Function.Key);
            var periodic = iniSection.Section.ReadProperty<string>(BoundaryRegion.Periodic.Key, true);

            Name = name;
            DataType = ToDataType(function);
            SetDischarge(iniSection.Table, periodic);
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

        private static void EnsureLateralSection(BcIniSection iniSection, string paramName)
        {
            if (!iniSection.Section.Name.EqualsCaseInsensitive(BoundaryRegion.BcLateralHeader) &&
                !iniSection.Section.Name.EqualsCaseInsensitive(BoundaryRegion.BcForcingHeader))
            {
                throw new ArgumentException($"{nameof(iniSection)} should have header {BoundaryRegion.BcLateralHeader} or " +
                                            $"{BoundaryRegion.BcForcingHeader} for laterals.", paramName);
            }
        }

        private void SetDischarge(IList<IBcQuantityData> table, string periodic)
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
                    Discharge = sectionParser.CreateConstant(table, lineNumber);
                    return;
            }
        }

        private IFunction CreateTimeSeries(IList<IBcQuantityData> table, string periodic)
        {
            if (!sectionParser.TryParseDateTimes(table[0].Values, table[0].Unit.Value, lineNumber, out IEnumerable<DateTime> argumentValues))
            {
                return null;
            }

            if (!sectionParser.TryParseDoubles(table[1].Values, lineNumber, out IEnumerable<double> functionValues))
            {
                return null;
            }

            TimeSeries function = HydroTimeSeriesFactory.CreateFlowTimeSeries();

            sectionParser.CompleteFunction(function, argumentValues, functionValues, periodic);

            return function;
        }

        private IFunction CreateFlowWaterLevelTable(IList<IBcQuantityData> table, string periodic)
        {
            if (!sectionParser.TryParseDoubles(table[0].Values, lineNumber, out IEnumerable<double> argumentValues))
            {
                return null;
            }

            if (!sectionParser.TryParseDoubles(table[1].Values, lineNumber, out IEnumerable<double> functionValues))
            {
                return null;
            }

            var function = new FlowWaterLevelTable();
            sectionParser.CompleteFunction(function, argumentValues, functionValues, periodic);
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