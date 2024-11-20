using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class HydraulicRuleSerializerTest
    {
        private const string ruleName = "hydraulic rule name";
        private const string inputName = "Maxau";
        private const string inputParameterName = "QIn";
        private const string outputName = "Iffezheim";
        private const string outputParameterName = "HSP";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";
        private int nTimeSteps;

        private Function tableFunction;
        private Input input;
        private Output output;

        [SetUp]
        public void SetUp()
        {
            tableFunction = HydraulicRule.DefineFunction();
            tableFunction[0.0] = 123.6;
            tableFunction[3400.0] = 123.6;
            tableFunction[4800.0] = 123.0;
            tableFunction[8000.0] = 123.0;
            input = new Input
            {
                ParameterName = inputParameterName,
                Feature = new RtcTestFeature {Name = inputName}
            };
            output = new Output
            {
                ParameterName = outputParameterName,
                Feature = new RtcTestFeature {Name = outputName}
            };
        }

        [Test]
        public void XmlGenerationWithTimeLag()
        {
            var hydraulicRule = new HydraulicRule
            {
                Name = ruleName,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                Function = tableFunction,
                TimeLag = 2000
            };

            hydraulicRule.SetTimeLagToTimeSteps(new TimeSpan(0, 0, 200));
            nTimeSteps = hydraulicRule.TimeLagInTimeSteps;

            var serializer = new HydraulicRuleSerializer(hydraulicRule);
            Assert.AreEqual(HydraulicRuleWithTimeLagXml(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var hydraulicRule = new HydraulicRule
            {
                Name = ruleName,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                Function = tableFunction,
                Interpolation = InterpolationType.Linear
            };

            var serializer = new HydraulicRuleSerializer(hydraulicRule);
            Assert.AreEqual(OriginXml(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        private string OriginXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<lookupTable id=\"" + "[HydraulicRule]" + ruleName + "\">" +
                   "<table>" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "</table>" +
                   "<interpolationOption>LINEAR</interpolationOption>" +
                   "<extrapolationOption>BLOCK</extrapolationOption>" +
                   "<input>" +
                   "<x ref=\"IMPLICIT\">" + RtcXmlTag.Input + inputName + "/" + inputParameterName + "</x>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + outputName + "/" + outputParameterName + "</y>" +
                   "</output>" +
                   "</lookupTable>" +
                   "</rule>";
        }

        private string HydraulicRuleWithTimeLagXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<lookupTable id=\"" + "[HydraulicRule]" + ruleName + "\">" +
                   "<table>" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "</table>" +
                   "<interpolationOption>BLOCK</interpolationOption>" +
                   "<extrapolationOption>BLOCK</extrapolationOption>" +
                   "<input>" +
                   "<x ref=\"EXPLICIT\">" + RtcXmlTag.Delayed + RtcXmlTag.Input + inputName + "/" + inputParameterName +
                   "[" + (nTimeSteps - 2) + "]</x>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + outputName + "/" + outputParameterName + "</y>" +
                   "</output>" +
                   "</lookupTable>" +
                   "</rule>";
        }
    }
}