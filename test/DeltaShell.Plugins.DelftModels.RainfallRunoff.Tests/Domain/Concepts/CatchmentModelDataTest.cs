using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts
{
    [TestFixture]
    public class CatchmentModelDataTest
    {
        public static IEnumerable<TestCaseData> CatchmentTypes
        {
            get
            {
                CatchmentModelData CreateHbvData(Catchment c) => new HbvData(c);
                CatchmentModelData CreateOpenWaterData(Catchment c) => new OpenWaterData(c);
                CatchmentModelData CreatePavedData(Catchment c) => new PavedData(c);
                CatchmentModelData CreateSacramentoData(Catchment c) => new SacramentoData(c);

                yield return new TestCaseData(CatchmentType.Hbv, (Func<Catchment, CatchmentModelData>)CreateHbvData).SetName("Hbv data");
                yield return new TestCaseData(CatchmentType.OpenWater, (Func<Catchment, CatchmentModelData>)CreateOpenWaterData).SetName("OpenWater data");
                yield return new TestCaseData(CatchmentType.Paved, (Func<Catchment, CatchmentModelData>)CreatePavedData).SetName("Paved data");
                yield return new TestCaseData(CatchmentType.Sacramento, (Func<Catchment, CatchmentModelData>)CreateSacramentoData).SetName("Sacramento data");
            }
        }

        [Test]
        [TestCaseSource(nameof(CatchmentTypes))]
        public void GivenCatchmentModelData_ChangingComputationArea_ShouldShouldChangeDependentGeometry(CatchmentType catchmentType, Func<Catchment, CatchmentModelData> createCatchmentDataFunc)
        {
            //Arrange
            var catchment = new Catchment
            {
                CatchmentType = catchmentType
            };

            CatchmentModelData catchmentModelData = createCatchmentDataFunc(catchment);
            
            // Act & Assert
            catchmentModelData.CalculationArea = 2000;

            Assert.IsAssignableFrom<Polygon>(catchment.Geometry);
            Assert.AreEqual(2000, catchment.GeometryArea, 1e-7);
            Assert.IsTrue(catchment.IsGeometryDerivedFromAreaSize);

            catchmentModelData.CalculationArea = 3000;

            Assert.IsAssignableFrom<Polygon>(catchment.Geometry);
            Assert.AreEqual(3000, catchment.GeometryArea, 1e-7);
            Assert.IsTrue(catchment.IsGeometryDerivedFromAreaSize);
        }


        [Test]
        [TestCaseSource(nameof(CatchmentTypes))]
        public void GivenCatchmentModelData_ChangingComputationArea_ShouldNotChangeIndependentGeometry(CatchmentType catchmentType, Func<Catchment, CatchmentModelData> createCatchmentDataFunc)
        {
            //Arrange
            var catchment = new Catchment
            {
                CatchmentType = catchmentType,
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(0,0),
                    new Coordinate(10,0),
                    new Coordinate(10,10),
                    new Coordinate(0,10),
                    new Coordinate(0,0)
                }))
            };

            CatchmentModelData catchmentModelData = createCatchmentDataFunc(catchment);
            var geometryArea = catchment.GeometryArea;

            // Act & Assert
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize);
            Assert.AreEqual(100, geometryArea);
            Assert.AreEqual(100, catchmentModelData.CalculationArea);

            catchmentModelData.CalculationArea = 2000;

            Assert.AreEqual(100, catchment.GeometryArea, 1e-7);
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize);
            Assert.AreEqual(2000, catchmentModelData.CalculationArea);

            catchmentModelData.CalculationArea = 3000;

            Assert.AreEqual(100, catchment.GeometryArea, 1e-7);
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize);
            Assert.AreEqual(3000,catchmentModelData.CalculationArea);
        }
    }
}