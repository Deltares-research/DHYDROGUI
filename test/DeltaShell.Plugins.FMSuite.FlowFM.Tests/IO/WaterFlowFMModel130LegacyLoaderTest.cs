using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class WaterFlowFMModel130LegacyLoaderTest
    {
        [Test]
        public void OnAfterProjectMigrated_ProjectNull_ThrowsArgumentNullException()
        {
            // Setup
            var legacyLoader = new WaterFlowFMModel130LegacyLoader();

            // Call
            void Call() => legacyLoader.OnAfterProjectMigrated(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("project"));
        }

        [Test]
        public void OnAfterProjectMigrated_RemovesCorrectDataItemsFromModel()
        {
            // Setup
            var legacyLoader = new WaterFlowFMModel130LegacyLoader();
            var model = new WaterFlowFMModel();
            Project project = GetProjectWith(model);

            List<IDataItem> dataItems = GetSpatialDataItems().ToList();
            model.DataItems.AddRange(dataItems);

            // Call
            legacyLoader.OnAfterProjectMigrated(project);

            // Assert
            Assert.That(model.DataItems.Intersect(dataItems), Is.Empty);
        }

        [Test]
        public void OnAfterInitialize_CannotParseDatabasePath_LogsError()
        {
            // Setup
            var model = new WaterFlowFMModel();
            var dbConnection = Substitute.For<IDbConnection>();
            var legacyLoader = new WaterFlowFMModel130LegacyLoader();

            // Call
            void Call() => legacyLoader.OnAfterInitialize(model, dbConnection);

            // Assert
            IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
            Assert.That(errors.Single(), Is.EqualTo("Could not determine dsproj location from database connection: "));
        }

        [Test]
        [TestCase("FlowFM.mdu")]
        [TestCase("FlowFM.ext")]
        [TestCase("small_grid.nc")]
        public void OnAfterInitialize_CannotFindFile_LogsError(string fileName)
        {
            string testData = TestHelper.GetTestFilePath(@"WaterFlowFMModel130LegacyLoaderTest\Project1.dsproj_data");

            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string projectDir = temp.CopyDirectoryToTempDirectory(testData);

                var model = new WaterFlowFMModel();
                model.DataItems.AddRange(GetSpatialDataItems());
                IDbConnection dbConnection = GetDbConnection(temp.Path);
                var legacyLoader = new WaterFlowFMModel130LegacyLoader();

                string filePath = Path.Combine(projectDir, "FlowFM", "input", fileName);
                File.Delete(filePath);

                // Call
                void Call() => legacyLoader.OnAfterInitialize(model, dbConnection);

                // Assert
                IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
                Assert.That(errors.Single(), Is.EqualTo($"Could not find the file: {filePath}"));
            }
        }

        [TestCaseSource(nameof(OnAfterInitialize_ArgumentNullCases))]
        public void OnAfterInitialize_ArgumentNull_ThrowsArgumentNullException(object entity, IDbConnection dbConnection, string expParamName)
        {
            // Setup
            var legacyLoader = new WaterFlowFMModel130LegacyLoader();

            // Call
            void Call() => legacyLoader.OnAfterInitialize(entity, dbConnection);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void OnAfterInitialize_UpdatesFiles()
        {
            string testData = TestHelper.GetTestFilePath(@"WaterFlowFMModel130LegacyLoaderTest\Project1.dsproj_data");
            string expectedData = TestHelper.GetTestFilePath(@"WaterFlowFMModel130LegacyLoaderTest\expected");

            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string projectDir = temp.CopyDirectoryToTempDirectory(testData);
                string expectedDir = temp.CopyDirectoryToTempDirectory(expectedData);

                var model = new WaterFlowFMModel();
                model.DataItems.AddRange(GetSpatialDataItems());
                IDbConnection dbConnection = GetDbConnection(temp.Path);
                var legacyLoader = new WaterFlowFMModel130LegacyLoader();

                // Call
                legacyLoader.OnAfterInitialize(model, dbConnection);

                // Assert
                string inputDir = Path.Combine(projectDir, "FlowFM", "input");

                AssertCorrectFile("FlowFM.ext");
                AssertCorrectFile("initialwaterlevel_samples.xyz");
                AssertCorrectFile("initialsalinity_samples.xyz");
                AssertCorrectFile("initialtemperature_samples.xyz");
                AssertCorrectFile("frictioncoefficient_samples.xyz");
                AssertCorrectFile("horizontaleddyviscositycoefficient_samples.xyz");
                AssertCorrectFile("horizontaleddydiffusivitycoefficient_samples.xyz");
                AssertCorrectFile("initialtracerSomeTracer_samples.xyz");

                void AssertCorrectFile(string fileName)
                {
                    string filePath = Path.Combine(inputDir, fileName);
                    Assert.That(filePath, Does.Exist);

                    string actual = File.ReadAllText(filePath);
                    string expected = File.ReadAllText(Path.Combine(expectedDir, fileName));
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }
        }

        private static IEnumerable<TestCaseData> OnAfterInitialize_ArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IDbConnection>(), "entity");
            yield return new TestCaseData(new object(), null, "dbConnection");
        }

        private static IDbConnection GetDbConnection(string path)
        {
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.ConnectionString = $@"Data Source={path}\Project1.dsproj;Synchronous=Off;Enlist=N;";

            return dbConnection;
        }

        private static Project GetProjectWith(object model)
        {
            var project = new Project();
            project.RootFolder.Add(model);
            return project;
        }

        private static IEnumerable<IDataItem> GetSpatialDataItems()
        {
            yield return CreateDataItem<UnstructuredGridCoverage>("Bed Level", 36,
                                                                  new UnstructuredGridVertexCoverage(null, false));
            yield return CreateDataItem("Initial Water Level", 25,
                                        new UnstructuredGridCellCoverage(null, false));
            yield return CreateDataItem("Initial Salinity", 25,
                                        new UnstructuredGridCellCoverage(null, false));
            yield return CreateDataItem("Initial Temperature", 25,
                                        new UnstructuredGridCellCoverage(null, false));
            yield return CreateDataItem("Viscosity", 40,
                                        new UnstructuredGridFlowLinkCoverage(null, false));
            yield return CreateDataItem("Diffusivity", 40,
                                        new UnstructuredGridFlowLinkCoverage(null, false));
            yield return CreateDataItem("Roughness", 40,
                                        new UnstructuredGridFlowLinkCoverage(null, false));
            yield return CreateDataItem("SomeTracer", 25,
                                        new UnstructuredGridCellCoverage(null, false));
            yield return CreateDataItem("SomeFraction_SedConc", 25,
                                        new UnstructuredGridCellCoverage(null, false));
            yield return CreateDataItem("SomeFraction_IniSedThick", 25,
                                        new UnstructuredGridCellCoverage(null, false));
        }

        private static IDataItem CreateDataItem<T>(string name, int nValues, T originalCoverage) where T : UnstructuredGridCoverage
        {
            originalCoverage.Name = name;
            originalCoverage.Components[0].NoDataValue = -999d;
            FunctionHelper.SetValuesRaw(originalCoverage.Components[0], Enumerable.Repeat(5d, nValues));

            var valueConverter = Substitute.For<SpatialOperationSetValueConverter>();
            valueConverter.OriginalValue.Returns(originalCoverage);
            return new DataItem(null, name, typeof(T), DataItemRole.Input, string.Empty) {ValueConverter = valueConverter};
        }
    }
}