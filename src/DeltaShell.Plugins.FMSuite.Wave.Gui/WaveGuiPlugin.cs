using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Mono.Addins;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    [Extension(typeof(IPlugin))]
    public class WaveGuiPlugin : GuiPlugin
    {
        private WaveModelMapLayerProvider mapLayerProvider;

        public override string Name
        {
            get { return "Delft3D Wave (Gui)"; }
        }

        public override string DisplayName
        {
            get { return "D-Waves Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "A 2D/3D Waves module"; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public override bool CanCopy(IProjectItem item)
        {
            if (item is WaveModel)
                return false;
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
                    v.GetChangedPropertyName = (sender, propertyName) => (sender as WaveModelProperty)?.PropertyDefinition.FilePropertyName;
                }
            };

            // observation points
            var obsPointViewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2DPoint, WaveModel>("Observation Points (Waves)", m => m.ObservationPoints,() => Gui);
            yield return obsPointViewInfo;
            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(obsPointViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);

            yield return ViewInfoWrapper<Feature2DPoint>.Create(obsPointViewInfo,
                                                                o =>
                                                                WaveModels.First(m => m.ObservationPoints.Contains(o))
                                                                          .ObservationPoints,
                                                                o =>
                                                                WaveModels.Any(m => m.ObservationPoints.Contains(o)));

            // obs. cross sections  
            var obsCrossSectionViewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaveModel>("Observation Cross Section (Waves)",
                                                                                     m => m.ObservationCrossSections,
                                                                                     () => Gui);
            yield return obsCrossSectionViewInfo;
            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(obsCrossSectionViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);

            yield return ViewInfoWrapper<Feature2D>.Create(obsCrossSectionViewInfo,
                                                           o => WaveModels.First(m => m.ObservationCrossSections.Contains(o))
                                                                     .ObservationCrossSections,
                                                           o => WaveModels.Any(m => m.ObservationCrossSections.Contains(o)));
            
            // time points
            var timePointViewInfo = new ViewInfo<WaveInputFieldData, WaveTimePointEditor>
            {
                Description = "Time Point Editor",
                GetViewName = (v, o) => "Time Point Editor (" + WaveModels.First(m => Equals(o, m.TimePointData)).Name + ")",
                AdditionalDataCheck = o => WaveModels.Any(m => Equals(o, m.TimePointData)),
                AfterCreate = (v, o) =>
                {
                    var model = WaveModels.First(m => Equals(o, m.TimePointData));
                    v.ImportFileIntoModelDirectory = model.ImportIntoModelDirectory;
                    v.ExportToBoundaryConditions =
                        () => ExportTimesToBoundaryConditions(WaveModels.First(m => Equals(o, m.TimePointData)));
                    v.ImportFromBoundaryCondition =
                        () => ImportTimesFromBoundaryCondition(WaveModels.First(m => Equals(o, m.TimePointData)));
                }
            };
            yield return timePointViewInfo;
            var fromTreeShortcut = ViewInfoWrapper<WaveModelTreeShortcut>.Create(timePointViewInfo, o => o.Data, o => o.Data is WaveInputFieldData && o.ShortCutType == ShortCutType.FeatureSet);
            fromTreeShortcut.CloseForData = (v, o) => o.Equals(v.Data);

            yield return fromTreeShortcut;

            // boundary table view
            var boundaryListView = new ViewInfo<WaveModel, WaveBoundaryConditionListView>()
                {
                    Description = "Boundary Conditions",
                    GetViewName = (v, o) => "Boundary Conditions (" + o.Name + ")",
                    CompositeViewType = typeof (ProjectItemMapView),
                    GetCompositeViewData = o => o
                };
            yield return ViewInfoWrapper<IEventedList<WaveBoundaryCondition>>.Create(boundaryListView,
                o => WaveModels.First(
                    m => m.BoundaryConditions.Equals(o)), o => WaveModels.Any(m => m.BoundaryConditions.Equals(o)));

            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(boundaryListView, o => o.Model, o => o.WaveModel.BoundaryConditions.Equals(o.Data) && o.ShortCutType == ShortCutType.FeatureSet);
            
            // boundary condition editor
            var boundaryConditionViewInfo = new ViewInfo<WaveBoundaryCondition, WaveBoundaryConditionEditor>()
            {
                Description = "Boundary Condition Editor",
                GetViewName = (v, o) => "Boundary Condition (" + o.Name + ")",
                AdditionalDataCheck = o => WaveModels.Any(m => m.BoundaryConditions.Contains(o)),
                AfterCreate = (v, o) =>
                {
                    var model = WaveModels.First(m => m.BoundaryConditions.Contains(o));
                    v.BoundaryConditionEditor.BoundaryConditionFactory = new WaveBoundaryConditionFactory();

                    var controller = new WaveBoundaryConditionEditorController
                    {
                        ImportIntoModelDirectory = model.ImportIntoModelDirectory,
                        Model = model
                    };
                    v.BoundaryConditionEditor.Controller = controller;
                    v.BoundaryConditionEditor.BoundaryConditionPropertiesControl = new WaveBoundaryConditionPropertiesControl
                    {
                        Controller = controller
                    };

                    v.BoundaryConditionEditor.ShowSupportPointChainages = true;
                },
                CloseForData = (v, o) => v.Data.Equals(o)
            };
            yield return boundaryConditionViewInfo;
            yield return ViewInfoWrapper<Feature2D>.Create(boundaryConditionViewInfo, FindBoundaryConditionForFeature, IsModelBoundary);

            // obstacles
            var obstacleViewInfo = new ViewInfo<IEventedList<WaveObstacle>, WaveObstacleListView>()
                {
                    Description = "Obstacles",
                    GetViewName = (v, o) => "Obstacles",
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => WaveModels.FirstOrDefault(m => m.Obstacles.Equals(o)),
                    AfterCreate = (v, o) => v.RemoveObstacles = obs => 
                        {
                            var model = WaveModels.FirstOrDefault(m => m.Obstacles.Equals(o));
                            if (model != null)
                            {
                                obs.ForEach(f => model.Obstacles.Remove(f));
                            }
                        }
                };
            yield return obstacleViewInfo;
            yield return ViewInfoWrapper<WaveObstacle>.Create(obstacleViewInfo, o =>
                {
                    var model = WaveModels.FirstOrDefault(m => m.Obstacles.Contains(o));
                    return model != null ? model.Obstacles : null;
                });
            yield return ViewInfoWrapper<Feature2D>.Create(obstacleViewInfo, FindObstaclesForFeature, IsModelObstacle);
            yield return ViewInfoWrapper<WaveModelTreeShortcut>.Create(obstacleViewInfo, o => o.Data, o => o.WaveModel.Obstacles.Equals(o.Data) && o.ShortCutType == ShortCutType.FeatureSet);

            yield return new ViewInfo<WaveModelTreeShortcut, WaveModel, WpfSettingsView>
            {
                Description = "Wave settings",
                GetViewName = (v, o) => o.Name + _wavesSettings,
                GetViewData = o => o.WaveModel,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as WaveModelTreeShortcut;
                    if (shortcut == null) return;
                    v.EnsureVisible(shortcut.Data);
                },
                AfterCreate = (v, o) =>
                {
                    //Set the properties.
                    v.SettingsCategories = WaveSettingsHelper.GetWpfGuiCategories(o.WaveModel, Gui);
                    v.GetChangedPropertyName = (sender, propertyName) => (sender as WaveModelProperty)?.PropertyDefinition.FilePropertyName;
                }
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

            yield return new ViewInfo<WaveSpectralFileImporter,BoundaryConditionImportDialog>();
        }

        private static void ImportTimesFromBoundaryCondition(WaveModel model)
        {
            var dialog = new WaveBoundaryTimeSelectionDialog
            {
                Data = model.BoundaryConditions, 
                Text = "Select support point"
            };
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            var selectedTimePoints = dialog.SelectedDateTimes;
            var uniqueTimePionts = selectedTimePoints.Except(model.TimePointData.TimePoints);

            model.TimePointData.InputFields.Arguments[0].AddValues(uniqueTimePionts);
        }
        
        private static void ExportTimesToBoundaryConditions(WaveModel model)
        {
            var timepoints = model.TimePointData.TimePoints;
            if (!timepoints.Any()) return;

            var timeDepBoundaries =
                model.BoundaryConditions.Where(
                    bc => bc.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries);

            foreach (var boundary in timeDepBoundaries)
            {
                foreach (var timeSeries in boundary.PointData)
                {
                    var uniqueValues = timepoints.Except(timeSeries.Arguments[0].GetValues<DateTime>()).ToList();
                    if (uniqueValues.Any())
                    {
                        timeSeries.Arguments[0].AddValues(uniqueValues);
                    }
                }
            }
        }

        private bool IsModelObstacle(Feature2D f)
        {
            return WaveModels.FirstOrDefault(m => m.Obstacles.Contains(f)) != null;
        }

        private IEventedList<WaveObstacle> FindObstaclesForFeature(Feature2D f)
        {
            return WaveModels.First(m => m.Obstacles.Contains(f)) != null ? WaveModels.First(m => m.Obstacles.Contains(f)).Obstacles : null;
        }

        private bool IsModelBoundary(Feature2D f)
        {
            return WaveModels.FirstOrDefault(m => m.Boundaries.Contains(f)) != null;
        }

        private WaveBoundaryCondition FindBoundaryConditionForFeature(Feature2D f)
        {
            var model = WaveModels.FirstOrDefault(m => m.Boundaries.Contains(f));
            return model != null ? model.BoundaryConditions.FirstOrDefault(bc => bc.Feature.Equals(f)) : null;
        }

        private IEnumerable<WaveModel> WaveModels
        {
            get { return Gui != null ? Gui.Application.GetAllModelsInProject().OfType<WaveModel>() : Enumerable.Empty<WaveModel>(); }
        }

        public override IMapLayerProvider MapLayerProvider
        {
            get
            {
                return mapLayerProvider ?? (mapLayerProvider = new WaveModelMapLayerProvider
                {
                    GetWaveModels = () => Gui?.Application?.GetAllModelsInProject().OfType<WaveModel>() ?? Enumerable.Empty<WaveModel>()
                });
            }
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<GridBaseLayer, DynamicObjectProperties>();
            yield return new PropertyInfo<WaveModel, DynamicObjectProperties>();
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            Func<BoundaryCondition, WaveModel> getModelFromBoundaryConditionFunc =
                bc => WaveModels.FirstOrDefault(m => m.BoundaryConditions.Contains(bc));

            yield return new WaveModelNodePresenter(this);
            yield return new WaveDomainNodePresenter(d => WaveModels.FirstOrDefault(m => WaveDomainHelper.GetAllDomains(m.OuterDomain).Contains(d)));
            yield return new WaveBoundaryNodePresenter(getModelFromBoundaryConditionFunc) { GuiPlugin = this };
            yield return new WavmFileFunctionStoreNodePresenter {GuiPlugin = this};
            yield return new WaveModelTreeShortcutNodePresenter {GuiPlugin = this};
        }

        public override DelftTools.Shell.Gui.Forms.IRibbonCommandHandler RibbonCommandHandler
        {
            get { return new Ribbon.Ribbon(); }
        }

        public WaveGuiPlugin()
        {
            getActiveMapViewFunc = GetActiveMapView;
        }

        private static Func<MapView> getActiveMapViewFunc;
        private string _wavesSettings = " (Waves settings)";

        public static MapView ActiveMapView
        {
            get { return getActiveMapViewFunc(); }
        }

        private MapView GetActiveMapView()
        {
            if (Gui == null || Gui.DocumentViews == null) return null;

            var activeView = Gui.DocumentViews.ActiveView;
            if (activeView == null)
                return null;

            var mapView = FindMapView(activeView);
            if (mapView != null)
                return mapView;
            return null;
        }

        public override void OnActiveViewChanged(IView view)
        {
            if (view == null)
                return;

            var mapView = FindMapView(view);
            if (mapView != null)
                WaveMapViewDecorator.AddMapToolsIfMissing(mapView);
        }

        private static MapView FindMapView(IView activeView)
        {
            var mapView = activeView as MapView;

            if (mapView != null)
                return mapView;

            var compositeView = activeView as ICompositeView;

            //todo: recursion
            return compositeView != null ? compositeView.ChildViews.OfType<MapView>().FirstOrDefault() : null;
        }

        public override IGui Gui
        {
            get
            {

                return base.Gui;
                
            }
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
                if(value != null)
                {
                    SharpMapGisGuiPlugin.Instance.SpatialOperationsEnabled = true;
                }
            }
        }

        [InvokeRequired]
        private void OnActivityRunnerStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (sender is FileImportActivity)
            {
                var importer = ((FileImportActivity)sender).FileImporter;
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
            if (!(sender is WaveModel) || activityStatusChangedEventArgs.NewStatus != ActivityStatus.Failed) return;

            Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
        }
    }
}