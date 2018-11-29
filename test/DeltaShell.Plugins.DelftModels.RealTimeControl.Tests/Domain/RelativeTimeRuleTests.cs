using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class RelativeTimeRuleTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string Name = "Relative time rule";
        private Output output;
        private const string OutputParameterName = "output parameter";
        private const string OutputName = "output name";

        private Function tableFunction;

        [SetUp]
        public void SetUp()
        {
            tableFunction = RelativeTimeRule.DefineFunction();
            tableFunction[0.0] = 1.2;
            tableFunction[60.0] = 3.4;
            tableFunction[120.0] = 5.6;
            tableFunction[180.0] = 7.8;
            output = new Output
            {
                ParameterName = OutputParameterName,
                Feature = new RtcTestFeature { Name = OutputName },
            };
        }

        [Test]
        public void CheckXmlGenerationAbsolute()
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = Name,
                //Inputs = new EventedList<Input> { input },
                Outputs = new EventedList<Output> { output },
                FromValue = false,
                Function = tableFunction
            };
            var xmlAbsolute = "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<timeRelative id=\"/Relative time rule\">" +
                   "<mode>RETAINVALUEWHENINACTIVE</mode>" +
                   "<valueOption>ABSOLUTE</valueOption>" +  // RelativeTimeseries is ABSOLUTE; RelativeTimeseries is RELATIVE
                   "<maximumPeriod>0</maximumPeriod>" +
                   "<controlTable>" +
                   "<record time=\"0\" value=\"1.2\" />" +
                   "<record time=\"60\" value=\"3.4\" />" +
                   "<record time=\"120\" value=\"5.6\" />" +
                   "<record time=\"180\" value=\"7.8\" />" +
                   "<record time=\"181\" value=\"7.8\" />" + // see RelativeTimeRule::GetTable
                   "</controlTable>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + "output name/output parameter</y>" +
                   "<timeActive>[RelativeTimeRule]/Relative time rule</timeActive>" +
                   "</output>" +
                   "</timeRelative>" +
                   "</rule>";

            var xDocument = relativeTimeRule.ToXml(Fns, "");
            Assert.AreEqual(xmlAbsolute, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationRelativeFromValue()
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = Name,
                Outputs = new EventedList<Output> { output },
                FromValue = true,
                Function = tableFunction
            };
            var xmlRelative = "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<timeRelative id=\"/Relative time rule\">" +
                   "<mode>RETAINVALUEWHENINACTIVE</mode>" +
                   "<valueOption>RELATIVE</valueOption>" +  // RelativeTimeseries is ABSOLUTE; RelativeTimeseries is RELATIVE
                   "<maximumPeriod>0</maximumPeriod>" +
                   "<controlTable>" +
                   "<record time=\"0\" value=\"1.2\" />" +
                   "<record time=\"60\" value=\"3.4\" />" +
                   "<record time=\"120\" value=\"5.6\" />" +
                   "<record time=\"180\" value=\"7.8\" />" +
                   "<record time=\"181\" value=\"7.8\" />" + // see RelativeTimeRule::GetTable
                   "</controlTable>" +
                   "<input>" +
                   "<y>" + RtcXmlTag.Output + "output name/output parameter[AsInputFor]Relative time rule</y>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + "output name/output parameter</y>" +
                   "<timeActive>[RelativeTimeRule]/Relative time rule</timeActive>" +
                   "</output>" +
                   "</timeRelative>" +
                   "</rule>";

            var xDocument = relativeTimeRule.ToXml(Fns, "");
            Assert.AreEqual(xmlRelative, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new RelativeTimeRule
            {
                Name = "test",
                FromValue = false,
                Interpolation = InterpolationType.Linear
            };

            var newRule = new RelativeTimeRule();
            var argumentValues = new[] { 60, 120.0, 360.0 };
            var componentValues = new[] { 8.0, 9.0, 10.0 };
            for (int i = 0; i < argumentValues.Count(); i++)
            {
                source.Function[argumentValues[i]] = componentValues[i];
            }
            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            for (int i = 0; i < source.Function.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.Function.Arguments[0].Values[i], newRule.Function.Arguments[0].Values[i]);
                Assert.AreEqual(source.Function.Components[0].Values[i], newRule.Function.Components[0].Values[i]);
            }
            Assert.AreEqual(source.FromValue, newRule.FromValue);
            Assert.AreEqual(source.Interpolation, newRule.Interpolation);
            
            var clone = (RelativeTimeRule)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }
    }

}
