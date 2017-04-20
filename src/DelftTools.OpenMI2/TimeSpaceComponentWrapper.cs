using DelftTools.Shell.Core.Workflow;
using Deltares.OpenMI2.Oatc.Sdk.Backbone;
using OpenMI.Standard2.TimeSpace;

namespace DelftTools.OpenMI2
{
    /// <summary>
    /// Wrapper around ITimeDependentModel to ITimeSpaceComponent
    /// </summary>
    public class TimeSpaceComponentWrapper : LinkableComponentWrapper, ITimeSpaceComponentWrapper
    {
        public ITimeDependentModel Model
        {
            get; private set;
        }

        public TimeSpaceComponentWrapper(ITimeDependentModel model) : base(model)
        {
            Model = model;
        }


        public ITimeSet TimeExtent
        {
            get
            {
                return new TimeSet { TimeHorizon = new Time(Model.StartTime, Model.StopTime) };
            }
        }
        
    }
}