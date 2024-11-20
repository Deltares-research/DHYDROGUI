using System;
using System.Linq;
using System.Xml.Linq;
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
    public class TimeRuleSerializerTest
    {
        private const string name = "TimeRule";
        private const string outputLocationName = "output location name";
        private const string outputParameterName = "output parameter name";

        private const InterpolationType interpolationOptions = InterpolationType.Linear;
        private const string inputReferenceEnumStringType = "IMPLICIT";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";
        private Output output;

        [SetUp]
        public void SetUp()
        {
            output = new Output
            {
                ParameterName = outputParameterName,
                Feature = new RtcTestFeature {Name = outputLocationName}
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var timeRule = new TimeRule
            {
                Name = name,
                Outputs = new EventedList<Output> {output},
                InterpolationOptionsTime = interpolationOptions,
                Reference = inputReferenceEnumStringType
            };

            var serializer = new TimeRuleSerializer(timeRule);

            Assert.AreEqual(OriginXml(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void XmlTimeSeriesGeneration()
        {
            var timeRule = new TimeRule
            {
                Name = name,
                Outputs = new EventedList<Output> {output},
                InterpolationOptionsTime = interpolationOptions
            };

            var start = new DateTime(2011, 1, 1, 9, 30, 0);
            var stop = new DateTime(2011, 1, 1, 15, 30, 0);
            var step = new TimeSpan(0, 6, 0, 0);

            var serializer = new TimeRuleSerializer(timeRule);

            var xmlTimeSeries = serializer.XmlImportTimeSeries("prefix/", start, stop, step).First()
                                          .GetTimeSeriesXElementForTimeSeriesFile("", new TimeSpan(0, 1, 0, 0)).ToString(SaveOptions.DisableFormatting);

            Assert.AreEqual(TimeSeriesXml(), xmlTimeSeries);
        }

        private static string OriginXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<timeAbsolute id=\"[TimeRule]" + name + "\">" +
                   "<input>" +
                   "<x ref=\"" + inputReferenceEnumStringType + "\">" + name + "</x>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + outputLocationName + "/" + outputParameterName + "</y>" +
                   "</output>" +
                   "</timeAbsolute>" +
                   "</rule>";
        }

        private static string TimeSeriesXml()
        {
            return "<series>" +
                   "<header>" +
                   "<type>instantaneous</type>" +
                   "<locationId>[TimeRule]prefix/TimeRule</locationId>" +
                   "<parameterId>TimeSeries</parameterId>" +
                   "<timeStep unit=\"hour\" multiplier=\"1\" />" +
                   "<startDate date=\"2011-01-01\" time=\"09:30:00\" />" +
                   "<endDate date=\"2011-01-01\" time=\"15:30:00\" />" +
                   "<missVal>-999.0</missVal>" +
                   "<stationName />" +
                   "<units />" +
                   "</header>" +
                   "<event date=\"2011-01-01\" time=\"09:30:00\" value=\"0\" />" +
                   "<event date=\"2011-01-01\" time=\"15:30:00\" value=\"0\" />" +
                   "</series>";
        }
    }
}