using DelftTools.Shell.Core.Workflow;
using Deltares.OpenMI.Oatc.Sdk.Wrapper;

namespace DeltaShell.OpenMIWrapper
{
    public class DeltaShellOpenMILinkableComponent : LinkableEngine
    {
        /// <summary>
        /// Constructor for unit testing purposes
        /// </summary>
        /// <param name="timeDependentModel">rootmodel (hydro, flow1, etc.) to be exposed by OpenMI wrapper</param>
        public DeltaShellOpenMILinkableComponent(ITimeDependentModel rootModel)
        {
            _engineApiAccess = new DeltaShellOpenMIEngine(rootModel);
        }

        /// <summary>
        /// Default empty constructor (uses by OpenMI GUI/deployer)
        /// </summary>
        public DeltaShellOpenMILinkableComponent()
        {
        }

        protected override void SetEngineApiAccess()
        {
            if (_engineApiAccess == null)
            {
                _engineApiAccess = new DeltaShellOpenMIEngine();
            }
        }

        public override void Prepare()
        {
            ((DeltaShellOpenMIEngine) _engineApiAccess).Prepare(GetAcceptingLinks(), GetProvidingLinks());
            base.Prepare();
        }
    }
}
