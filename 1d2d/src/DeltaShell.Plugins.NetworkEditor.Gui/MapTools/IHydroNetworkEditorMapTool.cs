using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    //added interface for mocking and to get a little more insight about what the tool contains..
    //move members up as needed
    public interface IHydroNetworkEditorMapTool:IMapTool
    {
        /// <summary>
        /// The active coveragelayer used by the NetworkLocationTool.
        /// </summary>
        INetworkCoverageGroupLayer ActiveNetworkCoverageGroupLayer { get; set; }
    }
}