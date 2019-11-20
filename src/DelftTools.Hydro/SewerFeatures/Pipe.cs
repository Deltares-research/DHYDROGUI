using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    [Serializable]
    [Entity]
    public class Pipe : SewerConnection, IPipe
    {
        private static ILog Log = LogManager.GetLogger(typeof(Pipe));
        private CrossSectionDefinitionStandard crossSectionDefinition;

        public string PipeId { get; set; }

        public Pipe()
        {
            PropertyChanged += OnPipePropertyChanged;
        }

        private void OnPipePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pipe = sender as IPipe;
            if (pipe != null && e.PropertyName == nameof(Network))
            {
                AddCrossSectionSectionToDefinition((IHydroNetwork) Network);
            }
        }

        private void AddCrossSectionSectionToDefinition(IHydroNetwork hydroNetwork)
        {
            if (hydroNetwork == null) return;

            var sewerCrossSectionSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(csst => string.Equals(csst.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (sewerCrossSectionSectionType != null && CrossSectionDefinition != null)
            {
                CrossSectionDefinition.Sections?.Add(new CrossSectionSection {SectionType = sewerCrossSectionSectionType});
            }
        }

        public CrossSectionDefinitionStandard CrossSectionDefinition
        {
            get { return crossSectionDefinition; }
            set
            {
                crossSectionDefinition = value;

                if(crossSectionDefinition != null)
                    CrossSectionDefinitionName = crossSectionDefinition.Name;
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
                        Log.ErrorFormat(Resources.Pipe_BranchFeaturesOnCollectionChanging_Pipe__0__does_not_allow_any_branch_feature_on_it_, Name);
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
        [EditAction]
        protected override void AddCrossSectionDefinition(IHydroNetwork hydroNetwork)
        {
            if (CrossSectionDefinitionName == null) CrossSectionDefinition = (CrossSectionDefinitionStandard)CrossSectionDefinitionStandard.CreateDefault();
            else
            {
                CrossSectionDefinition = hydroNetwork.SharedCrossSectionDefinitions.FirstOrDefault(cs => cs.Name == CrossSectionDefinitionName) as CrossSectionDefinitionStandard;
                if (CrossSectionDefinition != null)
                    Material = (SewerProfileMapping.SewerProfileMaterial)typeof(SewerProfileMapping.SewerProfileMaterial).GetEnumValueFromDescription(CrossSectionDefinition.Shape.MaterialName);
            }
        }
        [EditAction]
        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs NotifyCollectionChangedEventArgs)
        {
            if (NotifyCollectionChangedEventArgs.Action != NotifyCollectionChangeAction.Add) return;
            if (!(NotifyCollectionChangedEventArgs.Item is LateralSource))
            {
                NotifyCollectionChangedEventArgs.Cancel = true;
                Log.ErrorFormat(Resources.Pipe_BranchFeaturesOnCollectionChanging_Pipe__0__does_not_allow_any_branch_feature_on_it_, Name);
            }
        }
    }
}