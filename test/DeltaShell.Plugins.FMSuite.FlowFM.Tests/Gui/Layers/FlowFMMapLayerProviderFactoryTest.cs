using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    [TestFixture]
    public class FlowFMMapLayerProviderFactoryTest
    {
        [Test]
        public void ConstructMapLayerProvider_ExpectedResults()
        {
            IMapLayerProvider provider = FlowFMMapLayerProviderFactory.ConstructMapLayerProvider();
            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void GetSubProviders_ContainsExpectedSubProviders()
        {
            List<ILayerSubProvider> result = 
                FlowFMMapLayerProviderFactory.GetSubProviders().ToList();

            Assert.That(result, Has.One.Items.TypeOf<FlowFMModelLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<ImportedFMNetFileLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<LeveeBreachWidthCoverageLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<Input1DGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<InputPropertyLayerSubProvider<Model1DProperty.BoundaryConditions1D>>());
            Assert.That(result, Has.One.Items.TypeOf<InputPropertyLayerSubProvider<Model1DProperty.LateralSources>>());
            Assert.That(result, Has.One.Items.TypeOf<InitialConditionsGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<DefinitionsLayerSubProvider<ChannelInitialConditionDefinition>>());
            Assert.That(result, Has.One.Items.TypeOf<FrictionsGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<DefinitionsLayerSubProvider<ChannelFrictionDefinition>>());
            Assert.That(result, Has.One.Items.TypeOf<DefinitionsLayerSubProvider<PipeFrictionDefinition>>());
            Assert.That(result, Has.One.Items.TypeOf<FunctionStoreLayerSubProvider<FunctionStore.FM1DFileFunctionStoreDescription, FM1DFileFunctionStore>>());
            Assert.That(result, Has.One.Items.TypeOf<Input2DGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<InputPropertyLayerSubProvider<Model2DProperty.Boundaries>>());
            Assert.That(result, Has.One.Items.TypeOf<InputPropertyLayerSubProvider<Model2DProperty.BoundaryConditionSets>>());
            Assert.That(result, Has.One.Items.TypeOf<InputPropertyLayerSubProvider<Model2DProperty.Links1D2D>>());
            Assert.That(result, Has.One.Items.TypeOf<InputPropertyLayerSubProvider<Model2DProperty.Pipes>>());
            Assert.That(result, Has.One.Items.TypeOf<EstimatedSnappedFeatureGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<EstimatedSnappedFeatureLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<OutputGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<FunctionStoreLayerSubProvider<FunctionStore.ClassMapFileFunctionStoreDescription, FMClassMapFileFunctionStore>>());
            Assert.That(result, Has.One.Items.TypeOf<FunctionStoreLayerSubProvider<FunctionStore.FouFileFunctionStoreDescription, FouFileFunctionStore>>());
            Assert.That(result, Has.One.Items.TypeOf<FunctionStoreLayerSubProvider<FunctionStore.HisFileFunctionStoreDescription, FMHisFileFunctionStore>>());
            Assert.That(result, Has.One.Items.TypeOf<FunctionStoreLayerSubProvider<FunctionStore.MapFileFunctionStoreDescription, FMMapFileFunctionStore>>());
            Assert.That(result, Has.One.Items.TypeOf<FunctionGroupingLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<Links1D2DOutputLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<OutputSnappedFeatureGroupLayerSubProvider>());
            Assert.That(result, Has.One.Items.TypeOf<OutputSnappedFeatureLayerSubProvider>());
        }
    }
}