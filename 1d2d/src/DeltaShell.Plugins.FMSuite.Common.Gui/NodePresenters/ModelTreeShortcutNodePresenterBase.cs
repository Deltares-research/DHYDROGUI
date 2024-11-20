using System.Collections;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    /// <summary>
    /// Base class for a <see cref="ITreeNodePresenter"/> for a <see cref="ModelTreeShortcut"/>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="ModelTreeShortcut"/></typeparam>
    public abstract class ModelTreeShortcutNodePresenterBase<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : ModelTreeShortcut
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T nodeData)
        {
            node.Text = nodeData.Text;
            node.Image = nodeData.Image;
        }

        public override IEnumerable GetChildNodeObjects(T parentNodeData, ITreeNode node)
        {
            if (parentNodeData.ChildObjects == null || !parentNodeData.ChildObjects.Any())
            {
                return base.GetChildNodeObjects(parentNodeData, node);
            }

            return parentNodeData.ChildObjects;
        }

        public override bool OnNodeDoubleClicked(object nodeData)
        {
            var shortcut = nodeData as T;
            if (shortcut == null) return true;

            switch (shortcut.ShortCutType)
            {
                case ShortCutType.SettingsTab:
                    return true;
                case ShortCutType.Grid:
                    Gui?.CommandHandler?.OpenView(shortcut.Model);
                    Gui?.MainWindow?.SetActiveRibbonTab("Grid");
                    return false;
                case ShortCutType.SpatialCoverage:
                    ConfigureSpatialEditor(shortcut);
                    return true;
                case ShortCutType.FeatureSet:
                    return true;
                default:
                    return true;
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menuBase = base.GetContextMenu(sender, nodeData);
            var menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);
            if (menuBase != null)
                menu.Add(menuBase);

            var shortcut = nodeData as ModelTreeShortcut;
            if (shortcut == null || shortcut.ShortCutType == ShortCutType.SettingsTab) return menu;

            var menu1 = ContextMenuFactory.CreateMenuFor(shortcut.Data, Gui, this, sender);
            menu.Add(new MenuItemContextMenuStripAdapter(menu1));
            return menu;
        }

        protected ProjectItemMapView GetProjectItemMapView(ModelTreeShortcut shortcut)
        {
            return Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(mv => ContainsModel(mv.Data, shortcut.Model));
        }

        private void ConfigureSpatialEditor(ModelTreeShortcut shortcut)
        {
            var projectItemMapView = GetProjectItemMapView(shortcut);
            if (projectItemMapView == null) return;

            var layer = projectItemMapView.MapView.Map.GetAllLayers(true).FirstOrDefault(l => MatchLayerForCoverage(l, shortcut.Data as ICoverage));

            if (layer != null)
            {
                projectItemMapView.SetSpatialOperationLayer(layer, true);
                SharpMapGisGuiPlugin.Instance.FocusSpatialOperationView();
            }
        }

        private static bool ContainsModel(object viewData, IModel model)
        {
            if (Equals(viewData, model)) return true;

            var compositeActivity = viewData as ICompositeActivity;
            return compositeActivity != null && compositeActivity.GetAllActivitiesRecursive<IModel>().Contains(model);
        }

        /// <summary>
        /// Find the layer that contains the coverage, but return the spatial operation set layer when spatial operations
        /// have been used on the original layer.
        /// </summary>
        private static bool MatchLayerForCoverage(ILayer l, ICoverage coverage)
        {
            var covLayer = l as ICoverageLayer;
            if (covLayer != null)
            {
                // for spatialoperationsetlayer, one will be a clone of the other and
                // we should not compare references. Therefore we will check on unique 
                // properties, in this case: Coverage.Name..
                return Equals(covLayer.Coverage.Name, coverage.Name);
            }

            var setLayer = l as SpatialOperationSetLayer;
            if (setLayer != null)
            {
                return MatchLayerForCoverage(setLayer.InputLayer, coverage);
            }

            return false;
        }
    }
}