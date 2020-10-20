using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Mono.Addins;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    [Extension(typeof(IPlugin))]
    public class WaveGuiPlugin : GuiPlugin
    {
        private static Func<MapView> getActiveMapViewFunc;
        private IMapLayerProvider mapLayerProvider;
        private string _wavesSettings = " (Waves settings)";

        public WaveGuiPlugin()
        {
            getActiveMapViewFunc = GetActiveMapView;
        }

        public override string Name => "Delft3D Wave (Gui)";

        public override string DisplayName => "D-Waves Plugin (UI)";

        public override string Description => "A 2D/3D Waves module";

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "1.1.0.0";

        public override IMapLayerProvider MapLayerProvider =>
            mapLayerProvider
            ?? (mapLayerProvider = WaveMapLayerProviderFactory.ConstructMapLayerProvider(GetWaveModels));

        public override IRibbonCommandHandler RibbonCommandHandler => new Ribbon.Ribbon();

        public override IGui Gui
        {
            get => base.Gui;
            set
            {
                if (base.Gui != null)
                {
                    base.Gui.Application.ActivityRunner.ActivityStatusChanged -= OnActivityRunnerStatusChanged;
                }

                base.Gui = value;
                if (base.Gui != null)
                {
                    base.Gui.Application.ActivityRunner.ActivityStatusChanged += OnActivityRunnerStatusChanged;
                }

                // HACK: setting the Gui happens just before Activate in DeltaShellGui, 
                // hence we set the spatial operations flag here:
                if (value != null)
                {
                    SharpMapGisGuiPlugin.Instance.SpatialOperationsEnabled = true;
                }
            }
        }

        public static MapView ActiveMapView => getActiveMapViewFunc();

        public override bool CanCopy(IProjectItem item)
        {
            if (item is WaveModel)
            {
                return false;
            }

            return true;
        }

        public override bool CanCut(IProjectItem item)
        {
            return CanCopy(item);
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<WaveModel, WpfSettingsView>
            {
                Description = "Wave settings",
                GetViewName = (v, o) => o.Name + _wavesSettings,
                AfterCreate = (v, o) =>
                {
                    //Set the properties.
                    v.SettingsCategories = WaveSettingsHelper.GetWpfGuiCategories(o, Gui);
                    v.GetChangedPropertyName = (sender, propertyName) =>
                        (sender as WaveModelProperty)?.PropertyDefinition.FilePropertyName;
                }
            };

            // observation points
            ViewInfo<IEventedList<Feature2DPoint>, ILayer, VectorLayerAttributeTableView> obsPointViewInfo =
                FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2DPoint, WaveModel>(
                    "Observation Points (Waves)", m => m.ObservationPoints, () => Gui);
            yield return obsPointViewInfo;
            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(obsPointViewInfo, o => o.Data,
                                                                       o => o.ShortCutType == ShortCutType.FeatureSet);

            yield return ViewInfoWrapper<Feature2DPoint>.Create(obsPointViewInfo,
                                                                o =>
                                                                    WaveModels
                                                                        .First(m => m.ObservationPoints.Contains(o))
                                                                        .ObservationPoints,
                                                                o =>
                                                                    WaveModels.Any(
                                                                        m => m.ObservationPoints.Contains(o)));

            // obs. cross sections  
            ViewInfo<IEventedList<Feature2D>, ILayer, VectorLayerAttributeTableView> obsCrossSectionViewInfo =
                FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaveModel>(
                    "Observation Cross Section (Waves)",
                    m => m.ObservationCrossSections,
                    () => Gui);
            yield return obsCrossSectionViewInfo;
            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(obsCrossSectionViewInfo, o => o.Data,
                                                                       o => o.ShortCutType == ShortCutType.FeatureSet);

            yield return ViewInfoWrapper<Feature2D>.Create(obsCrossSectionViewInfo,
                                                           o => WaveModels
                                                                .First(m => m.ObservationCrossSections.Contains(o))
                                                                .ObservationCrossSections,
                                                           o => WaveModels.Any(
                                                               m => m.ObservationCrossSections.Contains(o)));

            // time points
            var timePointViewInfo = new ViewInfo<WaveInputFieldData, WaveTimePointEditor>
            {
                Description = "Time Point Editor",
                GetViewName =
                    (v, o) => "Time Point Editor (" + WaveModels.First(m => Equals(o, m.TimePointData)).Name + ")",
                AdditionalDataCheck = o => WaveModels.Any(m => Equals(o, m.TimePointData)),
                AfterCreate = (v, o) =>
                {
                    WaveModel model = WaveModels.First(m => Equals(o, m.TimePointData));
                    v.ImportFileIntoModelDirectory = model.ImportIntoModelDirectory;
                }
            };
            yield return timePointViewInfo;
            ViewInfo fromTreeShortcut = ViewInfoWrapper<WaveModelTreeShortcut>.Create(
                timePointViewInfo, o => o.Data,
                o => o.Data is WaveInputFieldData && o.ShortCutType == ShortCutType.FeatureSet);
            fromTreeShortcut.CloseForData = (v, o) => o.Equals(v.Data);

            yield return fromTreeShortcut;

            // Spatially varying boundary editor
            var boundaryViewInfo = new ViewInfo<IWaveBoundary, WaveBoundaryConditionEditorView>()
            {
                Description = Properties.Resources.WaveGuiPlugin_Spatially_Varying_Boundary_Editor,
                GetViewName = (v, o) =>
                    string.Format(Properties.Resources.WaveGuiPlugin_Boundary_Editor____0___, o.Name),
                AdditionalDataCheck = o => WaveModels.Any(m => m.BoundaryContainer.Boundaries.Contains(o)),
                AfterCreate = (view, data) =>
                {
                    WaveModel model =
                        WaveModels.FirstOrDefault(m => m.BoundaryContainer.Boundaries.Contains(data));

                    if (model == null)
                    {
                        return;
                    }

                    var geometryFactory = new WaveBoundaryGeometryFactory(model.BoundaryContainer,
                                                                          model.BoundaryContainer);
                    var referenceDateTimeProvider = new ModelDefinitionReferenceDateTimeProvider(model.ModelDefinition);

                    var geometryPreviewConfigurator = new GeometryPreviewMapConfigurator(geometryFactory,
                                                                                         new WaveLayerInstanceCreator(),
                                                                                         model.CoordinateSystem);

                    view.DataContext = new WaveBoundaryConditionEditorViewModel(data,
                                                                                geometryPreviewConfigurator,
                                                                                referenceDateTimeProvider);
                },
                CloseForData = (v, o) => v.Data.Equals(o)
            };

            yield return boundaryViewInfo;
            yield return ViewInfoWrapper<BoundaryLineFeature>.Create(boundaryViewInfo,
                                                                     f => f.ObservedWaveBoundary);

            // obstacles
            var obstacleViewInfo = new ViewInfo<IEventedList<WaveObstacle>, WaveObstacleListView>()
            {
                Description = "Obstacles",
                GetViewName = (v, o) => "Obstacles",
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => WaveModels.FirstOrDefault(m => m.Obstacles.Equals(o)),
                AfterCreate = (v, o) => v.RemoveObstacles = obs =>
                {
                    WaveModel model = WaveModels.FirstOrDefault(m => m.Obstacles.Equals(o));
                    if (model != null)
                    {
                        obs.ForEach(f => model.Obstacles.Remove(f));
                    }
                }
            };
            yield return obstacleViewInfo;
            yield return ViewInfoWrapper<WaveObstacle>.Create(obstacleViewInfo, o =>
            {
                WaveModel model = WaveModels.FirstOrDefault(m => m.Obstacles.Contains(o));
                return model != null ? model.Obstacles : null;
            });
            yield return ViewInfoWrapper<Feature2D>.Create(obstacleViewInfo, FindObstaclesForFeature, IsModelObstacle);
            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(obstacleViewInfo, o => o.Data,
                                                                       o => o.WaveModel.Obstacles.Equals(o.Data) &&
                                                                            o.ShortCutType == ShortCutType.FeatureSet);

            yield return new ViewInfo<WaveModelTreeShortcut, WaveModel, WpfSettingsView>
            {
                Description = "Wave settings",
                GetViewName = (v, o) => o.Name + _wavesSettings,
                GetViewData = o => o.WaveModel,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as WaveModelTreeShortcut;
                    if (shortcut == null)
                    {
                        return;
                    }

                    v.EnsureVisible(shortcut.Data);
                },
                AfterCreate = (v, o) => ConfigureWpfSettingsView(v, o.WaveModel)
            };

            yield return new ViewInfo<WaveValidationShortcut, WaveModel, WpfSettingsView>
            {
                Description = "Wave settings",
                GetViewName = (v, o) => o.Name + _wavesSettings,
                GetViewData = o => o.WaveModel,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as WaveValidationShortcut;
                    if (shortcut == null)
                    {
                        return;
                    }

                    v.EnsureVisible(shortcut.TabName);
                },
                AfterCreate = (v, o) => ConfigureWpfSettingsView(v, o.WaveModel)
            };

            yield return new ViewInfo<WaveModel, ValidationView>
            {
                Description = "Validation Report",
                GetViewName = (v, o) => "Validation Report (" + o.Name + ")",
                Image = Common.Gui.Properties.Resources.validation,
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    v.OnValidate = d => new WaveModelValidator().Validate(d as WaveModel, d as WaveModel);
                }
            };
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<GridBaseLayer, DynamicObjectProperties>();
            yield return new PropertyInfo<WaveModel, DynamicObjectProperties>();
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new WaveModelNodePresenter(this);
            yield return new WaveDomainNodePresenter(
                d => WaveModels.FirstOrDefault(m => WaveDomainHelper.GetAllDomains(m.OuterDomain).Contains(d)));
            yield return new WavmFileFunctionStoreNodePresenter {GuiPlugin = this};
            yield return new WaveModelTreeShortcutNodePresenter {GuiPlugin = this};

            IBoundaryContainer GetBoundaryContainerFromBoundaryFunc(IWaveBoundary boundary) =>
                WaveModels.Select(wm => wm.BoundaryContainer)
                          .FirstOrDefault(bc => bc.Boundaries.Contains(boundary));

            yield return new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainerFromBoundaryFunc);
            yield return new WaveOutputDataNodePresenter { GuiPlugin = this};
        }

        public override void OnActiveViewChanged(IView view)
        {
            if (view == null)
            {
                return;
            }

            MapView mapView = FindMapView(view);
            if (mapView != null)
            {
                WaveMapViewDecorator.AddMapToolsIfMissing(mapView);
            }
        }

        private IEnumerable<WaveModel> WaveModels => Gui != null
                                                         ? Gui.Application.GetAllModelsInProject().OfType<WaveModel>()
                                                         : Enumerable.Empty<WaveModel>();

        private bool IsModelObstacle(Feature2D f)
        {
            return WaveModels.FirstOrDefault(m => m.Obstacles.Contains(f)) != null;
        }

        private IEventedList<WaveObstacle> FindObstaclesForFeature(Feature2D f)
        {
            return WaveModels.First(m => m.Obstacles.Contains(f)) != null
                       ? WaveModels.First(m => m.Obstacles.Contains(f)).Obstacles
                       : null;
        }

        private IEnumerable<WaveModel> GetWaveModels() =>
            Gui?.Application?.GetAllModelsInProject().OfType<WaveModel>() ??
            Enumerable.Empty<WaveModel>();

        private MapView GetActiveMapView()
        {
            if (Gui == null || Gui.DocumentViews == null)
            {
                return null;
            }

            IView activeView = Gui.DocumentViews.ActiveView;
            if (activeView == null)
            {
                return null;
            }

            MapView mapView = FindMapView(activeView);
            if (mapView != null)
            {
                return mapView;
            }

            return null;
        }

        private static MapView FindMapView(IView activeView)
        {
            var mapView = activeView as MapView;

            if (mapView != null)
            {
                return mapView;
            }

            var compositeView = activeView as ICompositeView;

            //todo: recursion
            return compositeView?.ChildViews.OfType<MapView>().FirstOrDefault();
        }

        [InvokeRequired]
        private void OnActivityRunnerStatusChanged(object sender,
                                                   ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (sender is FileImportActivity)
            {
                IFileImporter importer = ((FileImportActivity) sender).FileImporter;
                if (importer is WaveModelFileImporter &&
                    activityStatusChangedEventArgs.NewStatus == ActivityStatus.Finished)
                {
                    Gui.MainWindow.ProjectExplorer.TreeView.Refresh();

                    if (ActiveMapView != null)
                    {
                        ActiveMapView.Map.ZoomToExtents();
                    }
                }

                if (importer is WaveGridFileImporter &&
                    activityStatusChangedEventArgs.NewStatus == ActivityStatus.Finished && ActiveMapView != null)
                {
                    ActiveMapView.Map.ZoomToExtents();
                }
            }

            if (!(sender is WaveModel) || activityStatusChangedEventArgs.NewStatus != ActivityStatus.Failed)
            {
                return;
            }

            Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
        }

        private void ConfigureWpfSettingsView(WpfSettingsView view, WaveModel waveModel)
        {
            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaveSettingsHelper.GetWpfGuiCategories(waveModel, Gui);

            // Look for the time properties to synchronize the model updates with
            IEnumerable<WpfGuiProperty> guiProperties = wpfGuiCategories.SelectMany(gp => gp.Properties).ToArray();

            WpfGuiProperty[] propertiesToSynchronize =
            {
                guiProperties.Single(prop => string.Equals(prop.Label, Properties.Resources.WaveSettingsHelper_GetWaveSettings_Coupling_time_step)),
                guiProperties.Single(prop => string.Equals(prop.Label, Properties.Resources.WaveSettingsHelper_GetWaveSettings_Coupling_start_time)),
                guiProperties.Single(prop => string.Equals(prop.Label, Properties.Resources.WaveSettingsHelper_GetWaveSettings_Coupling_stop_time))
            };

            view.SettingsCategories = wpfGuiCategories;
            view.SetSynchronizedProperties(propertiesToSynchronize);
            view.GetChangedPropertyName = (sender, propertyName) =>
                (sender as WaveModelProperty)?.PropertyDefinition.FilePropertyName;
        }
    }
}