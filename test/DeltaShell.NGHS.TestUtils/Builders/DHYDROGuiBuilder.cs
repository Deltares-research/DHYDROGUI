using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Dimr.Gui;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Persistence;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Persistence;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Persistence;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Persistence;
using DeltaShell.Plugins.DHYDRO.Persistence;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Persistence;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Persistence;

namespace DeltaShell.NGHS.TestUtils.Builders
{
    public sealed class DHYDROGuiBuilder
    {
        private readonly IList<IPlugin> customPlugins = new List<IPlugin>();

        private static IEnumerable<IPlugin> DefaultPlugins
        {
            get
            {
                yield return new DHYDROPersistencePlugin();
                yield return new NetworkEditorApplicationPlugin();
                yield return new NetworkEditorGuiPlugin();
                yield return new NetworkEditorPersistencePlugin();
            }
        }

        public DHYDROGuiBuilder WithFlowFM()
        {
            customPlugins.Add(new FlowFMApplicationPlugin());
            customPlugins.Add(new FlowFMGuiPlugin());
            customPlugins.Add(new FlowFMPersistencePlugin());

            return this;
        }

        public DHYDROGuiBuilder WithRainfallRunoff()
        {
            customPlugins.Add(new RainfallRunoffApplicationPlugin());
            customPlugins.Add(new RainfallRunoffGuiPlugin());
            customPlugins.Add(new RainfallRunoffPersistencePlugin());

            return this;
        }

        public DHYDROGuiBuilder WithRealTimeControl()
        {
            customPlugins.Add(new RealTimeControlApplicationPlugin());
            customPlugins.Add(new RealTimeControlGuiPlugin());
            customPlugins.Add(new RealTimeControlPersistencePlugin());
            customPlugins.Add(new RealTimeControlGuiPersistencePlugin());

            return this;
        }

        public DHYDROGuiBuilder WithDimr()
        {
            customPlugins.Add(new DimrGuiPlugin());

            return this;
        }

        public DHYDROGuiBuilder WithHydroModel()
        {
            customPlugins.Add(new HydroModelApplicationPlugin());
            customPlugins.Add(new HydroModelGuiPlugin());
            customPlugins.Add(new HydroModelPersistencePlugin());

            return this;
        }

        public DHYDROGuiBuilder WithSobekImport()
        {
            customPlugins.Add(new SobekImportApplicationPlugin());
            customPlugins.Add(new SobekImportGuiPlugin());

            return this;
        }

        public IGui Build()
        {
            IEnumerable<IPlugin> allPlugins = DeltaShellPlugins.All
                                                               .Concat(customPlugins)
                                                               .Concat(DefaultPlugins);

            return new DeltaShellGuiBuilder().WithPlugins(allPlugins).Build();
        }
    }
}