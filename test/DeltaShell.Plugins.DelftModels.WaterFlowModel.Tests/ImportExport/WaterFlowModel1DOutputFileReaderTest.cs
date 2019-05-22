using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DOutputFileReaderTest
    {
        private const double Delta = 0.0000000001;

        [TestCase("gridpoints.nc", 27, "branch1_0.000", "branch2_150.000", 5,
                  "water_level", "Gridpoint water level", "m", "water_total_width", "Gridpoint water total width", "m")]

        [TestCase("laterals.nc", 2, "LateralSource1", "LateralSource2", 4,
                  "actual_lateral_discharge", "Actual lateral discharge", "m3 s-1", "water_level_at_lateral", "Water level at lateral", "m")]

        [TestCase("observations.nc", 3, "observationPoint1", "observationPoint3", 4,
                  "water_level", "Observed water level", "m", "velocity", "Observed water velocity", "m/s")]

        [TestCase("reachsegments.nc", 25, "branch1_1", "branch2_15", 6,
                  "water_discharge", "Reach segment water discharge", "m3/s", "water_chezy_fp2", "Reach segment chezy value in flood plain 2", "m(1/2)/s")]

        public void TestReadMetaData(string fileName, int numLocations, string expectedFirstLocationId, string expectedLastLocationId, int numTimeDependentVariables,
                                     string firstVarName, string firstVarLongName, string firstVarUnit, string lastVarName, string lastVarLongName, string lastVarUnit)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + fileName);
            var metaData = WaterFlowModel1DOutputFileReader.ReadMetaData(filePath);

            // check times
            Assert.AreEqual(2, metaData.NumTimes);

            var firstTime = metaData.Times.First();
            Assert.AreEqual("2016-08-10 10:40:00", firstTime.ToString(WaterFlowModel1DOutputFileConstants.DateTimeFormat));

            var lastTime = metaData.Times.Last();
            Assert.AreEqual("2016-08-10 10:40:30", lastTime.ToString(WaterFlowModel1DOutputFileConstants.DateTimeFormat));

            // check locations (ids are required, others are optional)
            Assert.AreEqual(numLocations, metaData.NumLocations);

            var firstLocationId = metaData.Locations.First().Id;
            Assert.AreEqual(expectedFirstLocationId, firstLocationId);

            var lastLocationId = metaData.Locations.Last().Id;
            Assert.AreEqual(expectedLastLocationId, lastLocationId);

            // check time dependent variables
            var variables = metaData.TimeDependentVariables;
            Assert.AreEqual(numTimeDependentVariables, variables.Count);

            var firstVariable = variables.First();
            Assert.AreEqual(firstVarName, firstVariable.Name);
            Assert.AreEqual(firstVarLongName, firstVariable.LongName);
            Assert.AreEqual(firstVarUnit, firstVariable.Unit);

            var lastVariable = variables.Last();
            Assert.AreEqual(lastVarName, lastVariable.Name);
            Assert.AreEqual(lastVarLongName, lastVariable.LongName);
            Assert.AreEqual(lastVarUnit, lastVariable.Unit);
        }

        [TestCase("gridpoints.nc", 0.1, 0.1, 0.1459562376, 0.1000000146)]
        [TestCase("laterals.nc", 0.0, 0.0, 0.0, 0.0)]
        [TestCase("observations.nc", 0.1, 0.1, 0.1459562376, 0.1459562376)]
        [TestCase("reachsegments.nc", 1.0, 0.1, 1.0000000733, 0.0995589744)]

        public void TestGetTimeSeriesData(string fileName, double firstTimeStepFirstLocationValue, double firstTimeStepLastLocationValue,
                                        double lastTimeStepFirstLocationValue, double lastTimeStepLastLocationValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + fileName);
            var metaData = WaterFlowModel1DOutputFileReader.ReadMetaData(filePath);

            var variableName = metaData.TimeDependentVariables.First().Name;
            var allTimeData = WaterFlowModel1DOutputFileReader.GetAllVariableData(filePath, variableName, metaData);

            Assert.AreEqual(metaData.NumTimes, allTimeData.GetLength(0));
            Assert.AreEqual(metaData.NumLocations, allTimeData.GetLength(1));

            Assert.AreEqual(firstTimeStepFirstLocationValue, allTimeData[0, 0], Delta);
            Assert.AreEqual(firstTimeStepLastLocationValue, allTimeData[0, metaData.NumLocations - 1], Delta);

            Assert.AreEqual(lastTimeStepFirstLocationValue, allTimeData[1, 0], Delta);
            Assert.AreEqual(lastTimeStepLastLocationValue, allTimeData[1, metaData.NumLocations - 1], Delta);
        }

        
        [TestCase("gridpoints.nc", 0.1, 0.1000002364)]
        [TestCase("laterals.nc", 0.0, 0.0)]
        [TestCase("observations.nc", 0.1, 0.1459562376)]
        [TestCase("reachsegments.nc", 1.0, 0.0995589744)]

        public void TestGetTimeStepData(string fileName, double firstExpectedValue, double secondExpectedValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + fileName);
            var metaData = WaterFlowModel1DOutputFileReader.ReadMetaData(filePath);
            var timeDependentVariableName = metaData.TimeDependentVariables.First().Name;

            var numTimesToRead = metaData.NumTimes/2;
            var numLocationsToRead = metaData.NumLocations/2;

            var origin = new[] { 0, 0 };
            var shape = new[] { numTimesToRead, numLocationsToRead };

            var firstHalfOfVariableData = WaterFlowModel1DOutputFileReader.GetSelectionOfVariableData(filePath, timeDependentVariableName, origin, shape);

            Assert.AreEqual(numTimesToRead * numLocationsToRead, firstHalfOfVariableData.Count);
            Assert.AreEqual(firstExpectedValue, firstHalfOfVariableData.First(), Delta);

            origin = new[] { numTimesToRead, numLocationsToRead };
            shape = new[] { numTimesToRead, numLocationsToRead };

            var secondHalfOfVariableData = WaterFlowModel1DOutputFileReader.GetSelectionOfVariableData(filePath, timeDependentVariableName, origin, shape);

            Assert.AreEqual(numTimesToRead * numLocationsToRead, secondHalfOfVariableData.Count);
            Assert.AreEqual(secondExpectedValue, secondHalfOfVariableData.Last(), Delta);
        }

        [TestCase("observations.nc", "water_temperature")]
        [TestCase("gridpoints.nc", "water_temperature")]
        [TestCase("gridpoints.nc", "effective_background_radiation")]
        [TestCase("gridpoints.nc", "forced_heatloss_convection")]
        [TestCase("gridpoints.nc", "forced_heatloss_evaporation")]
        [TestCase("gridpoints.nc", "free_heatloss_convection")]
        [TestCase("gridpoints.nc", "free_heatloss_evaporation")]
        [TestCase("gridpoints.nc", "heat_loss_convection")]
        [TestCase("gridpoints.nc", "heatloss_evaporation")]
        [TestCase("gridpoints.nc", "netto_solar_radiation")]
        [TestCase("gridpoints.nc", "rad_flux_clear_sky")]
        [TestCase("gridpoints.nc", "total_heat_flux")]
        public void TestGetVariableValuesFromOutputFile(string fileName, string variableName)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/temperature/" + fileName);
            var metaData = WaterFlowModel1DOutputFileReader.ReadMetaData(filePath);

            // check time dependent variables
            var variables = metaData.TimeDependentVariables;

            var firstVariable = variables.FirstOrDefault( v => v.Name.Equals(variableName));
            Assert.NotNull(firstVariable);
        }
    }
}
