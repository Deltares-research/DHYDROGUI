using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class LateralFactoryTest
    {
        [Test]
        [TestCaseSource(nameof(CreateLateral_ArgNullCases))]
        public void CreateLateral_ArgNull_ThrowsArgumentNullException(LateralDTO lateralDTO, ILateralTimeSeriesSetter lateralTimeSeriesSetter)
        {
            // Setup
            var lateralFactory = new LateralFactory();

            // Call
            void Call() => lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateLateral_LateralDTONotDischargeForcingType_ThrowsInvalidOperationException()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 1.23
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Unsupported, LateralLocationType.TwoD,
                                            null, null, null, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            void Call() => lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(Call, Throws.InvalidOperationException);
        }

        [Test]
        public void CreateLateral_DischargeModeUndefinedSteerableMode_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = (SteerableMode)999,
                ConstantValue = 1.23
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            null, null, null, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            void Call() => lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CreateLateral_LateralDTOWithMultipleCoordinates_ReturnsTheCorrectLateralWithAPolygonGeometry()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 1.23
            };
            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            4, xCoordinates, yCoordinates, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            Lateral lateral = lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(lateral.Name, Is.EqualTo("some_id"));
            Assert.That(lateral.Feature.Geometry, Is.EqualTo(new Polygon(new LinearRing(new[] { new Coordinate(1.23, 4.56), new Coordinate(2.34, 5.67), new Coordinate(3.45, 6.78), new Coordinate(1.23, 4.56) }))));
            Assert.That(lateral.Data.Discharge.Type, Is.EqualTo(LateralDischargeType.Constant));
            Assert.That(lateral.Data.Discharge.Constant, Is.EqualTo(1.23));
        }

        [Test]
        public void CreateLateral_LateralDTOWithOneCoordinate_ReturnsTheCorrectLateralWithAPointGeometry()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 1.23
            };
            var xCoordinates = new[] { 1.23 };
            var yCoordinates = new[] { 4.56 };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            1, xCoordinates, yCoordinates, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            Lateral lateral = lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(lateral.Name, Is.EqualTo("some_id"));
            Assert.That(lateral.Feature.Geometry, Is.EqualTo(new Point(new Coordinate(1.23, 4.56))));
            Assert.That(lateral.Data.Discharge.Type, Is.EqualTo(LateralDischargeType.Constant));
            Assert.That(lateral.Data.Discharge.Constant, Is.EqualTo(1.23));
        }

        [Test]
        public void CreateLateral_LateralDTOWithTimeSeriesDischarge_ReturnsTheCorrectLateralWithATimeSeriesDischarge()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = "lateral_discharge.bc"
            };
            var xCoordinates = new[] { 1.23 };
            var yCoordinates = new[] { 4.56 };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            1, xCoordinates, yCoordinates, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            Lateral lateral = lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(lateral.Name, Is.EqualTo("some_id"));
            Assert.That(lateral.Feature.Geometry, Is.EqualTo(new Point(new Coordinate(1.23, 4.56))));
            Assert.That(lateral.Data.Discharge.Type, Is.EqualTo(LateralDischargeType.TimeSeries));
            lateralTimeSeriesSetter.Received(1).SetDischargeFunction("some_id", lateral.Data.Discharge.TimeSeries);
        }

        [Test]
        public void CreateLateral_LateralDTOWithRealTimeDischarge_ReturnsTheCorrectLateralWithARealTimeDischarge()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = SteerableMode.External,
            };
            var xCoordinates = new[] { 1.23 };
            var yCoordinates = new[] { 4.56 };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            1, xCoordinates, yCoordinates, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            Lateral lateral = lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(lateral.Name, Is.EqualTo("some_id"));
            Assert.That(lateral.Feature.Geometry, Is.EqualTo(new Point(new Coordinate(1.23, 4.56))));
            Assert.That(lateral.Data.Discharge.Type, Is.EqualTo(LateralDischargeType.RealTime));
        }

        [Test]
        public void CreateLateral_LateralDTOWithConstantDischarge_ReturnsTheCorrectLateralWithAConstantDischarge()
        {
            // Setup
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 1.23
            };
            var xCoordinates = new[] { 1.23 };
            var yCoordinates = new[] { 4.56 };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            1, xCoordinates, yCoordinates, discharge);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            var lateralFactory = new LateralFactory();

            // Call
            Lateral lateral = lateralFactory.CreateLateral(lateralDTO, lateralTimeSeriesSetter);

            // Assert
            Assert.That(lateral.Name, Is.EqualTo("some_id"));
            Assert.That(lateral.Feature.Geometry, Is.EqualTo(new Point(new Coordinate(1.23, 4.56))));
            Assert.That(lateral.Data.Discharge.Type, Is.EqualTo(LateralDischargeType.Constant));
            Assert.That(lateral.Data.Discharge.Constant, Is.EqualTo(1.23));
        }

        private static IEnumerable<TestCaseData> CreateLateral_ArgNullCases()
        {
            var lateralDTO = new LateralDTO(null, null, LateralForcingType.None, LateralLocationType.None, null, null, null, null);
            var lateralTimeSeriesSetter = Substitute.For<ILateralTimeSeriesSetter>();

            yield return new TestCaseData(null, lateralTimeSeriesSetter);
            yield return new TestCaseData(lateralDTO, null);
        }
    }
}