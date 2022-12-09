using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui
{
    [Extension(typeof(IPlugin))]
    public class RainfallRunoffGuiPlugin : GuiPlugin, IRainfallRunoffGuiPlugin
    {
        public override string Name => 
            "Rainfall runoff model (UI)";

        public override string DisplayName => 
            "D-Rainfall Runoff Plugin (UI)";

        public override string Description => 
            RainfallRunoff.Properties.Resources.RainfallRunoffApplicationPlugin_Description;

        public override string Version => 
            GetType().Assembly.GetName().Version.ToString();

        public override string FileFormatVersion => "3.5.0.0";

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<RainfallRunoffModel, RainfallRunoffModelProperties>();
            yield return new PropertyInfo<TreeFolder, RainfallRunoffOutputSettingsProperties>
                {
                    AdditionalDataCheck = o => o.Text == "Output" && o.Parent is RainfallRunoffModel,
                    GetObjectPropertiesData = o => o.Parent
                };
            yield return new PropertyInfo<RRInitialConditionsWrapper, RRInitialConditionsWrapperProperties>();
            yield return new PropertyInfo<RainfallRunoffBoundaryData, RainfallRunoffBoundaryDataProperties>();
            yield return new PropertyInfo<UnpavedData, UnpavedDataProperties>();
            yield return new PropertyInfo<PavedData, PavedDataProperties>();
            yield return new PropertyInfo<GreenhouseData, GreenhouseDataProperties>();
            yield return new PropertyInfo<OpenWaterData, OpenWaterDataProperties>();
            yield return new PropertyInfo<SacramentoData, SacramentoDataProperties>();
            yield return new PropertyInfo<HbvData, HbvDataProperties>();
            yield return new PropertyInfo<MeteoData, MeteoDataProperties>();
            yield return new PropertyInfo<Catchment, CatchmentProperties>
            {
                AfterCreate = cp =>
                {
                    cp.CatchmentData = Gui.Application.GetAllModelsInProject()
                                          .OfType<IRainfallRunoffModel>()
                                          .SelectMany(m => m.ModelData)
                                          .FirstOrDefault(d => ReferenceEquals(d.Catchment, cp.Data));
                }
            };
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            return RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(this);
        }         

        public override IMapLayerProvider MapLayerProvider => 
            new RainfallRunoffMapLayerProvider();

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            if (data is CatchmentModelData)
            {
                return new CatchmentModelDataProjectNodePresenter(this).GetContextMenu(null, data);
            }
            return null;
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new RainfallRunoffModelProjectNodePresenter(this);
            yield return new CatchmentModelDataProjectNodePresenter(this);
            yield return new CatchmentModelDataTreeFolderProjectNodePresenter(this);
            yield return new MeteoDataProjectNodePresenter(this);
            yield return new RainfallRunoffInitialConditionsProjectNodePresenter();
            yield return new RainfallRunoffBoundaryDataProjectNodePresenter();
            yield return new DryWeatherFlowDefinitionsProjectNodePresenter();
            yield return new NwrwDefinitionsProjectNodePresenter();
        }
        public override IGui Gui
        {
            get { return base.Gui; }
            set
            {
                if (base.Gui != null)
                {
                    Gui.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                }

                base.Gui = value;

                if (base.Gui != null)
                {
                    Gui.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
                }
            }
        }

        private void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            if (e.NewStatus != ActivityStatus.Failed || !(sender is RainfallRunoffModel rainfallRunoffModel)) return;
            OpenValidationView(rainfallRunoffModel);
        }

        [InvokeRequired]
        private void OpenValidationView(RainfallRunoffModel rainfallRunoffModel)
        {
            Gui.CommandHandler.OpenView(rainfallRunoffModel, typeof(ValidationView));
        }
    }
}