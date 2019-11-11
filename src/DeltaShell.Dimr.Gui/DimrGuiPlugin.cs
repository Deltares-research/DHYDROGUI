using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DelftTools.Controls;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using Mono.Addins;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Dimr.Gui
{

    [Extension(typeof(IPlugin))]
    public class DimrGuiPlugin : GuiPlugin
    {
        public DimrGuiPlugin()
        {
            Instance = this;
        }

        public static DimrGuiPlugin Instance { get; private set; }

        public override string Name
        {
            get { return "Dimr (UI)"; }
        }

        public virtual bool IsOnlyDimrModelSelected
        {
            get
            {
                if (!IsActiveViewMapViewWithRegion()) return false;
                if(Gui.SelectedModel is IDimrModel) return true;
                var compositeModel = Gui.SelectedModel as ICompositeActivity;

                return compositeModel != null && compositeModel.CurrentWorkflow != null && compositeModel.CurrentWorkflow.Activities != null &&
                        compositeModel.CurrentWorkflow.Activities.GetActivitiesOfType<IModel>().Count() ==
                        compositeModel.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>().Count();

            }
        }

        private bool IsActiveViewMapViewWithRegion()
        {
            var mapView = Gui?.DocumentViews.GetViewsOfType<MapView>().FirstOrDefault();

            if (mapView == null || mapView.Map == null)
            {
                return false;
            }

            if (mapView.Map.GetAllLayers(true).Any(l => l is HydroRegionMapLayer))
            {
                return true;
            }

            return false;
        }

        public override string DisplayName
        {
            get { return "Dimr configuration Plugin (UI)"; }
        }

        public override string Description
        {
            get { return Properties.Resources.DimrGuiPlugin_Description_Provides_possibilities_to_configure_DIMR_settings; }
        }

        [ExcludeFromCodeCoverage]
        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        [ExcludeFromCodeCoverage]
        public override string FileFormatVersion
        {
            get { return "1.0.0.0"; }
        }

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get { return new Ribbon(); }
        }
        
        public override void Dispose()
        {
            base.Dispose();

            Instance = null;
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }
    }
}