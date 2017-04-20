using System.Linq;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    class ImportBranchesCommand : NetworkEditorCommand
    {
        protected IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByType<ImportBranchesFromSelectionMapTool>();
            }
        }
        protected override void OnExecute(params object[] arguments)
        {
            var exportMapTool = MapView.MapControl.Tools.First(tool => tool is ImportBranchesFromSelectionMapTool);
            exportMapTool.Execute();
        }
        }
    }
