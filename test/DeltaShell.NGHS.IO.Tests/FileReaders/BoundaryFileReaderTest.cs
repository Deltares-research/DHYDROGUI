using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders.Boundary;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class BoundaryFileReaderTest
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void ReadLateralSourcesFromBcFile_FilePathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Setup
            var reader = new BoundaryFileReader();

            // Call
            void Call() => reader.ReadLateralSourcesFromBcFile(filePath).ToArray();

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        public void ReadLateralSourcesFromBcFile_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Setup
            var reader = new BoundaryFileReader();

            // Call
            void Call() => reader.ReadLateralSourcesFromBcFile("this/file/does/not.exist").ToArray();

            // Assert
            var e = Assert.Throws<FileNotFoundException>(Call);
            Assert.That(e.FileName, Is.EqualTo("this/file/does/not.exist"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLateralSourcesFromBcFile_ReadsDataCorrectly()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string fileContent =
                    "[General]                                                    " + Environment.NewLine +
                    "    fileVersion           = 1.01                             " + Environment.NewLine +
                    "    fileType              = boundConds                       " + Environment.NewLine +
                    "                                                             " + Environment.NewLine +
                    "[forcing]                                                    " + Environment.NewLine +
                    "    name                  = some_lateral_source_1            " + Environment.NewLine +
                    "    function              = timeseries                       " + Environment.NewLine +
                    "    timeInterpolation     = linear                           " + Environment.NewLine +
                    "    quantity              = time                             " + Environment.NewLine +
                    "    unit                  = seconds since 2021-01-01 00:00:00" + Environment.NewLine +
                    "    quantity              = lateral_discharge                " + Environment.NewLine +
                    "    unit                  = m³/s                             " + Environment.NewLine +
                    "    60  1.23                                                 " + Environment.NewLine +
                    "    120 4.56                                                 " + Environment.NewLine +
                    "    180 7.89                                                 " + Environment.NewLine +
                    "                                                             " + Environment.NewLine +
                    "[forcing]                                                    " + Environment.NewLine +
                    "    name                  = some_lateral_source_2            " + Environment.NewLine +
                    "    function              = qhtable                          " + Environment.NewLine +
                    "    timeInterpolation     = linear                           " + Environment.NewLine +
                    "    quantity              = qhbnd waterlevel                 " + Environment.NewLine +
                    "    unit                  = m                                " + Environment.NewLine +
                    "    quantity              = lateral_discharge                " + Environment.NewLine +
                    "    unit                  = m³/s                             " + Environment.NewLine +
                    "    10.1 1.23                                                " + Environment.NewLine +
                    "    20.2 3.45                                                " + Environment.NewLine +
                    "    30.3 5.67                                                " + Environment.NewLine +
                    "                                                             " + Environment.NewLine +
                    "[forcing]                                                    " + Environment.NewLine +
                    "    name                  = some_lateral_source_3            " + Environment.NewLine +
                    "    function              = constant                         " + Environment.NewLine +
                    "    timeInterpolation     = linear                           " + Environment.NewLine +
                    "    quantity              = lateral_discharge                " + Environment.NewLine +
                    "    unit                  = m³/s                             " + Environment.NewLine +
                    "    99.9                                                     ";

                string file = temp.CreateFile("Flow_model_lateral_sources.bc", fileContent);
                var logHandler = Substitute.For<ILogHandler>();

                var reader = new BoundaryFileReader();

                // Call
                ILateralSourceBcCategory[] categories = reader.ReadLateralSourcesFromBcFile(file, logHandler).ToArray();

                // Assert
                Assert.That(categories.Length, Is.EqualTo(3));

                // - Lateral source 1
                Assert.That(categories[0].Name, Is.EqualTo("some_lateral_source_1"));
                Assert.That(categories[0].DataType, Is.EqualTo(Model1DLateralDataType.FlowTimeSeries));
                Assert.That(categories[0].Discharge, Is.EqualTo(0));

                var timeSeries = categories[0].DischargeFunction as TimeSeries;
                Assert.That(timeSeries, Is.Not.Null);
                Assert.That(timeSeries.Name, Is.EqualTo("flow time series"));
                Assert.That(timeSeries.Arguments, Has.Count.EqualTo(1));
                Assert.That(timeSeries.Arguments[0].Name, Is.EqualTo("Time"));
                Assert.That(timeSeries.Components, Has.Count.EqualTo(1));
                Assert.That(timeSeries.Components[0].Name, Is.EqualTo("flow"));
                Assert.That(timeSeries.Components[0].Unit.Symbol, Is.EqualTo("m3/s"));

                IMultiDimensionalArray<DateTime> times = timeSeries.Time.Values;
                IMultiDimensionalArray values = timeSeries.Components[0].Values;
                Assert.That(times[0], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(60)));
                Assert.That(values[0], Is.EqualTo(1.23));
                Assert.That(times[1], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(120)));
                Assert.That(values[1], Is.EqualTo(4.56));
                Assert.That(times[2], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(180)));
                Assert.That(values[2], Is.EqualTo(7.89));

                // - Lateral source 2
                Assert.That(categories[1].Name, Is.EqualTo("some_lateral_source_2"));
                Assert.That(categories[1].DataType, Is.EqualTo(Model1DLateralDataType.FlowWaterLevelTable));
                Assert.That(categories[1].Discharge, Is.EqualTo(0));

                IFunction function = categories[1].DischargeFunction;
                Assert.That(function, Is.Not.Null);
                Assert.That(function.Name, Is.EqualTo("Discharge Water Level Series"));
                Assert.That(function.Arguments, Has.Count.EqualTo(1));
                Assert.That(function.Arguments[0].Name, Is.EqualTo("Water Level"));
                Assert.That(function.Arguments[0].Unit.Symbol, Is.EqualTo("m"));
                Assert.That(function.Components, Has.Count.EqualTo(1));
                Assert.That(function.Components[0].Name, Is.EqualTo("Discharge"));
                Assert.That(function.Components[0].Unit.Symbol, Is.EqualTo("m³/s"));

                IMultiDimensionalArray arguments = function.Arguments[0].Values;
                values = function.Components[0].Values;
                Assert.That(arguments[0], Is.EqualTo(10.1));
                Assert.That(values[0], Is.EqualTo(1.23));
                Assert.That(arguments[1], Is.EqualTo(20.2));
                Assert.That(values[1], Is.EqualTo(3.45));
                Assert.That(arguments[2], Is.EqualTo(30.3));
                Assert.That(values[2], Is.EqualTo(5.67));

                // - Lateral source 3
                Assert.That(categories[2].Name, Is.EqualTo("some_lateral_source_3"));
                Assert.That(categories[2].DataType, Is.EqualTo(Model1DLateralDataType.FlowConstant));
                Assert.That(categories[2].Discharge, Is.EqualTo(99.9));
                Assert.That(categories[2].DischargeFunction, Is.Null);
            }
        }
    }
}