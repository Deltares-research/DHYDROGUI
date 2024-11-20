using System;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public sealed class FeatureDataSyncer<TFeat, TData> : IDisposable where TFeat : IFeature
    {
        private bool synchronizing;

        public FeatureDataSyncer(IEventedList<TFeat> features, IEventedList<TData> modelData,
                                 Func<TFeat, TData> createDataForFeature)
        {
            Features = features;
            ModelData = modelData;
            CreateDataForFeature = createDataForFeature;
            features.CollectionChanged += OnFeaturesCollectionChanged;
            modelData.CollectionChanged += OnDataCollectionChanged;
        }

        public void Dispose()
        {
            if (Features != null)
            {
                Features.CollectionChanged -= OnFeaturesCollectionChanged;
            }

            if (ModelData != null)
            {
                ModelData.CollectionChanged -= OnDataCollectionChanged;
            }

            Features = null;
            ModelData = null;
            CreateDataForFeature = null;
        }

        private IEventedList<TFeat> Features { get; set; }
        private IEventedList<TData> ModelData { get; set; }
        private Func<TFeat, TData> CreateDataForFeature { get; set; }

        private void OnFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (synchronizing)
            {
                return;
            }

            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    synchronizing = true;
                    try
                    {
                        ModelData.Add(CreateDataForFeature((TFeat) removedOrAddedItem));
                    }
                    finally
                    {
                        synchronizing = false;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    synchronizing = true;
                    try
                    {
                        ModelData.Remove(ModelData.First(md =>
                        {
                            if (md is IFeatureData featureData)
                            {
                                return Equals(featureData.Feature, removedOrAddedItem);
                            }

                            if (md is IFeature feature)
                            {
                                return Equals(feature, removedOrAddedItem);
                            }

                            return false;
                        }));
                    }
                    finally
                    {
                        synchronizing = false;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        private void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (synchronizing)
            {
                return;
            }

            if (!(e.GetRemovedOrAddedItem() is IFeatureData featureData))
            {
                return;
            }

            if (!(featureData.Feature is TFeat))
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    synchronizing = true;
                    try
                    {
                        if (!Features.Contains((TFeat) featureData.Feature))
                        {
                            Features.Add((TFeat) featureData.Feature);
                        }
                    }
                    finally
                    {
                        synchronizing = false;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    synchronizing = true;
                    try
                    {
                        var replaced = false;
                        IFeature feature = ((IFeatureData) e.OldItems[0]).Feature;
                        if (feature is TFeat)
                        {
                            int featureIndex = Features.IndexOf((TFeat) feature);
                            if (featureIndex != -1 && !Features.Contains((TFeat) featureData.Feature))
                            {
                                Features[featureIndex] = (TFeat) featureData.Feature;
                                replaced = true;
                            }
                        }

                        if (!replaced && !Features.Contains((TFeat) featureData.Feature))
                        {
                            Features.Add((TFeat) featureData.Feature);
                        }
                    }
                    finally
                    {
                        synchronizing = false;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }
    }
}