using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRRPavedImporterTest
    {
        [Test]
        public void ReadPavedDryWeatherFlow_ValidComputationOption_CorrectlySetsWaterUse()
        {
            // Arrange
            var dryWeatherFlowsById = new Dictionary<string, SobekRRDryWeatherFlow>
            {
                { "validDryWeatherFlowId", new SobekRRDryWeatherFlow { ComputationOption = DWAComputationOption.ConstantDWAPerHour, WaterUsePerHourForConstant = 1, WaterCapacityPerHour = Enumerable.Range(0, 24).Select(i => (double)i).ToArray()} }
            };

            var sobekPaved = new SobekRRPaved { DryWeatherFlowId = "validDryWeatherFlowId" };
            var pavedData = new PavedData(new Catchment());
            // Arrange a mock logger
            var log = Substitute.For<ILog>();

            // Act
            SobekRRPavedDryWeatherFlowUpdater.UpdatePavedDryWeatherFlow(log, dryWeatherFlowsById, sobekPaved, pavedData);

            // Assert
            Assert.AreEqual(PavedEnums.DryWeatherFlowOptions.ConstantDWF, pavedData.DryWeatherFlowOptions);
            Assert.AreEqual(24, pavedData.WaterUse);
        }
        
        [Test]
        public void ReadPavedDryWeatherFlow_ValidVariableComputationOption_CorrectlySetsWaterUse()
        {
            // Arrange
            var dryWeatherFlowsById = new Dictionary<string, SobekRRDryWeatherFlow>
            {
                { "validDryWeatherFlowId", new SobekRRDryWeatherFlow { ComputationOption = DWAComputationOption.VariablePerHour, WaterUsePerDayForVariable= 801, WaterCapacityPerHour = Enumerable.Range(0, 24).Select(i => (double)i).ToArray()} }
            };

            var sobekPaved = new SobekRRPaved { DryWeatherFlowId = "validDryWeatherFlowId" };
            var pavedData = new PavedData(new Catchment());
            // Arrange a mock logger
            var log = Substitute.For<ILog>();

            // Act
            SobekRRPavedDryWeatherFlowUpdater.UpdatePavedDryWeatherFlow(log, dryWeatherFlowsById, sobekPaved, pavedData);

            // Assert
            Assert.AreEqual(PavedEnums.DryWeatherFlowOptions.VariableDWF, pavedData.DryWeatherFlowOptions);
            Assert.AreEqual(801, pavedData.WaterUse);
            List<double> variableWaterUsages = pavedData.VariableWaterUseFunction.GetValues<double>().ToList();
            Assert.AreEqual(24, variableWaterUsages.Count);
            Assert.AreEqual(23.0, pavedData.VariableWaterUseFunction[23]);
        }

        [Test]
        public void ReadPavedDryWeatherFlow_ValidButNotImplementedComputationOption_CorrectlySetsWaterUse()
        {
            // Arrange
            var dryWeatherFlowsById = new Dictionary<string, SobekRRDryWeatherFlow>
            {
                { "validDryWeatherFlowId", new SobekRRDryWeatherFlow { ComputationOption = DWAComputationOption.UseTable, WaterCapacityPerHour = Enumerable.Range(0, 24).Select(i => (double)i).ToArray()} }
            };

            var sobekPaved = new SobekRRPaved { DryWeatherFlowId = "validDryWeatherFlowId" };
            var pavedData = new PavedData(new Catchment());
            // Arrange a mock logger
            var log = Substitute.For<ILog>();

            // Act
            SobekRRPavedDryWeatherFlowUpdater.UpdatePavedDryWeatherFlow(log, dryWeatherFlowsById, sobekPaved, pavedData);

            // Assert
            log.Received().WarnFormat(Arg.Any<string>(), Arg.Any<string>());
            Assert.AreEqual(PavedEnums.DryWeatherFlowOptions.ConstantDWF, pavedData.DryWeatherFlowOptions);
        }

        [Test]
        public void ReadPavedDryWeatherFlow_InvalidComputationOption_LogsError()
        {
            // Arrange
            var dryWeatherFlowsById = new Dictionary<string, SobekRRDryWeatherFlow>
            {
                { "invalidDryWeatherFlowId", new SobekRRDryWeatherFlow { ComputationOption = (DWAComputationOption)100 } }
            };

            var sobekPaved = new SobekRRPaved { DryWeatherFlowId = "invalidDryWeatherFlowId" };
            var pavedData = new PavedData(new Catchment());

            // Arrange a mock logger
            var log = Substitute.For<ILog>();

            // Act
            SobekRRPavedDryWeatherFlowUpdater.UpdatePavedDryWeatherFlow(log, dryWeatherFlowsById, sobekPaved, pavedData);

            // Assert
            log.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}