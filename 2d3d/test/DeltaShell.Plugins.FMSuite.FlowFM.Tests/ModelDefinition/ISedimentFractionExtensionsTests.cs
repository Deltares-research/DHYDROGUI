using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelDefinition
{
    [TestFixture]
    public class ISedimentFractionExtensionsTests
    {
        [Test]
        public void UpdateSpatiallyVaryingNamesTest()
        {
            var fraction = new SedimentFraction();
            fraction.Name = "MyName";
            ISpatiallyVaryingSedimentProperty spatiallyVaryingProperty =
                fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault();
            Assert.IsNotNull(spatiallyVaryingProperty);
            Assert.AreEqual("SedConc", spatiallyVaryingProperty.Name);
            Assert.IsNull(spatiallyVaryingProperty.SpatiallyVaryingName);
            fraction.UpdateSpatiallyVaryingNames();
            spatiallyVaryingProperty = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault();
            Assert.IsNotNull(spatiallyVaryingProperty);
            Assert.AreEqual("SedConc", spatiallyVaryingProperty.Name);
            Assert.IsNotNull(spatiallyVaryingProperty.SpatiallyVaryingName);
            Assert.That(spatiallyVaryingProperty.SpatiallyVaryingName, Does.Match("MyName_SedConc"));
        }

        [Test]
        public void CompileAndSetVisibilityAndIfEnabledTest()
        {
            var fraction = new SedimentFraction();
            ISpatiallyVaryingSedimentProperty spatiallyVaryingProperty =
                fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault();
            Assert.IsNotNull(spatiallyVaryingProperty);
            Assert.AreEqual("SedConc", spatiallyVaryingProperty.Name);
            Assert.IsFalse(spatiallyVaryingProperty.IsEnabled);
            Assert.IsFalse(spatiallyVaryingProperty.IsVisible);
            fraction.CompileAndSetVisibilityAndIfEnabled();
            spatiallyVaryingProperty = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault();
            Assert.IsNotNull(spatiallyVaryingProperty);
            Assert.AreEqual("SedConc", spatiallyVaryingProperty.Name);
            Assert.IsTrue(spatiallyVaryingProperty.IsEnabled);
            Assert.IsTrue(spatiallyVaryingProperty.IsVisible);
        }

        [Test]
        public void SetTransportFormulaInCurrentSedimentTypeTest()
        {
            var fraction = new SedimentFraction();
            var traFrm = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "TraFrm") as
                             ISedimentProperty<int>;
            Assert.IsNotNull(traFrm);
            Assert.AreEqual(-1, traFrm.Value);
            fraction.CurrentFormulaType = fraction.SupportedFormulaTypes.ElementAt(2);
            traFrm = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "TraFrm") as
                         ISedimentProperty<int>;
            Assert.IsNotNull(traFrm);
            Assert.AreEqual(-1, traFrm.Value);
            fraction.SetTransportFormulaInCurrentSedimentType();
            traFrm = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "TraFrm") as
                         ISedimentProperty<int>;
            Assert.IsNotNull(traFrm);
            Assert.AreEqual(1, traFrm.Value);
        }
    }
}