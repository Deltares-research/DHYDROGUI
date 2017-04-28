using DelftTools.Shell.Core.Workflow;

namespace DelftTools.OpenMI2.Tests
{
    public class SimpleModel : ModelBase
    {
        protected override void OnInitialize()
        {
        }

        protected override void OnExecute()
        {
            Status = ActivityStatus.Done;
        }
    }
}