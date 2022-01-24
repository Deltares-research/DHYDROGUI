using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class LateralSourceFileImporterTest
    {
        private const string featureIdColumnName = "Feature ID";
        private const string rowValue1 = "rowValue 1";
        private const string rowValue2 = "rowValue 2";
        private readonly DateTime nowPlusOneHour = DateTime.Now.AddHours(1);
        private readonly DateTime nowPlusTwoHours = DateTime.Now.AddHours(2);
        private const string filePath = "filePath";
        private const string waterLevel = "Water Level";
        private const string discharge = "Discharge";
        private const string time = "Time";
        private readonly ILog logger = LogManager.GetLogger(typeof(LateralSourceFileImporter));

        [Test]
        public void Constructor_ImporterNull_ThrowsArgumentNullException()
        {
            void Call() => new LateralSourceFileImporter(null, Substitute.For<ILog>());
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("importer"));
        }

        [Test]
        public void Constructor_LoggerNull_ThrowsArgumentNullException()
        {
            void Call() => new LateralSourceFileImporter(Substitute.For<ICsvImporter>(), null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("logger"));
        }
        
        [Test]
        public void GivenCsvImportModeOneFunction_ImportItemShouldReturnDataItem()
        {
            // Arrange
            var csvImporter = Substitute.For<ICsvImporter>();
            var dataTable = new DataTable();
            Assert.That(dataTable.HasErrors, Is.False);

            csvImporter.ImportCsv(null, null).ReturnsForAnyArgs(dataTable);

            var fileImporter = new LateralSourceFileImporter(csvImporter, Substitute.For<ILog>()) 
            { 
                CsvImporterMode = CsvImporterMode.OneFunction, 
                FilePath = filePath
            };

            // Act
            object result = fileImporter.ImportItem(null);
            var resultAsDataItem = result as DataItem;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(resultAsDataItem);
            Assert.AreEqual(filePath, resultAsDataItem.Name);
        }

        [Test]
        public void GivenCsvImportModeSeveralFunctionsBasedOnColumns_ImportItemShouldReturnFolder()
        {
            // Arrange
            var csvImporter = Substitute.For<ICsvImporter>();
            var dataTable = new DataTable();
            Assert.That(dataTable.HasErrors, Is.False);

            csvImporter.ImportCsv(null, null).ReturnsForAnyArgs(dataTable);

            var fileImporter = new LateralSourceFileImporter(csvImporter, Substitute.For<ILog>()) 
            { 
                CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnColumns, 
                FilePath = filePath
            };

            // Act
            object result = fileImporter.ImportItem(null);
            var resultAsFolder = result as Folder;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(resultAsFolder);
            Assert.AreEqual(filePath, resultAsFolder.Name);
        }

        [Test]
        public void GivenCsvImportModeSeveralFunctionsBasedOnDiscriminator_ImportItemShouldProduceErrorLog()
        {
            // Arrange
            var logMock = Substitute.For<ILog>();

            var fileImporter = new LateralSourceFileImporter(Substitute.For<ICsvImporter>(), logMock)
            {
                CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnDiscriminator
            };

            // Act
            object result = fileImporter.ImportItem(null);
            
            // Assert
            Assert.IsNull(result);
            logMock.Received(1).ErrorFormat(Arg.Any<string>());
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void GivenShouldCancelOrDataTableHasErrors_ImportFunctionsShouldProduceErrorMessage(bool shouldCancel, bool hasErrors)
        {
            // Assert
            var dataTable = new DataTable();
            DataRow row = dataTable.NewRow();
            row.RowError = hasErrors ? "some error" : null;
            dataTable.Rows.Add(row);

            var csvImporterMock = Substitute.For<ICsvImporter>();
            csvImporterMock.ImportCsv(Arg.Any<string>(), Arg.Any<CsvMappingData>()).Returns(dataTable);

            var logMock = Substitute.For<ILog>();

            var fileImporter = new LateralSourceFileImporter(csvImporterMock, logMock)
            {
                ShouldCancel = shouldCancel
            };
            
            // Act
            IEnumerable<IFunction> result = fileImporter.ImportFunctions();
            
            // Assert
            Assert.IsEmpty(result);
            logMock.Received(1).ErrorFormat(Arg.Any<string>());
        }

        [Test]
        public void GivenCsvImportModeOneFunction_ImportFunctions_ShouldYieldSingleFunction()
        {
            // Arrange
            const string componentName1 = "a";
            const double argumentValue1 = 3;
            const string componentName2 = "b";
            const double argumentValue2 = 4;

            DataTable dataTable = GetDataTable(
                ("Time", typeof(DateTime), new object[] { DateTime.Today}),
                (componentName1, typeof(double), new object[] { argumentValue1 }),
                (componentName2, typeof(double), new object[] { argumentValue2 }));

            var csvImporterMock = Substitute.For<ICsvImporter>();
            csvImporterMock.ImportCsv(Arg.Any<string>(), Arg.Any<CsvMappingData>())
                           .Returns(dataTable);
            var fileImporter = new LateralSourceFileImporter(csvImporterMock, logger)
            {
                CsvImporterMode = CsvImporterMode.OneFunction
            };
            
            // Act
            IList<IFunction> result = fileImporter.ImportFunctions()?.ToList();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));

            IFunction function = result.First();
            Assert.That(function.Components, Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenCsvImportModeBasedOnColumns_ImportFunctions_ShouldYieldOneFunctionPerColumn()
        {
            // Arrange
            const string componentName1 = "a";
            const double argumentValue1 = 3;
            const string componentName2 = "b";
            const double argumentValue2 = 4;

            DataTable dataTable = GetDataTable(
                ("Time", typeof(DateTime), new object[] { DateTime.Today}),
                (componentName1, typeof(double), new object[] { argumentValue1 }),
                (componentName2, typeof(double), new object[] { argumentValue2 }));

            var csvImporterMock = Substitute.For<ICsvImporter>();
            csvImporterMock.ImportCsv(Arg.Any<string>(), Arg.Any<CsvMappingData>())
                           .Returns(dataTable);
            var fileImporter = new LateralSourceFileImporter(csvImporterMock, logger)
            {
                CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnColumns
            };
            
            // Act
            IList<IFunction> result = fileImporter.ImportFunctions()?.ToList();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        [TestCase(discharge, BoundaryRelationType.Qt)]
        [TestCase(waterLevel, BoundaryRelationType.Ht)]
        public void ImportFunctions_CsvImporterModeSeveralFunctionsBasedOnDiscriminator_TimeSeries_ExpectedResults(
            string dataColumnName,
            BoundaryRelationType relationType)
        {
            const double value1 = 2.5;
            const double value2 = 5.0;
            DataTable table = GetDataTable(
                (featureIdColumnName, typeof(string), new object[] { rowValue1,  rowValue2}),
                (time, typeof(DateTime), new object[] { nowPlusOneHour,  nowPlusTwoHours }),
                (dataColumnName, typeof(double), new object[] { value1,  value2})
            );
            var csvImporterMock = Substitute.For<ICsvImporter>();
            csvImporterMock.ImportCsv(Arg.Any<string>(), Arg.Any<CsvMappingData>()).Returns(table);
            var fileImporter = new LateralSourceFileImporter(csvImporterMock, logger)
            {
                ShouldCancel = false,
                BoundaryRelationType = relationType,
                CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnDiscriminator
            };
            
            // Act
            List<IFunction> result = fileImporter.ImportFunctions().ToList();
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.AreEqual(typeof(TimeSeries), result.First().GetType());
            Assert.AreEqual(nowPlusOneHour, result.First().Arguments[0].Values[0]);
            Assert.AreEqual(nowPlusTwoHours, result.Last().Arguments[0].Values[0]);
            Assert.AreEqual(value1, result.First().Components[0].Values[0]);
            Assert.AreEqual(value2, result.Last().Components[0].Values[0]);
        }

        [Test]
        public void GivenBoundaryRelationTypeQh_ImportSeriesSeveralFunctionsBasedOnDiscriminator_ShouldEditFunctionArgumentsAndComponentsWithTupleOfDoubles()
        {
            // Arrange
            const double argumentValue1 = 2;
            const double dischargeValue1 = 2.5;
            const double argumentValue2 = 3;
            const double dischargeValue2 = 5.0;
            
            DataTable dataTable = GetDataTable(
                (featureIdColumnName, typeof(string), new object[] { rowValue1,  rowValue2}),
                (waterLevel, typeof(double), new object[] { argumentValue1,  argumentValue2}),
                (discharge, typeof(double), new object[] { dischargeValue1,  dischargeValue2})
            );

            var csvImporterMock = Substitute.For<ICsvImporter>();
            csvImporterMock.ImportCsv(Arg.Any<string>(), Arg.Any<CsvMappingData>()).Returns(dataTable);

            var fileImporter = new LateralSourceFileImporter(csvImporterMock, logger)
            {
                ShouldCancel = false,
                BoundaryRelationType = BoundaryRelationType.Qh,
                CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnDiscriminator
            };
            
            // Act
            List<IFunction> result = fileImporter.ImportFunctions().ToList();
            
            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(typeof(Function), result.First().GetType());
            Assert.AreEqual(argumentValue1, result.First().Arguments[0].Values[0]);
            Assert.AreEqual(argumentValue2, result.Last().Arguments[0].Values[0]);
            Assert.AreEqual(dischargeValue1, result.First().Components[0].Values[0]);
            Assert.AreEqual(dischargeValue2, result.Last().Components[0].Values[0]);

        }

        [Test]
        [TestCase(discharge, BoundaryRelationType.Q)]
        [TestCase(waterLevel, BoundaryRelationType.H)]
        public void GivenBoundaryRelationTypeQ_ImportSeriesSeveralFunctionsBasedOnDiscriminator_ShouldEditFunctionComponentsWithDoubles(
            string componentName,
            BoundaryRelationType relationType)
        {
            // Arrange
            const double argumentValue1 = 2.5;
            const double argumentValue2 = 5.0;
            DataTable table = GetDataTable(
                (featureIdColumnName, typeof(string), new object[] { rowValue1,  rowValue2}),
                (componentName, typeof(double), new object[] { argumentValue1,  argumentValue2})
            );
            
            var csvImporterMock = Substitute.For<ICsvImporter>();
            csvImporterMock.ImportCsv(Arg.Any<string>(), Arg.Any<CsvMappingData>()).Returns(table);
            var fileImporter = new LateralSourceFileImporter(csvImporterMock, logger)
            {
                ShouldCancel = false,
                BoundaryRelationType = relationType,
                CsvImporterMode = CsvImporterMode.SeveralFunctionsBasedOnDiscriminator
            };
            
            // Act
            List<IFunction> result = fileImporter.ImportFunctions().ToList();
            
            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(typeof(Function), result.First().GetType());
            Assert.AreEqual(argumentValue1, result.First().Components[0].Values[0]);
            Assert.AreEqual(argumentValue2, result.Last().Components[0].Values[0]);
        }
        
        private static DataTable GetDataTable(params ValueTuple<string, Type, object[]>[] data)
        {
            var table = new DataTable();

            foreach ((string columnName, Type columnType, object[] _) in data)
            {
                table.Columns.Add(columnName, columnType);
            }

            List<DataRow> rows = Enumerable.Range(0, data[0].Item3.Length)
                                           .Select(_ => table.NewRow())
                                           .ToList();

            foreach ((string columnName, Type _, object[] columnData) in data)
            {
                foreach ((DataRow row, object v) in rows.Zip(columnData, (row, elem) => new ValueTuple<DataRow, object>(row, elem)))
                {
                    row[columnName] = v;
                }
            }

            foreach (DataRow dataRow in rows)
            {
                table.Rows.Add(dataRow);
            }

            return table;
        }
    }
}