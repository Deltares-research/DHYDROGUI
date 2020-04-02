using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Roughness;
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
        
        private ICrossSection crossSection;

        public string PipeId { get; set; }

        public Pipe()
        {
            PropertyChanged += OnPipePropertyChanged;
        }

        [DisplayName("Type")]
        [FeatureAttribute(Order = 30)]
        public virtual CrossSectionStandardShapeType CrossSectionStandardShapeType
        {
            get { return Profile.ShapeType; }
        }

        [FeatureAttribute(Order = 31)]
        public virtual double Width
        {
            get { return CrossSection.Definition.Width; }
        }

        [DisplayName("Definition")]
        [FeatureAttribute(Order = 32, ExportName = "DefName")]
        public virtual string DefinitionName
        {
            get { return CrossSection.Definition.Name; }
        }

        private void OnPipePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pipe = sender as IPipe;
            if (pipe != null && e.PropertyName == nameof(Network))
            {
                AddCrossSectionSectionToDefinition((IHydroNetwork) Network);
            }
        }
        [EditAction]
        private void AddCrossSectionSectionToDefinition(IHydroNetwork hydroNetwork)
        {
            var sewerCrossSectionSectionType = hydroNetwork?.CrossSectionSectionTypes?.FirstOrDefault(csst => string.Equals(csst.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (sewerCrossSectionSectionType != null)
            {
                if (CrossSectionDefinition != null && CrossSectionDefinition.Sections.All(css => css.SectionType != sewerCrossSectionSectionType))
                    CrossSectionDefinition?.Sections?.Add(new CrossSectionSection {SectionType = sewerCrossSectionSectionType});
            }
        }

        public ICrossSection CrossSection
        {
            get { return crossSection; }
            set
            {
                crossSection = value;
                if (crossSection?.Definition != null)
                    CrossSectionDefinitionName = crossSection?.Definition?.Name;

                if (crossSection != null)
                {
                    crossSection.Branch = this;
                    crossSection.Chainage = Length / 2;
                    if (HydroNetwork != null)
                    {
                        var sharedCrossSectionDefinition = HydroNetwork?.SharedCrossSectionDefinitions?.FirstOrDefault(scsd => scsd.Name.Equals(crossSection.Definition.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (sharedCrossSectionDefinition != null)
                        {
                            crossSection?.UseSharedDefinition(sharedCrossSectionDefinition);
                        }
                        else
                        {
                            crossSection?.ShareDefinitionAndChangeToProxy();
                        }

                        AddCrossSectionSectionToDefinition(HydroNetwork);
                    }
                }
            }
        }

        public ICrossSectionDefinition CrossSectionDefinition
        {
            get { return CrossSection?.Definition; }
        }

        public CrossSectionDefinitionStandard Profile
        {
            get
            {
                if (CrossSectionDefinition is CrossSectionDefinitionProxy crossSectionDefinitionProxy)
                {
                    return crossSectionDefinitionProxy.InnerDefinition as CrossSectionDefinitionStandard;
                }

                return CrossSectionDefinition as CrossSectionDefinitionStandard;
            }
        }

        public Action<object, EventArgs> EditSharedDefinitionClicked { get; set; }

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
        [EditAction]
        protected override void AddCrossSectionDefinition(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            if (CrossSectionDefinitionName == null)
            {
                CrossSection = CrossSections.CrossSection.CreateDefault(CrossSectionType.Standard, this, Length/2);
            }
            else
            {
                var crossSectionDefinition = hydroNetwork.SharedCrossSectionDefinitions.FirstOrDefault(cs => cs.Name == CrossSectionDefinitionName) as CrossSectionDefinitionStandard;
                if (crossSectionDefinition != null)
                {
                    var pipeCrossSection = CrossSections.CrossSection.CreateDefault(CrossSectionType.Standard, this, Length / 2);
                    pipeCrossSection.Name = "SewerProfile_0";
                    //pipeCrossSection.Name = NamingHelper.GetUniqueName("SewerProfile_{0}", hydroNetwork.CrossSections, typeof(ICrossSection), true);
                    pipeCrossSection.UseSharedDefinition(crossSectionDefinition);
                    helper?.PipeCrossSections.Add(pipeCrossSection);
                    CrossSection = pipeCrossSection;
                    CrossSectionDefinitionName = crossSection.Definition.Name;
                    
                    Material = (SewerProfileMapping.SewerProfileMaterial)typeof(SewerProfileMapping.SewerProfileMaterial).GetEnumValueFromDescription(((CrossSectionDefinitionStandard)crossSectionDefinition).Shape.MaterialName);
                }
            }
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