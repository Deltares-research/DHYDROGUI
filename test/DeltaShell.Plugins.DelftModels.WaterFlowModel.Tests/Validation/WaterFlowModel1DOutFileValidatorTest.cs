using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Validation
{
    [TestFixture]
    public class WaterFlowModel1DOutFileValidatorTest
    {
        #region TestFileGeneration
        
        [Test]
        [Ignore]
        public void CreateTestFiles()
        {
            // Test used to generate test-data, 
            // test-data has now been checked in - this is only here for future adaptation

            var testDirectory = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/");
            if (!Directory.Exists(testDirectory)) Directory.CreateDirectory(testDirectory);

            CreateDummyTestFile_MissingTimeDimension(testDirectory);
            CreateDummyTestFile_MissingTimeVariable(testDirectory);
            CreateDummyTestFile_TimeVariableNoUnit(testDirectory);
            CreateDummyTestFile_TimeVariableUnitFormat1(testDirectory);
            CreateDummyTestFile_TimeVariableUnitFormat2(testDirectory);
            CreateDummyTestFile_MissingLocationIdVariable(testDirectory);
            CreateDummyTestFile_MultipleLocationIdVariables(testDirectory);
            CreateDummyTestFile_NoTimeDependentVariables(testDirectory);
        }

        private void CreateDummyTestFile_MissingTimeDimension(string testDirectory)
        {
            // File has a valid location variable, and some time-dependent variables, but no time variable

            var filePath = Path.Combine(testDirectory, "invalidFile_NoTimeDimension.nc");
            if (File.Exists(filePath)) File.Delete(filePath);

            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension("InvalidName", 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddValidTimeVariable(netCdfFile, timeDimension);
            AddLocationIdVariable(netCdfFile, "location_id", locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }

        private void CreateDummyTestFile_MissingTimeVariable(string testDirectory)
        {
            // File has a valid location variable, and some time-dependent variables, but no time variable

            var filePath = Path.Combine(testDirectory, "invalidFile_NoTimeVariable.nc");
            if (File.Exists(filePath)) File.Delete(filePath);

            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddLocationIdVariable(netCdfFile, "location_id", locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }
        
        private void CreateDummyTestFile_TimeVariableNoUnit(string testDirectory)
        {
            // File has a valid location variable, and some time-dependent variables, and a time variable
            // but time variable has no unit attribute

            var filePath = Path.Combine(testDirectory, "invalidFile_TimeVariableNoUnit.nc");
            if (File.Exists(filePath)) File.Delete(filePath);

            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddInvalidTimeVariable_NoUnits(netCdfFile, timeDimension);
            AddLocationIdVariable(netCdfFile, "location_id", locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }

        private void CreateDummyTestFile_TimeVariableUnitFormat1(string testDirectory)
        {
            // File has a valid location variable, and some time-dependent variables, and a time variable
            // but time variable format is not 'seconds since'

            var filePath = Path.Combine(testDirectory, "invalidFile_TimeVariableUnitFormat1.nc");
            if (File.Exists(filePath)) File.Delete(filePath);

            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddInvalidTimeVariable_BadFormat1(netCdfFile, timeDimension);
            AddLocationIdVariable(netCdfFile, "location_id", locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }

        private void CreateDummyTestFile_TimeVariableUnitFormat2(string testDirectory)
        {
            // File has a valid location variable, and some time dependent-variables, and a time variable
            // but time variable format is not 'yyyy-MM-dd HH:mm:ss'

            var filePath = Path.Combine(testDirectory, "invalidFile_TimeVariableUnitFormat2.nc");
            if (File.Exists(filePath)) File.Delete(filePath);
            
            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddInvalidTimeVariable_BadFormat2(netCdfFile, timeDimension);
            AddLocationIdVariable(netCdfFile, "location_id", locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }

        private void CreateDummyTestFile_MissingLocationIdVariable(string testDirectory)
        {
            // File has some time dependent variables, and a valid time variable, but no location variable

            var filePath = Path.Combine(testDirectory, "invalidFile_NoLocationIdVariable.nc");
            if (File.Exists(filePath)) File.Delete(filePath);

            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddValidTimeVariable(netCdfFile, timeDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }

        private void CreateDummyTestFile_MultipleLocationIdVariables(string testDirectory)
        {
            // File has some time dependent variables, and a valid time variable
            // but multiple potential location variables

            var filePath = Path.Combine(testDirectory, "invalidFile_MultipleLocationIdVariables.nc");
            if (File.Exists(filePath)) File.Delete(filePath);
            
            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddValidTimeVariable(netCdfFile, timeDimension);
            AddLocationIdVariable(netCdfFile, "gridpoints_id", locationDimension);
            AddLocationIdVariable(netCdfFile, "observations_id", locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_level", timeDimension, locationDimension);
            AddTimeDependentVariable(netCdfFile, "water_depth", timeDimension, locationDimension);
            netCdfFile.Close();
        }

        private void CreateDummyTestFile_NoTimeDependentVariables(string testDirectory)
        {
            // File has a valid location variable, and a valid time variable
            // but no time dependent variables

            var filePath = Path.Combine(testDirectory, "NoTimeDependentVariables.nc");
            if (File.Exists(filePath)) File.Delete(filePath);
            
            var netCdfFile = NetCdfFile.CreateNew(filePath);
            var timeDimension = netCdfFile.AddDimension(WaterFlowModel1DOutputFileConstants.DimensionKeys.Time, 5);
            var locationDimension = netCdfFile.AddDimension("Locations", 10);
            AddValidTimeVariable(netCdfFile, timeDimension);
            AddLocationIdVariable(netCdfFile, "location_id", locationDimension);
            netCdfFile.Close();
        }

        #region HelperMethods

        private void AddValidTimeVariable(NetCdfFile netCdfFile, NetCdfDimension timeDimension)
        {
            var timeVariable = netCdfFile.AddVariable("time", typeof(double), new[] { timeDimension });
            netCdfFile.AddAttribute(timeVariable, new NetCdfAttribute("units", "seconds since 2001-01-01 12:30:00"));
        }

        private void AddInvalidTimeVariable_NoUnits(NetCdfFile netCdfFile, NetCdfDimension timeDimension)
        {
            netCdfFile.AddVariable("time", typeof(double), new[] { timeDimension });
        }

        private void AddInvalidTimeVariable_BadFormat1(NetCdfFile netCdfFile, NetCdfDimension timeDimension)
        {
            var timeVariable = netCdfFile.AddVariable("time", typeof(double), new[] { timeDimension });
            netCdfFile.AddAttribute(timeVariable, new NetCdfAttribute("units", "minutes since 2001-01-01 12:30:00"));
        }

        private void AddInvalidTimeVariable_BadFormat2(NetCdfFile netCdfFile, NetCdfDimension timeDimension)
        {
            var timeVariable = netCdfFile.AddVariable("time", typeof(double), new[] { timeDimension });
            netCdfFile.AddAttribute(timeVariable, new NetCdfAttribute("units", "seconds since 01/01/2001 12:30:00"));
        }
        
        private void AddLocationIdVariable(NetCdfFile netCdfFile, string variableName, NetCdfDimension locationDimension)
        {
            var locationVariable = netCdfFile.AddVariable(variableName, typeof(double), new[] { locationDimension });
            netCdfFile.AddAttribute(locationVariable, new NetCdfAttribute("cf_role", "timeseries_id"));
        }

        private void AddTimeDependentVariable(NetCdfFile netCdfFile, string variableName, NetCdfDimension timeDimension, NetCdfDimension locationDimension)
        {
            netCdfFile.AddVariable(variableName, typeof(double), new[] { locationDimension, timeDimension });
        }
        
        #endregion HelperMethods

        #endregion TestFileGeneration

        [TestCase("gridpoints.nc")]
        [TestCase("laterals.nc")]
        [TestCase("observations.nc")]
        [TestCase("reachsegments.nc")]

        public void TestValidationPassesForTestFiles(string fileName)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + fileName);
            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);

            Assert.AreEqual(0, validationReport.ErrorCount, "Unexpected validation errors");
            Assert.AreEqual(0, validationReport.WarningCount, "Unexpected validation warnings");
        }

        [Test]
        public void TestValidationFailsWhenFileDoesNotExist()
        {
            var filePath = "fileDoesNotExist.nc";
            Assert.IsFalse(File.Exists(filePath));
            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationFailsWhenTimeDimensionDoesNotExist()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_NoTimeDimension.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationFailsWhenTimeVariableDoesNotExist()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_NoTimeVariable.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationFailsWhenTimeVariableHasNoUnit()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_TimeVariableNoUnit.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationFailsWhenTimeVariableContainsInvalidUnitFormatSecondsSince()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_TimeVariableUnitFormat1.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationFailsWhenTimeVariableContainsInvalidUnitFormatDateFormat()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_TimeVariableUnitFormat2.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0);
        }

        [Test]
        public void TestValidationFailsWhenLocationIdVariableDoesNotExist()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_NoLocationIdVariable.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationFailsWhenMoreThan1LocationIdVariableExists()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "invalidFile_MultipleLocationIdVariables.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount > 0, "Number of validation errors should be greater than zero");
        }

        [Test]
        public void TestValidationWarnsWhenNoTimeDependentVariablesExist()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/invalid_output/" + "NoTimeDependentVariables.nc");
            Assert.IsTrue(File.Exists(filePath), string.Format("Test file not found: {0}", filePath));

            var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(filePath);
            Assert.IsTrue(validationReport.ErrorCount == 0, "Unexpected validation errors");
            Assert.IsTrue(validationReport.WarningCount > 0, "Number of validation warnings should be greater than zero");
        }
    }
}
