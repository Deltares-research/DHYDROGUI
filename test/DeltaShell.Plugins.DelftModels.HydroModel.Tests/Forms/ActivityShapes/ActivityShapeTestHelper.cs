using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.ActivityShapes
{
    public static class ActivityShapeTestHelper
    {
        /// <summary>
        /// Creates a simple stubbed implementation of <see cref="IActivity"/>.
        /// </summary>
        /// <param name="name">Optional parameter: Name of the activity.</param>
        /// <returns>A basic, non-runnable activity for testing purposes.</returns>
        public static IActivity CreateSimpleActivity(string name = "Simple activity")
        {
            return new SimpleActivity{ Name = name };
        }

        /// <summary>
        /// Creates a simple stubbed implementation of <see cref="SimpleCompositeActivity"/>.
        /// </summary>
        /// <param name="name">Optional parameter: Name of the activity.</param>
        /// <returns>A basic, non-runnable composite activity for testing purposes</returns>
        public static ICompositeActivity CreateSimpleCompositeActivity(string name = "Simple composite activity")
        {
            return new SimpleCompositeActivity { Name = name };
        }

        #region Nested Type: SimpleActivity

        private class SimpleActivity : Activity
        {
            protected override void OnInitialize()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnExecute()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnCancel()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnCleanUp()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnFinish()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return Name;
            }
        }

        #endregion

        #region NestedType: SimpleCompositeActivity

        private class SimpleCompositeActivity : Activity, ICompositeActivity
        {
            private IEventedList<IActivity> activities = new EventedList<IActivity>();
            private ICompositeActivity currentWorkflow;

            protected override void OnInitialize()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnExecute()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnCancel()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnCleanUp()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnFinish()
            {
                throw new System.NotImplementedException();
            }

            public IEventedList<IActivity> Activities
            {
                get { return activities; }
                set { activities = value; }
            }

            public bool ReadOnly { get; set; }

            public ICompositeActivity CurrentWorkflow
            {
                get { return currentWorkflow; }
                set { currentWorkflow = value; }
            }

            public override string ToString()
            {
                return Name;
            }
        }

        #endregion
    }
}