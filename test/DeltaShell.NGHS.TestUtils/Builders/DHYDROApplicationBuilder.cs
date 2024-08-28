using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Persistence;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Persistence;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Persistence;
using DeltaShell.Plugins.DHYDRO.Persistence;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Persistence;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Persistence;
using DeltaShell.Plugins.NetworkEditor;

namespace DeltaShell.NGHS.TestUtils.Builders
{
    public sealed class DHYDROApplicationBuilder
    {
        private readonly IList<IPlugin> customPlugins = new List<IPlugin>();

        private static IEnumerable<IPlugin> DefaultPlugins
        {
            get
            {
                yield return new DHYDROPersistencePlugin();
                yield return new NetworkEditorApplicationPlugin();
            }
        }

        public DHYDROApplicationBuilder WithFlowFM()
        {
            customPlugins.Add(new FlowFMApplicationPlugin());
            customPlugins.Add(new FlowFMPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithRealTimeControl()
        {
            customPlugins.Add(new RealTimeControlApplicationPlugin());
            customPlugins.Add(new RealTimeControlPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithWaterQuality()
        {
            customPlugins.Add(new WaterQualityModelApplicationPlugin());
            customPlugins.Add(new WaterQualityPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithWaves()
        {
            customPlugins.Add(new WaveApplicationPlugin());
            customPlugins.Add(new WavesPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithHydroModel()
        {
            customPlugins.Add(new HydroModelApplicationPlugin());
            customPlugins.Add(new HydroModelPersistencePlugin());

            return this;
        }

        public IApplication Build()
        {
            IEnumerable<IPlugin> allPlugins = DeltaShellPlugins.AllNonGui
                                                               .Concat(DefaultPlugins)
                                                               .Concat(customPlugins);

            return new DeltaShellApplicationBuilder().WithPlugins(allPlugins).Build();
        }
    }
}