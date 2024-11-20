using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.ActivityShapes;
using Netron.GraphLib;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms
{
    [TestFixture]
    public class WorkflowEditorControlTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithoutData()
        {
            var control = new WorkflowEditorControl {Workflows = null};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEmptyList()
        {
            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity>()};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEmptyParallelActivity()
        {
            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {new ParallelActivity {Name = "Parallel Activity"}}};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithFilledParallelActivity()
        {
            var nestedParallelActivity = new ParallelActivity {Name = "Nested Parallel Activity"};
            nestedParallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            var parallelActivity = new ParallelActivity {Name = "Parallel Activity"};
            parallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedParallelActivity
            });

            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {parallelActivity}};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEmptySequentialActivity()
        {
            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {new SequentialActivity {Name = "Sequential Activity"}}};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithFilledSequentialActivity()
        {
            var nestedSequentialActivity = new SequentialActivity {Name = "Nested Sequential Activity"};
            nestedSequentialActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            var sequentialActivity = new SequentialActivity {Name = "Sequential Activity"};
            sequentialActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedSequentialActivity
            });

            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {sequentialActivity}};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEmptyCompositeActivity()
        {
            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {ActivityShapeTestHelper.CreateSimpleCompositeActivity("Empty composite activity")}};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithFilledCompositeActivity()
        {
            ICompositeActivity nestedSimpleCompositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Nested Composite Activity");
            nestedSimpleCompositeActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            ICompositeActivity simpleCompositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Composite Activity");
            simpleCompositeActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedSimpleCompositeActivity
            });

            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {simpleCompositeActivity}};
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithAllActivities()
        {
            var nestedParallelActivity = new ParallelActivity {Name = "Nested Parallel Activity"};
            nestedParallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            var parallelActivity = new ParallelActivity {Name = "Parallel Activity"};
            parallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedParallelActivity
            });

            var nestedSequentialActivity = new SequentialActivity {Name = "Nested Sequential Activity"};
            nestedSequentialActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            var sequentialActivity = new SequentialActivity {Name = "Sequential Activity"};
            sequentialActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedSequentialActivity
            });

            ICompositeActivity nestedSimpleCompositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Nested Composite Activity");
            nestedSimpleCompositeActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            ICompositeActivity simpleCompositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Composite Activity");
            simpleCompositeActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedSimpleCompositeActivity
            });

            var control = new WorkflowEditorControl
            {
                Workflows = new EventedList<ICompositeActivity>
                {
                    parallelActivity,
                    sequentialActivity,
                    simpleCompositeActivity
                }
            };

            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        public void AllActivitiesAreSeperateEntitiesInControl()
        {
            var nestedParallelActivity = new ParallelActivity {Name = "Nested Parallel Activity"};
            nestedParallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            var parallelActivity = new ParallelActivity {Name = "Parallel Activity"};
            parallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedParallelActivity
            });

            var nestedSequentialActivity = new SequentialActivity {Name = "Nested Sequential Activity"};
            nestedSequentialActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            var sequentialActivity = new SequentialActivity {Name = "Sequential Activity"};
            sequentialActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedSequentialActivity
            });

            ICompositeActivity nestedSimpleCompositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Nested Composite Activity");
            nestedSimpleCompositeActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Nested Simple Activity"),
                ActivityShapeTestHelper.CreateSimpleActivity("Another Nested Activity")
            });
            ICompositeActivity simpleCompositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Composite Activity");
            simpleCompositeActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity(),
                nestedSimpleCompositeActivity
            });

            var control = new WorkflowEditorControl
            {
                Workflows = new EventedList<ICompositeActivity>
                {
                    parallelActivity,
                    sequentialActivity,
                    simpleCompositeActivity
                }
            };

            control.CurrentWorkflow = control.Workflows[0];

            ShapeCollection shapes = control.GraphControl.Shapes;
            Assert.AreEqual(5, shapes.Count);
            Assert.AreEqual(3, shapes.OfType<SimpleActivityShape>().Count());
            Assert.AreEqual(2, shapes.OfType<ParallelActivityShape>().Count());
            Assert.AreEqual(0, shapes.OfType<SequentialActivityShape>().Count());
            Assert.AreEqual(0, shapes.OfType<CompositeActivityShape>().Count());

            control.CurrentWorkflow = control.Workflows[1];

            shapes = control.GraphControl.Shapes;
            Assert.AreEqual(5, shapes.Count);
            Assert.AreEqual(3, shapes.OfType<SimpleActivityShape>().Count());
            Assert.AreEqual(0, shapes.OfType<ParallelActivityShape>().Count());
            Assert.AreEqual(2, shapes.OfType<SequentialActivityShape>().Count());
            Assert.AreEqual(0, shapes.OfType<CompositeActivityShape>().Count());

            control.CurrentWorkflow = control.Workflows[2];

            shapes = control.GraphControl.Shapes;
            Assert.AreEqual(5, shapes.Count);
            Assert.AreEqual(3, shapes.OfType<SimpleActivityShape>().Count());
            Assert.AreEqual(0, shapes.OfType<ParallelActivityShape>().Count());
            Assert.AreEqual(0, shapes.OfType<SequentialActivityShape>().Count());
            Assert.AreEqual(2, shapes.OfType<CompositeActivityShape>().Count());
        }

        [Test]
        public void SyncCollectionChangesInAvailableWorkflows()
        {
            ICompositeActivity compositeActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Test");
            var activities = new EventedList<ICompositeActivity> {compositeActivity};

            var control = new WorkflowEditorControl {Workflows = activities};

            ListBox list = TypeUtils.GetField<WorkflowEditorControl, ListBox>(control, "workflowSelectionListBox");

            Assert.AreSame(compositeActivity, control.CurrentWorkflow);
            Assert.AreEqual(1, list.Items.Count);

            ICompositeActivity anotherActivity = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Another activity");
            activities.Add(anotherActivity);

            Assert.AreSame(compositeActivity, control.CurrentWorkflow);
            Assert.AreEqual(2, list.Items.Count);

            // Select and remove that activity
            control.CurrentWorkflow = anotherActivity;
            activities.Remove(anotherActivity);

            Assert.AreSame(anotherActivity, control.CurrentWorkflow,
                           "Even though 'Another activity' has been removed from the list, it should remain as CurrentWorkflow");
            Assert.AreEqual(1, list.Items.Count);

            activities.Remove(compositeActivity);
            Assert.AreSame(anotherActivity, control.CurrentWorkflow,
                           "Even though the collection of workflows is empty, the CurrentWorkflow should remain");
            Assert.AreEqual(0, list.Items.Count);
        }

        [Test]
        public void SyncSelectedCurrentWorkflow()
        {
            ICompositeActivity simpleActivity1 = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Test 1");
            ICompositeActivity simpleActivity2 = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Test 2");
            var activities = new EventedList<ICompositeActivity>
            {
                simpleActivity1,
                simpleActivity2
            };

            var control = new WorkflowEditorControl {Workflows = activities};

            ListBox list = TypeUtils.GetField<WorkflowEditorControl, ListBox>(control, "workflowSelectionListBox");
            Assert.AreSame(simpleActivity1, control.CurrentWorkflow);
            Assert.AreSame(simpleActivity1, list.SelectedItem);

            control.CurrentWorkflow = simpleActivity2;
            Assert.AreSame(simpleActivity2, list.SelectedItem);

            list.SelectedItem = simpleActivity1;
            Assert.AreSame(simpleActivity1, control.CurrentWorkflow);

            // Presetting allowed:
            ICompositeActivity activityNotInWorkflows = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Test 3");
            control.CurrentWorkflow = activityNotInWorkflows;
            Assert.IsNull(list.SelectedItem,
                          "When CurrentWorkflow is preset but not in collection of workflows, there should be no selection in listview");
            Assert.AreEqual(1, control.GraphControl.Shapes.Count);

            // Setting workflows with preset CurrentWorkflow in the collection:
            control.Workflows = new EventedList<ICompositeActivity>(new[]
            {
                activityNotInWorkflows
            });
            Assert.AreSame(activityNotInWorkflows, control.CurrentWorkflow,
                           "Preset CurrentWorkflow is now in workflow collection, so selection is corrected");
            Assert.AreSame(activityNotInWorkflows, list.SelectedItem,
                           "Preset CurrentWorkflow is now in workflow collection, so selection is corrected");
            Assert.AreEqual(1, control.GraphControl.Shapes.Count);

            // Changing collection should not change CurrentWorkflow:
            control.Workflows = activities;
            Assert.IsNull(list.SelectedItem,
                          "When CurrentWorkflow is preset but not in collection of workflows, there should be no selection in listview");
            Assert.AreSame(activityNotInWorkflows, control.CurrentWorkflow,
                           "Preset CurrentWorkflow is now in workflow collection, so selection is corrected");
            Assert.AreEqual(1, control.GraphControl.Shapes.Count);
        }

        [Test]
        public void TestCurrentWorkflowChanged()
        {
            ICompositeActivity simpleActivity1 = ActivityShapeTestHelper.CreateSimpleCompositeActivity("Test 1");
            var control = new WorkflowEditorControl {Workflows = new EventedList<ICompositeActivity> {simpleActivity1}};

            var workflowChangedCount = 0;
            control.CurrentWorkflowChanged += delegate { workflowChangedCount++; };

            var workflowSelectedCount = 0;
            control.SelectedActivityChanged += delegate { workflowSelectedCount++; };

            control.CurrentWorkflow = simpleActivity1;
            Assert.AreEqual(0, workflowChangedCount, "Should not fire CurrentWorkflowChanged event because value hasn't changed");
            Assert.AreEqual(1, workflowSelectedCount, "Should fire CurrentWorkflowSelected event because value has been selected");

            control.CurrentWorkflow = null;
            Assert.AreEqual(1, workflowChangedCount, "Should fire CurrentWorkflowChanged event due to value change");
            Assert.AreEqual(1, workflowSelectedCount, "Should not fire CurrentWorkflowSelected event because value is null");
        }

        [Test]
        public void DefaultActivityShapesShouldBeAvailable()
        {
            var control = new WorkflowEditorControl();
            // These calls demonstrate that shapes are added to library of shapes:
            control.GraphControl.AddShape(SimpleActivityShape.NetronLibraryKey, new PointF(0, 0));
            control.GraphControl.AddShape(SequentialActivityShape.NetronLibraryKey, new PointF(0, 0));
            control.GraphControl.AddShape(CompositeActivityShape.NetronLibraryKey, new PointF(0, 0));
            control.GraphControl.AddShape(ParallelActivityShape.NetronLibraryKey, new PointF(0, 0));

            ShapeCollection shapeCollection = control.GraphControl.Shapes;
            Assert.AreEqual(4, shapeCollection.Count);
            Assert.AreEqual(1, shapeCollection.OfType<SimpleActivityShape>().Count());
            Assert.AreEqual(1, shapeCollection.OfType<SequentialActivityShape>().Count());
            Assert.AreEqual(1, shapeCollection.OfType<CompositeActivityShape>().Count());
            Assert.AreEqual(1, shapeCollection.OfType<ParallelActivityShape>().Count());
        }
    }
}