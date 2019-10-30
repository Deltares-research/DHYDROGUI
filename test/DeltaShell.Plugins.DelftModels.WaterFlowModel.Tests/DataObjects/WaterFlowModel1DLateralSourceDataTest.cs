using System;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.DataObjects
{
    [TestFixture]
    public class WaterFlowModel1DLateralSourceDataTest
    {
        [Test]
        public void TestWaterFlowModel1DLateralSourceDataGeometryOfDiffuseSource()
        {
            var lateralSource = new LateralSource
                {
                    Geometry = new LineString(new []{new Coordinate(0,0),new Coordinate(0,100) }),
                    Length = 100
                };

            var lateralSourceData = new Model1DLateralSourceData { Feature = lateralSource };

            Assert.IsTrue(lateralSourceData.Geometry is IPoint);
            Assert.AreEqual(50, ((IPoint)lateralSourceData.Geometry).Y);
        }

        [Test]
        public void Clone()
        {
            var lateralSourceData = new Model1DLateralSourceData
            {
                Feature = new LateralSource(),
                DataType = Model1DLateralDataType.FlowTimeSeries,
                Flow = 5,
                UseSalt = true,
                SaltLateralDischargeType = SaltLateralDischargeType.ConcentrationTimeSeries,
                SaltConcentrationDischargeConstant = 1.2223,
                SaltMassDischargeConstant = 2.3334,
                UseTemperature = true,
                TemperatureLateralDischargeType = TemperatureLateralDischargeType.TimeDependent,
                TemperatureConstant = 3.4445    
            };
            
            lateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)] = 4.5556;
            lateralSourceData.SaltMassTimeSeries[new DateTime(2000, 1, 1)] = 5.6667;
            lateralSourceData.TemperatureTimeSeries[new DateTime(2000, 1, 1)] = 6.7778;
            
            var clonedLateralSourceData = (Model1DLateralSourceData)lateralSourceData.Clone();

            Assert.AreEqual(lateralSourceData.DataType, clonedLateralSourceData.DataType);
            Assert.AreEqual(lateralSourceData.Flow, clonedLateralSourceData.Flow);

            Assert.AreEqual(lateralSourceData.UseSalt, clonedLateralSourceData.UseSalt);
            Assert.AreEqual(lateralSourceData.SaltLateralDischargeType, clonedLateralSourceData.SaltLateralDischargeType);
            Assert.AreEqual(lateralSourceData.SaltConcentrationDischargeConstant, clonedLateralSourceData.SaltConcentrationDischargeConstant);
            Assert.AreEqual(lateralSourceData.SaltMassDischargeConstant, clonedLateralSourceData.SaltMassDischargeConstant);
            Assert.AreEqual(lateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)], clonedLateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)]);
            Assert.AreEqual(lateralSourceData.SaltMassTimeSeries[new DateTime(2000, 1, 1)], clonedLateralSourceData.SaltMassTimeSeries[new DateTime(2000, 1, 1)]);

            Assert.AreEqual(lateralSourceData.UseTemperature, clonedLateralSourceData.UseTemperature);
            Assert.AreEqual(lateralSourceData.TemperatureLateralDischargeType, clonedLateralSourceData.TemperatureLateralDischargeType);
            Assert.AreEqual(lateralSourceData.TemperatureConstant, clonedLateralSourceData.TemperatureConstant);
            Assert.AreEqual(lateralSourceData.TemperatureTimeSeries[new DateTime(2000, 1, 1)], clonedLateralSourceData.TemperatureTimeSeries[new DateTime(2000, 1, 1)]);
        }
    }
}
