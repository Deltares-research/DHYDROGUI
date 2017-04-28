using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using OpenMI.Standard2;

namespace DelftTools.OpenMI2
{
    /// <summary>
    /// Class responible to DeltaShell's IModel interface to OpenMI2's LinkableComponent
    /// </summary>
    [Obsolete("TODO: remove this assembly, move to OpenDA / OpenMI plugin")]
    public class LinkableComponentWrapper : IBaseLinkableComponent
    {
        private static IDictionary<ActivityStatus, LinkableComponentStatus> ActivityStatusToLinkableComponentStatus;
        static LinkableComponentWrapper()
        {
            ActivityStatusToLinkableComponentStatus = new Dictionary<ActivityStatus, LinkableComponentStatus>();
            ActivityStatusToLinkableComponentStatus[ActivityStatus.None] = LinkableComponentStatus.Created;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Executing] = LinkableComponentStatus.Updating;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Executed] = LinkableComponentStatus.Updated;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Done] = LinkableComponentStatus.Done;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Failed] = LinkableComponentStatus.Failed;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Finishing] = LinkableComponentStatus.Finishing;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Finished] = LinkableComponentStatus.Finished;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Initialized] = LinkableComponentStatus.Initialized;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.Initializing] = LinkableComponentStatus.Initializing;
            ActivityStatusToLinkableComponentStatus[ActivityStatus.WaitingForData] = LinkableComponentStatus.WaitingForData;
        }

        private readonly IModel model;

        public LinkableComponentWrapper(IModel model)
        {
            this.model = model;
        }

        public string Caption
        {
            get { return model.Name; }
            set { model.Name = value; }
        }

        public string Description
        {
            get { return ""; }
            set { throw new NotImplementedException(); }
        }

        public string Id
        {
            get { return model.Name; }
        }

        public virtual void Initialize()
        {
            model.Initialize();
            Status = GetOpenMiStatus(model.Status);
        }

        public virtual string[] Validate()
        {
            Status = LinkableComponentStatus.Valid;
            //TODO: add validation stuff
            return new string[] {};
            //validate the model etc.
            
        }

        public virtual void Prepare()
        {

            Status = LinkableComponentStatus.Updated;
        }

        public virtual void Update(params IBaseOutput[] requiredOutput)
        {
            model.Execute();
            Status = GetOpenMiStatus(model.Status);
        }

        public virtual void Finish()
        {
            Status = LinkableComponentStatus.Finished;
        }

        public IList<IArgument> Arguments
        {
            get { return new List<IArgument>(); }
        }


        public LinkableComponentStatus Status
        {
            get;set;
        }

        
        private static LinkableComponentStatus GetOpenMiStatus(ActivityStatus status)
        {
            return ActivityStatusToLinkableComponentStatus[status];
        }

        public virtual IList<IBaseInput> Inputs
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IBaseOutput> Outputs
        {
            get { throw new NotImplementedException(); }
        }

        public List<IAdaptedOutputFactory> AdaptedOutputFactories
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<LinkableComponentStatusChangeEventArgs> StatusChanged;
    }
}
