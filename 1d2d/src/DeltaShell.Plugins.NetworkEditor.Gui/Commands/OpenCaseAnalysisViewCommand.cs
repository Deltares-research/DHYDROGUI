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
        protected override void OnExecute(params object[] arguments)
        {
            var project = GetProject(Gui);

            if (project != null)
            {
                Gui.DocumentViews.Add(new CoverageAnalysisView{ Data = project});
            }
        }
        
        public override bool Enabled
        {
            get { return GetProject(Gui) != null; }
        }

        private static Project GetProject(IGui gui)
        {
            return gui.Application.ProjectService.Project;
        }

        public IGui Gui { get; set; }
    }
}