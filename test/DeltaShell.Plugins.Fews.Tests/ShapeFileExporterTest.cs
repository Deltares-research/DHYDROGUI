using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class ShapeFileExporterTest : FewsAdapterTestBase
    {
        [SetUp]
        public void SetUp()
        {
            CheckWorkingDirectoryForTestRuns(this);
        }

        [TearDown]
        public void TearDown()
        {
            CheckWorkingDirectoryForTestRuns(this);
        }

        [Test]

        [Ignore(TestCategory.WorkInProgress)] // test doen't work where resharper runs it in temp
        public void Export_Project_DirectoryCreatedOnSpecifiedPath()
        {
            // setup
            var project = CreateTestProject();

            var exporter = new ShapeFileExporter();

            // call
            const string path = "TestExportFiles";
            exporter.Export(project, path);

            // checks
            string message = string.Format("The directory is {0} not created", path);
            Assert.IsTrue(Directory.Exists(path), message);
        }

        #region Helper Methods

        private Project CreateTestProject()
        {
            var model = CreateDemoNetworkWithLateralSources();
            model.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Laterals).
                AggregationOptions = AggregationOptions.Current;

            model.Initialize();

            var project = new Project();
            project.RootFolder.Add(model);
            return project;
        }

        #endregion
    }
}