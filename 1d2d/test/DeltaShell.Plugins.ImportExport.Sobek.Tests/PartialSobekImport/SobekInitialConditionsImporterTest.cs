using System;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekInitialConditionsImporterTest
    {
        private const double Tolerance = 0.000001;

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportingInitialConditionsWithGlobalDefinition_CorrectlyUpdatesGlobalQuantityAndValueModelProperties()
        {
            // Setup
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"InitCond.lit\1\NETWORK.TP"); // model with WaterLevel as global quantity
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            network.Branches.Add(new Channel(){ Name = "1" });
            network.Branches.Add(new Channel(){ Name = "3" });
            Assert.That(network.Branches.Count, Is.EqualTo(2));

            var modelDefinition = fmModel.ModelDefinition;
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, "80085");
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{ (int)InitialConditionQuantity.WaterDepth }");

            var importer = new SobekInitialConditionsImporter()
            {
                TargetObject = fmModel,
                PathSobek = pathToSobekNetwork
            };

            // Call
            importer.Import();

            // Assert
            var globalValue = (double)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;
            var globalQuantity = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
            Assert.That(globalValue, Is.EqualTo(1.25));
            Assert.That(globalQuantity, Is.EqualTo(InitialConditionQuantity.WaterLevel));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(InitialConditionQuantity.WaterLevel)]
        [TestCase(InitialConditionQuantity.WaterDepth)]
        public void ImportingInitialConditionsWithoutGlobalDefinition_DoesNotChangeGlobalPropertiesButGivesWarning(InitialConditionQuantity globalQuantity)
        {
            // Setup
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"InitCondWithoutGlobal.lit\1\NETWORK.TP"); // model without global
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            network.Branches.Add(new Channel() { Name = "1" });
            network.Branches.Add(new Channel() { Name = "3" });
            Assert.That(network.Branches.Count, Is.EqualTo(2));

            var modelDefinition = fmModel.ModelDefinition;
            var expectedGlobalValue = 8008.135;
            var expectedGlobalQuantity = globalQuantity;
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, expectedGlobalValue.ToString(CultureInfo.InvariantCulture));
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{ (int)expectedGlobalQuantity }");

            var importer = new SobekInitialConditionsImporter()
            {
                TargetObject = fmModel,
                PathSobek = pathToSobekNetwork
            };

            // Call
            Action action = () => importer.Import();
            TestHelper.AssertLogMessageIsGenerated(action, "Globally defined flow conditions are not imported yet.");

            // Assert
            var actualGlobalValue = (double)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;
            var actualGlobalQuantity = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
            Assert.That(actualGlobalValue, Is.EqualTo(expectedGlobalValue).Within(Tolerance));
            Assert.That(actualGlobalQuantity, Is.EqualTo(actualGlobalQuantity));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithDefaultChannelInitialConditionDefinitions_WhenImporting_ThenChannelInitialConditionDefinitionsAreCorrectlyUpdated()
        {
            // Given
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"InitCond.lit\1\NETWORK.TP");

            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            network.Branches.Add(new Channel() { Name = "1" });
            network.Branches.Add(new Channel() { Name = "2" });
            network.Branches.Add(new Channel() { Name = "3" });
            network.Branches.Add(new Channel() { Name = "4" });
            Assert.That(network.Branches.Count, Is.EqualTo(4));

            var channelInitialConditionDefinitions = fmModel.ChannelInitialConditionDefinitions;
            Assert.That(channelInitialConditionDefinitions.Count, Is.EqualTo(4));
            foreach (var definition in channelInitialConditionDefinitions)
            {
                Assert.That(definition.SpecificationType, Is.EqualTo(ChannelInitialConditionSpecificationType.ModelSettings));
                Assert.That(definition.ConstantChannelInitialConditionDefinition, Is.Null);
                Assert.That(definition.SpatialChannelInitialConditionDefinition, Is.Null);
            }

            var importer = new SobekInitialConditionsImporter()
            {
                TargetObject = fmModel,
                PathSobek = pathToSobekNetwork
            };

            // When
            importer.Import();

            // Then
            var importedDefinition = channelInitialConditionDefinitions.FirstOrDefault(definition => definition.Channel.Name.Equals("1"));
            Assert.That(importedDefinition, Is.Not.Null);
            Assert.That(importedDefinition.SpecificationType, Is.EqualTo(ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition));
            Assert.That(importedDefinition.SpatialChannelInitialConditionDefinition, Is.Null);
            var constantDefinition = importedDefinition.ConstantChannelInitialConditionDefinition;
            Assert.That(constantDefinition.Value, Is.EqualTo(0.88)); // see INITIAL.dat
            Assert.That(constantDefinition.Quantity, Is.EqualTo(InitialConditionQuantity.WaterLevel));

            importedDefinition = channelInitialConditionDefinitions.FirstOrDefault(definition => definition.Channel.Name.Equals("2"));
            Assert.That(importedDefinition, Is.Not.Null);
            Assert.That(importedDefinition.SpecificationType, Is.EqualTo(ChannelInitialConditionSpecificationType.ModelSettings));
            Assert.That(importedDefinition.SpatialChannelInitialConditionDefinition, Is.Null);
            Assert.That(importedDefinition.ConstantChannelInitialConditionDefinition, Is.Null);

            importedDefinition = channelInitialConditionDefinitions.FirstOrDefault(definition => definition.Channel.Name.Equals("3"));
            Assert.That(importedDefinition, Is.Not.Null);
            Assert.That(importedDefinition.SpecificationType, Is.EqualTo(ChannelInitialConditionSpecificationType.ModelSettings));
            Assert.That(importedDefinition.SpatialChannelInitialConditionDefinition, Is.Null);
            Assert.That(importedDefinition.ConstantChannelInitialConditionDefinition, Is.Null);

            importedDefinition = channelInitialConditionDefinitions.FirstOrDefault(definition => definition.Channel.Name.Equals("4"));
            Assert.That(importedDefinition, Is.Not.Null);
            Assert.That(importedDefinition.SpecificationType, Is.EqualTo(ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition));
            Assert.That(importedDefinition.ConstantChannelInitialConditionDefinition, Is.Null);
            var spatialDefinition = importedDefinition.SpatialChannelInitialConditionDefinition;
            Assert.That(spatialDefinition, Is.Not.Null);
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[0].Chainage, Is.EqualTo(0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[0].Value, Is.EqualTo(0.19));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Chainage, Is.EqualTo(1405));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Value, Is.EqualTo(0.29));
        }

    }
}
