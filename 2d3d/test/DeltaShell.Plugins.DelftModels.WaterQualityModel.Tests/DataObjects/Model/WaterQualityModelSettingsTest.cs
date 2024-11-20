using System;
using System.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.Model
{
    [TestFixture]
    public class WaterQualityModelSettingsTest
    {
        [Test]
        public void TestCreate()
        {
            var waterQualityModel1DSettings = new WaterQualityModelSettings();

            Assert.AreEqual(DateTime.Now.Date, waterQualityModel1DSettings.HisStartTime);
            Assert.AreEqual(waterQualityModel1DSettings.HisStartTime.AddHours(24), waterQualityModel1DSettings.HisStopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), waterQualityModel1DSettings.HisTimeStep);
            Assert.AreEqual(DateTime.Now.Date, waterQualityModel1DSettings.MapStartTime);
            Assert.AreEqual(waterQualityModel1DSettings.MapStartTime.AddHours(24), waterQualityModel1DSettings.MapStopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), waterQualityModel1DSettings.MapTimeStep);
            Assert.AreEqual(DateTime.Now.Date, waterQualityModel1DSettings.BalanceStartTime);
            Assert.AreEqual(waterQualityModel1DSettings.BalanceStartTime.AddHours(24), waterQualityModel1DSettings.BalanceStopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), waterQualityModel1DSettings.BalanceTimeStep);
            Assert.AreEqual(NumericalScheme.Scheme15, waterQualityModel1DSettings.NumericalScheme);
            Assert.IsTrue(waterQualityModel1DSettings.NoDispersionIfFlowIsZero);
            Assert.IsTrue(waterQualityModel1DSettings.NoDispersionOverOpenBoundaries);
            Assert.IsFalse(waterQualityModel1DSettings.UseFirstOrder);
            Assert.IsTrue(waterQualityModel1DSettings.LumpProcesses);
            Assert.IsTrue(waterQualityModel1DSettings.LumpTransport);
            Assert.IsTrue(waterQualityModel1DSettings.LumpLoads);
            Assert.IsTrue(waterQualityModel1DSettings.SuppressSpace);
            Assert.IsTrue(waterQualityModel1DSettings.SuppressTime);
            Assert.IsTrue(waterQualityModel1DSettings.ProcessesActive);
            Assert.AreEqual(MonitoringOutputLevel.None, waterQualityModel1DSettings.MonitoringOutputLevel);
            Assert.IsTrue(waterQualityModel1DSettings.CorrectForEvaporation);
            Assert.IsTrue(waterQualityModel1DSettings.ClosureErrorCorrection);
            Assert.AreEqual(2, waterQualityModel1DSettings.NrOfThreads);
            Assert.AreEqual(0.001, waterQualityModel1DSettings.DryCellThreshold);
            Assert.AreEqual(100, waterQualityModel1DSettings.IterationMaximum);
            Assert.AreEqual(1e-7, waterQualityModel1DSettings.Tolerance);
            Assert.IsFalse(waterQualityModel1DSettings.WriteIterationReport);
        }

        [Test]
        public void TestClone()
        {
            var waterQualityModel1DSettings = new WaterQualityModelSettings
            {
                HisStartTime = new DateTime(2010, 1, 1),
                HisStopTime = new DateTime(2010, 1, 2),
                HisTimeStep = new TimeSpan(1, 1, 1),
                MapStartTime = new DateTime(2011, 1, 1),
                MapStopTime = new DateTime(2011, 1, 2),
                MapTimeStep = new TimeSpan(2, 2, 2),
                BalanceStartTime = new DateTime(2012, 1, 1),
                BalanceStopTime = new DateTime(2012, 1, 2),
                BalanceTimeStep = new TimeSpan(3, 3, 3),
                NumericalScheme = NumericalScheme.Scheme1,
                NoDispersionIfFlowIsZero = true,
                NoDispersionOverOpenBoundaries = true,
                UseFirstOrder = false,
                UseForesterFilter = true,
                UseAnticreepFilter = true,
                Balance = true,
                LumpProcesses = false,
                LumpTransport = false,
                LumpLoads = false,
                SuppressSpace = false,
                SuppressTime = false,
                BalanceUnit = BalanceUnit.Gram,
                ProcessesActive = true,
                MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas,
                CorrectForEvaporation = false,
                Id = 2,
                OutputDirectory = "output_directory",
                WorkingOutputDirectory = "working_output_directory"
            };

            var waterQualityModel1DSettingsClone = waterQualityModel1DSettings.Clone() as WaterQualityModelSettings;

            Assert.IsNotNull(waterQualityModel1DSettingsClone);
            Assert.AreNotSame(waterQualityModel1DSettings, waterQualityModel1DSettingsClone);
            Assert.AreEqual(waterQualityModel1DSettings.WorkDirectory, waterQualityModel1DSettingsClone.WorkDirectory);
            Assert.AreEqual(waterQualityModel1DSettings.HisStartTime, waterQualityModel1DSettingsClone.HisStartTime);
            Assert.AreEqual(waterQualityModel1DSettings.HisStopTime, waterQualityModel1DSettingsClone.HisStopTime);
            Assert.AreEqual(waterQualityModel1DSettings.HisTimeStep, waterQualityModel1DSettingsClone.HisTimeStep);
            Assert.AreEqual(waterQualityModel1DSettings.MapStartTime, waterQualityModel1DSettingsClone.MapStartTime);
            Assert.AreEqual(waterQualityModel1DSettings.MapStopTime, waterQualityModel1DSettingsClone.MapStopTime);
            Assert.AreEqual(waterQualityModel1DSettings.MapTimeStep, waterQualityModel1DSettingsClone.MapTimeStep);
            Assert.AreEqual(waterQualityModel1DSettings.BalanceStartTime, waterQualityModel1DSettingsClone.BalanceStartTime);
            Assert.AreEqual(waterQualityModel1DSettings.BalanceStopTime, waterQualityModel1DSettingsClone.BalanceStopTime);
            Assert.AreEqual(waterQualityModel1DSettings.BalanceTimeStep, waterQualityModel1DSettingsClone.BalanceTimeStep);
            Assert.AreEqual(waterQualityModel1DSettings.NumericalScheme, waterQualityModel1DSettingsClone.NumericalScheme);
            Assert.AreEqual(waterQualityModel1DSettings.NoDispersionIfFlowIsZero, waterQualityModel1DSettingsClone.NoDispersionIfFlowIsZero);
            Assert.AreEqual(waterQualityModel1DSettings.NoDispersionOverOpenBoundaries, waterQualityModel1DSettingsClone.NoDispersionOverOpenBoundaries);
            Assert.AreEqual(waterQualityModel1DSettings.UseFirstOrder, waterQualityModel1DSettingsClone.UseFirstOrder);
            Assert.AreEqual(waterQualityModel1DSettings.UseForesterFilter, waterQualityModel1DSettingsClone.UseForesterFilter);
            Assert.AreEqual(waterQualityModel1DSettings.UseAnticreepFilter, waterQualityModel1DSettingsClone.UseAnticreepFilter);
            Assert.AreEqual(waterQualityModel1DSettings.Balance, waterQualityModel1DSettingsClone.Balance);
            Assert.AreEqual(waterQualityModel1DSettings.LumpProcesses, waterQualityModel1DSettingsClone.LumpProcesses);
            Assert.AreEqual(waterQualityModel1DSettings.LumpTransport, waterQualityModel1DSettingsClone.LumpTransport);
            Assert.AreEqual(waterQualityModel1DSettings.LumpLoads, waterQualityModel1DSettingsClone.LumpLoads);
            Assert.AreEqual(waterQualityModel1DSettings.SuppressSpace, waterQualityModel1DSettingsClone.SuppressSpace);
            Assert.AreEqual(waterQualityModel1DSettings.SuppressTime, waterQualityModel1DSettingsClone.SuppressTime);
            Assert.AreEqual(waterQualityModel1DSettings.BalanceUnit, waterQualityModel1DSettingsClone.BalanceUnit);
            Assert.AreEqual(waterQualityModel1DSettings.ProcessesActive, waterQualityModel1DSettingsClone.ProcessesActive);
            Assert.AreEqual(waterQualityModel1DSettings.MonitoringOutputLevel, waterQualityModel1DSettingsClone.MonitoringOutputLevel);
            Assert.AreEqual(waterQualityModel1DSettings.CorrectForEvaporation, waterQualityModel1DSettingsClone.CorrectForEvaporation);
            Assert.AreEqual(0, waterQualityModel1DSettingsClone.Id);
            Assert.AreEqual(waterQualityModel1DSettings.OutputDirectory, waterQualityModel1DSettingsClone.OutputDirectory);
            Assert.AreEqual(waterQualityModel1DSettings.WorkingOutputDirectory, waterQualityModel1DSettingsClone.WorkingOutputDirectory);
        }

        [Test]
        public void WorkDirectoryShouldBeCreatedFromTheCorrespondingFunctionTest()
        {
            var modelSettings = new WaterQualityModelSettings
            {
                WorkingDirectoryPathFuncWithModelName =
                    () => Path.Combine(Path.GetTempPath(), "test1", "test2", "test3")
            };

            Assert.AreEqual(Path.Combine(Path.GetTempPath(), "test1", "test2", "test3"), modelSettings.WorkDirectory);
        }

        [Test]
        public void WorkDirectoryFunctionDefaultTest()
        {
            var modelSettings = new WaterQualityModelSettings();

            Assert.AreEqual(Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory", "Water_Quality"), modelSettings.WorkDirectory);
        }
    }
}