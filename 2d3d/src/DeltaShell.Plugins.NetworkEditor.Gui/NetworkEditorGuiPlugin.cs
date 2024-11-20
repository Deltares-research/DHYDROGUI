using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors;
using DeltaShell.Plugins.NetworkEditor.Gui.Export;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using Mono.Addins;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using PropertyInfo = DelftTools.Shell.Gui.PropertyInfo;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    [Extension(typeof(IPlugin))]
    public class NetworkEditorGuiPlugin : GuiPlugin
    {
        private const string AddGroupToolStripMenuKey = "AddGroup";
        private const string RemoveGroupToolStripMenuKey = "RemoveGroup";
        private const string RemoveUngroupedToolStripMenuKey = "RemoveUngrouped";
        private const string SeparatorToolStripMenuKey = "Separator";
        private ContextMenuStrip hydroRegionContextMenu;
        private IGui gui;
        private bool disposed;

        public NetworkEditorGuiPlugin()
        {
            InitializeComponent();
            Instance = this;
            MapLayerProvider = NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider();
        }

        public override string Name
        {
            get
            {
                return "Network (UI)";
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Hydro Region Plugin (UI)";
            }
        }

        public override string Description
        {
            get
            {
                return "Provides network editing functionality";
            }
        }

        public override string Version
        {
            get
            {
                return AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;
            }
        }

        public override string FileFormatVersion => "3.5.0.0";

        public override IGui Gui
        {
            get
            {
                return gui;
            }
            set
            {
                if (gui != null)
                {
                    gui.SelectionChanged -= GuiSelectionChanged;
                }

                gui = value;

                if (gui != null)
                {
                    gui.SelectionChanged += GuiSelectionChanged;
                }
            }
        }

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get
            {
                return new Ribbon(Gui);
            }
        }

        public override IMapLayerProvider MapLayerProvider { get; }

        public static NetworkEditorGuiPlugin Instance { get; private set; }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<HydroRegion, HydroRegionProperties>();
            yield return new PropertyInfo<IFeatureCoverage, FeatureCoverageProperties>();
            yield return new PropertyInfo<HydroArea, HydroAreaProperties>();
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<IStructureObject, AreaStructureView>()
            {
                Description = "Structure Editor"
            };

            yield return CreateAreaStructureCollectionViewInfo<Pump>(HydroAreaLayerNames.PumpsPluralName);
            yield return CreateAreaStructureCollectionViewInfo<Structure>(HydroAreaLayerNames.StructuresPluralName);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.LandBoundaries);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.DryPoints);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.DryAreas);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.ThinDams);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.FixedWeirs);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.ObservationPoints);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.ObservationCrossSections);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.Enclosures);
            yield return GetViewInfoForHydroAreaFeatureCollection(ha => ha.BridgePillars);
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new HydroRegionShapeFileExporter(Gui);
        }

        public override void Activate()
        {
            if (Gui.DocumentViews.ActiveView != null)
            {
                AddHydroRegionEditorMapTool();
            }

            Gui.SelectionChanged += GuiSelectionChanged;

            base.Activate();
        }

        public override void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            if (Gui != null)
            {
                Gui.SelectionChanged -= GuiSelectionChanged;
            }

            base.Deactivate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Instance = null;
            }

            base.Dispose(disposing);
            disposed = true;
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            if (data is HydroRegion)
            {
                return new MenuItemContextMenuStripAdapter(hydroRegionContextMenu);
            }

            return null;
        }

        public override bool CanDrop(object source, object target)
        {
            return false;
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new HydroRegionProjectTreeViewNodePresenter { GuiPlugin = this };
            yield return new HydroAreaProjectTreeViewNodePresenter { GuiPlugin = this };
        }

        public override void OnViewAdded(IView view)
        {
            OnDocumentViewAdded(view);
        }

        public override void OnViewRemoved(IView view)
        {
            if (view is MapView mapView)
            {
                HydroRegionEditorHelper.RemoveHydroRegionEditorMapTool(mapView.MapControl);
            }

            //if the view contains a mapview remove the tool from the mapcontrol..
            if (view is ICompositeView compositeView)
            {
                compositeView.ChildViews
                             .OfType<MapView>()
                             .ForEach(mv => HydroRegionEditorHelper.RemoveHydroRegionEditorMapTool(mv.MapControl));
            }
        }

        public override void OnActiveViewChanged(IView view)
        {
            AddHydroRegionEditorMapTool();
        }

        internal static MapView GetFocusedMapView()
        {
            IView viewToSearch = Instance.Gui?.DocumentViews?.ActiveView;
            return viewToSearch.GetViewsOfType<MapView>().FirstOrDefault();
        }

        private ViewInfo GetViewInfoForHydroAreaFeatureCollection<TFeature>(Func<HydroArea, IEventedList<TFeature>> getCollection)
        {
            ViewInfo<IEnumerable<TFeature>, ILayer, VectorLayerAttributeTableView> viewInfo1 = SharpMapGisGuiPlugin.CreateAttributeTableViewInfo(getCollection, () => Gui);

            ViewInfo<IEnumerable<TFeature>, ILayer, VectorLayerAttributeTableView> viewInfo = SharpMapGisGuiPlugin.CreateAttributeTableViewInfo(getCollection, () => Gui);
            viewInfo.AfterCreate = (view, features) =>
            {
                viewInfo1.AfterCreate(view, features);
                view.CanAddDeleteAttributes = false;
                view.DynamicAttributeVisible = s => s == Feature2D.LocationKey;

                if (typeof(TFeature).Implements(typeof(IGroupableFeature)))
                {
                    CreateAddRemoveContextMenu(features, view.TableView.RowContextMenu.Items);
                    view.TableView.RowContextMenu.Opening += (f, v) => UpdateContextMenu(features, view.TableView.RowContextMenu.Items);
                }
            };

            return viewInfo;
        }

        private ViewInfo<IEventedList<T>, ILayer, VectorLayerAttributeTableView> CreateAreaStructureCollectionViewInfo<T>(string name) where T : IStructureObject
        {
            return new ViewInfo<IEventedList<T>, ILayer, VectorLayerAttributeTableView>
            {
                Description = name,
                AdditionalDataCheck = o => o != null,
                GetViewData = o =>
                {
                    ProjectItemMapView centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => v.MapView.GetLayerForData(o) != null);
                    return centralMap?.MapView.GetLayerForData(o);
                },
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => Gui.Application.ModelService.GetModelsByData(Gui.Application.ProjectService.Project, o).FirstOrDefault(),
                AfterCreate = (view, features) =>
                {
                    ProjectItemMapView centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(vi => vi.MapView.GetLayerForData(features) != null);
                    if (centralMap == null)
                    {
                        return;
                    }

                    view.DeleteSelectedFeatures = () => centralMap.MapView.MapControl.DeleteTool.DeleteSelection();
                    view.OpenViewMethod = ob => Gui.CommandHandler.OpenView(ob);
                    view.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(feature);
                    view.CanAddDeleteAttributes = false;
                    ConfigureAreaFeatureRowCreation<T>(view);

                    if (typeof(T).Implements(typeof(IGroupableFeature)))
                    {
                        CreateAddRemoveContextMenu(features, view.TableView.RowContextMenu.Items);
                        view.TableView.RowContextMenu.Opening += (f, v) => UpdateContextMenu(features, view.TableView.RowContextMenu.Items);
                    }
                }
            };
        }

        private void CreateAddRemoveContextMenu<TFeature>(IEnumerable<TFeature> features, ToolStripItemCollection stripItemCollection) //where TFeature : IGroupableFeature
        {
            var eventedList = (IEventedList<TFeature>)features;

            // add separator
            var separator = new ToolStripSeparator() { Name = SeparatorToolStripMenuKey };
            stripItemCollection.Add(separator);

            // add addgroup
            var addAreaItemsGroup = new ToolStripMenuItem
            {
                Name = AddGroupToolStripMenuKey,
                Text = Properties.Resources.NetworkEditorGuiPlugin_GetViewInfoForHydroAreaFeatureCollection_Add_group,
                Enabled = Gui.CommandHandler.CanImportOn(eventedList)
            };
            addAreaItemsGroup.Click += (s, e) => { Gui.CommandHandler.ImportOn(eventedList); };
            stripItemCollection.Add(addAreaItemsGroup);

            // add remove by group
            var removeAreaItemsGroup = new ToolStripMenuItem
            {
                Name = RemoveGroupToolStripMenuKey,
                Text = Properties.Resources.NetworkEditorGuiPlugin_CreateAreaStructureCollectionViewInfo_Remove_group,
                Visible = eventedList.OfType<IGroupableFeature>().Select(g => g.GroupName).Any(name => !string.IsNullOrWhiteSpace(name))
            };

            removeAreaItemsGroup.DropDownOpening += (sender, args) =>
            {
                removeAreaItemsGroup.DropDownItems.Clear();

                IEnumerable<string> groupableList = eventedList.OfType<IGroupableFeature>().Select(g => g.GroupName).Where(name => !string.IsNullOrWhiteSpace(name));
                List<string> groups = groupableList.Distinct().ToList();
                foreach (string groupName in groups)
                {
                    var groupMenuItem = new ToolStripMenuItem { Text = groupName };

                    groupMenuItem.Click += (s, e) => eventedList.RemoveGroup(groupName);
                    removeAreaItemsGroup.DropDownItems.Add(groupMenuItem);
                }
            };
            stripItemCollection.Add(removeAreaItemsGroup);

            // add remove ungrouped
            var removeUngroupedItems = new ToolStripMenuItem()
            {
                Name = RemoveUngroupedToolStripMenuKey,
                Text = Properties.Resources.NetworkEditorGuiPlugin_CreateAddRemoveContextMenu_Remove_ungrouped,
                Visible = eventedList.OfType<IGroupableFeature>().Where(g => string.IsNullOrWhiteSpace(g.GroupName)).OfType<TFeature>().Any()
            };
            removeUngroupedItems.Click += (s, e) => eventedList.RemoveUngroupedItems();
            stripItemCollection.Add(removeUngroupedItems);
        }

        private void UpdateContextMenu<TFeature>(IEnumerable<TFeature> features, ToolStripItemCollection items)
        {
            var eventedList = (IEventedList<TFeature>)features;

            if (items.ContainsKey(AddGroupToolStripMenuKey))
            {
                ToolStripItem item = items[AddGroupToolStripMenuKey];
                item.Enabled = Gui.CommandHandler.CanImportOn(eventedList);
            }

            if (items.ContainsKey(RemoveGroupToolStripMenuKey))
            {
                ToolStripItem item = items[RemoveGroupToolStripMenuKey];
                item.Visible = eventedList.OfType<IGroupableFeature>().Select(g => g.GroupName).Any(name => !string.IsNullOrWhiteSpace(name));
            }

            if (items.ContainsKey(RemoveUngroupedToolStripMenuKey))
            {
                ToolStripItem item = items[RemoveUngroupedToolStripMenuKey];
                item.Visible = eventedList.OfType<IGroupableFeature>().Where(g => string.IsNullOrWhiteSpace(g.GroupName)).OfType<TFeature>().Any();
            }
        }

        private static void ConfigureAreaFeatureRowCreation<T>(VectorLayerAttributeTableView view) where T : IStructureObject
        {
            if (typeof(T) == typeof(Structure))
            {
                view.SetCreateFeatureRowFunction(feature => new FMWeirPropertiesRow((Structure)feature));
            }

            if (typeof(T) == typeof(Pump))
            {
                view.SetCreateFeatureRowFunction(feature => new FMPumpPropertiesRow((Pump)feature));
            }
        }

        private void InitializeComponent()
        {
            hydroRegionContextMenu = new ContextMenuStrip
            {
                Name = "addNewHydroRegion",
                Size = new Size(210, 48)
            };
        }

        private void OnDocumentViewAdded(IView view)
        {
            var coverageView = view as CoverageView;
            if (coverageView == null && view is ICompositeView compositeView)
            {
                coverageView = compositeView.ChildViews.OfType<CoverageView>().FirstOrDefault();
            }

            if (coverageView != null)
            {
                MapView mapView = coverageView.ChildViews.OfType<MapView>().FirstOrDefault();
                if (mapView == null)
                {
                    return;
                }

                IMap map = mapView.Map;

                if (coverageView.Coverage is IFeatureCoverage featureCoverage && !map.Layers.OfType<HydroRegionMapLayer>().Any())
                {
                    // add region
                    IRegion region = GetRegionForFeatureCoverage(featureCoverage);
                    if (region != null)
                    {
                        AddRegionLayer(region, map);
                    }
                }
            }
        }

        private void AddRegionLayer(IRegion region, IMap map)
        {
            ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(region, null, Gui.Plugins.Select(p => p.MapLayerProvider).ToList());
            map.DoWithLayerRecursive(layer, l =>
            {
                var layerToSet = l as Layer;
                if (layerToSet == null)
                {
                    return;
                }

                layerToSet.ReadOnly = true;
            });
            map.Layers.Add(layer);
        }

        private IRegion GetRegionForFeatureCoverage(IFeatureCoverage featureCoverage)
        {
            if (featureCoverage.Features.Any())
            {
                IFeature firstFeature = featureCoverage.Features[0];
                IEnumerable<IRegion> allRegions = Gui.Application.ProjectService.Project.GetAllItemsRecursive().OfType<IRegion>();
                IRegion matchingRegion = allRegions.FirstOrDefault(hr => hr.GetDirectChildren().Contains(firstFeature));
                return GetRootRegion(matchingRegion);
            }

            return null;
        }

        private IRegion GetRootRegion(IRegion matchingRegion)
        {
            if (matchingRegion == null)
            {
                return null;
            }

            return matchingRegion.Parent != null
                       ? GetRootRegion(matchingRegion.Parent)
                       : matchingRegion;
        }

        private void AddHydroRegionEditorMapTool()
        {
            IView activeView = Gui.DocumentViews.ActiveView;
            if (activeView?.Data == null)
            {
                return; // strange bug
            }

            if (activeView is MapView || activeView is ProjectItemMapView)
            {
                // when region is dragged onto an opened mapview
                MapView mapView = activeView is ProjectItemMapView view
                                      ? view.MapView
                                      : (MapView)activeView;

                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapView.MapControl);
            }
        }

        private void GuiSelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            //show network if selected
            if (Gui.Selection is IDataItem dataItem && typeof(IHydroRegion).IsAssignableFrom(dataItem.ValueType))
            {
                return;
            }

            //no network selected, so get it from the active view
            AddHydroRegionEditorMapTool();
        }
    }
}