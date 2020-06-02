using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
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
    public partial class HydroNetwork : Network, IHydroNetwork
    {
        public const string ImportBranchesActionName = "Import branches";
        public static string CrossSectionSectionFormat = "Section{0:D3}";
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetwork));

        private IEventedList<Route> routes;

        private IEventedList<ICrossSectionDefinition> sharedCrossSectionDefinitions;

        private IEventedList<CrossSectionSectionType> crossSectionSectionTypes;

        public HydroNetwork()
        {
            Name = "network1";
            CrossSectionSectionTypes = new EventedList<CrossSectionSectionType>();
            SharedCrossSectionDefinitions = new EventedList<ICrossSectionDefinition>();
            Routes = new EventedList<Route>();

            var section = new CrossSectionSectionType {Name = "Main"};

            CrossSectionSectionTypes.Add(section);

            Links = new EventedList<HydroLink>();
            SubRegions = new EventedList<IRegion>();
        }

        [Aggregation]
        public virtual ICrossSectionDefinition DefaultCrossSectionDefinition { get; set; }

        public virtual IEventedList<Route> Routes
        {
            get => routes;
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

        public virtual IEventedList<ICrossSectionDefinition> SharedCrossSectionDefinitions
        {
            get => sharedCrossSectionDefinitions;
            protected set
            {
                if (sharedCrossSectionDefinitions != null)
                {
                    sharedCrossSectionDefinitions.CollectionChanging -= SharedCrossSectionDefinitionsCollectionChanging;
                }

                sharedCrossSectionDefinitions = value;
                if (sharedCrossSectionDefinitions != null)
                {
                    sharedCrossSectionDefinitions.CollectionChanging += SharedCrossSectionDefinitionsCollectionChanging;
                }
            }
        }

        public virtual IEventedList<CrossSectionSectionType> CrossSectionSectionTypes
        {
            get => crossSectionSectionTypes;
            set
            {
                if (crossSectionSectionTypes != null)
                {
                    crossSectionSectionTypes.CollectionChanging -= SectionTypesCollectionChanging;
                    ((INotifyPropertyChanged) crossSectionSectionTypes).PropertyChanged -= SectionTypesPropertyChanged;
                }

                crossSectionSectionTypes = value;
                if (crossSectionSectionTypes != null)
                {
                    crossSectionSectionTypes.CollectionChanging += SectionTypesCollectionChanging;
                    ((INotifyPropertyChanged) crossSectionSectionTypes).PropertyChanged += SectionTypesPropertyChanged;
                }
            }
        }

        public override IEventedList<IBranch> Branches
        {
            get => base.Branches;
            set
            {
                base.Branches = value;

                Pipes = Branches.OfType<IPipe>();
                Channels = Branches.OfType<IChannel>();

                // always use filter to Interface or multiple data editor will fail on Cast<T>; reason unclear; too much smart linqing

                Structures = BranchFeatures.OfType<IStructure1D>(); // TODO: join with node features (manholes)
                CompositeBranchStructures = BranchFeatures.OfType<ICompositeBranchStructure>();

                CrossSections = BranchFeatures.OfType<ICrossSection>();
                Pumps = BranchFeatures.OfType<IPump>();
                Weirs = BranchFeatures.OfType<IWeir>();
                Gates = BranchFeatures.OfType<IGate>();
                Gullies = BranchFeatures.OfType<IGully>();
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
            get => base.Nodes;
            set
            {
                base.Nodes = value;

                Manholes = NodeFeatures.OfType<IManhole>();
                HydroNodes = Nodes.OfType<IHydroNode>();
            }
        }

        public virtual IEnumerable<IHydroNode> HydroNodes { get; protected set; }

        public virtual IEnumerable<IPipe> Pipes { get; protected set; }
        public virtual IEnumerable<IChannel> Channels { get; protected set; }

        public virtual IEnumerable<IManhole> Manholes { get; protected set; }

        public virtual IEnumerable<IStructure1D> Structures { get; protected set; }
        public virtual IEnumerable<ICompositeBranchStructure> CompositeBranchStructures { get; protected set; }
        public virtual IEnumerable<ICrossSection> CrossSections { get; protected set; }
        public virtual IEnumerable<IPump> Pumps { get; protected set; }
        public virtual IEnumerable<IWeir> Weirs { get; protected set; }
        public virtual IEnumerable<IGate> Gates { get; protected set; }
        public virtual IEnumerable<IGully> Gullies { get; protected set; }
        public virtual IEnumerable<ICulvert> Culverts { get; protected set; }
        public virtual IEnumerable<IBridge> Bridges { get; protected set; }
        public virtual IEnumerable<IExtraResistance> ExtraResistances { get; protected set; }
        public virtual IEnumerable<ILateralSource> LateralSources { get; protected set; }
        public virtual IEnumerable<IRetention> Retentions { get; protected set; }
        public virtual IEnumerable<IObservationPoint> ObservationPoints { get; protected set; }

        public virtual IEventedList<IRegion> SubRegions { get; set; }

        public virtual IEnumerable<IRegion> AllRegions => HydroRegion.GetAllRegions(this);

        [Aggregation]
        public virtual IRegion Parent { get; set; }

        public virtual IEnumerable<IHydroObject> AllHydroObjects =>
            Nodes.Cast<IHydroObject>()
                 .Concat(Branches.Cast<IHydroObject>())
                 .Concat(BranchFeatures.Cast<IHydroObject>())
                 .Concat(NodeFeatures.Cast<IHydroObject>());

        public virtual IEventedList<HydroLink> Links { get; set; }

        public new virtual bool EditWasCancelled
        {
            get => base.EditWasCancelled;
            set => base.EditWasCancelled = value;
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            foreach (Route route in Routes)
            {
                yield return route;
            }

            foreach (IChannel channel in Channels)
            {
                yield return channel;
            }

            foreach (IHydroNode node in HydroNodes)
            {
                yield return node;
            }

            foreach (CrossSectionSectionType crossSectionSectionType in CrossSectionSectionTypes)
            {
                yield return crossSectionSectionType;
            }
        }

        public override object Clone()
        {
            var clone = (HydroNetwork) base.Clone();
            clone.crossSectionSectionTypes.Clear();
            foreach (CrossSectionSectionType crossSectionSectionType in CrossSectionSectionTypes)
            {
                clone.crossSectionSectionTypes.Add((CrossSectionSectionType) crossSectionSectionType.Clone());
            }

            foreach (ICrossSectionDefinition definition in SharedCrossSectionDefinitions)
            {
                var definitionClone = (ICrossSectionDefinition) definition.Clone();
                clone.SharedCrossSectionDefinitions.Add(definitionClone);
                if (Equals(definition, DefaultCrossSectionDefinition))
                {
                    clone.DefaultCrossSectionDefinition = definitionClone;
                }
            }

            foreach (Route route in Routes)
            {
                var clonedRoute = (Route) route.Clone();
                NetworkCoverage.ReplaceNetworkForClone(clone, clonedRoute);
                clone.Routes.Add(clonedRoute);
            }

            //update sectiontypes in cloned definitions
            IEnumerable<ICrossSectionDefinition> clonedLocalDefinitions =
                clone.CrossSections.Select(cs => cs.Definition).Where(def => !def.IsProxy);
            IEnumerable<CrossSectionSection> allClonedSections =
                clone.SharedCrossSectionDefinitions.Concat(clonedLocalDefinitions).SelectMany(def => def.Sections);

            foreach (CrossSectionSection clonedSection in allClonedSections)
            {
                //is this a valid situation???
                if (clonedSection.SectionType != null)
                {
                    clonedSection.SectionType =
                        clone.CrossSectionSectionTypes.FirstOrDefault(
                            type => type.Name == clonedSection.SectionType.Name);
                }
            }

            //rewire proxy crosssection))))
            foreach (CrossSectionDefinitionProxy proxy in clone
                                                          .CrossSections.Select(c => c.Definition)
                                                          .OfType<CrossSectionDefinitionProxy>())
            {
                int index = SharedCrossSectionDefinitions.IndexOf(proxy.InnerDefinition);
                proxy.InnerDefinition = clone.SharedCrossSectionDefinitions[index];
            }

            foreach (IRegion subRegion in SubRegions)
            {
                clone.SubRegions.Add((IHydroRegion) subRegion.Clone());
            }

            clone.Links = new EventedList<HydroLink>(Links);

            return clone;
        }

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

        public virtual INode GetNodeByName(string nodeName)
        {
            return string.IsNullOrEmpty(nodeName)
                       ? null
                       : Nodes.FirstOrDefault(n => n.Name == nodeName);
        }
    }
}