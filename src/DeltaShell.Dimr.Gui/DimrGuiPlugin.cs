using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Controls;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using Mono.Addins;

namespace DeltaShell.Dimr.Gui
{

    [Extension(typeof(IPlugin))]
    public class DimrGuiPlugin : GuiPlugin
    {
        private bool settingGuiSelection;
        private readonly IMapLayerProvider networkEditorMapLayerProvider;
        //private IGui gui;

        public DimrGuiPlugin()
        {
            Instance = this;
        }

        public static DimrGuiPlugin Instance { get; private set; }

        public override string Name
        {
            get { return "Dimr (UI)"; }
        }

        public bool IsOnlyDimrModelSelected
        {
            get
            {
                if(Gui.SelectedModel is IDimrModel) return true;
                var compositeModel = Gui.SelectedModel as ICompositeActivity;

                return compositeModel != null && compositeModel.CurrentWorkflow != null && compositeModel.CurrentWorkflow.Activities != null &&
                        compositeModel.CurrentWorkflow.Activities.GetActivitiesOfType<IModel>().Count() ==
                        compositeModel.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>().Count();

            }
        }

        public override string DisplayName
        {
            get { return "Dimr configuration Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Provides possibilities to configure DIMR settings"; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.0.0.0"; }
        }

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get { return new Ribbon(); }
        }

        
        public override IMapLayerProvider MapLayerProvider
        {
            get { return networkEditorMapLayerProvider; }
        }

        

        public override void Dispose()
        {
            base.Dispose();

            Instance = null;
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
           

            return null;
        }


        

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }
        
        

        
    }
}