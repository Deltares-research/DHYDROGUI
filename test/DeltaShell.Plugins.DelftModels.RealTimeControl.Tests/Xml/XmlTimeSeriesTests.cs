using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Xml
{
    [TestFixture]
    public class XmlTimeSeriesTests
    {
        private XmlTimeSeries xmlTimeSeries;

        [SetUp]
        public void SetUp()
        {
            xmlTimeSeries = new XmlTimeSeries
            {
                Name = "setpoint_const3",
                LocationId = "vier",
                ParameterId = "WaterLevelSetPoint",
                StartTime = new DateTime(2009, 11, 8, 17, 0, 0),
                EndTime = new DateTime(2009, 11, 8, 23, 0, 0),
                TimeStep = new TimeSpan(0, 0, 3600)
            };

            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>
            {
                Name = "Value",
                NoDataValue = -999.0
            });

            var time = new DateTime(2009, 11, 8, 17, 0, 0);
            for (var i = 3; i < 10; i++)
            {
                timeSeries[time] = i + 0.5;
                time += new TimeSpan(1, 0, 0);
            }

            xmlTimeSeries.TimeSeries = timeSeries;
        }

        [Test]
        public void DataConfig()
        {
            const string refXml = "<timeSeries id=\"setpoint_const3\">" +
                                  "<PITimeSeries>" +
                                  "<locationId>vier</locationId>" +
                                  "<parameterId>WaterLevelSetPoint</parameterId>" +
                                  "<interpolationOption>BLOCK</interpolationOption>" +
                                  "<extrapolationOption>BLOCK</extrapolationOption>" +
                                  "</PITimeSeries>" +
                                  "</timeSeries>";

            XElement xElement = xmlTimeSeries.GetTimeSeriesXElementForDataConfigFile("", false);

            Assert.AreEqual(refXml, xElement.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void TimeSeriesConfigCultureEnUS()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");

            XElement xElement = xmlTimeSeries.GetTimeSeriesXElementForTimeSeriesFile("", new TimeSpan(0, 1, 0, 0));

            Thread.CurrentThread.CurrentCulture = currentCulture;

            Assert.AreEqual(GetRefXml(), xElement.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void TimeSeriesBooleanToComplexType()
        {
            TimeSeries timeSeries = xmlTimeSeries.TimeSeries;
            timeSeries.Components.Clear();

            var lstEvenIndexIsTrue = new List<bool>();
            for (var i = 0; i < timeSeries.Time.Values.Count; i++)
            {
                lstEvenIndexIsTrue.Add(i % 2 == 0);
            }

            Assert.IsTrue(lstEvenIndexIsTrue[0]);
            Assert.IsFalse(lstEvenIndexIsTrue[1]);

            var boolComponent = new Variable<bool>
            {
                Name = "Value",
                DefaultValue = false,
                NoDataValue = false,
                IsAutoSorted = false
            };
            timeSeries.Components.Add(boolComponent);
            boolComponent.SetValues(lstEvenIndexIsTrue.ToArray());

            XElement xElement = xmlTimeSeries.GetTimeSeriesXElementForTimeSeriesFile("", new TimeSpan(0, 1, 0, 0));

            Assert.AreEqual(GetBooleanRefXml(), xElement.ToString(SaveOptions.DisableFormatting));
        }

        private static string GetRefXml()
        {
            return "<series>" +
                   "<header>" +
                   "<type>instantaneous</type>" +
                   "<locationId>vier</locationId>" +
                   "<parameterId>WaterLevelSetPoint</parameterId>" +
                   "<timeStep unit=\"hour\" multiplier=\"1\" />" +
                   "<startDate date=\"2009-11-08\" time=\"17:00:00\" />" +
                   "<endDate date=\"2009-11-08\" time=\"23:00:00\" />" +
                   "<missVal>-999.0</missVal>" +
                   "<stationName />" +
                   "<units />" +
                   "</header>" +
                   "<event date=\"2009-11-08\" time=\"17:00:00\" value=\"3.5\" />" +
                   "<event date=\"2009-11-08\" time=\"18:00:00\" value=\"4.5\" />" +
                   "<event date=\"2009-11-08\" time=\"19:00:00\" value=\"5.5\" />" +
                   "<event date=\"2009-11-08\" time=\"20:00:00\" value=\"6.5\" />" +
                   "<event date=\"2009-11-08\" time=\"21:00:00\" value=\"7.5\" />" +
                   "<event date=\"2009-11-08\" time=\"22:00:00\" value=\"8.5\" />" +
                   "<event date=\"2009-11-08\" time=\"23:00:00\" value=\"9.5\" />" +
                   "</series>";
        }

        private static string GetBooleanRefXml()
        {
            //   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //   rtcTools -> boolean true = 0, boolean false = 1
            //   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return "<series>" +
                   "<header>" +
                   "<type>instantaneous</type>" +
                   "<locationId>vier</locationId>" +
                   "<parameterId>WaterLevelSetPoint</parameterId>" +
                   "<timeStep unit=\"hour\" multiplier=\"1\" />" +
                   "<startDate date=\"2009-11-08\" time=\"17:00:00\" />" +
                   "<endDate date=\"2009-11-08\" time=\"23:00:00\" />" +
                   "<missVal>-999.0</missVal><stationName /><units />" +
                   "</header>" +
                   "<event date=\"2009-11-08\" time=\"17:00:00\" value=\"0\" />" + // = true
                   "<event date=\"2009-11-08\" time=\"18:00:00\" value=\"1\" />" + // = false
                   "<event date=\"2009-11-08\" time=\"19:00:00\" value=\"0\" />" + // = true
                   "<event date=\"2009-11-08\" time=\"20:00:00\" value=\"1\" />" + // = false
                   "<event date=\"2009-11-08\" time=\"21:00:00\" value=\"0\" />" + // = true
                   "<event date=\"2009-11-08\" time=\"22:00:00\" value=\"1\" />" + // = false
                   "<event date=\"2009-11-08\" time=\"23:00:00\" value=\"0\" />" + // = true
                   "</series>";
        }
    }
}