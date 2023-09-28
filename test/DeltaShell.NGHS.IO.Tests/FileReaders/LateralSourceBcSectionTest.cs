using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;
using Is = NUnit.Framework.Is;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class LateralSourceBcSectionTest
    {
        private BcIniSection bcIniSection;
        private ILogHandler logHandlerSubstitute;
        private IBcSectionParser bcSectionParser;
        
        [SetUp]
        public void SetUp()
        {
            bcIniSection = new BcIniSection("boundary");
            logHandlerSubstitute = Substitute.For<ILogHandler>();
            bcSectionParser = new BcSectionParser(logHandlerSubstitute);
        }
        
        [Test]
        public void Constructor_SectionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralSourceBcSection(null, bcSectionParser);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("iniSection"));
        }

        [Test]
        public void Constructor_SectionParserNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralSourceBcSection(bcIniSection, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("sectionParser"));
        }

        [Test]
        public void Constructor_NotLateralSection_ThrowsArgumentException()
        {
            // Call
            void Call() => new LateralSourceBcSection(new BcIniSection("some_name"), bcSectionParser);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("iniSection"));
        }

        [TestCase("forcing")]
        [TestCase("LateralDischarge")]
        public void Constructor_DataTypeConstant_InitializesInstanceCorrectly(string iniSectionName)
        {
            // Setup
            var iniSection = new BcIniSection(iniSectionName);
            iniSection.Section.AddProperty(CreateProperty("name", "lateral_source_name"));
            iniSection.Section.AddProperty(CreateProperty("function", "constant"));
            iniSection.Section.AddProperty(CreateProperty("timeInterpolation", "linear"));
            iniSection.Table.Add(CreateQuantity("lateral_discharge", "m³/s", 1.23));

            // Call
            var section = new LateralSourceBcSection(iniSection, bcSectionParser);

            // Assert
            Assert.That(section.Name, Is.EqualTo("lateral_source_name"));
            Assert.That(section.DataType, Is.EqualTo(Model1DLateralDataType.FlowConstant));
            Assert.That(section.Discharge, Is.EqualTo(1.23));
            Assert.That(section.DischargeFunction, Is.Null);
        }

        [TestCase("forcing")]
        [TestCase("LateralDischarge")]
        public void Constructor_DataTypeTimeSeries_SecondsSinceReference_InitializesInstanceCorrectly(string iniSectionName)
        {
            // Setup
            var iniSection = new BcIniSection(iniSectionName);
            iniSection.Section.AddProperty(CreateProperty("name", "lateral_source_name"));
            iniSection.Section.AddProperty(CreateProperty("function", "timeseries"));
            iniSection.Section.AddProperty(CreateProperty("timeInterpolation", "linear"));
            iniSection.Table.Add(CreateQuantity("time", "seconds since 2021-01-01 00:00:00", 100, 200, 300));
            iniSection.Table.Add(CreateQuantity("lateral_discharge", "m³/s", 1.23, 4.56, 7.89));

            // Call
            var section = new LateralSourceBcSection(iniSection, bcSectionParser);

            // Assert
            Assert.That(section.Name, Is.EqualTo("lateral_source_name"));
            Assert.That(section.DataType, Is.EqualTo(Model1DLateralDataType.FlowTimeSeries));
            Assert.That(section.Discharge, Is.EqualTo(0));

            var function = section.DischargeFunction as TimeSeries;
            Assert.That(function, Is.Not.Null);
            Assert.That(function.Name, Is.EqualTo("flow time series"));
            Assert.That(function.Arguments, Has.Count.EqualTo(1));
            Assert.That(function.Arguments[0].Name, Is.EqualTo("Time"));
            Assert.That(function.Components, Has.Count.EqualTo(1));
            Assert.That(function.Components[0].Name, Is.EqualTo("flow"));
            Assert.That(function.Components[0].Unit.Symbol, Is.EqualTo("m3/s"));

            IMultiDimensionalArray<DateTime> times = function.Time.Values;
            IMultiDimensionalArray values = function.Components[0].Values;
            Assert.That(times[0], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(100)));
            Assert.That(values[0], Is.EqualTo(1.23));
            Assert.That(times[1], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(200)));
            Assert.That(values[1], Is.EqualTo(4.56));
            Assert.That(times[2], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(300)));
            Assert.That(values[2], Is.EqualTo(7.89));
        }

        [TestCase("forcing")]
        [TestCase("LateralDischarge")]
        public void Constructor_DataTypeTimeSeries_MinutesSinceReference_InitializesInstanceCorrectly(string iniSectionName)
        {
            // Setup
            var iniSection = new BcIniSection(iniSectionName);
            iniSection.Section.AddProperty(CreateProperty("name", "lateral_source_name"));
            iniSection.Section.AddProperty(CreateProperty("function", "timeseries"));
            iniSection.Section.AddProperty(CreateProperty("timeInterpolation", "linear"));
            iniSection.Table.Add(CreateQuantity("time", "minutes since 2021-01-01 00:00:00", 100, 200, 300));
            iniSection.Table.Add(CreateQuantity("lateral_discharge", "m³/s", 1.23, 4.56, 7.89));

            // Call
            var section = new LateralSourceBcSection(iniSection, bcSectionParser);

            // Assert
            Assert.That(section.Name, Is.EqualTo("lateral_source_name"));
            Assert.That(section.DataType, Is.EqualTo(Model1DLateralDataType.FlowTimeSeries));
            Assert.That(section.Discharge, Is.EqualTo(0));

            var function = section.DischargeFunction as TimeSeries;
            Assert.That(function, Is.Not.Null);
            Assert.That(function.Name, Is.EqualTo("flow time series"));
            Assert.That(function.Arguments, Has.Count.EqualTo(1));
            Assert.That(function.Arguments[0].Name, Is.EqualTo("Time"));
            Assert.That(function.Components, Has.Count.EqualTo(1));
            Assert.That(function.Components[0].Name, Is.EqualTo("flow"));
            Assert.That(function.Components[0].Unit.Symbol, Is.EqualTo("m3/s"));

            IMultiDimensionalArray<DateTime> times = function.Time.Values;
            IMultiDimensionalArray values = function.Components[0].Values;
            Assert.That(times[0], Is.EqualTo(new DateTime(2021, 1, 1).AddMinutes(100)));
            Assert.That(values[0], Is.EqualTo(1.23));
            Assert.That(times[1], Is.EqualTo(new DateTime(2021, 1, 1).AddMinutes(200)));
            Assert.That(values[1], Is.EqualTo(4.56));
            Assert.That(times[2], Is.EqualTo(new DateTime(2021, 1, 1).AddMinutes(300)));
            Assert.That(values[2], Is.EqualTo(7.89));
        }

        [TestCase("forcing")]
        [TestCase("LateralDischarge")]
        public void Constructor_DataTypeTimeSeries_HoursSinceReference_InitializesInstanceCorrectly(string iniSectionName)
        {
            // Setup
            var iniSection = new BcIniSection(iniSectionName);
            iniSection.Section.AddProperty(CreateProperty("name", "lateral_source_name"));
            iniSection.Section.AddProperty(CreateProperty("function", "timeseries"));
            iniSection.Section.AddProperty(CreateProperty("timeInterpolation", "linear"));
            iniSection.Table.Add(CreateQuantity("time", "hours since 2021-01-01 00:00:00", 100, 200, 300));
            iniSection.Table.Add(CreateQuantity("lateral_discharge", "m³/s", 1.23, 4.56, 7.89));

            // Call
            var section = new LateralSourceBcSection(iniSection, bcSectionParser);

            // Assert
            Assert.That(section.Name, Is.EqualTo("lateral_source_name"));
            Assert.That(section.DataType, Is.EqualTo(Model1DLateralDataType.FlowTimeSeries));
            Assert.That(section.Discharge, Is.EqualTo(0));

            var function = section.DischargeFunction as TimeSeries;
            Assert.That(function, Is.Not.Null);
            Assert.That(function.Name, Is.EqualTo("flow time series"));
            Assert.That(function.Arguments, Has.Count.EqualTo(1));
            Assert.That(function.Arguments[0].Name, Is.EqualTo("Time"));
            Assert.That(function.Components, Has.Count.EqualTo(1));
            Assert.That(function.Components[0].Name, Is.EqualTo("flow"));
            Assert.That(function.Components[0].Unit.Symbol, Is.EqualTo("m3/s"));

            IMultiDimensionalArray<DateTime> times = function.Time.Values;
            IMultiDimensionalArray values = function.Components[0].Values;
            Assert.That(times[0], Is.EqualTo(new DateTime(2021, 1, 1).AddHours(100)));
            Assert.That(values[0], Is.EqualTo(1.23));
            Assert.That(times[1], Is.EqualTo(new DateTime(2021, 1, 1).AddHours(200)));
            Assert.That(values[1], Is.EqualTo(4.56));
            Assert.That(times[2], Is.EqualTo(new DateTime(2021, 1, 1).AddHours(300)));
            Assert.That(values[2], Is.EqualTo(7.89));
        }

        [TestCase("forcing")]
        [TestCase("LateralDischarge")]
        public void Constructor_DataTypeQhTable_InitializesInstanceCorrectly(string iniSectionName)
        {
            // Setup
            var iniSection = new BcIniSection(iniSectionName);
            iniSection.Section.AddProperty(CreateProperty("name", "lateral_source_name"));
            iniSection.Section.AddProperty(CreateProperty("function", "qhtable"));
            iniSection.Section.AddProperty(CreateProperty("timeInterpolation", "linear"));
            iniSection.Table.Add(CreateQuantity("qhbnd waterlevel", "m", 10.1, 20.2, 30.3));
            iniSection.Table.Add(CreateQuantity("lateral_discharge", "m³/s", 1.23, 4.56, 7.89));

            // Call
            var section = new LateralSourceBcSection(iniSection, bcSectionParser);

            // Assert
            Assert.That(section.Name, Is.EqualTo("lateral_source_name"));
            Assert.That(section.DataType, Is.EqualTo(Model1DLateralDataType.FlowWaterLevelTable));
            Assert.That(section.Discharge, Is.EqualTo(0));

            IFunction function = section.DischargeFunction;
            Assert.That(function, Is.Not.Null);
            Assert.That(function.Name, Is.EqualTo("Discharge Water Level Series"));
            Assert.That(function.Arguments, Has.Count.EqualTo(1));
            Assert.That(function.Arguments[0].Name, Is.EqualTo("Water Level"));
            Assert.That(function.Arguments[0].Unit.Symbol, Is.EqualTo("m"));
            Assert.That(function.Components, Has.Count.EqualTo(1));
            Assert.That(function.Components[0].Name, Is.EqualTo("Discharge"));
            Assert.That(function.Components[0].Unit.Symbol, Is.EqualTo("m³/s"));

            IMultiDimensionalArray arguments = function.Arguments[0].Values;
            IMultiDimensionalArray values = function.Components[0].Values;
            Assert.That(arguments[0], Is.EqualTo(10.1));
            Assert.That(values[0], Is.EqualTo(1.23));
            Assert.That(arguments[1], Is.EqualTo(20.2));
            Assert.That(values[1], Is.EqualTo(4.56));
            Assert.That(arguments[2], Is.EqualTo(30.3));
            Assert.That(values[2], Is.EqualTo(7.89));
        }

        [TestCase("centuries since 2021-01-01 00:00:00", "Cannot interpret 'centuries since 2021-01-01 00:00:00', see section on line 7.")]
        [TestCase("minutes since yesterday", "Cannot parse 'yesterday' to a date time, see section on line 7.")]
        public void Constructor_DataTypeTimeSeries_CannotInterpretUnit_ReportsError(string timeUnit, string expError)
        {
            // Setup
            var iniSection = new BcIniSection("forcing");
            iniSection.Section.LineNumber = 7;
            iniSection.Section.AddProperty(CreateProperty("name", "lateral_source_name"));
            iniSection.Section.AddProperty(CreateProperty("function", "timeseries"));
            iniSection.Section.AddProperty(CreateProperty("timeInterpolation", "linear"));
            iniSection.Table.Add(CreateQuantity("time", timeUnit, 100, 200, 300));
            iniSection.Table.Add(CreateQuantity("lateral_discharge", "m³/s", 1.23, 4.56, 7.89));

            var logHandler = Substitute.For<ILogHandler>();

            // Call
            var section = new LateralSourceBcSection(iniSection, new BcSectionParser(logHandler));

            // Assert
            logHandler.Received(1).ReportError(expError);

            Assert.That(section.Name, Is.EqualTo("lateral_source_name"));
            Assert.That(section.DataType, Is.EqualTo(Model1DLateralDataType.FlowTimeSeries));
            Assert.That(section.Discharge, Is.EqualTo(0));
            Assert.That(section.DischargeFunction, Is.Null);
        }

        private static BcQuantityData CreateQuantity(string quantity, string unit, params double[] values)
        {
            IniProperty quantityProperty = CreateProperty("quantity", quantity);
            IniProperty unitProperty = CreateProperty("unit", unit);

            return new BcQuantityData(quantityProperty, unitProperty, values);
        }

        private static IniProperty CreateProperty(string name, string value)
        {
            return new IniProperty
            (
                name,
                value,
                string.Empty
            );
        }
    }
}