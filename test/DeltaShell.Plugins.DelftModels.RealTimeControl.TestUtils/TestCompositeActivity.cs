using System;
using System.Collections.Specialized;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils
{
    [Entity(FireOnPropertyChange = false)]
    public class TestCompositeActivity : Activity, ICompositeActivity
    {
        private IEventedList<IActivity> activities;

        public TestCompositeActivity()
        {
            Activities = new EventedList<IActivity>();
        }

        public IEventedList<IActivity> Activities
        {
            get
            {
                return activities;
            }
            set
            {
                if (activities != null)
                {
                    activities.CollectionChanged -= OnActivitiesCollectionChanged;
                }

                activities = value;

                if (activities != null)
                {
                    activities.CollectionChanged += OnActivitiesCollectionChanged;
                }
            }
        }

        [Aggregation]
        public ICompositeActivity CurrentWorkflow { get; set; }

        public bool ReadOnly { get; set; }

        protected override void OnInitialize()
        {
            foreach (IActivity activity in Activities)
            {
                activity.Initialize();

                if (activity.Status == ActivityStatus.Failed)
                {
                    throw new InvalidOperationException(string.Format("Model initialiation has failed: {0}.{1}", this,
                                                                      activity));
                }
            }
        }

        protected override void OnExecute()
        {
            var allModelsFinished = true;

            foreach (IActivity model in Activities)
            {
                if (model.Status != ActivityStatus.Done)
                {
                    model.Execute();

                    if (model.Status == ActivityStatus.Failed)
                    {
                        throw new InvalidOperationException(string.Format("Model run has failed: {0}.{1}", this,
                                                                          model));
                    }
                }

                if (model.Status != ActivityStatus.Finished)
                {
                    allModelsFinished = false;
                }
            }

            if (allModelsFinished)
            {
                Status = ActivityStatus.Done;
            }
        }

        protected override void OnCancel() {}

        protected override void OnCleanUp() {}

        protected override void OnFinish() {}

        private void OnActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Equals(sender, activities))
            {
                return;
            }

            var model = (IModel) e.GetRemovedOrAddedItem();
            if (model != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        model.Owner = this;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        model.Owner = null;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}