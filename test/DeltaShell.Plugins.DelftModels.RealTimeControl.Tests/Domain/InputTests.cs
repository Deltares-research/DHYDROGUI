using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class InputTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string Element = "InputName";
        private const string Name = "ParameterName";
        private const string Point = "Test";

        private Input input;
       
        [SetUp]
        public void SetUp()
        {
            input = new Input
                        {
                            ParameterName = Name,
                            Feature = new RtcTestFeature { Name = Element },
                            SetPoint = Point
                        };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            Assert.AreEqual(OriginXml(), input.ToXml(Fns, "x","setpoint").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationWithFilledValuesOnly()
        {
            input.SetPoint = string.Empty;
            Assert.AreEqual(OriginXmlSingleVariableFilled(), input.ToXml(Fns, "x").ToString(SaveOptions.DisableFormatting));
        }

        private static string OriginXml()
        {
            return "<input xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<x>input_" + Element + "_" + Name + "</x>" +
                   "<setpoint>" + Point + "</setpoint>" + //Not sure what setpoint means yet ... Set by the pidRule!!
                   "</input>";
        }

        private static string OriginXmlSingleVariableFilled()
        {
            return "<input xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<x>input_" + Element + "_" + Name + "</x>" +
                   "</input>";
        }

        [Test]
        public void CopyFromAndCreateClone()
        {
            var source = new Input
                             {
                                 ParameterName = "parameter",
                                 Feature = new RtcTestFeature { Name = "location" }
                             };

            var newInput = new Input();
            newInput.CopyFrom(source);
            Assert.AreEqual(source.Name, newInput.Name);
            var clone = (Input)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
            Assert.IsTrue(ReferenceEquals(source.Feature, clone.Feature));
            // SetPoint and SetPointXmlTag will not be cloned; there regenerated during the xml writing process.
        }
    }
}
