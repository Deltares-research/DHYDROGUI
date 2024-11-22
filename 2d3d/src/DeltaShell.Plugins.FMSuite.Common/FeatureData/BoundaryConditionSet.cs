﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    [Entity]
    public class BoundaryConditionSet : FeatureData<IEventedList<IBoundaryCondition>, Feature2D>, IFeature
    {
        private IEventedList<IBoundaryCondition> boundaryConditions;

        private string cachedFeatureName;

        public BoundaryConditionSet()
        {
            boundaryConditions = new EventedList<IBoundaryCondition>();
        }

        public override Feature2D Feature
        {
            get => base.Feature;
            set
            {
                if (base.Feature != null)
                {
                    ((INotifyPropertyChange) base.Feature).PropertyChanging -= OnPropertyChanging;
                    ((INotifyPropertyChange) base.Feature).PropertyChanged -= OnPropertyChanged;
                }

                base.Feature = value;
                if (base.Feature != null)
                {
                    AfterFeatureSet();
                    ((INotifyPropertyChange) base.Feature).PropertyChanging += OnPropertyChanging;
                    ((INotifyPropertyChange) base.Feature).PropertyChanged += OnPropertyChanged;
                }
            }
        }

        public IList<string> SupportPointNames
        {
            get
            {
                if (Feature == null)
                {
                    return null;
                }

                return Feature.Attributes[Feature2D.LocationKey] as IList<string>;
            }
        }
        
        public IEventedList<IBoundaryCondition> BoundaryConditions
        {
            get => boundaryConditions;
            set
            {
                boundaryConditions = value;
                Data = value;
            }
        }

        public string VariableDescription => BoundaryConditions.Any() ? BoundaryConditions.First().Description : "";

        public IGeometry Geometry
        {
            get => Feature.Geometry.Centroid;
            set => throw new Exception("Boundary condition sets cannot be moved");
        }

        public IFeatureAttributeCollection Attributes { get; set; }

        public static string DefaultLocationName(IFeature feature, int i)
        {
            var feature2D = feature as Feature2D;
            if (feature2D != null)
            {
                return CreateNameByIndex(feature2D.Name, i);
            }

            return (i + 1).ToString("D4");
        }

        public bool ContainsData()
        {
            return BoundaryConditions.Any(bc => bc.DataPointIndices.Any());
        }

        public object Clone()
        {
            return new BoundaryConditionSet
            {
                Feature = Feature,
                BoundaryConditions =
                    new EventedList<IBoundaryCondition>(
                        BoundaryConditions.Select(bc => (IBoundaryCondition) bc.Clone()))
            };
        }

        protected override void UpdateName()
        {
            Name = Feature != null ? Feature.Name + " (Boundary conditions)" : "";
        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName.Equals("Name"))
            {
                cachedFeatureName = base.Feature.Name;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Name"))
            {
                UpdateName();
                for (var i = 0; i < SupportPointNames.Count; ++i)
                {
                    if (cachedFeatureName == null || IsDefaultLocationName(SupportPointNames[i], cachedFeatureName))
                    {
                        SupportPointNames[i] = DefaultLocationName(base.Feature, i);
                    }
                }
            }
        }

        private void AfterFeatureSet()
        {
            if (Feature.Attributes == null)
            {
                Feature.Attributes = new DictionaryFeatureAttributeCollection();
            }

            if (!Feature.Attributes.ContainsKey(Feature2D.LocationKey))
            {
                Feature.Attributes.Add(Feature2D.LocationKey, CreateSyncedList());
            }
            else if (!(Feature.Attributes[Feature2D.LocationKey] is BoundaryConditionsPointsSyncedList))
            {
                BoundaryConditionsPointsSyncedList geometryPointsSyncedList = CreateSyncedList();

                var locations = Feature.Attributes[Feature2D.LocationKey] as IList<string>;

                if (locations != null)
                {
                    for (var i = 0; i < geometryPointsSyncedList.Count; ++i)
                    {
                        if (i == locations.Count)
                        {
                            break;
                        }

                        geometryPointsSyncedList[i] = locations[i];
                    }
                }
            }
        }

        private BoundaryConditionsPointsSyncedList CreateSyncedList()
        {
            return new BoundaryConditionsPointsSyncedList
            {
                CreationMethod = DefaultLocationName,
                RecreateAllItems = false,
                SyncWithGeometryMove = false,
                Feature = Feature
            };
        }

        private static string CreateNameByIndex(string featureName, int i)
        {
            return featureName + "_" + (i + 1).ToString("D4");
        }

        private static bool IsDefaultLocationName(string locationName, string featureName)
        {
            if (locationName.StartsWith(featureName + "_"))
            {
                string numString = locationName.Substring(featureName.Length + 1);
                return numString.Length == 4 && int.TryParse(numString, out int _);
            }

            return false;
        }
    }
}