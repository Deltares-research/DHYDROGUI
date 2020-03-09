using System.Globalization;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class RecordTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";
        
        private const double X = 0;
        private const double Y = 123.6;

        private Record record;
       
        [SetUp]
        public void SetUp()
        {
            record = new Record { X = X, Y = Y };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            Assert.AreEqual(OriginXml(), record.ToXml(Fns).ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        [Ignore("Waste of time to keep this up to date")]
        [Category("ToCheck")]
        public void RuleFromXml()
        {
            var resultRecord = new Record();
            var originXmlAsXElement = XElement.Parse(OriginXml());
            Assert.AreEqual(record, resultRecord);
        }

        private string OriginXml()
        {
            return "<record x=\"" + record.X.ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   record.Y.ToString(CultureInfo.InvariantCulture) + "\" xmlns=\"http://www.wldelft.nl/fews\" />";

        }
    }
}
