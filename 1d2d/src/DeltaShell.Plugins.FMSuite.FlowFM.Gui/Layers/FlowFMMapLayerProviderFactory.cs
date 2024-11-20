using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    /// <summary>
    /// <see cref="FlowFMMapLayerProviderFactory"/> is responsible for creating a
    /// <see cref="IMapLayerProvider"/> capable of providing the layers of the FlowFM
    /// GUI plugin.
    /// </summary>
    public static class FlowFMMapLayerProviderFactory
    {
        /// <summary>
        /// Construct a new <see cref="IMapLayerProvider"/> capable of providing the
        /// layers of the FlowFM GUI plugin.
        /// </summary>
        /// <returns>
        /// The <see cref="IMapLayerProvider"/> of the <see cref="FlowFMGuiPlugin"/>
        /// </returns>
        public static IMapLayerProvider ConstructMapLayerProvider()
        {
            ILayerSubProvider[] subProviders = GetSubProviders().ToArray();
            var provider = new MapLayerProvider();

            provider.RegisterSubProviders(subProviders);
            return provider;
        }

        /// <summary>
        /// Gets the iteration of <see cref="ILayerSubProvider"/> objects responsible
        /// for creating the layers of the FlowFM GUI plugin.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerable{ILayerSubProvider}"/> containing all
        /// <see cref="ILayerSubProvider"/> objects of the FlowFM GUI plugin.
        /// </returns>
        internal static IEnumerable<ILayerSubProvider> GetSubProviders()
        {
            var instanceCreator = new FlowFMLayerInstanceCreator();
            yield return new FlowFMModelLayerSubProvider(instanceCreator);
            yield return new ImportedFMNetFileLayerSubProvider(instanceCreator);
            yield return new LeveeBreachWidthCoverageLayerSubProvider(instanceCreator);

            //   - input 1D
            yield return new Input1DGroupLayerSubProvider(instanceCreator);

            yield return new InputPropertyLayerSubProvider<Model1DProperty.BoundaryConditions1D>(instanceCreator);
            yield return new InputPropertyLayerSubProvider<Model1DProperty.LateralSources>(instanceCreator);
            
            yield return new InitialConditionsGroupLayerSubProvider(instanceCreator);
            yield return new DefinitionsLayerSubProvider<ChannelInitialConditionDefinition>(
                FlowFMLayerNames.ChannelInitialConditionDefinitionsLayerName,
                FeatureType.InitialConditions,
                instanceCreator);

            yield return new FrictionsGroupLayerSubProvider(instanceCreator);
            yield return new DefinitionsLayerSubProvider<ChannelFrictionDefinition>(
                FlowFMLayerNames.ChannelFrictionDefinitionsLayerName,
                FeatureType.Friction,
                instanceCreator);
            yield return new DefinitionsLayerSubProvider<PipeFrictionDefinition>(
                FlowFMLayerNames.PipeFrictionDefinitionsLayerName,
                FeatureType.Friction,
                instanceCreator);

            //   - input 2D
            yield return new Input2DGroupLayerSubProvider(instanceCreator);

            yield return new InputPropertyLayerSubProvider<Model2DProperty.Boundaries>(instanceCreator);
            yield return new InputPropertyLayerSubProvider<Model2DProperty.BoundaryConditionSets>(instanceCreator);
            yield return new InputPropertyLayerSubProvider<Model2DProperty.Links1D2D>(instanceCreator);
            yield return new InputPropertyLayerSubProvider<Model2DProperty.Pipes>(instanceCreator);

            yield return new EstimatedSnappedFeatureGroupLayerSubProvider(instanceCreator);
            yield return new EstimatedSnappedFeatureLayerSubProvider(instanceCreator);

            //   - output
            yield return new OutputGroupLayerSubProvider(instanceCreator);

            yield return new FunctionStoreLayerSubProvider<FunctionStore.FM1DFileFunctionStoreDescription,
                                                           FM1DFileFunctionStore>(instanceCreator);
            yield return new FunctionStoreLayerSubProvider<FunctionStore.ClassMapFileFunctionStoreDescription,
                                                           FMClassMapFileFunctionStore>(instanceCreator);
            yield return new FunctionStoreLayerSubProvider<FunctionStore.FouFileFunctionStoreDescription,
                                                           FouFileFunctionStore>(instanceCreator);
            yield return new FunctionStoreLayerSubProvider<FunctionStore.HisFileFunctionStoreDescription,
                                                           FMHisFileFunctionStore>(instanceCreator);
            yield return new FunctionStoreLayerSubProvider<FunctionStore.MapFileFunctionStoreDescription,
                                                           FMMapFileFunctionStore>(instanceCreator);

            yield return new FunctionGroupingLayerSubProvider(instanceCreator);
            yield return new Links1D2DOutputLayerSubProvider(instanceCreator);

            yield return new OutputSnappedFeatureGroupLayerSubProvider(instanceCreator);
            yield return new OutputSnappedFeatureLayerSubProvider(instanceCreator);
        }
    }
}