using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Views;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui
{
    /// <summary>
    /// <see cref="RainfallRunoffViewInfoBuilder"/> creates all <see cref="ViewInfo"/>
    /// objects for RR views through the <see cref="BuildViewInfoObjects"/> method.
    /// </summary>
    public static class RainfallRunoffViewInfoBuilder
    {
        private static System.Tuple<RainfallRunoffModel, IEnumerable<IDataRowProvider>> multipleDataEditorData = new System.Tuple<RainfallRunoffModel, IEnumerable<IDataRowProvider>>(null, null);

        /// <summary>
        /// Creates all <see cref="ViewInfo"/> objects for all views of the RR plugin.
        /// </summary>
        /// <param name="rainfallRunoffGuiPlugin">The plugin to obtain the relevant information for each <see cref="ViewInfo"/> from.</param>
        /// <returns>
        /// The collection of <see cref="ViewInfo"/> objects of the RR plugin.
        /// </returns>
        public static IEnumerable<ViewInfo> BuildViewInfoObjects(IRainfallRunoffGuiPlugin rainfallRunoffGuiPlugin)
        {
            yield return new ViewInfo<IEventedList<NwrwDryWeatherFlowDefinition>, NwrwDryWeatherFlowDefinitionView>
            {
                Description = "Dryweather flow view",
                ViewDataContainsData = (v, o) =>
                {
                    RainfallRunoffModel model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    if (model == null) return false;

                    return v.Data is IEventedList<NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitions && ReferenceEquals(model.NwrwDryWeatherFlowDefinitions, nwrwDryWeatherFlowDefinitions);
                },
                GetViewName = (v, o) => "Dryweather Flow Definitions"
            };
            yield return new ViewInfo<IEventedList<NwrwDefinition>, NwrwDefinitionView>
            {
                Description = "Nwrw surface settings view",
                ViewDataContainsData = (v, o) =>
                {
                    RainfallRunoffModel model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    if (model == null) return false;

                    return v.Data is IEventedList<NwrwDefinition> nwrwDefinitions && ReferenceEquals(model.NwrwDefinitions, nwrwDefinitions);
                },
                GetViewName = (v, o) => "Nwrw Surface Settings",
            };
            yield return new ViewInfo<UnpavedData, UnpavedDataView>
            {
                Description = "Unpaved view",
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<PavedData, PavedDataView>
            {
                Description = "Paved view",
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<OpenWaterData, OpenWaterDataView>
            {
                Description = "Open water view",
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<GreenhouseData, GreenhouseDataView>
            {
                Description = "Greenhouse view",
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<SacramentoData, SacramentoDataView>
            {
                Description = "Sacramento view",
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<HbvData, HbvDataView>
            {
                Description = "HBV view",
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<RunoffBoundary, RunoffBoundaryData, RunoffBoundaryDataView>
            {
                GetViewName = (v, o) => "Runoff Boundary Data",
                Description = "Runoff boundary data",
                GetViewData = o => GetModelForRunoffBoundary(o, rainfallRunoffGuiPlugin.Gui).BoundaryData.First(bd => bd.Boundary == o),
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<Catchment, UnpavedData, UnpavedDataView>
            {
                Description = "Unpaved view",
                GetViewData = o => (UnpavedData)GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui),
                AdditionalDataCheck = o => GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui) is UnpavedData,
                AfterCreate = (v, o) => DefaultAfterCreate(v, v.Data, rainfallRunoffGuiPlugin.Gui)
            };
            yield return new ViewInfo<Catchment, PavedData, PavedDataView>
            {
                Description = "Paved view",
                GetViewData = o => (PavedData)GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui),
                AdditionalDataCheck = o => GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui) is PavedData,
                AfterCreate = (v, o) => DefaultAfterCreate(v, v.Data, rainfallRunoffGuiPlugin.Gui)
            };

            yield return new ViewInfo<Catchment, OpenWaterData, OpenWaterDataView>
            {
                Description = "Open water view",
                GetViewData = o => (OpenWaterData)GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui),
                AdditionalDataCheck = o => GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui) is OpenWaterData,
                AfterCreate = (v, o) => DefaultAfterCreate(v, v.Data, rainfallRunoffGuiPlugin.Gui)
            };

            yield return new ViewInfo<Catchment, GreenhouseData, GreenhouseDataView>
            {
                Description = "Greenhouse view",
                GetViewData = o => (GreenhouseData)GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui),
                AdditionalDataCheck = o => GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui) is GreenhouseData,
                AfterCreate = (v, o) => DefaultAfterCreate(v, v.Data, rainfallRunoffGuiPlugin.Gui)
            };

            yield return new ViewInfo<Catchment, SacramentoData, SacramentoDataView>
            {
                Description = "Sacramento view",
                GetViewData = o => (SacramentoData)GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui),
                AdditionalDataCheck = o => GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui) is SacramentoData,
                AfterCreate = (v, o) => DefaultAfterCreate(v, v.Data, rainfallRunoffGuiPlugin.Gui)
            };

            yield return new ViewInfo<Catchment, HbvData, HbvDataView>
            {
                Description = "HBV view",
                GetViewData = o => (HbvData)GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui),
                AdditionalDataCheck = o => GetCatchmentModelData(o, rainfallRunoffGuiPlugin.Gui) is HbvData,
                AfterCreate = (v, o) => DefaultAfterCreate(v, v.Data, rainfallRunoffGuiPlugin.Gui)
            };

            yield return new ViewInfo<IMeteoData, MeteoEditorView>
            {
                Description = Resources.RainfallRunoffViewInfoBuilder_BuildViewInfoObjects_Meteorological_data_viewer,
                AdditionalDataCheck = o =>
                {
                    var model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    if (model == null) return false;

                    return ReferenceEquals(o, model.Precipitation);
                },
                AfterCreate = (v, o) =>
                {
                    var model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    var stationListViewModel = new MeteoStationsListViewModel(model.MeteoStations);
                    var tableViewAdapter = new TableViewMeteoStationSelectionAdapter(v.MultipleFunctionView.TableView);

                    v.DataContext = new MeteoEditorViewModel(o,
                                                             stationListViewModel,
                                                             tableViewAdapter,
                                                             new TimeDependentFunctionSplitter(),
                                                             model.StartTime, 
                                                             model.StopTime);
                }
            };
            
            yield return new ViewInfo<IEvaporationMeteoData, MeteoEditorView>
            {
                Description = Resources.RainfallRunoffViewInfoBuilder_BuildViewInfoObjects_Meteorological_data_viewer,
                AdditionalDataCheck = o =>
                {
                    var model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    if (model == null) return false;

                    return ReferenceEquals(o, model.Evaporation);
                },
                AfterCreate = (v, o) =>
                {
                    var model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    var stationListViewModel = new MeteoStationsListViewModel(model.MeteoStations);
                    var tableViewAdapter = new TableViewMeteoStationSelectionAdapter(v.MultipleFunctionView.TableView);

                    v.DataContext = new MeteoEditorEvaporationViewModel(o,
                                                                        stationListViewModel,
                                                                        tableViewAdapter,
                                                                        new TimeDependentFunctionSplitter(),
                                                                        model.StartTime, 
                                                                        model.StopTime);
                }
            };

            yield return new ViewInfo<IMeteoData, MeteoEditorView>
            {
                Description = Resources.RainfallRunoffViewInfoBuilder_BuildViewInfoObjects_Meteorological_data_viewer,
                AdditionalDataCheck = o =>
                {
                    var model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    if (model == null) return false;

                    return ReferenceEquals(o, model.Temperature);
                },
                AfterCreate = (v, o) =>
                {
                    var model = GetModelForData(o, rainfallRunoffGuiPlugin.Gui);
                    var stationListViewModel = new MeteoStationsListViewModel(model.TemperatureStations);
                    var tableViewAdapter = new TableViewMeteoStationSelectionAdapter(v.MultipleFunctionView.TableView);

                    v.DataContext = new MeteoEditorViewModel(o,
                                                             stationListViewModel,
                                                             tableViewAdapter,
                                                             new TimeDependentFunctionSplitter(),
                                                             model.StartTime, 
                                                             model.StopTime);
                }
            };

            yield return new ViewInfo<RRInitialConditionsWrapper, IEnumerable<IDataRowProvider>, MultipleDataEditor>
            {
                Description = "Multiple data editor (D-RR)",
                GetViewName = (v, o) => "Multiple data editor (D-RR)",
                ViewDataContainsData = (v, o) => v.Data is IEnumerable<IDataRowProvider> dataRowProviders && dataRowProviders.All(drp => ReferenceEquals(drp.Model, o)),
                GetViewData = GetInitialConditionsWrapperDataRowProviders,
                AfterCreate = (v, o) => DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui)
            };

            yield return new ViewInfo<IEnumerable<Catchment>, IEnumerable<IDataRowProvider>, MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions>
            {
                Description = "Multiple data editor (D-RR)",
                GetViewName = (v, o) => "Multiple data editor (D-RR)",
                AdditionalDataCheck = o =>
                        {
                            var model = o.Any()
                            ? GetModelForCatchment(o.First(), rainfallRunoffGuiPlugin.Gui)
                            : rainfallRunoffGuiPlugin.Gui.Application.GetAllModelsInProject()
                                .OfType<RainfallRunoffModel>()
                                .FirstOrDefault(rrm => rrm.Basin.Catchments == o);
                            return model != null;
                        },
                ViewDataContainsData = (v, o) => v.Data is IEnumerable<IDataRowProvider> dataRowProviders && dataRowProviders.All(drp => ReferenceEquals(drp.Model, o)),
                GetViewData = o =>
                    {
                        var model = o.Any()
                            ? GetModelForCatchment(o.First(), rainfallRunoffGuiPlugin.Gui)
                            : rainfallRunoffGuiPlugin.Gui.Application.GetAllModelsInProject()
                                .OfType<RainfallRunoffModel>()
                                .FirstOrDefault(rrm => rrm.Basin.Catchments == o);
                        if (!ReferenceEquals(multipleDataEditorData.Item1, model))
                        {
                            multipleDataEditorData = Tuple.Create(model, RainfallRunoffDataRowProviderFactory.GetDataRowProviders(model, new Catchment[] { }).AsEnumerable());
                        }

                        return multipleDataEditorData.Item2;
                    },
                AfterCreate = (v, o) =>
                {
                    var model = o.Any()
                        ? GetModelForCatchment(o.First(), rainfallRunoffGuiPlugin.Gui)
                        : rainfallRunoffGuiPlugin.Gui.Application.GetAllModelsInProject()
                            .OfType<RainfallRunoffModel>()
                            .FirstOrDefault(rrm => rrm.Basin.Catchments == o);
                    if (model != null)
                        model.GetRainfallRunoffMDEData = () => Enumerable.Repeat(multipleDataEditorData.Item2, 1);
                    DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui);
                }
            };
            yield return new ViewInfo<TreeFolder, IEnumerable<IDataRowProvider>, MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions>
            {
                Description = "Catchment attribute viewer",
                GetViewName = (v, o) => "Multiple data editor (D-RR)",
                AdditionalDataCheck = o => (o.Parent is RainfallRunoffModel &&
                                            o.Text == RainfallRunoffModelProjectNodePresenter.CatchmentDataFolderName),
                ViewDataContainsData = (v, o) => v.Data is IEnumerable<IDataRowProvider> dataRowProviders && dataRowProviders.All(drp => ReferenceEquals(drp.Model, o)),
                GetViewData = o =>
                {
                    var model = (RainfallRunoffModel)o.Parent;
                    if (!ReferenceEquals(multipleDataEditorData.Item1, model))
                    {
                        multipleDataEditorData = Tuple.Create(model, RainfallRunoffDataRowProviderFactory.GetDataRowProviders(model, new Catchment[] { }).AsEnumerable());
                    }

                    return multipleDataEditorData.Item2;
                },
                AfterCreate = (v, o) =>
                {
                    ((RainfallRunoffModel)o.Parent).GetRainfallRunoffMDEData = () => Enumerable.Repeat(multipleDataEditorData.Item2, 1);
                    DefaultAfterCreate(v, o, rainfallRunoffGuiPlugin.Gui);
                }
            };
            yield return new ViewInfo<RainfallRunoffModel, ValidationView>
            {
                Description = "Validation Report",
                AfterCreate = (v, o) =>
                {
                    v.Gui = rainfallRunoffGuiPlugin.Gui;
                    v.OnValidate = d => RainfallRunoffModelValidator.Validate(d as RainfallRunoffModel);
                }
            };
            yield return new ViewInfo<IFeatureCoverage, CoverageTableView>
            {
                Description = "Output",
                AdditionalDataCheck = o => GetModelForFeatureCoverage(o, rainfallRunoffGuiPlugin.Gui) != null,
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => GetModelForFeatureCoverage(o, rainfallRunoffGuiPlugin.Gui)
            };
        }

        private static RainfallRunoffModel GetModelForFeatureCoverage(IFeatureCoverage featureCoverage, IGui gui)
        {
            return gui.Application.GetAllModelsInProject()
                .OfType<RainfallRunoffModel>()
                .FirstOrDefault(m =>
                    m.OutputCoverages != null &&
                    m.OutputCoverages.Contains(featureCoverage));
        }
        private static IEnumerable<IDataRowProvider> GetInitialConditionsWrapperDataRowProviders(RRInitialConditionsWrapper wrapper)
        {
            if (wrapper == null)
                return Enumerable.Empty<IDataRowProvider>();

            switch (wrapper.Type)
            {
                case RRInitialConditionsWrapper.InitialConditionsType.Unpaved:
                    return new[] { new ConceptDataRowProvider<UnpavedData, UnpavedInitialConditionsDataRow>(wrapper.Model, "Unpaved") };
                case RRInitialConditionsWrapper.InitialConditionsType.Paved:
                    return new[] { new ConceptDataRowProvider<PavedData, PavedInitialConditionsDataRow>(wrapper.Model, "Paved") };
                case RRInitialConditionsWrapper.InitialConditionsType.Greenhouse:
                    return new[] { new ConceptDataRowProvider<GreenhouseData, GreenhouseInitialConditionsDataRow>(wrapper.Model, "Greenhouse") };
                case RRInitialConditionsWrapper.InitialConditionsType.OpenWater:
                    return new[] { new ConceptDataRowProvider<OpenWaterData, OpenWaterInitialConditionsDataRow>(wrapper.Model, "Openwater") };
                default:
                    throw new NotImplementedException("Unknown initial conditions type");
            }
        }
        private static void DefaultAfterCreate(IView view, object actualData, IGui gui)
        {
            if (view is IRRUnitAwareView unitAwareView)
            {
                if (!(actualData is CatchmentModelData data))
                {
                    throw new NotImplementedException();
                }

                RainfallRunoffModel model = GetModelForModelData(data, gui);
                if (model != null)
                {
                    SyncAreaUnitChanges(model, unitAwareView);
                }
            }

            if (view is IRRModelTimeAwareView modelTimeAwareView)
            {
                var model = GetModelForData(actualData, gui);
                if (model != null)
                {
                    modelTimeAwareView.StartTime = model.StartTime;
                    modelTimeAwareView.StopTime = model.StopTime;
                    modelTimeAwareView.TimeStep = model.TimeStep;
                }
            }

            if (view is IRRMeteoStationAwareView stationAwareView)
            {
                RainfallRunoffModel model = GetModelForData(actualData, gui);
                if (model != null)
                {
                    stationAwareView.MeteoStations = model.MeteoStations;
                    SyncUseMeteoStationChanges(model, stationAwareView);
                }
            }

            if (view is IRRTemperatureStationAwareView tempStationAwareView)
            {
                var model = GetModelForData(actualData, gui);
                if (model != null)
                {
                    tempStationAwareView.TemperatureStations = model.TemperatureStations;
                    SyncUseTemperatureStationChanges(model, tempStationAwareView);
                }
            }

            if (view is IRRModelRunModeAwareView modelModeAwareView)
            {
                RainfallRunoffModel model = GetModelForData(actualData, gui);
                modelModeAwareView.GetIsModelRunningParallelWithFlowFunc = model.IsRunningParallelWithFlow;
            }
        }

        private static RainfallRunoffModel GetModelForCatchment(Catchment catchment, IGui gui)
        {
            return GetModelByPredicate(m => Equals(m.Basin, catchment.Region), gui);
        }

        private static RainfallRunoffModel GetModelForModelData(CatchmentModelData modelData, IGui gui)
        {
            return GetModelByPredicate(m => m.GetAllModelData().Contains(modelData), gui);
        }

        private static RainfallRunoffModel GetModelForData(object data, IGui gui)
        {
            if (data is CatchmentModelData modelData)
            {
                return GetModelForModelData(modelData, gui);
            }
            return GetModelByPredicate(m => m.GetAllItemsRecursive().Any(i => Equals(i, data)), gui);
        }

        private static RainfallRunoffModel GetModelByPredicate(Func<RainfallRunoffModel, bool> modelPredicate, IGui gui)
        {
            return gui?.Application
                      ?.GetAllModelsInProject()
                      .OfType<RainfallRunoffModel>()
                      .FirstOrDefault(modelPredicate);
        }

        private static CatchmentModelData GetCatchmentModelData(Catchment catchment, IGui gui)
        {
            RainfallRunoffModel model = GetModelForCatchment(catchment, gui);
            return model != null ? model.GetCatchmentModelData(catchment) : null;
        }

        private static RainfallRunoffModel GetModelForRunoffBoundary(RunoffBoundary boundary, IGui gui)
        {
            return GetModelByPredicate(m => Equals(m.Basin, boundary.Region), gui);
        }

        private static void SyncAreaUnitChanges(RainfallRunoffModel model, IRRUnitAwareView pUnitAwareView)
        {
            pUnitAwareView.AreaUnit = model.AreaUnit;
            var weakRef = new WeakReference(pUnitAwareView); //to prevent holding onto view after close
            model.AreaUnitChanged += (s, e) =>
            {
                if (weakRef.Target is IRRUnitAwareView unitAwareView)
                {
                    unitAwareView.AreaUnit = model.AreaUnit;
                }
            };
        }

        private static void SyncUseMeteoStationChanges(RainfallRunoffModel model, IRRMeteoStationAwareView stationAwareView)
        {
            stationAwareView.UseMeteoStations = model.Precipitation.DataDistributionType ==
                                                MeteoDataDistributionType.PerStation;
            var weakRef = new WeakReference(stationAwareView); //to prevent holding onto view after close
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) =>
            {
                if (!Equals(s, model.Precipitation) ||
                    e.PropertyName != nameof(model.Precipitation.DataDistributionType))
                    return;

                if (weakRef.Target is IRRMeteoStationAwareView unitAwareView)
                {
                    unitAwareView.UseMeteoStations = model.Precipitation.DataDistributionType ==
                                                     MeteoDataDistributionType.PerStation;
                }
            };
        }

        private static void SyncUseTemperatureStationChanges(RainfallRunoffModel model, IRRTemperatureStationAwareView tempStationAwareView)
        {
            tempStationAwareView.UseTemperatureStations = model.Temperature.DataDistributionType == MeteoDataDistributionType.PerStation;

            var weakRef = new WeakReference(tempStationAwareView); //to prevent holding onto view after close
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) =>
            {
                if (!Equals(s, model.Temperature) ||
                    e.PropertyName != nameof(model.Temperature.DataDistributionType))
                    return;

                if (weakRef.Target is IRRTemperatureStationAwareView unitAwareView)
                {
                    unitAwareView.UseTemperatureStations = 
                        model.Temperature.DataDistributionType == MeteoDataDistributionType.PerStation;
                }
            };
        }
    }
}