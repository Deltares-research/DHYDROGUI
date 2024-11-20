using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    internal class WaterFlowFMModelSedimentFractionsTest
    {
        [Test]
        public void TestGetAllSpatiallyVaryingPropertyNamesShouldNotBeNull()
        {
            /* We should always retrieve spatially varying prop names as the properties for sediment
             are hardcoded (for now). If this would fail means we are no longer hardcoding them
             but retrieving them from a file or so. */
            var fraction = new SedimentFraction();
            List<string> spatiallyVaryingPropNames = fraction.GetAllSpatiallyVaryingPropertyNames();
            Assert.NotNull(spatiallyVaryingPropNames);
            Assert.IsNotEmpty(spatiallyVaryingPropNames);
        }

        [Test]
        public void CurrentFormulaTypeRetrievesSedimentFormulaTypeEvenWhenStart()
        {
            var fraction = new SedimentFraction();
            //The first supported formula by each Sediment type should be the one added.
            foreach (ISedimentType sedType in fraction.AvailableSedimentTypes)
            {
                fraction.CurrentSedimentType = sedType;
                Assert.AreEqual(fraction.CurrentSedimentType, sedType);
                //There are always supported formulatypes.
                Assert.NotNull(fraction.SupportedFormulaTypes);
                Assert.IsTrue(fraction.SupportedFormulaTypes.Contains(fraction.CurrentFormulaType));

                ISedimentFormulaType newFormula = fraction.SupportedFormulaTypes.FirstOrDefault(sf => sf != fraction.CurrentFormulaType);
                fraction.CurrentFormulaType = newFormula;
                if (newFormula == null)
                {
                    Assert.NotNull(fraction.CurrentFormulaType);
                }
            }
        }
    }
}