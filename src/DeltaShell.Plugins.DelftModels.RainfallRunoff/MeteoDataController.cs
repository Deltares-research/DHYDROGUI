using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// MeteoDataController has event listeners to the RainfallRunoffModel to keep the meteo data up-to-date
    /// </summary>
    public class MeteoDataController : ICatchmentCoverageMaintainer
    {
        private readonly ICatchmentModelDataSynchronizer synchronizer;

        private RainfallRunoffModel rainfallRunoffModel;
        private bool isUpdating;

        public MeteoDataController(RainfallRunoffModel model, ICatchmentModelDataSynchronizer customSynchronizer = null)
        {
            Model = model;

            synchronizer = customSynchronizer ?? new CatchmentModelDataSynchronizer<CatchmentModelData>(model);
            synchronizer.OnAreaAddedOrModified = OnAreaAddedOrModified;
            synchronizer.OnAreaRemoved = OnAreaRemoved;
        }

        private RainfallRunoffModel Model
        {
            get { return rainfallRunoffModel; }
            set
            {
                rainfallRunoffModel = value;
                SubscribeToModel();
            }
        }

        public void Initialize(IFeatureCoverage featureCoverage)
        {
            if (featureCoverage != null)
            {
                throw new ArgumentException("FeatureCoverage should be null");
            }
        }
        
        private void SubscribeToModel()
        {
            ((INotifyPropertyChanged) Model).PropertyChanged += RainfallRunoffModelPropertyChanged;
            Model.CollectionChanged += StationsCollectionChanged;
        }

        [EditAction]
        private void RainfallRunoffModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is MeteoData meteoData))
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(MeteoData.IsEditing):
                {
                    SynchronizeForMeteoDataIsEditing(meteoData);
                    break;
                }
                case nameof(MeteoData.DataDistributionType):
                {
                    SynchronizeForDataDistributionType(meteoData);
                    break;
                }
            }
        }

        private void SynchronizeForMeteoDataIsEditing(MeteoData meteoData)
        {
            if (Equals(meteoData, Model.Precipitation))
            {
                SynchroniseModelWithMeteoData(Model.Precipitation);

                //change evaporation
                if (Model.Evaporation.DataDistributionType != MeteoDataDistributionType.Global)
                {
                    DoAndPreventReEntry(() => Model.Evaporation.DataDistributionType = Model.Precipitation.DataDistributionType);
                }
            }
            else if (Equals(meteoData, Model.Temperature))
            {
                SynchroniseModelWithMeteoData(Model.Temperature);
            }
        }

        private void SynchronizeForDataDistributionType(MeteoData meteoData)
        {
            var precipitation = Model.Precipitation;
            var evaporation = Model.Evaporation;

            if (Equals(meteoData, precipitation))
            {
                UpdateMeteoDataByType(precipitation, Model.MeteoStations);

                if (evaporation.DataDistributionType == MeteoDataDistributionType.Global)
                    return;

                //change evaporation
                if (evaporation.DataDistributionType != precipitation.DataDistributionType)
                {
                    DoAndPreventReEntry(() => evaporation.DataDistributionType = precipitation.DataDistributionType);
                }
            }

            if (Equals(meteoData, evaporation))
            {
                UpdateMeteoDataByType(evaporation, Model.MeteoStations);

                if (evaporation.DataDistributionType == MeteoDataDistributionType.Global)
                    return;

                //change precipitation
                if (precipitation.DataDistributionType != evaporation.DataDistributionType)
                {
                    DoAndPreventReEntry(() => precipitation.DataDistributionType = evaporation.DataDistributionType);
                }
            }

            if (Equals(meteoData, Model.Temperature))
            {
                UpdateMeteoDataByType(Model.Temperature, Model.TemperatureStations);
            }
        }

        private void UpdateMeteoDataByType(MeteoData source, IEnumerable<string> stations)
        {
            if (source.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                SetCatchmentsToFeatureCoverage((IFeatureCoverage) source.Data, Model.Basin.Catchments);
            }
            if (source.DataDistributionType == MeteoDataDistributionType.PerStation)
            {
                SetStationNamesToFunction(source.Data, stations);
            }
        }

        private void SynchroniseModelWithMeteoData(MeteoData meteoData)
        {
            if (meteoData.IsEditing)
            {
                return;
            }

            if (meteoData.DataDistributionType == MeteoDataDistributionType.PerStation)
            {
                modelIsUpdating = true;
                var stations = ReferenceEquals(meteoData, Model.Temperature)
                                   ? Model.TemperatureStations
                                   : Model.MeteoStations;
                stations.Clear();
                stations.AddRange(meteoData.Data.Arguments[1].Values.Cast<string>());
                modelIsUpdating = false;
            }
        }
        
        private void DoAndPreventReEntry(Action action)
        {
            if (isUpdating)
                return;

            isUpdating = true;
            try
            {
                action(); //keep in sync
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void SetCatchmentsToFeatureCoverage(IFeatureCoverage featureCoverage, IEnumerable<Catchment> catchments)
        {
            foreach (Catchment catchment in catchments)
            {
                AddCatchmentToMeteoDataCoverage(featureCoverage, catchment);
            }
        }

        private void AddCatchmentToMeteoDataCoverages(Catchment catchment)
        {
            if (Model.Precipitation.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                AddCatchmentToMeteoDataCoverage(Model.Precipitation.Data as IFeatureCoverage, catchment);
            }

            if (Model.Evaporation.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                AddCatchmentToMeteoDataCoverage(Model.Evaporation.Data as IFeatureCoverage, catchment);
            }

            if (Model.Temperature.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                AddCatchmentToMeteoDataCoverage(Model.Temperature.Data as IFeatureCoverage, catchment);
            }
        }

        private static void AddCatchmentToMeteoDataCoverage(IFeatureCoverage featureCoverage, Catchment catchment)
        {
            if (featureCoverage.Features.Contains(catchment))
            {
                return; //already added
            }
            featureCoverage.Features.Add(catchment);
            featureCoverage.FeatureVariable.Values.Add(catchment);
        }

        private void RemoveCatchmentFromMeteoDataCoverages(Catchment catchment)
        {
            if (Model.Precipitation.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                RemoveCatchmentFromMeteoDataCoverage(Model.Precipitation.Data as IFeatureCoverage, catchment);
            }

            if (Model.Evaporation.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                RemoveCatchmentFromMeteoDataCoverage(Model.Evaporation.Data as IFeatureCoverage, catchment);
            }

            if (Model.Temperature.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                RemoveCatchmentFromMeteoDataCoverage(Model.Temperature.Data as IFeatureCoverage, catchment);
            }
        }

        private static void RemoveCatchmentFromMeteoDataCoverage(IFeatureCoverage featureCoverage, Catchment catchment)
        {
            if (featureCoverage.Features.Contains(catchment))
            {
                featureCoverage.Features.Remove(catchment);
                featureCoverage.FeatureVariable.Values.Remove(catchment);
            }
        }

        private void OnAreaAddedOrModified(CatchmentModelData area)
        {
            AddCatchmentToMeteoDataCoverages(area.Catchment);
        }

        private void OnAreaRemoved(CatchmentModelData area)
        {
            RemoveCatchmentFromMeteoDataCoverages(area.Catchment);
        }

        #region Meteo/Temperature Stations

        void StationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (modelIsUpdating)
            {
                return;
            }
            if (Equals(sender, Model.MeteoStations))
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var affectedStation = (string) e.GetRemovedOrAddedItem();
                    var affectedCatchmentDatas =
                        Model.GetAllModelData().Where(md => Equals(md.MeteoStationName, affectedStation));
                    affectedCatchmentDatas.ForEach(md => { md.MeteoStationName = ""; }); //clear
                }
                if (Model.Precipitation.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    OnStationsCollectionChanged(Model.Precipitation, e);
                }
                if (Model.Evaporation.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    OnStationsCollectionChanged(Model.Evaporation, e);
                }
            }
            if (Equals(sender, Model.TemperatureStations))
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var affectedStation = (string)e.GetRemovedOrAddedItem();
                    var affectedCatchmentDatas =
                        Model.GetAllModelData().Where(md => Equals(md.TemperatureStationName, affectedStation));
                    affectedCatchmentDatas.ForEach(md => { md.TemperatureStationName = ""; }); //clear
                }
                if (Model.Temperature.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    OnStationsCollectionChanged(Model.Temperature, e);
                }
            }
        }

        private static void SetStationNamesToFunction(IFunction data, IEnumerable<string> meteoStations)
        {
            data.Arguments[1].Values.Clear();
            data.Arguments[1].Values.AddRange(meteoStations.ToList());
        }

        private static void OnStationsCollectionChanged(MeteoData meteoData, NotifyCollectionChangedEventArgs e)
        {
            var function = meteoData.Data;
            var affectedStation = (string)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if(!function.Arguments[1].Values.Contains(affectedStation)) function.Arguments[1].Values.Add(affectedStation);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    function.Arguments[1].Values.Remove(affectedStation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool modelIsUpdating;

        #endregion
    }
}