using DelftTools.Shell.Core.Workflow;
using OpenMI.Standard2.TimeSpace;

namespace DelftTools.OpenMI2
{
    /// <summary>
    /// This is the interface to be implemented if you want your wrapper to be two-way. Usefull for OpenDA coupling.
    /// </summary>
    public interface ITimeSpaceComponentWrapper : ITimeSpaceComponent
    {
        ITimeDependentModel Model { get; }
    }
}