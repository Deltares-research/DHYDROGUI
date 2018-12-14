using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlToolsConfigXmlReaderTest
    {
        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGiven()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();
            var connectionPoints = new List<ConnectionPoint>();

            // Then
            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                // When
                RealTimeControlToolsConfigXmlReader.Read(filePath, controlGroups, connectionPoints);
            },
                string.Format(Resources.RealTimeControlToolsConfigXmlReader_Read_File___0___does_not_exist_, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForControlgroups_WhenReading_ThenMethodIsReturnedAndNothingHappens()
        {
            // Given
            const string fileName = "rtcToolsConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "ToolsConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            var connectionPoints = new List<ConnectionPoint>();

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlToolsConfigXmlReader.Read(filePath, null, connectionPoints);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForConnectionPoints_WhenReading_ThenMethodReturnsNoInputsOrOutputs()
        {
            // Given
            const string fileName = "rtcToolsConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "ToolsConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            Assert.DoesNotThrow(() =>
            {
                RealTimeControlToolsConfigXmlReader.Read(filePath, controlGroups, null);
            });

            Assert.AreEqual(0, controlGroups.SelectMany(c => c.Inputs).Count());
            Assert.AreEqual(0, controlGroups.SelectMany(c => c.Outputs).Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenCorrectOutputValuesAreSet()
        {
            // Given
            const string fileName = "rtcToolsConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "ToolsConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var controlGroup = new ControlGroup { Name = "control_group" };
            var timeRule = new TimeRule("time_rule");
            var relativeTimeRule = new RelativeTimeRule("relative_time_rule", true);
            var timeCondition = new TimeCondition { Name = "time_condition" };
            var standardCondition = new StandardCondition { Name = "standard_condition" };

            controlGroup.Conditions.AddRange(new[] { timeCondition, standardCondition });
            controlGroup.Rules.AddRange(new RuleBase[] { timeRule, relativeTimeRule });

            var input = new Input {Name = "[Input]parameter/quantity"};
            var output = new Output {Name = "[Output]parameter/quantity"};
          
            var controlGroups = new List<ControlGroup> {controlGroup};
            var connectionPoints = new List<ConnectionPoint> {input, output};

            // When
            RealTimeControlToolsConfigXmlReader.Read(filePath, controlGroups, connectionPoints);

            // Then
            // time rule
            Assert.AreEqual(0, timeRule.Inputs.Count);
            Assert.AreEqual(1, timeRule.Outputs.Count);
            Assert.AreEqual(output, timeRule.Outputs[0]);

            // relative time rule
            Assert.AreEqual(3, relativeTimeRule.MinimumPeriod); 
            Assert.AreEqual(1, relativeTimeRule.Function.Arguments[0].Values[0]);
            Assert.AreEqual(60, relativeTimeRule.Function.Arguments[0].Values[1]);
            Assert.AreEqual(3600, relativeTimeRule.Function.Arguments[0].Values[2]);
            Assert.AreEqual(86400, relativeTimeRule.Function.Arguments[0].Values[3]);
            Assert.AreEqual(2, relativeTimeRule.Function.Components[0].Values[0]);
            Assert.AreEqual(4, relativeTimeRule.Function.Components[0].Values[1]);
            Assert.AreEqual(16, relativeTimeRule.Function.Components[0].Values[2]);
            Assert.AreEqual(256, relativeTimeRule.Function.Components[0].Values[3]);
            Assert.AreEqual(0, relativeTimeRule.Inputs.Count);
            Assert.AreEqual(1, relativeTimeRule.Outputs.Count);
            Assert.AreEqual(output, relativeTimeRule.Outputs[0]);

            // time condition
            Assert.AreEqual(StandardCondition.ReferenceType.Implicit, timeCondition.Reference);
            Assert.AreEqual(Operation.Equal, timeCondition.Operation);
            Assert.AreEqual(0, timeCondition.Value);
            Assert.AreEqual(1, timeCondition.TrueOutputs.Count);
            Assert.AreEqual(timeRule, timeCondition.TrueOutputs[0]);
            Assert.AreEqual(1, timeCondition.FalseOutputs.Count);
            Assert.AreEqual(standardCondition, timeCondition.FalseOutputs[0]);

            // standard condition
            Assert.AreEqual(StandardCondition.ReferenceType.Explicit, standardCondition.Reference);
            Assert.AreEqual(Operation.GreaterEqual, standardCondition.Operation);
            Assert.AreEqual(5, standardCondition.Value);
            Assert.AreEqual(1, standardCondition.TrueOutputs.Count);
            Assert.AreEqual(relativeTimeRule, standardCondition.TrueOutputs[0]);
            Assert.AreEqual(0, standardCondition.FalseOutputs.Count);
        }
    }
}
