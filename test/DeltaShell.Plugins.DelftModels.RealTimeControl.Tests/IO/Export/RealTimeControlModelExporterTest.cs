using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class RealTimeControlModelExporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModelWithRestartInputAndUseRestartTrue_WhenExported_ThenRestartFileWrittenToPath()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var model = new RealTimeControlModel {RestartInput = new RealTimeControlRestartFile("Restart File", "file content here")};
                AddControlGroupToModel(model);

                // When
                new RealTimeControlModelExporter().Export(model, tempDirectory.Path);

                // Then
                Assert.That(File.ReadAllText(Path.Combine(tempDirectory.Path, RealTimeControlXmlFiles.XmlImportState)), Is.EqualTo("file content here"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModelWithUseRestartFalse_WhenExported_ThenRestartFileShouldContainStateVectorsWithZeroOrNan()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var model = new RealTimeControlModel();
                AddControlGroupToModel(model);

                // When
                new RealTimeControlModelExporter().Export(model, tempDirectory.Path);

                string expectedFileContentPath = Path.Combine(tempDirectory.Path, "expected_state_import.xml");
                RealTimeControlXmlWriter.GetStateVectorXml(tempDirectory.Path, model.ControlGroups).Save(expectedFileContentPath);
                string exportedRestartFile = Path.Combine(tempDirectory.Path, RealTimeControlXmlFiles.XmlImportState);

                // Then
                FileAssert.AreEqual(expectedFileContentPath, exportedRestartFile);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Export_ShouldSetLastExportInputFilesAndDirectoriesPathsPropertyOfModel()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                var rtcModelExporter = new RealTimeControlModelExporter();
                var rtcModel = new RealTimeControlModel();

                // Act
                string exportDirectory = tempDirectory.Path;
                bool result = rtcModelExporter.Export(rtcModel, exportDirectory);

                // Assert
                string[] files = Directory.GetFiles(exportDirectory);
                string[] directories = Directory.GetDirectories(exportDirectory);

                string[] allInputPaths = files.Concat(directories).ToArray();

                Assert.AreEqual(allInputPaths.Length, rtcModel.LastExportedPaths.Length);
                Assert.IsTrue(allInputPaths.Any(i=>rtcModel.LastExportedPaths.Contains(i)));
                Assert.IsTrue(result);
            }
        }

        private static void AddControlGroupToModel(IRealTimeControlModel model)
        {
            ControlGroup controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            var hydraulicRule1A = (HydraulicRule) controlGroup1.Rules[0];
            var hydraulicCondition1A = (StandardCondition) controlGroup1.Conditions[0];
            hydraulicRule1A.Function[0.0] = 1.0;

            var hydraulicRule1B = new HydraulicRule();
            var hydraulicCondition1B = new StandardCondition();

            hydraulicCondition1A.FalseOutputs.Add(hydraulicCondition1B);
            hydraulicCondition1B.TrueOutputs.Add(hydraulicRule1B);
            hydraulicCondition1B.Input = controlGroup1.Inputs[0];
            hydraulicRule1B.Outputs.Add(controlGroup1.Outputs[0]);
            hydraulicRule1B.Inputs.Add(controlGroup1.Inputs[0]);
            hydraulicRule1B.Function[0.0] = 1.0;

            controlGroup1.Rules.Add(hydraulicRule1B);
            controlGroup1.Conditions.Add(hydraulicCondition1B);

            model.ControlGroups.Add(controlGroup1);
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup1);
        }
    }
}