using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects
{
    /// <summary>
    /// Presentation object for visualizing <see cref="IEventedList{PipeFrictionDefinition}"/>
    /// in project explorer and map tree.
    /// </summary>
    /// <remarks>
    /// Directly using the <see cref="IEventedList{PipeFrictionDefinition}"/> for presentation purposes results
    /// in unwanted selection synchronization after clicking the related node in the project explorer. Implemented
    /// the singleton pattern in order to guarantee equality.
    /// </remarks>
    public sealed class PipeFrictionDefinitionsWrapper
    {
        private static readonly IList<PipeFrictionDefinitionsWrapper> Instances = new List<PipeFrictionDefinitionsWrapper>();

        public static PipeFrictionDefinitionsWrapper GetInstance(IEventedList<PipeFrictionDefinition> wrappedData)
        {
            var instance = Instances.FirstOrDefault(i => ReferenceEquals(i.WrappedData, wrappedData));
            if (instance != null)
            {
                return instance;
            }

            instance = new PipeFrictionDefinitionsWrapper(wrappedData);

            Instances.Add(instance);

            return instance;
        }

        private PipeFrictionDefinitionsWrapper(IEventedList<PipeFrictionDefinition> wrappedData)
        {
            WrappedData = wrappedData;
        }

        public IEventedList<PipeFrictionDefinition> WrappedData { get; }
    }
}
