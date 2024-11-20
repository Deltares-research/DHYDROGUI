using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffUnitConverterTest
    {
        [Test]
        public void CheckTestsAreUpToDate()
        {
            //should anything be added to the enum, this fixture needs more tests
            Assert.AreEqual(2, Enum.GetValues(typeof(RainfallRunoffEnums.RainfallCapacityUnit)).Length);
        }

        [Test]
        public void ConvertSameHour()
        {
            var expected = 15;
            var result = RainfallRunoffUnitConverter.ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit.mm_hr,
                                                        RainfallRunoffEnums.RainfallCapacityUnit.mm_hr, expected);
            Assert.AreEqual(expected,result);
        }

        [Test]
        public void ConvertSameDay()
        {
            var expected = 15;
            var result = RainfallRunoffUnitConverter.ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit.mm_day,
                                                        RainfallRunoffEnums.RainfallCapacityUnit.mm_day, expected);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertHrToDay()
        {
            var expected = 15;
            var result = RainfallRunoffUnitConverter.ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit.mm_hr,
                                                        RainfallRunoffEnums.RainfallCapacityUnit.mm_day, expected);
            Assert.AreEqual(360, result);
        }

        [Test]
        public void ConvertCycle()
        {
            var expected = 15;
            var result = RainfallRunoffUnitConverter.ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit.mm_hr,
                                                        RainfallRunoffEnums.RainfallCapacityUnit.mm_day, expected);
            var final = RainfallRunoffUnitConverter.ConvertRainfall(RainfallRunoffEnums.RainfallCapacityUnit.mm_day,
                                                        RainfallRunoffEnums.RainfallCapacityUnit.mm_hr, result);
            Assert.AreEqual(expected, final);
        }

        [Test]
        public void AreaConversions()
        {
            var expected = 99.1;
            double delta = 1e-3;

            // unitFrom == unitTo
            var result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2,
                                                                  RainfallRunoffEnums.AreaUnit.m2, expected);
            var result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.km2,
                                                                  RainfallRunoffEnums.AreaUnit.km2, expected);
            var result3 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.ha,
                                                                  RainfallRunoffEnums.AreaUnit.ha, expected);
            Assert.AreEqual(expected, result1, delta);
            Assert.AreEqual(expected, result2, delta);
            Assert.AreEqual(expected, result3, delta);

            // m2 to other types
            result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2,
                                                              RainfallRunoffEnums.AreaUnit.km2, expected);
            result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2,
                                                              RainfallRunoffEnums.AreaUnit.ha, expected);
            Assert.AreEqual(0.0000991, result1, delta);
            Assert.AreEqual(0.00991, result2, delta);

            // ha to other types
            result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.ha,
                                                              RainfallRunoffEnums.AreaUnit.m2, expected);
            result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.ha,
                                                              RainfallRunoffEnums.AreaUnit.km2, expected);
            Assert.AreEqual(991000, result1, delta);
            Assert.AreEqual(0.991, result2, delta);

            // km2 to other types
            result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.km2,
                                                              RainfallRunoffEnums.AreaUnit.m2, expected);
            result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.km2,
                                                              RainfallRunoffEnums.AreaUnit.ha, expected);
            Assert.AreEqual(99100000, result1, delta);
            Assert.AreEqual(9910, result2, delta);

            // test some conversion cycles
            result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2,
                                                              RainfallRunoffEnums.AreaUnit.ha, expected);
            result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.ha,
                                                              RainfallRunoffEnums.AreaUnit.km2, result1);
            result3 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.km2,
                                                              RainfallRunoffEnums.AreaUnit.m2, result2);
            Assert.AreEqual(expected, result3, delta);

            result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2,
                                                              RainfallRunoffEnums.AreaUnit.km2, expected);
            result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.km2,
                                                              RainfallRunoffEnums.AreaUnit.ha, result1);
            result3 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.ha,
                                                              RainfallRunoffEnums.AreaUnit.m2, result2);
            Assert.AreEqual(expected, result3, delta);

            result1 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.km2,
                                                              RainfallRunoffEnums.AreaUnit.m2, expected);
            result2 = RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2,
                                                              RainfallRunoffEnums.AreaUnit.km2, result1);
            Assert.AreEqual(expected, result2, delta);
        }

        [Test]
        public void StorageConversions()
        {
            var value = 99.1;
            var area = 246.33; // m2
            double delta = 1e-3;

            // unitFrom == unitTo
            var result1 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.m3,
                                                                     RainfallRunoffEnums.StorageUnit.m3, value, area);
            var result2 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm,
                                                                     RainfallRunoffEnums.StorageUnit.mm, value, area);
            Assert.AreEqual(value, result1, delta);
            Assert.AreEqual(value, result2, delta);

            // mm to m3
            result1 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm,
                                                                 RainfallRunoffEnums.StorageUnit.m3, value, area);
            Assert.AreEqual(24.411303, result1, delta);

            // m3 to mm
            result1 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.m3,
                                                                 RainfallRunoffEnums.StorageUnit.mm, value, area);
            Assert.AreEqual(402.30585, result1, delta);

            // conversion cycles
            result1 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm,
                                                                 RainfallRunoffEnums.StorageUnit.m3, value, area);
            result2 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.m3,
                                                                 RainfallRunoffEnums.StorageUnit.mm, result1, area);
            Assert.AreEqual(value, result2, delta);

            result1 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.m3,
                                                                 RainfallRunoffEnums.StorageUnit.mm, value, area);
            result2 = RainfallRunoffUnitConverter.ConvertStorage(RainfallRunoffEnums.StorageUnit.mm,
                                                                 RainfallRunoffEnums.StorageUnit.m3, result1, area);
            Assert.AreEqual(value, result2, delta);
        }

        [Test]
        public void PumpCapacityConversions()
        {
            var value = 99.1;
            var area = 543.2; // m2
            double delta = 1e-3;

            // unitFrom == unitTo
            var result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                          PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                          value, area);
            var result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                          PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                          value, area);
            var result3 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                          PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                          value, area);
            var result4 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                          PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                          value, area);
            Assert.AreEqual(value, result1, delta);
            Assert.AreEqual(value, result2, delta);
            Assert.AreEqual(value, result3, delta);
            Assert.AreEqual(value, result4, delta);

            // m3_hr to other types
            result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      value, area);
            result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      value, area);
            result3 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      value, area);
            Assert.AreEqual(1.65167, result1, delta);
            Assert.AreEqual(182.437, result2, delta);
            Assert.AreEqual(0.0275278, result3, delta);

            // m3_min to other types
            result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      value, area);
            result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      value, area);
            result3 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      value, area);
            Assert.AreEqual(5946, result1, delta);
            Assert.AreEqual(10946.2445, result2, delta);
            Assert.AreEqual(1.651667, result3, delta);

            // mm_hr to other types
            result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      value, area);
            result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      value, area);
            result3 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      value, area);
            Assert.AreEqual(53.83112, result1, delta);
            Assert.AreEqual(0.89719, result2, delta);
            Assert.AreEqual(0.0149531667, result3, delta);

            // test some conversion cycles
            result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      value, area);
            result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      result1, area);
            result3 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      result2, area);
            result4 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      result3, area);
            Assert.AreEqual(value, result4, delta);

            result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      value, area);
            result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      result1, area);
            result3 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      result2, area);
            result4 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_s,
                                                                      PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      result3, area);
            Assert.AreEqual(value, result4, delta);

            result1 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      value, area);
            result2 = RainfallRunoffUnitConverter.ConvertPumpCapacity(PavedEnums.SewerPumpCapacityUnit.mm_hr,
                                                                      PavedEnums.SewerPumpCapacityUnit.m3_min,
                                                                      result1, area);
            Assert.AreEqual(value, result2, delta);
        }

        [Test]
        public void WaterUseConversions()
        {
            var value = 99.1;
            double delta = 1e-3;

            // unitFrom == unitTo
            var result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day,
                                                                      PavedEnums.WaterUseUnit.l_day,
                                                                      value);
            var result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_hr,
                                                                      PavedEnums.WaterUseUnit.l_hr,
                                                                      value);
            var result3 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.m3_s,
                                                                      PavedEnums.WaterUseUnit.m3_s,
                                                                      value);
            Assert.AreEqual(value, result1, delta);
            Assert.AreEqual(value, result2, delta);
            Assert.AreEqual(value, result3, delta);

            // l_day to other types
            result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day,
                                                                  PavedEnums.WaterUseUnit.l_hr,
                                                                  value);
            result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day,
                                                                  PavedEnums.WaterUseUnit.m3_s,
                                                                  value);
            Assert.AreEqual(4.1292, result1, delta);
            Assert.AreEqual(2.7528e-5, result2, delta);

            // l_hr to other types
            result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_hr,
                                                                  PavedEnums.WaterUseUnit.l_day,
                                                                  value);
            result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_hr,
                                                                  PavedEnums.WaterUseUnit.m3_s,
                                                                  value);
            Assert.AreEqual(2378.4, result1, delta);
            Assert.AreEqual(2.7528e-5, result2, delta);

            // m3_s to other types
            result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.m3_s,
                                                                  PavedEnums.WaterUseUnit.l_day,
                                                                  value);
            result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.m3_s,
                                                                  PavedEnums.WaterUseUnit.l_hr,
                                                                  value);
            Assert.AreEqual(8562240000, result1, delta);
            Assert.AreEqual(356760000, result2, delta);

            // test some conversion cycles
            result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day,
                                                                  PavedEnums.WaterUseUnit.l_hr,
                                                                  value);
            result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_hr,
                                                                  PavedEnums.WaterUseUnit.m3_s,
                                                                  result1);
            result3 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.m3_s,
                                                                  PavedEnums.WaterUseUnit.l_day,
                                                                  result2);
            Assert.AreEqual(value, result3, delta);

            result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.m3_s,
                                                                  PavedEnums.WaterUseUnit.l_hr,
                                                                  value);
            result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_hr,
                                                                  PavedEnums.WaterUseUnit.l_day,
                                                                  result1);
            result3 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day,
                                                                  PavedEnums.WaterUseUnit.m3_s,
                                                                  result2);
            Assert.AreEqual(value, result3, delta);

            result1 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_day,
                                                                  PavedEnums.WaterUseUnit.l_hr,
                                                                  value);
            result2 = RainfallRunoffUnitConverter.ConvertWaterUse(PavedEnums.WaterUseUnit.l_hr,
                                                                  PavedEnums.WaterUseUnit.l_day,
                                                                  result1);
            Assert.AreEqual(value, result2, delta);
        }
    }
}
