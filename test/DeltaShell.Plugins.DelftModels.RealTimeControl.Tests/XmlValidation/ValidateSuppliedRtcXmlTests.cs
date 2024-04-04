using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.XmlValidation
{
    // todo remove these tests? They only seem to have any purpose during initial development of 
    // rtcmodel
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class ValidateSuppliedRtcXmlTests
    {
        private string XsdPath => DimrApiDataSet.RtcXsdDirectory;

        [Test]
        public void ValidateGeneratedXml()
        {
            var input = new Input
            {
                ParameterName = "Water level",
                Feature = new RtcTestFeature {Name = "MeasureStationA"},
                SetPoint = "someSetPoint"
            };
            var output = new Output
            {
                ParameterName = "Crest level",
                Feature = new RtcTestFeature {Name = "WeirdWeir"},
                IntegralPart = "someIntegralPart"
            };

            var pidRule = new PIDRule("PIDRule Test");

            pidRule.Inputs.Add(input);
            pidRule.Outputs.Add(output);

            pidRule.Kd = 0.1;
            pidRule.Ki = 0.2;
            pidRule.Kp = 0.3;
            pidRule.Setting = new Setting
            {
                Min = 1.1,
                Max = 1.2,
                MaxSpeed = 1.3
            };
            pidRule.TimeSeries[new DateTime(2010, 1, 19, 12, 0, 0)] = 3.0;
            pidRule.TimeSeries[new DateTime(2010, 1, 20, 12, 0, 0)] = 4.0;
            pidRule.TimeSeries[new DateTime(2010, 1, 21, 12, 0, 0)] = 5.0;

            var controlGroup = new ControlGroup();

            var condition = new StandardCondition {Name = "testCondition"};
            condition.FalseOutputs.Add(pidRule);
            condition.Input = input;

            controlGroup.Rules.Add(pidRule);
            controlGroup.Conditions.Add(condition);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            XDocument toolsConfigXml = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            var validator = new Validator(new List<string> {XsdPath + Path.DirectorySeparatorChar + "rtcToolsConfig.xsd"});
            validator.Validate(toolsConfigXml);
            Assert.IsTrue(validator.IsValid(toolsConfigXml));

            var model = new ControlledTestModel();
            XDocument dataConfigXml = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, model, new List<ControlGroup> {controlGroup}, null);

            validator = new Validator(new List<string> {XsdPath + Path.DirectorySeparatorChar + "rtcDataConfig.xsd"});
            validator.Validate(dataConfigXml);
            Assert.IsTrue(validator.IsValid(dataConfigXml));
        }
    }
}