using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects
{
    /// <summary>
    /// Presentation object for visualizing <see cref="IEventedList{T}"/>
    /// in project explorer and map tree.
    /// </summary>
    /// <remarks>
    /// Directly using the <see cref="IEventedList{ChannelInitialConditionDefinition}"/> for presentation purposes results
    /// in unwanted selection synchronization after clicking the related node in the project explorer. Implemented
    /// the singleton pattern in order to guarantee equality.
    /// </remarks>
    public sealed class ChannelInitialConditionDefinitionsWrapper
    {
        public static ChannelInitialConditionDefinitionsWrapper GetInstance(IEventedList<ChannelInitialConditionDefinition> wrappedData)
        {
            return new ChannelInitialConditionDefinitionsWrapper(wrappedData);
        }

        private ChannelInitialConditionDefinitionsWrapper(IEventedList<ChannelInitialConditionDefinition> wrappedData)
        {
            WrappedData = wrappedData;
        }

        public IEventedList<ChannelInitialConditionDefinition> WrappedData { get; }

        public override bool Equals(object Obj)
        {
            if (Obj is ChannelInitialConditionDefinitionsWrapper channelFrictionDefinition)
            {
                return Equals(channelFrictionDefinition);
            }

            return false;
        }

        private bool Equals(ChannelInitialConditionDefinitionsWrapper other)
        {
            return WrappedData == other?.WrappedData;
        }

        public override int GetHashCode()
        {
            return (WrappedData != null ? WrappedData.GetHashCode() : 0);
        }

        public static bool operator ==(ChannelInitialConditionDefinitionsWrapper wrapper1, ChannelInitialConditionDefinitionsWrapper wrapper2)
        {
            return wrapper1?.Equals(wrapper2) ?? ReferenceEquals(wrapper2, null);
        }

        public static bool operator !=(ChannelInitialConditionDefinitionsWrapper wrapper1, ChannelInitialConditionDefinitionsWrapper wrapper2)
        {
            return !(wrapper1 == wrapper2);
        }
    }
}