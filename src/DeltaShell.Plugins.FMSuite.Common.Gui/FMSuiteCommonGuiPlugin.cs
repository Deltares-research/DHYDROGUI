using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
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
            catch (Exception exception)
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
                if (valueObject is Model1DBoundaryNodeData)
                {
                    cmd.Execute(((Model1DBoundaryNodeData)valueObject).Feature);
                }
                if (valueObject is Model1DLateralSourceData)
                {
                    cmd.Execute(((Model1DLateralSourceData)valueObject).Feature);
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

        public override string FileFormatVersion
        {
            get { return "1.0.0.0"; }
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new Model1DBoundaryNodeDataProjectNodePresenter {GuiPlugin = this};
            yield return new Model1DLateralDataProjectNodePresenter {GuiPlugin = this};
        }

        public override IMenuItem GetContextMenu(object sender, object dataobject)
        {
            //TODO: method is a mess clean up.

            IFunction function;
            bool activeViewIsMapView =
                Gui != null && Gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().Count() == 1;

            if (dataobject is Model1DBoundaryNodeData)
            {
                //add zoom to functionality to context menu
                var waterFlowModel1DBoundaryNodeData = (Model1DBoundaryNodeData) dataobject;
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
            }
            else if (dataobject is Model1DLateralSourceData)
            {
                var waterFlowModel1DLateralSourceData = (Model1DLateralSourceData) dataobject;
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
            }
            else
            {
                return null;
            }

            var node = sender as TreeNode;
            if (node != null && node.Tag is IDataItem)
            {
                if (((IDataItem) node.Tag).Role != DataItemRole.Input)
                {
                    return null;
                }

                if (((IDataItem) node.Tag).IsLinked)
                {
                    return null;
                }
            }

            if (function == null || function is IVariable)
            {
                return null;
            }

            if (function.Arguments.Count > 0)
            {
                if (function.Arguments[0].ValueType != typeof(DateTime))
                {
                    return null;
                }
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

        public override IMapLayerProvider MapLayerProvider
        {
            get { return new FMSuiteCommonGuiMapLayerProvider(); }
        }

        public override IGui Gui
        {
            get { return base.Gui; }
            set
            {
                if (base.Gui != null)
                {
                    UnsubscribeToProjectEvents();
                    UnSubscribeToActivityEvents();
                }

                base.Gui = value;
                if (base.Gui != null)
                {
                    SubscribeToProjectEvents();
                    SubscribeToActivityEvents();
                    Gui.UndoRedoManager.Enabled = false;
                }
            }
        }


        private void SubscribeToProjectEvents()
        {
            base.Gui.Application.ProjectClosing += CloseAllViews;
            base.Gui.Application.ProjectSaving += CloseAllViewsBeforeSaving;
            base.Gui.Application.ProjectSaveFailed += OpenClosedViews;
            base.Gui.Application.ProjectSaved += OpenClosedViews;
        }

        private void CloseAllViews(Project obj)
        {
            CloseAllViews(obj, false);
        }

        private void SubscribeToActivityEvents()
        {
        }

        private void UnsubscribeToProjectEvents()
        {
            base.Gui.Application.ProjectSaving -= CloseAllViewsBeforeSaving;
            base.Gui.Application.ProjectSaveFailed -= OpenClosedViews;
            base.Gui.Application.ProjectSaved -= OpenClosedViews;
        }

        private void UnSubscribeToActivityEvents()
        {
        }
        private IDictionary<Type, IList<INameable>> ClosedViews { get; set; }

        private void CloseAllViewsBeforeSaving(Project project)
        {
            if (project == null || project.RootFolder == null) return;
            ClosedViews = new Dictionary<Type, IList<INameable>>();
            
            CloseAllViews(project, true);
        }

        private void CloseAllViews(Project project, bool saveViewList)
        {
            RemoveViewsOfType<Model1DBoundaryNodeDataViewWpf>(saveViewList);
            RemoveViewsOfType<Model1DLateralSourceDataViewWpf>(saveViewList);
        }

        private void RemoveViewsOfType<T>(bool saveViewList) where T:IView
        {
            var views = Gui.DocumentViews.AllViews.OfType<T>().Cast<IView>().ToList();
            if (views != null && saveViewList)
                views.ForEach(view => ClosedViews[view.GetType()] = views.Where(v=> v.Data is INameable).Select(v => v.Data).Cast<INameable>().ToList());
            Gui.CommandHandler.RemoveAllViewsForItem(views.Select(c => c.Data));
            views.ForEach(view => Gui.DocumentViews.Remove(view));
        }

        private void OpenClosedViews(Project project)
        {
            if (project == null || project.RootFolder == null || ClosedViews == null) return;

            try
            {
                if (ClosedViews == null || !ClosedViews.Any()) return;
                ClosedViews.ForEach(v =>
                {
                    v.Value.ForEach(o =>
                    {
                        //search same object in current model
                        project.RootFolder.GetAllItemsRecursive().OfType<IModel>().ForEach(model =>
                        {
                            //var items = model.GetAllItemsRecursive();
                            //var o1 = items.OfType<INameable>().FirstOrDefault(i => i.Name.Equals(o.Name));
                            //if (o1 != null)
                                Gui.CommandHandler.OpenView(o, v.Key);
                        });
                    });
                });
                
                ClosedViews = new Dictionary<Type, IList<INameable>>();
            }
            catch
            {
                //gulp
                ClosedViews = new Dictionary<Type, IList<INameable>>();
            }
        }
    }
}
