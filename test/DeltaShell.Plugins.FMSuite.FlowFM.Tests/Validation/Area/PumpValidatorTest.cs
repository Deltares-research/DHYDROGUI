using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class PumpValidatorTest
    {
        [Test]
        public void GivenFmModelWithAValidPump_WhenValidatingPumps_ThenNoValidationWarningsAreReturned()
        {
            var fmModel = GenerateFmModelWithARegularGrid();
            var pump = new Pump2D
            {
                Name = "myPump",
                Geometry = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) })
            };
            fmModel.Area.Pumps.Add(pump);
            var listOfPumps = new List<Pump2D> { pump };

            var validationIssues = PumpValidator.ValidatePumps(fmModel, listOfPumps);

            var validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(0));
        }

        [Test]
        public void
            GivenFmModelWithAPumpThatDoNotIntersectWithModelGrid_WhenValidatingPumps_ThenValidationWarningIsReturned()
        {
            var fmModel = GenerateFmModelWithARegularGrid();
            var pump = new Pump2D
            {
                Name = "myPump",
                // Pump geometry is far outside of grid extent
                Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) })
            };
            fmModel.Area.Pumps.Add(pump);
            var listOfPumps = new List<Pump2D> { pump };

            var validationIssues = PumpValidator.ValidatePumps(fmModel, listOfPumps);

            var validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage = $"pump '{pump.Name}' not within grid extent";
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenFmModelWithAPumpWithANegativeCapacity_WhenValidatingPumps_ThenValidationErrorIsReturned()
        {
            var fmModel = GenerateFmModelWithARegularGrid();
            var pump = new Pump2D
            {
                Name = "myPump",
                Geometry = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) }),
                Capacity = -2.0
            };
            fmModel.Area.Pumps.Add(pump);
            var listOfPumps = new List<Pump2D> { pump };

            var validationIssues = PumpValidator.ValidatePumps(fmModel, listOfPumps);

            var validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage =
                $"pump '{pump.Name}': Capacity must be greater than or equal to 0.";
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenFmModelWithAStartSuctionLevelThatIsSmallerThen_WhenValidatingPumps_ThenValidationErrorIsReturned()
        {
            var fmModel = GenerateFmModelWithARegularGrid();
            var pump = new Pump2D
            {
                Name = "myPump",
                Geometry = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) }),
                ControlDirection = PumpControlDirection.SuctionSideControl,
                StartSuction = 1.0,
                StopSuction = 2.0
            };
            fmModel.Area.Pumps.Add(pump);
            var listOfPumps = new List<Pump2D> { pump };

            var validationIssues = PumpValidator.ValidatePumps(fmModel, listOfPumps);

            var validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage =
                $"pump '{pump.Name}': Suction start level must be greater than or equal to suction stop level.";
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }

        private static WaterFlowFMModel GenerateFmModelWithARegularGrid()
        {
            var fmModel = new WaterFlowFMModel
            {
                Grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 2, 2)
            };

            return fmModel;
        }
    }
}
