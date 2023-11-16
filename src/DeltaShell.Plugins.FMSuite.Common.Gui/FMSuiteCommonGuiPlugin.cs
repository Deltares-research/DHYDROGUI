using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Mono.Addins;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    [Extension(typeof(IPlugin))]
    public class FMSuiteCommonGuiPlugin : GuiPlugin
    {
        private ClonableToolStripMenuItem generateDataInSeriesToolStripMenuItem;
        private ClonableToolStripMenuItem zoomToToolStripMenuItem;
        private ContextMenuStrip generateDataMenu;

        public FMSuiteCommonGuiPlugin()
        {
            Initialize();
        }

        private void Initialize()
        {
            generateDataInSeriesToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.add,
                Name = "generateDataInSeriesToolStripMenuItem",
                Text = "Generate Data in Series..."
            };

            zoomToToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Name = "zoomToToolStripMenuItem",
                Size = new Size(201, 22),
                Text = "Zoom to Feature"
            };

            generateDataInSeriesToolStripMenuItem.Click += GenerateDataInSeriesToolStripMenuItemClick;
            zoomToToolStripMenuItem.Click += ZoomToToolStripMenuItemClick;

            generateDataMenu = new ContextMenuStrip
            {
                Name = "generateDataMenu",
                Size = new Size(202, 48)
            };
            generateDataMenu.Items.AddRange(new ToolStripItem[] { generateDataInSeriesToolStripMenuItem, zoomToToolStripMenuItem });

        }

        private void GenerateDataInSeriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            // The function that was right-clicked on is in the menu item Tag
            var function = (IFunction)generateDataInSeriesToolStripMenuItem.Tag;
            var variable = function.Arguments[0];

            try
            {
                var wizard = new GenerateDataSeriesWizard(function, variable);

                if (variable.ValueType == typeof(DateTime))
                {
                    var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    wizard.StartValue = time;
                    wizard.EndValue = time + new TimeSpan(1, 0, 0, 0);
                    wizard.IntervalValue = new TimeSpan(0, 10, 0);
                    wizard.Value = function.Components[0].DefaultValue;
                }
                wizard.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

        private void ZoomToToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Send selection to all relevant views
            if ((Gui.Selection is DataItem) || (Gui.Selection == null))
            {
                var cmd = new MapZoomToFeatureCommand();
                var valueObject = ((DataItem)Gui.Selection).Value;
                switch (valueObject)
                {
                    case Model1DBoundaryNodeData model1DBoundaryNodeData:
                        cmd.Execute(model1DBoundaryNodeData.Feature);
                        break;
                    case Model1DLateralSourceData lateralSourceData:
                        cmd.Execute(lateralSourceData.Feature);
                        break;
                }
            }
        }

        public override string Name
        {
            get { return "FM Suite Common (Gui)"; }
        }

        public override string DisplayName
        {
            get { return "D-Flow Flexible Mesh Suite Common Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Common FM UI Forms and Tools."; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion => "1.0.0.0";

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new Model1DBoundaryNodeDataProjectNodePresenter {GuiPlugin = this};
            yield return new Model1DLateralDataProjectNodePresenter {GuiPlugin = this};
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            IFunction function;
            bool activeViewIsMapView =
                Gui != null && Gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().Count() == 1;

            switch (data)
            {
                case Model1DBoundaryNodeData model1DBoundaryNodeData:
                {
                    //add zoom to functionality to context menu
                    var waterFlowModel1DBoundaryNodeData = model1DBoundaryNodeData;
                    if (waterFlowModel1DBoundaryNodeData.IsLinked ||
                        waterFlowModel1DBoundaryNodeData.DataType == Model1DBoundaryNodeDataType.FlowConstant ||
                        waterFlowModel1DBoundaryNodeData.DataType == Model1DBoundaryNodeDataType.WaterLevelConstant)
                    {
                        if (activeViewIsMapView)
                        {
                            zoomToToolStripMenuItem.Available = true;
                            generateDataInSeriesToolStripMenuItem.Available = false;
                            return new MenuItemContextMenuStripAdapter(generateDataMenu);
                        }

                        return null;
                    }

                    function = waterFlowModel1DBoundaryNodeData.Data;
                    break;
                }
                case Model1DLateralSourceData lateralSourceData:
                {
                    var waterFlowModel1DLateralSourceData = lateralSourceData;
                    if (waterFlowModel1DLateralSourceData.IsLinked ||
                        waterFlowModel1DLateralSourceData.DataType == Model1DLateralDataType.FlowConstant)
                    {
                        if (activeViewIsMapView)
                        {
                            zoomToToolStripMenuItem.Available = true;
                            generateDataInSeriesToolStripMenuItem.Available = false;
                            return new MenuItemContextMenuStripAdapter(generateDataMenu);
                        }

                        return null;
                    }

                    function = waterFlowModel1DLateralSourceData.Data;
                    break;
                }
                default:
                    return null;
            }

            if (sender is TreeNode node && 
                node.Tag is IDataItem dataItem && 
                (dataItem.Role != DataItemRole.Input || dataItem.LinkedTo != null))
            {
                return null;
            }

            if (function == null || function is IVariable)
            {
                return null;
            }

            if (function.Arguments.Count > 0 && 
                function.Arguments[0].ValueType != typeof(DateTime))
            {
                return null;
            }

            zoomToToolStripMenuItem.Available = activeViewIsMapView;
            generateDataInSeriesToolStripMenuItem.Available = true;
            generateDataInSeriesToolStripMenuItem.Tag = function;
            // only support dataserieswizard for function with one argument for now.
            generateDataInSeriesToolStripMenuItem.Enabled = (function.Arguments.Count == 1);
            return new MenuItemContextMenuStripAdapter(generateDataMenu);

        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<Model1DBoundaryNodeData, Model1DBoundaryNodeDataViewWpf>
            {
                AdditionalDataCheck = data => (data.Node is IHydroNode && data.OutletCompartment == null) || (data.Node is IManhole manhole && manhole.OutletCompartments().Any()),
                Description = "Boundary Node Data View (Flow 1D)"
            };
            yield return new ViewInfo<Model1DLateralSourceData, Model1DLateralSourceDataViewWpf>
            {
                Description = "Lateral Source Data View (Flow 1D)"
            };
        }

        public override ResourceManager Resources { get; set; }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<Model1DBoundaryNodeData, Model1DBoundaryNodeDataProperties>();
            yield return new PropertyInfo<Model1DLateralSourceData, Model1DLateralDataProperties>();
        }
    }
}
