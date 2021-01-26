using System;
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
    public class TimeRuleTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";
        
        private const string Name = "TimeRule";
        private const string OutputLocationName = "output location name";
        private const string OutputParameterName = "output parameter name";
        
        private InterpolationType interpolationOptions = InterpolationType.Linear;
        private Output output;
        private const string InputReferenceEnumStringType = "IMPLICIT";

        [SetUp]
        public void SetUp()
        {
            output = new Output
            {
                ParameterName = OutputParameterName,
                Feature = new RtcTestFeature { Name = OutputLocationName },
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var timeRule = new TimeRule
                               {
                                   Name = Name,
                                   Outputs = new EventedList<Output> {output},
                                   InterpolationOptionsTime = interpolationOptions,
                                   Reference = InputReferenceEnumStringType
                               };
            Assert.AreEqual(OriginXml(), timeRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        private string OriginXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<timeAbsolute id=\"" + Name + "\">" +
                   "<input>" +
                   "<x ref=\"" + InputReferenceEnumStringType + "\">" + Name + "_TimeSeries" + "</x>" +
                   "</input>" +
                   "<output>" +
                   "<y>output_" + OutputLocationName + "_" + OutputParameterName + "</y>" +
                   "</output>" +
                   "</timeAbsolute>" +
                   "</rule>";
        }

        [Test]
        public void XmlTimeSeriesGeneration()
        {
            var timeRule = new TimeRule
            {
                Name = Name,
                Outputs = new EventedList<Output> { output },
                InterpolationOptionsTime = interpolationOptions
            };
            
            var start = new DateTime(2011, 1, 1, 9, 30, 0);
            var stop = new DateTime(2011, 1, 1, 15, 30, 0);
            var step = new TimeSpan(0, 6, 0, 0);

            var xmlTimeSeries = timeRule.XmlImportTimeSeries("prefix", start, stop, step)
                    .FirstOrDefault().ToTimeSeriesXml("", new TimeSpan(0, 1, 0, 0)).ToString(SaveOptions.DisableFormatting);

            Assert.AreEqual(TimeSeriesXml(), xmlTimeSeries);
        }

        private string TimeSeriesXml()
        {
            return "<series>" +
                   "<header>" +
                   "<type>instantaneous</type>" +
                   "<locationId>prefixTimeRule</locationId>" +
                   "<parameterId>TimeSeries</parameterId>" +
                   "<timeStep unit=\"hour\" multiplier=\"1\" />" +
                   "<startDate date=\"2011-01-01\" time=\"09:30:00\" />" +
                   "<endDate date=\"2011-01-01\" time=\"15:30:00\" />" +
                   "<missVal>-999.0</missVal>" +
                   "<stationName />" +
                   "<units />" +
                   "</header>" +
                   "<event date=\"2011-01-01\" time=\"15:30:00\" value=\"0\" />" +
                   "</series>";
        }                      

        [Test]
        public void CopyFromAndClone()
        {
            var source = new TimeRule()
            {
                Name = "test",
                InterpolationOptionsTime = InterpolationType.Linear,
                Periodicity = ExtrapolationType.Constant,
                //TimeSeries = new TimeSeries()
            };
            var timeSeries = new TimeSeries();
            timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Components.Add(new Variable<double>("someThing"));
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant; 
            var time = DateTime.Now;
            timeSeries[time] = 1.0;
            source.TimeSeries = timeSeries;
            var newRule = new TimeRule();

            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            Assert.AreEqual(source.InterpolationOptionsTime, newRule.InterpolationOptionsTime);
            Assert.AreEqual(source.Periodicity, newRule.Periodicity);
            for (int i = 0; i < source.TimeSeries.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.TimeSeries.Arguments[0].Values[i], newRule.TimeSeries.Arguments[0].Values[i]);
                Assert.AreEqual(source.TimeSeries.Components[0].Values[i], newRule.TimeSeries.Components[0].Values[i]);
            }
            var clone = (TimeRule)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void DoNotSupportNoneExtrapolation()
        {
            Assert.Throws<ArgumentException>(() => new TimeRule
            {
                Name = "test",
                InterpolationOptionsTime = InterpolationType.Linear,
                Periodicity = ExtrapolationType.None
            });
        }

        [Test]
        public void DoNotSupportLinearExtrapolation()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new TimeRule
                {
                    Name = "test",
                    InterpolationOptionsTime = InterpolationType.Linear,
                    Periodicity = ExtrapolationType.Linear
                };
            });
        }
    }

}
