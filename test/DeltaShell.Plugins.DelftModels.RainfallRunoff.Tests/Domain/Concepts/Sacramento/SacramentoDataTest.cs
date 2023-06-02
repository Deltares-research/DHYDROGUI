using System;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Sacramento
{
    [TestFixture]
    public class SacramentoDataTest
    {
        [Test]
        public void Constructor_SetsCatchmentModelDataOnCatchment()
        {
            // Setup
            var catchment = new Catchment();

            // Call
            var data = new SacramentoData(catchment);

            // Assert
            Assert.That(catchment.ModelData, Is.SameAs(data));
        }
        
        [Test]
        public void CloneSacramentoData()
        {
            var sacramentoData = new SacramentoData(new Catchment {Name = "catchment"});
            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(sacramentoData);
            var rng = new Random();
            for (var i = 0; i < 36; ++i)
            {
                sacramentoData.HydrographValues[i] = rng.NextDouble();
            }

            var sacramentoDataClone = sacramentoData.Clone() as SacramentoData;

            Assert.IsNotNull(sacramentoDataClone);
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(sacramentoData, sacramentoDataClone);
            for (var i = 0; i < 36; ++i)
            {
                Assert.AreEqual(sacramentoData.HydrographValues[i], sacramentoDataClone.HydrographValues[i]);
            }
        }
    }
}
