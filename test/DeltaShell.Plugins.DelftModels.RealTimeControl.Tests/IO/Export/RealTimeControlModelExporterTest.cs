using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
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
                var model = new RealTimeControlModel {RestartInput = new RealTimeControlRestartFile("Restart File", "file content here"), UseRestart = true};
                AddControlGroupToModel(model);

                // When
                new RealTimeControlModelExporter().Export(model, tempDirectory.Path);

                // Then
                Assert.That(File.ReadAllText(Path.Combine(tempDirectory.Path, RealTimeControlXMLFiles.XmlImportState)), Is.EqualTo("file content here"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModelWithRestartInputAndUseRestartFalse_WhenExported_ThenRestartFileNotWrittenToPath()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var model = new RealTimeControlModel {RestartInput = new RealTimeControlRestartFile("Restart File", "file content here"), UseRestart = false};
                AddControlGroupToModel(model);

                // When
                new RealTimeControlModelExporter().Export(model, tempDirectory.Path);

                // Then
                Assert.That(File.ReadAllText(Path.Combine(tempDirectory.Path, RealTimeControlXMLFiles.XmlImportState)), Is.Not.EqualTo("file content here"));
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
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup1);
        }
    }
}