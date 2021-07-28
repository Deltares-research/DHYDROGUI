using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    [Serializable]
    [Entity]
    public class Pipe : SewerConnection, IPipe
    {
        private static ILog Log = LogManager.GetLogger(typeof(Pipe));
        
        public string PipeId { get; set; }

        public Pipe()
        {
            PropertyChanged += OnPipePropertyChanged;
        }

        [DisplayName("Type")]
        [FeatureAttribute(Order = 30)]
        public virtual CrossSectionStandardShapeType? CrossSectionStandardShapeType
        {
            get { return Profile?.ShapeType; }
        }

        [FeatureAttribute(Order = 31)]
        public virtual double Width
        {
            get { return CrossSection.Definition.Width; }
        }

        private void OnPipePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pipe = sender as IPipe;
            if (pipe != null && e.PropertyName == nameof(Network))
            {
                AddCrossSectionSectionToDefinition((IHydroNetwork) Network);
            }
        }

        public SewerProfileMapping.SewerProfileMaterial Material { get; set; }

        [Aggregation]
        public override IEventedList<IBranchFeature> BranchFeatures
        {
            get { return base.BranchFeatures; }
            set
            {
                if (base.BranchFeatures != null)
                {
                    base.BranchFeatures.CollectionChanging -= BranchFeaturesOnCollectionChanging;
                }

                if (value != null)
                {
                    if (value.Count > 0)
                    {
                        Log.WarnFormat(Resources.Pipe_BranchFeaturesOnCollectionChanging_Pipe__0__does_not_allow_any_branch_feature_on_it_, Name);
                    }
                    else
                    {
                        base.BranchFeatures = value;
                    }
                }

                if (base.BranchFeatures != null)
                {
                    base.BranchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
                }

            }
        }

        protected override void OnAddCrossSectionDefinition(CrossSectionDefinitionStandard crossSectionDefinitionStandard)
        {
            base.OnAddCrossSectionDefinition(crossSectionDefinitionStandard);
            Material = (SewerProfileMapping.SewerProfileMaterial)typeof(SewerProfileMapping.SewerProfileMaterial).GetEnumValueFromDescription(crossSectionDefinitionStandard.Shape.MaterialName);
        }

        [EditAction]
        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs NotifyCollectionChangedEventArgs)
        {
            if (NotifyCollectionChangedEventArgs.Action != NotifyCollectionChangeAction.Add) return;
            if (!(NotifyCollectionChangedEventArgs.Item is LateralSource) && !(NotifyCollectionChangedEventArgs.Item is HydroLink))
            {
                NotifyCollectionChangedEventArgs.Cancel = true;
                Log.WarnFormat(Resources.Pipe_BranchFeaturesOnCollectionChanging_Pipe__0__does_not_allow_any_branch_feature_on_it_, Name);
            }
        }

        protected override void UpdateGeometryBasedOnSourceAndTargetCompartments()
        {
            base.UpdateGeometryBasedOnSourceAndTargetCompartments();
            if (TargetCompartment?.Geometry?.Coordinate != null && SourceCompartment?.Geometry?.Coordinate?.Distance(TargetCompartment?.Geometry?.Coordinate) == 0)
                Log.Error($"This pipe {Name} has a geometry with distance of 0 but a 'custom length' (read from GWSW) of {Length}, the source {SourceCompartment?.Name} has a coordinate on {SourceCompartment?.Geometry?.Coordinate}; the target {TargetCompartment?.Name} has a coordinate on {TargetCompartment?.Geometry?.Coordinate} ");

        }
    }
}