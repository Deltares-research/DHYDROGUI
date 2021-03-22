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
        public static ChannelFrictionDefinitionsWrapper GetInstance(IEventedList<ChannelFrictionDefinition> wrappedData)
        {
            return new ChannelFrictionDefinitionsWrapper(wrappedData);
        }

        private ChannelFrictionDefinitionsWrapper(IEventedList<ChannelFrictionDefinition> wrappedData)
        {
            WrappedData = wrappedData;
        }

        public IEventedList<ChannelFrictionDefinition> WrappedData { get; }

        public override bool Equals(object obj)
        {
            if (obj is ChannelFrictionDefinition channelFrictionDefinition)
            {
                return Equals(channelFrictionDefinition);
            }

            return false;
        }

        private bool Equals(ChannelFrictionDefinitionsWrapper other)
        {
            return WrappedData == other?.WrappedData;
        }

        public override int GetHashCode()
        {
            return (WrappedData != null ? WrappedData.GetHashCode() : 0);
        }

        public static bool operator ==(ChannelFrictionDefinitionsWrapper wrapper1, ChannelFrictionDefinitionsWrapper wrapper2)
        {
            return wrapper1?.Equals(wrapper2) ?? ReferenceEquals(wrapper2, null);
        }

        public static bool operator !=(ChannelFrictionDefinitionsWrapper wrapper1, ChannelFrictionDefinitionsWrapper wrapper2)
        {
            return !(wrapper1 == wrapper2);
        }
    }
}