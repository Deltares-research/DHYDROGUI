using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydrographic network (channels, pipes)
    /// </summary>
    [DisplayName("Hydro Network")]
    [Entity]
    public class HydroNetwork : Network, IHydroNetwork
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetwork));
        public const string CrossSectionSectionFormat = "Section{0:D3}";

        public const string ImportBranchesActionName = "Import branches";

        private Dictionary<Type, IDictionary<string, IBranchFeature>> branchFeatureNameCache;

        [Aggregation]
        public virtual ICrossSectionDefinition DefaultCrossSectionDefinition { get; set; }

        private IEventedList<Route> routes;
        public virtual IEventedList<Route> Routes
        {
            get { return routes; }
            protected set
            {
                if (routes != null)
                {
                    routes.CollectionChanged -= RoutesCollectionChanged;
                }

                routes = value;

                if (routes != null)
                {
                    routes.CollectionChanged += RoutesCollectionChanged;
                }
            }
        }

        [EditAction]
        void RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var route = e.GetRemovedOrAddedItem() as Route;

            if (route == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    route.Network = this;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    route.Network = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEventedList<ICrossSectionDefinition> sharedCrossSectionDefinitions;

        public virtual IEventedList<ICrossSectionDefinition> SharedCrossSectionDefinitions
        {
            get { return sharedCrossSectionDefinitions; }
            protected set
            {
                if (sharedCrossSectionDefinitions != null)
                {
                    sharedCrossSectionDefinitions.CollectionChanging -= SharedCrossSectionDefinitionsCollectionChanging;
                }
                
                sharedCrossSectionDefinitions = value;
                
                if (sharedCrossSectionDefinitions == null)
                {
                    return;
                }

                sharedCrossSectionDefinitions.CollectionChanging += SharedCrossSectionDefinitionsCollectionChanging;
            }
        }

        [EditAction]
        void SharedCrossSectionDefinitionsCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            //check if the section is in use somewhere..the deletion is not allowed
            if ((e.Action == NotifyCollectionChangeAction.Replace) || (e.Action == NotifyCollectionChangeAction.Remove))
            {
                var definitionBeingRemoved = e.Item as ICrossSectionDefinition;

                if (definitionBeingRemoved == null)
                {
                    return;
                }

                if (definitionBeingRemoved == DefaultCrossSectionDefinition)
                {
                    DefaultCrossSectionDefinition = null;
                }

                var crossSectionsUsingDefinitionBeingRemoved =
                    CrossSections.Where(
                        cs => cs.Definition.IsProxy &&
                              ((CrossSectionDefinitionProxy)cs.Definition).InnerDefinition == definitionBeingRemoved);

                if (crossSectionsUsingDefinitionBeingRemoved.Any())
                {
                    log.ErrorFormat(
                        "Cannot remove definition '{0}', it is in use by {1} cross section(s). (For example cross section: '{2}').",
                        definitionBeingRemoved.Name, crossSectionsUsingDefinitionBeingRemoved.Count(),
                        crossSectionsUsingDefinitionBeingRemoved.First().Name);
                    e.Cancel = true;
                }
            }
            else if (e.Action == NotifyCollectionChangeAction.Add &&
                     e.Item is CrossSectionDefinitionXYZ)
            {
                throw new NotSupportedException("XYZ cross sections cannot be added as definitions.");
            }
        }

        private IEventedList<CrossSectionSectionType> crossSectionSectionTypes;

        public virtual IEventedList<CrossSectionSectionType> CrossSectionSectionTypes
        {
            get { return crossSectionSectionTypes; }
            set
            {
                if (crossSectionSectionTypes != null)
                {
                    crossSectionSectionTypes.CollectionChanging -= SectionTypesCollectionChanging;
                    ((INotifyPropertyChanged)crossSectionSectionTypes).PropertyChanged -= SectionTypesPropertyChanged;
                }
                crossSectionSectionTypes = value;
                if (crossSectionSectionTypes != null)
                {
                    crossSectionSectionTypes.CollectionChanging += SectionTypesCollectionChanging;
                    ((INotifyPropertyChanged)crossSectionSectionTypes).PropertyChanged += SectionTypesPropertyChanged;
                }
            }
        }

        [EditAction]
        void SectionTypesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CrossSectionSectionType.Name))
            {
                var sectionName = ((CrossSectionSectionType)sender).Name;
                if (crossSectionSectionTypes.Count(sec => sec.Name == sectionName) > 1)
                {
                    ((CrossSectionSectionType)sender).Name = sectionName + "_1";
                    return;
                }
                CrossSections.Select(cs => cs.Definition)
                             .OfType<CrossSectionDefinitionZW>().ForEach(csd => csd.RemoveInvalidSections());
            }
        }

        [EditAction]
        void SectionTypesCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            //check if the section is in use somewhere..the deletion is not allowed
            if ((e.Action == NotifyCollectionChangeAction.Replace) || (e.Action == NotifyCollectionChangeAction.Remove))
            {
                var crossSectionSectionType = (CrossSectionSectionType)e.Item;

                var crossSection =
                    CrossSections.FirstOrDefault(
                        c => c.Definition.Sections.Any(sec => sec.SectionType == crossSectionSectionType));
                if (crossSection != null)
                {
                    log.ErrorFormat("Unable to remove section type. It is in use in cross section {0}.",
                                    crossSection.Name);
                    e.Cancel = true;
                }
            }
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                var sectionName = ((CrossSectionSectionType)e.Item).Name;
                if (crossSectionSectionTypes.Select(sec => sec.Name).Contains(sectionName))
                {
                    log.ErrorFormat("Unable to add cross section section type with non-identical name {0}.", sectionName);
                    e.Cancel = true;
                }
            }
        }

        public HydroNetwork()
        {
            Name = "network1";
            CrossSectionSectionTypes = new EventedList<CrossSectionSectionType>();
            SharedCrossSectionDefinitions = new EventedList<ICrossSectionDefinition>();
            Routes = new EventedList<Route>();

            var section = new CrossSectionSectionType
            {
                Name = RoughnessDataSet.MainSectionTypeName
            };

            CrossSectionSectionTypes.Add(section);

            Links = new EventedList<HydroLink>();
            SubRegions = new EventedList<IRegion>();

            PropertyChanged += (s, a) =>
            {
                if (s is IBranchFeature && string.Equals(a.PropertyName, nameof(IBranchFeature.Name)))
                {
                    branchFeatureNameCache = null;
                }
            };

            CollectionChanged += (sender, args) =>
            {
                if (args.GetRemovedOrAddedItem() is IBranchFeature)
                {
                    branchFeatureNameCache = null;
                }
            };
        }

        public override IEventedList<IBranch> Branches
        {
            get { return base.Branches; }
            set
            {
                base.Branches = value;

                Pipes = Branches.OfType<IPipe>();
                SewerConnections = Branches.OfType<ISewerConnection>();
                Channels = Branches.OfType<IChannel>();

                // always use filter to Interface or multiple data editor will fail on Cast<T>; reason unclear; too much smart linqing

                Structures = BranchFeatures.OfType<IStructure1D>(); // TODO: join with node features (manholes)
                CompositeBranchStructures = BranchFeatures.OfType<ICompositeBranchStructure>();

                CrossSections = BranchFeatures.OfType<ICrossSection>();
                Pumps = BranchFeatures.OfType<IPump>();
                Orifices = BranchFeatures.OfType<Orifice>();
                Weirs = BranchFeatures.OfType<IWeir>();
                Gates = BranchFeatures.OfType<IGate>();
                Culverts = BranchFeatures.OfType<ICulvert>();
                Bridges = BranchFeatures.OfType<IBridge>();
                ExtraResistances = BranchFeatures.OfType<IExtraResistance>();
                LateralSources = BranchFeatures.OfType<ILateralSource>();
                Retentions = BranchFeatures.OfType<IRetention>();
                ObservationPoints = BranchFeatures.OfType<IObservationPoint>();
            }
        }

        public override IEventedList<INode> Nodes
        {
            get { return base.Nodes; }
            set
            {
                base.Nodes = value;

                Manholes = Nodes.OfType<IManhole>();
                OutletCompartments = Manholes.SelectMany(m => m.OutletCompartments());
                Compartments = Manholes.SelectMany(m => m.Compartments.OfType<Compartment>());
                HydroNodes = Nodes.OfType<IHydroNode>();
            }
        }

        public virtual IEnumerable<IHydroNode> HydroNodes { get; protected set; }

        public virtual IEnumerable<IPipe> Pipes { get; protected set; }
        public virtual IEnumerable<ISewerConnection> SewerConnections { get; protected set; }
        public virtual IEnumerable<IChannel> Channels { get; protected set; }

        public virtual IEnumerable<IManhole> Manholes { get; protected set; }
        public virtual IEnumerable<OutletCompartment> OutletCompartments { get; protected set; }
        public virtual IEnumerable<Compartment> Compartments { get; protected set; }
        public virtual IEnumerable<IOrifice> Orifices { get; protected set; }
        public virtual IEnumerable<IStructure1D> Structures { get; protected set; }
        public virtual IEnumerable<ICompositeBranchStructure> CompositeBranchStructures { get; protected set; }
        public virtual IEnumerable<ICrossSection> CrossSections { get; protected set; }
        public virtual IEnumerable<IPump> Pumps { get; protected set; }
        public virtual IEnumerable<IWeir> Weirs { get; protected set; }
        public virtual IEnumerable<IGate> Gates { get; protected set; }
        public virtual IEnumerable<ICulvert> Culverts { get; protected set; }
        public virtual IEnumerable<IBridge> Bridges { get; protected set; }
        public virtual IEnumerable<IExtraResistance> ExtraResistances { get; protected set; }
        public virtual IEnumerable<ILateralSource> LateralSources { get; protected set; }
        public virtual IEnumerable<IRetention> Retentions { get; protected set; }
        public virtual IEnumerable<IObservationPoint> ObservationPoints { get; protected set; }

        /// <inheritdoc cref="IHydroNetwork"/>
        public virtual T GetBranchFeatureByName<T>(string featureName) where T : IBranchFeature
        {
            if (branchFeatureNameCache == null)
            {
                RecreateCache();
            }

            return branchFeatureNameCache.TryGetValue(typeof(T), out var features) && 
                   features.TryGetValue(featureName, out var feature) 
                       ? (T) feature 
                       : default(T);
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            foreach (var route in Routes)
            {
                yield return route;
            }
            foreach (var channel in Channels)
            {
                yield return channel;
            }
            foreach (var node in HydroNodes)
            {
                yield return node;
            }
            foreach (var manhole in Manholes)
            {
                yield return manhole;
            }
            foreach (var crossSectionSectionType in CrossSectionSectionTypes)
            {
                yield return crossSectionSectionType;
            }
        }

        public override object Clone()
        {
            var clone = (HydroNetwork)base.Clone();
            clone.crossSectionSectionTypes.Clear();
            foreach (var crossSectionSectionType in CrossSectionSectionTypes)
            {
                clone.crossSectionSectionTypes.Add((CrossSectionSectionType)crossSectionSectionType.Clone());
            }
            foreach (var definition in SharedCrossSectionDefinitions)
            {
                var definitionClone = (ICrossSectionDefinition)definition.Clone();
                clone.SharedCrossSectionDefinitions.Add(definitionClone);
                if (Equals(definition, DefaultCrossSectionDefinition))
                {
                    clone.DefaultCrossSectionDefinition = definitionClone;
                }
            }

            foreach (var route in Routes)
            {
                var clonedRoute = (Route)route.Clone();
                NetworkCoverage.ReplaceNetworkForClone(clone, clonedRoute);
                clone.Routes.Add(clonedRoute);
            }

            //update sectiontypes in cloned definitions
            var clonedLocalDefinitions = clone.CrossSections.Select(cs => cs.Definition).Where(def => !def.IsProxy);
            var allClonedSections = clone.SharedCrossSectionDefinitions.Concat(clonedLocalDefinitions).SelectMany(def => def.Sections);

            foreach (var clonedSection in allClonedSections)
            {
                //is this a valid situation???
                if (clonedSection.SectionType != null)
                {
                    clonedSection.SectionType = clone.CrossSectionSectionTypes.FirstOrDefault(type => type.Name == clonedSection.SectionType.Name);
                }
            }

            //rewire proxy crosssection))))
            foreach (var proxy in clone.CrossSections.Select(c => c.Definition).OfType<CrossSectionDefinitionProxy>())
            {
                var index = SharedCrossSectionDefinitions.IndexOf(proxy.InnerDefinition);
                proxy.InnerDefinition = clone.SharedCrossSectionDefinitions[index];
            }

            foreach (var subRegion in SubRegions)
            {
                clone.SubRegions.Add((IHydroRegion)subRegion.Clone());
            }

            clone.Links = new EventedList<HydroLink>(Links);
            return clone;
        }

        public virtual IEventedList<IRegion> SubRegions { get; set; }

        public virtual IEnumerable<IRegion> AllRegions { get { return HydroRegion.GetAllRegions(this); } }

        [Aggregation]
        public virtual IRegion Parent { get; set; }

        public virtual IEnumerable<IHydroObject> AllHydroObjects
        {
            get
            {
                return Nodes.OfType<IHydroObject>()
                    .Concat(Branches.OfType<IHydroObject>())
                    .Concat(BranchFeatures.OfType<IHydroObject>())
                    .Concat(NodeFeatures.OfType<IHydroObject>());
            }
        }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual HydroLink AddNewLink(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveLink(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanLinkTo(IHydroObject source, IHydroObject target)
        {
            return HydroRegion.CanLinkTo(source, target);
        }

        public override INode NewNode()
        {
            return new HydroNode();
        }

        public virtual new bool EditWasCancelled
        {
            get { return base.EditWasCancelled; }
            set
            {
                base.EditWasCancelled = value;
            }
        }

        private void RecreateCache()
        {
            branchFeatureNameCache = new Dictionary<Type, IDictionary<string, IBranchFeature>>
            {
                [typeof(IOrifice)] = CreateDictionaryForBranchFeatures(nameof(Orifices), Orifices),
                [typeof(ICrossSection)] = CreateDictionaryForBranchFeatures(nameof(CrossSections), CrossSections),
                [typeof(IPump)] = CreateDictionaryForBranchFeatures(nameof(Pumps), Pumps),
                [typeof(ICulvert)] = CreateDictionaryForBranchFeatures(nameof(Culverts), Culverts),
                [typeof(IBridge)] = CreateDictionaryForBranchFeatures(nameof(Bridges), Bridges),
                [typeof(IWeir)] = CreateDictionaryForBranchFeatures(nameof(Weirs), Weirs),
                [typeof(IGate)] = CreateDictionaryForBranchFeatures(nameof(Gates), Gates),
                [typeof(ILateralSource)] = CreateDictionaryForBranchFeatures(nameof(LateralSources), LateralSources),
                [typeof(IRetention)] = CreateDictionaryForBranchFeatures(nameof(Retentions), Retentions),
                [typeof(IObservationPoint)] = CreateDictionaryForBranchFeatures(nameof(ObservationPoints), ObservationPoints),
                [typeof(IExtraResistance)] = CreateDictionaryForBranchFeatures(nameof(ExtraResistances), ExtraResistances),
                [typeof(ICompositeBranchStructure)] = CreateDictionaryForBranchFeatures(nameof(CompositeBranchStructures), CompositeBranchStructures),
            };
        }

        private Dictionary<string, IBranchFeature> CreateDictionaryForBranchFeatures(string context, IEnumerable<IBranchFeature> branchFeatures)
        {
            return branchFeatures.ToDictionaryWithDuplicateLogging(context, o => o.Name, bf => bf, comparer: StringComparer.InvariantCultureIgnoreCase);
        }
    }
}