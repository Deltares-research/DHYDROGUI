using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    /// <summary>
    /// Command to open simple case analysis view: hacking this in for now
    /// </summary>
    public class OpenCaseAnalysisViewCommand : Command, IGuiCommand
    {
        public override bool Enabled
        {
            get
            {
                return GetProject(Gui) != null;
            }
        }

        public IGui Gui { get; set; }

        protected override void OnExecute(params object[] arguments)
        {
            Project project = GetProject(Gui);

            if (project != null)
            {
                Gui.DocumentViews.Add(new CoverageAnalysisView {Data = project});
            }
        }

        private static Project GetProject(IGui gui)
        {
            IApplication app = gui.Application;

            if (app == null)
            {
                return null;
            }

            return app.Project;
        }
    }
}