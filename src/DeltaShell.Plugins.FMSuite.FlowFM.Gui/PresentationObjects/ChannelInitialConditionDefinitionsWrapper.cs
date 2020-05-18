using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects
{
    /// <summary>
    /// Presentation object for visualizing <see cref="IEventedList{ChannelInitialConditionDefinition}"/>
    /// in project explorer and map tree.
    /// </summary>
    /// <remarks>
    /// Directly using the <see cref="IEventedList{ChannelInitialConditionDefinition}"/> for presentation purposes results
    /// in unwanted selection synchronization after clicking the related node in the project explorer. Implemented
    /// the singleton pattern in order to guarantee equality.
    /// </remarks>
    public sealed class ChannelInitialConditionDefinitionsWrapper
    {
        private static readonly IList<ChannelInitialConditionDefinitionsWrapper> Instances = new List<ChannelInitialConditionDefinitionsWrapper>();

        public static ChannelInitialConditionDefinitionsWrapper GetInstance(IEventedList<ChannelInitialConditionDefinition> wrappedData)
        {
            var instance = Instances.FirstOrDefault(i => ReferenceEquals(i.WrappedData, wrappedData));
            if (instance != null)
            {
                return instance;
            }

            instance = new ChannelInitialConditionDefinitionsWrapper(wrappedData);

            Instances.Add(instance);

            return instance;
        }

        private ChannelInitialConditionDefinitionsWrapper(IEventedList<ChannelInitialConditionDefinition> wrappedData)
        {
            WrappedData = wrappedData;
        }

        public IEventedList<ChannelInitialConditionDefinition> WrappedData { get; }
    }
}