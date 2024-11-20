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
    public class LookupSignalSerializerTest
    {
        private const string signalName = "lookup signal name";
        private const string inputName = "Lobith";
        private const string inputParameterName = "H-Lobith";
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private Function tableFunction;
        private Input input;

        [SetUp]
        public void SetUp()
        {
            tableFunction = LookupSignal.DefineFunction();
            tableFunction[8.65] = 8.20;
            tableFunction[9.10] = 8.05;
            tableFunction[9.60] = 7.60;
            tableFunction[10.0] = 7.40;
            input = new Input
            {
                ParameterName = inputParameterName,
                Feature = new RtcTestFeature {Name = inputName}
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var lookupSignal = new LookupSignal()
            {
                Name = signalName,
                Inputs = new EventedList<Input> {input},
                Function = tableFunction,
                Interpolation = InterpolationType.Linear
            };

            var serializer = new LookupSignalSerializer(lookupSignal);
            Assert.AreEqual(OriginXml(), serializer.ToXml(Fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        private string OriginXml()
        {
            return "<signal xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<lookupTable id=\"[LookupSignal]" + signalName + "\">" +
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
                   "<output><y>" + RtcXmlTag.Signal + signalName + "</y></output>" +
                   "</lookupTable>" +
                   "</signal>";
        }
    }
}