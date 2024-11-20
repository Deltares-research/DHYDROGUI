using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.HydroModel.Persistence;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Persistence;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Persistence;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Persistence;
using DeltaShell.Plugins.DHYDRO.Persistence;
using DeltaShell.Plugins.FMSuite.FlowFM.Persistence;
using DeltaShell.Plugins.FMSuite.Wave.Persistence;
using DeltaShell.Plugins.NetworkEditor.Gui.Persistence;

namespace DeltaShell.NGHS.TestUtils.Builders
{
    public sealed class DHYDRONHibernateProjectRepositoryBuilder
    {
        private readonly IList<IPlugin> customPlugins = new List<IPlugin>();

        private static IEnumerable<IPlugin> DefaultPlugins
        {
            get
            {
                yield return new DHYDROPersistencePlugin();
                yield return new HydroModelPersistencePlugin();
                yield return new NetworkEditorGuiPersistencePlugin();
            }
        }

        public DHYDRONHibernateProjectRepositoryBuilder WithFlowFM()
        {
            customPlugins.Add(new FlowFMPersistencePlugin());
            return this;
        }

        public DHYDRONHibernateProjectRepositoryBuilder WithWaterQuality()
        {
            customPlugins.Add(new WaterQualityPersistencePlugin());
            return this;
        }

        public DHYDRONHibernateProjectRepositoryBuilder WithWaves()
        {
            customPlugins.Add(new WavesPersistencePlugin());

            return this;
        }

        public DHYDRONHibernateProjectRepositoryBuilder WithRealTimeControl()
        {
            customPlugins.Add(new RealTimeControlPersistencePlugin());
            customPlugins.Add(new RealTimeControlGuiPersistencePlugin());

            return this;
        }

        public NHibernateProjectRepository Build()
        {
            IEnumerable<IPlugin> allPlugins = DeltaShellPlugins.AllPersistence
                                                               .Concat(DefaultPlugins)
                                                               .Concat(customPlugins);

            IntegrationTestsPluginManager pluginsManager = new IntegrationTestsPluginManagerBuilder().AddPlugins(allPlugins).Build();
            return new NHibernateProjectRepositoryBuilder(pluginsManager).Build();
        }
    }
}