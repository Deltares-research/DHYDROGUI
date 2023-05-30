using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Unpaved
{
    [TestFixture]
    public class UnpavedDataTest
    {
        [Test]
        public void CloneWithDeZeeuw()
        {
            var unpaved = new UnpavedData(new Catchment());
            var zeeuw = (unpaved.DrainageFormula as DeZeeuwHellingaDrainageFormula);
            zeeuw.HorizontalInflow = 23423;
            zeeuw.LevelOneEnabled = true;
            var clone = (UnpavedData)unpaved.Clone();
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(unpaved, clone);

            var clonedZeeuw = clone.DrainageFormula as DeZeeuwHellingaDrainageFormula;
            
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(zeeuw, clonedZeeuw);
        }

        [Test]
        public void CloneWithKrayenhoff()
        {
            var unpaved = new UnpavedData(new Catchment());
            unpaved.SwitchDrainageFormula<KrayenhoffVanDeLeurDrainageFormula>();
            var krayenhoff = (unpaved.DrainageFormula as KrayenhoffVanDeLeurDrainageFormula);
            krayenhoff.ResevoirCoefficient = 23423;
            var clone = (UnpavedData)unpaved.Clone();
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(unpaved, clone);

            var clonedKrayenhoff = clone.DrainageFormula as KrayenhoffVanDeLeurDrainageFormula;

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(krayenhoff, clonedKrayenhoff);
        }

        [Test]
        public void CloneWithSeepageSeries()
        {
            var unpaved = new UnpavedData(new Catchment());
            var seepageSeries = new TimeSeries();
            seepageSeries.Components.Add(new Variable<double>("values"));
            unpaved.SeepageSeries = seepageSeries;
            seepageSeries[new DateTime(2000, 1, 1)] = 55.0;
            seepageSeries[new DateTime(2000, 1, 2)] = 55.0;
            seepageSeries[new DateTime(2000, 1, 3)] = 33.0;
            seepageSeries[new DateTime(2000, 1, 4)] = 55.0;
            seepageSeries[new DateTime(2000, 1, 5)] = 55.0;

            var clone = (UnpavedData)unpaved.Clone();

            Assert.AreNotSame(unpaved.SeepageSeries, clone.SeepageSeries);
            Assert.AreEqual(unpaved.SeepageSeries.Components[0].Values, clone.SeepageSeries.Components[0].Values);
        }

        [Test]
        public void CloneWithCropDictionary()
        {
            var unpaved = new UnpavedData(new Catchment());

            unpaved.AreaPerCrop[UnpavedEnums.CropType.GreenhouseArea] = 45;
            unpaved.AreaPerCrop[UnpavedEnums.CropType.Orchard] = 2;
            unpaved.AreaPerCrop[UnpavedEnums.CropType.Potatoes] = 1;
            unpaved.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 8;

            var clone = (UnpavedData)unpaved.Clone();

            Assert.AreNotSame(unpaved.AreaPerCrop, clone.AreaPerCrop);
            Assert.AreEqual(unpaved.AreaPerCrop.Values, clone.AreaPerCrop.Values);
        }
        
        [Test]
        public void CloneWithUseLocalBoundaryData()
        {
            var unpaved = new UnpavedData(new Catchment());

            unpaved.UseLocalBoundaryData = true;

            var clone = (UnpavedData)unpaved.Clone();

            Assert.AreNotSame(unpaved.UseLocalBoundaryData, clone.UseLocalBoundaryData);
            Assert.AreEqual(unpaved.UseLocalBoundaryData, clone.UseLocalBoundaryData);
        }
    }
}
