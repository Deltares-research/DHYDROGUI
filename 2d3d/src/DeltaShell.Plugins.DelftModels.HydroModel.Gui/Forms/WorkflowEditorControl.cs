using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes;
using Netron.GraphLib;
using Netron.GraphLib.UI;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class WorkflowEditorControl : UserControl
    {
        private IEventedList<ICompositeActivity> workflows;
        private ICompositeActivity currentWorkflow;

        [Category("Property Changed")]
        [Description("Indicates when 'CurrentWorkflow' property has changed due to user input.")]
        [Browsable(true)]
        public event EventHandler<EventArgs> CurrentWorkflowChanged;

        [Category("Property Selected")]
        [Description("Indicates when 'CurrentWorkflow' property has been selected by user.")]
        [Browsable(true)]
        public event EventHandler<EventArgs<IActivity>> SelectedActivityChanged;

        public WorkflowEditorControl()
        {
            InitializeComponent();

            graphControl.AddLibrary(GetType().Module.FullyQualifiedName);
            graphControl.NetronGraph.EnableContextMenu = false;
            graphControl.NetronGraph.OnShowProperties += NetronGraphOnShowProperties;
        }

        public GraphControl GraphControl
        {
            get
            {
                return graphControl.NetronGraph;
            }
        }

        public IEventedList<ICompositeActivity> Workflows
        {
            get
            {
                return workflows;
            }
            set
            {
                if (workflows != null)
                {
                    workflows.CollectionChanged -= WorkflowsOnCollectionChanged;
                }

                workflows = value;
                SetWorkflowSelectionListBoxItems();

                if (workflows != null)
                {
                    workflows.CollectionChanged += WorkflowsOnCollectionChanged;

                    if (CurrentWorkflow != null)
                    {
                        // CurrentWorkflow is preset:
                        workflowSelectionListBox.SelectedItem = workflows.Contains(CurrentWorkflow) ? CurrentWorkflow : null;
                    }
                    else
                    {
                        CurrentWorkflow = workflows.Any() ? workflows[0] : null;
                    }
                }
            }
        }

        public ICompositeActivity CurrentWorkflow
        {
            get
            {
                return currentWorkflow;
            }
            set
            {
                // Allow the user to preset Current selected item:
                if (workflows != null && workflows.Contains(value))
                {
                    workflowSelectionListBox.SelectedItem = value;
                }
                else
                {
                    workflowSelectionListBox.SelectedItem = null;
                }

                if (currentWorkflow == value)
                {
                    if (currentWorkflow != null && SelectedActivityChanged != null)
                    {
                        SelectedActivityChanged(this, new EventArgs<IActivity>(currentWorkflow));
                    }

                    return;
                }

                graphControl.NetronGraph.NewDiagram(true);

                currentWorkflow = value;

                if (currentWorkflow != null)
                {
                    ActivityShapeBase shape = ShapeFactory.CreateShapeFromActivity(currentWorkflow, graphControl.NetronGraph);
                    graphControl.NetronGraph.Shapes.Add(shape);
                }

                if (CurrentWorkflowChanged != null)
                {
                    CurrentWorkflowChanged(this, new EventArgs());
                }
            }
        }

        private void SetWorkflowSelectionListBoxItems()
        {
            workflowSelectionListBox.Items.Clear();
            if (workflows != null)
            {
                workflowSelectionListBox.Items.AddRange(workflows.OfType<object>().ToArray());
            }
        }

        [InvokeRequired]
        private void WorkflowsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SetWorkflowSelectionListBoxItems();
        }

        private void WorkflowSelectionListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentWorkflow = (ICompositeActivity)workflowSelectionListBox.SelectedItem;
        }

        private void NetronGraphOnShowProperties(object sender, object[] props)
        {
            if (SelectedActivityChanged == null || props.Length != 1)
            {
                return;
            }

            var propertyBag = props[0] as PropertyBag;
            if (propertyBag == null)
            {
                return;
            }

            IActivity activity = ((ActivityShapeBase)propertyBag.Owner).Activity;
            var wrapper = activity as ActivityWrapper;
            if (wrapper != null)
            {
                activity = wrapper.Activity;
            }

            SelectedActivityChanged(this, new EventArgs<IActivity>(activity));
        }
    }
}