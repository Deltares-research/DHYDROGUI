using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects
{
    /// <summary>
    /// Presentation object for visualizing <see cref="IEventedList{T}"/>
    /// in project explorer and map tree.
    /// </summary>
    /// <remarks>
    /// Directly using the <see cref="IEventedList{ChannelFrictionDefinition}"/> for presentation purposes results
    /// in unwanted selection synchronization after clicking the related node in the project explorer. Implemented
    /// the singleton pattern in order to guarantee equality.
    /// </remarks>
    public sealed class ChannelFrictionDefinitionsWrapper 
    {
        private static readonly IList<ChannelFrictionDefinitionsWrapper> Instances = new List<ChannelFrictionDefinitionsWrapper>();

        public static ChannelFrictionDefinitionsWrapper GetInstance(IEventedList<ChannelFrictionDefinition> wrappedData)
        {
            var instance = Instances.FirstOrDefault(i => ReferenceEquals(i.WrappedData, wrappedData));
            if (instance != null)
            {
                return instance;
            }

            instance = new ChannelFrictionDefinitionsWrapper(wrappedData);

            Instances.Add(instance);

            return instance;
        }

        private ChannelFrictionDefinitionsWrapper(IEventedList<ChannelFrictionDefinition> wrappedData)
        {
            WrappedData = wrappedData;
        }

        public IEventedList<ChannelFrictionDefinition> WrappedData { get; }
    }
}