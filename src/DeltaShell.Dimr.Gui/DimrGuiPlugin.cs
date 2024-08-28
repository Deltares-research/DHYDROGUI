using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using Mono.Addins;

namespace DeltaShell.Dimr.Gui
{
    [Extension(typeof(IPlugin))]
    public class DimrGuiPlugin : GuiPlugin
    {
        private bool disposed;

        public DimrGuiPlugin()
        {
            Instance = this;
        }

        public override string Name
        {
            get
            {
                return "Dimr (UI)";
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Dimr configuration Plugin (UI)";
            }
        }

        public override string Description
        {
            get
            {
                return Properties.Resources.DimrGuiPlugin_Description_Provides_possibilities_to_configure_DIMR_settings;
            }
        }

        [ExcludeFromCodeCoverage]
        public override string Version
        {
            get
            {
                return AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;
            }
        }

        [ExcludeFromCodeCoverage]
        public override string FileFormatVersion => "1.0.0.0";

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get
            {
                return new Ribbon();
            }
        }

        public static DimrGuiPlugin Instance { get; private set; }

        public virtual bool IsOnlyDimrModelSelected
        {
            get
            {
                if (Gui?.SelectedModel is IDimrModel)
                {
                    return true;
                }

                IEventedList<IActivity> activities = (Gui?.SelectedModel as ICompositeActivity)?.CurrentWorkflow?.Activities;

                return activities != null &&
                       activities.GetActivitiesOfType<IModel>().Count() ==
                       activities.GetActivitiesOfType<IDimrModel>().Count();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Instance = null;
            }

            base.Dispose(disposing);
            disposed = true;
        }
    }
}