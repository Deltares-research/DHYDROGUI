using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Persistence;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Persistence;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Persistence;
using DeltaShell.Plugins.DHYDRO.Persistence;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Persistence;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Persistence;

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
                yield return new NetworkEditorPersistencePlugin();
                yield return new SobekImportApplicationPlugin();
            }
        }

        public DHYDROApplicationBuilder WithFlowFM()
        {
            customPlugins.Add(new FlowFMApplicationPlugin());
            customPlugins.Add(new FlowFMPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithRainfallRunoff()
        {
            customPlugins.Add(new RainfallRunoffApplicationPlugin());
            customPlugins.Add(new RainfallRunoffPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithRealTimeControl()
        {
            customPlugins.Add(new RealTimeControlApplicationPlugin());
            customPlugins.Add(new RealTimeControlPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithHydroModel()
        {
            customPlugins.Add(new HydroModelApplicationPlugin());
            customPlugins.Add(new HydroModelPersistencePlugin());

            return this;
        }

        public DHYDROApplicationBuilder WithSobekImport()
        {
            customPlugins.Add(new SobekImportApplicationPlugin());

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