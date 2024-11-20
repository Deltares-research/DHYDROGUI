using System;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common
{
    public class FeatureDataSyncer<TFeat,TData> : IDisposable where TFeat : IFeature
    {
        private IEventedList<TFeat> Features { get; set; }
        private IEventedList<TData> ModelData { get; set; }
        private Func<TFeat, TData> CreateDataForFeature { get; set; }
 
        private bool synchronizing;

        public FeatureDataSyncer(IEventedList<TFeat> features, IEventedList<TData> modelData, Func<TFeat, TData> createDataForFeature)
        {
            Features = features;
            ModelData = modelData;
            CreateDataForFeature = createDataForFeature;
            features.CollectionChanged += OnFeaturesCollectionChanged;
            modelData.CollectionChanged += OnDataCollectionChanged;
        }
        
        private void OnFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (synchronizing) return;
            
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    synchronizing = true;
                    try
                    {
                        ModelData.Add(CreateDataForFeature((TFeat) e.GetRemovedOrAddedItem()));
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
                            var featureData = md as IFeatureData;
                            if (featureData != null)
                            {
                                return Equals(featureData.Feature, e.GetRemovedOrAddedItem());
                            }

                            var feature = md as IFeature;
                            if (feature != null)
                            {
                                return Equals(feature, e.GetRemovedOrAddedItem());
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (synchronizing) return;
            var featureData = e.GetRemovedOrAddedItem() as IFeatureData;
            if (featureData == null) return;
            if (!(featureData.Feature is TFeat)) return;
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
                    synchronizing = true;
                    try
                    {
                        Features.Remove((TFeat) featureData.Feature);
                    }
                    finally
                    {
                        synchronizing = false;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    synchronizing = true;
                    try
                    {
                        bool replaced = false;
                        var feature = ((IFeatureData) e.OldItems[0]).Feature;
                        if (feature is TFeat)
                        {
                            var featureIndex = Features.IndexOf((TFeat) feature);
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
                    throw new ArgumentOutOfRangeException();
            }
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
    }
}