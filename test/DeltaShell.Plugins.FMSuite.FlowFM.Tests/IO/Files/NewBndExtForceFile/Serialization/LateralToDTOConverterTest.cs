using System;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Serialization
{
    [TestFixture]
    public class LateralToDTOConverterTest
    {
        [Test]
        public void Convert_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new LateralToDTOConverter();

            // Call
            void Call() => converter.Convert(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Convert_LateralDischargeTypeUndefined_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var converter = new LateralToDTOConverter();
            var feature = new Feature2D
            {
                Name = "some_name",
                Geometry = new Point(1.23, 2.34)
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = (LateralDischargeType)999;
            lateral.Data.Discharge.Constant = 3.45;

            // Call
            void Call() => converter.Convert(lateral);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Convert_LateralWithConstantDischarge()
        {
            // Setup
            var converter = new LateralToDTOConverter();
            var feature = new Feature2D
            {
                Name = "some_name",
                Geometry = new Point(1.23, 2.34)
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.Constant;
            lateral.Data.Discharge.Constant = 3.45;

            // Call
            LateralDTO lateralDTO = converter.Convert(lateral);

            // Assert
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 3.45
            };
            var expLateralDTO = new LateralDTO("some_name", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               1, new[] { 1.23 }, new[] { 2.34 }, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Convert_LateralWithTimeSeriesDischarge()
        {
            // Setup
            var converter = new LateralToDTOConverter();
            var feature = new Feature2D
            {
                Name = "some_name",
                Geometry = new Point(1.23, 2.34)
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.TimeSeries;

            // Call
            LateralDTO lateralDTO = converter.Convert(lateral);

            // Assert
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = "lateral_discharge.bc"
            };
            var expLateralDTO = new LateralDTO("some_name", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               1, new[] { 1.23 }, new[] { 2.34 }, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Convert_LateralWithRealTimeDischarge()
        {
            // Setup
            var converter = new LateralToDTOConverter();
            var feature = new Feature2D
            {
                Name = "some_name",
                Geometry = new Point(1.23, 2.34)
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.RealTime;

            // Call
            LateralDTO lateralDTO = converter.Convert(lateral);

            // Assert
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.External,
            };
            var expLateralDTO = new LateralDTO("some_name", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               1, new[] { 1.23 }, new[] { 2.34 }, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Convert_LateralWithPolygonGeometry()
        {
            // Setup
            var converter = new LateralToDTOConverter();
            var coordinates = new[] { new Coordinate(1.23, 4.56), new Coordinate(2.34, 5.67), new Coordinate(3.45, 6.78), new Coordinate(1.23, 4.56) };
            var feature = new Feature2D
            {
                Name = "some_name",
                Geometry = new Polygon(new LinearRing(coordinates))
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.Constant;
            lateral.Data.Discharge.Constant = 3.45;

            // Call
            LateralDTO lateralDTO = converter.Convert(lateral);

            // Assert
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 3.45
            };
            var expLateralDTO = new LateralDTO("some_name", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, new[] { 1.23, 2.34, 3.45 }, new[] { 4.56, 5.67, 6.78 }, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }
    }
}