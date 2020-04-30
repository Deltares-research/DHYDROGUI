using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects
{
    /// <summary>
    /// Presentation object for visualizing <see cref="IEventedList{ChannelFrictionDefinition}"/>
    /// in project explorer and map tree.
    /// </summary>
    /// <remarks>
    /// Directly using the <see cref="IEventedList{ChannelFrictionDefinition}"/> for presentation results
    /// in unwanted selection synchronization after clicking the related node in the project explorer.
    /// </remarks>
    public class ChannelFrictionDefinitionsWrapper
    {
        public ChannelFrictionDefinitionsWrapper(IEventedList<ChannelFrictionDefinition> wrappedData)
        {
            WrappedData = wrappedData;
        }

        public IEventedList<ChannelFrictionDefinition> WrappedData { get; private set; }
    }
}
