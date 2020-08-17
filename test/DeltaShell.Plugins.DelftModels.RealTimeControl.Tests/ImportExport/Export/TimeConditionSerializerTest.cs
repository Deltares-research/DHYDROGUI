using System;
using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.Export
{
    [TestFixture]
    public class TimeConditionSerializerTest
    {
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        [Test]
        public void GivenStandardConditionWithoutConnectedObjects_WhenSerializedToXml_ThenExpectedXmlReturned()
        {
            var serializer = new TimeConditionSerializer(new TimeCondition());
            Assert.AreEqual(XmlWithoutConnectedObjects(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        private string XmlWithoutConnectedObjects()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<standard id=\"[TimeCondition]Time Condition\">" +
                   "<condition>" +
                   "<x1Series ref=\"IMPLICIT\">Time Condition</x1Series>" +
                   "<relationalOperator>Equal</relationalOperator>" +
                   "<x2Value>0</x2Value>" +
                   "</condition>" +
                   "<output>" +
                   "<status>[Status]Time Condition</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>";
        }
    }
}
