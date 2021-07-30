using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class SewerConnection : Branch, ISewerConnection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SewerConnection));

        private INode source;
        private INode target;
        private ICompartment sourceCompartment;
        private ICompartment targetCompartment;
        private string sourceCompartmentName;
        private string targetCompartmentName;
        private ICrossSection crossSection;

        public SewerConnection() : this("SewerConnection")
        {
        }

        public SewerConnection(string name)
            : base(name, null, null, 0)
        {
        }

        [DisplayName("Invert level from")]
        [FeatureAttribute(ExportName = "Level source", Order = 15)]
        public double LevelSource { get; set; }

        [DisplayName("Invert level to")]
        [FeatureAttribute(ExportName = "Level target", Order = 16)]
        public double LevelTarget { get; set; }

        [DisplayName("Sewer type")]
        [FeatureAttribute(ExportName = "Sewer type", Order = 20)]
        public SewerConnectionWaterType WaterType { get; set; }

        // This property is used in the NetworkLayerStyleFactory, do not remove :)
        [DisplayName("Sewer special connection type")]
        [FeatureAttribute(ExportName = "Sewer Special Connection type", Order = 21)]
        [InvokeRequired]
        public SewerConnectionSpecialConnectionType SpecialConnectionType { get { return GetConnectionType(); } }

        #region Source and Target

        [DisplayName("From manhole")]
        [FeatureAttribute(ExportName = "From manhole", Order = 3)]
        [ReadOnly(true)]
        public override INode Source
        {
            get { return source; }
            set { SetSource(value); }
        }

        
        private void SetSource(INode value)
        {
            BeforeSetSource();
            if (value is HydroNode)
                source = value;
            else if (value is Manhole manhole)
            {
                
                if (manhole?.Compartments != null && manhole.Compartments.Any())
                {
                    source = value;
                    if (sourceCompartment == null || !manhole.ContainsCompartmentWithName(sourceCompartment.Name))
                    {
                        if (manhole.ContainsCompartmentWithName(SourceCompartmentName))
                        {
                            sourceCompartment = manhole.Compartments.FirstOrDefault(c =>
                                c.Name.Equals(SourceCompartmentName, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else if (sourceCompartment != null &&
                                 manhole.ContainsCompartmentWithName(sourceCompartment.Name))
                        {
                            sourceCompartment = manhole.Compartments.FirstOrDefault(c =>
                                c.Name.Equals(sourceCompartment.Name, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else
                        {
                            sourceCompartment = manhole.Compartments.FirstOrDefault();
                        }

                        UpdateSource(sourceCompartment);
                        UpdateSourceCompartmentId();
                        UpdateGeometryBasedOnSourceAndTargetCompartments();
                    }

                }
                else
                {
                    if (network is IHydroNetwork hydroNetwork && manhole.Compartments != null)
                    {
                        var uniqueCompartmentName = NetworkHelper.GetUniqueName("Compartment{0:D3}",
                            hydroNetwork.Manholes.SelectMany(m => m.Compartments), "Compartment");
                        var newCompartment = new Compartment(uniqueCompartmentName);
                        lock (manhole.Compartments)
                        {
                            manhole.Compartments.Add(newCompartment);
                        }
                        SourceCompartment = newCompartment;
                    }
                    else
                    {
                        source = null;
                    }
                }
            }

            AfterSetSource();
        }

        private void BeforeSetSource()
        {
            source?.OutgoingBranches.Remove(this);
        }

        private void AfterSetSource()
        {
            if (source == null || source.OutgoingBranches.Contains(this)) return;
            lock (source.OutgoingBranches)
            {
                source?.OutgoingBranches.Add(this);
            }
        }

        [DisplayName("To manhole")]
        [FeatureAttribute(ExportName = "To manhole", Order = 3)]
        [ReadOnly(true)]
        public override INode Target
        {
            get { return target; }
            set { SetTarget(value); }
        }

        
        private void SetTarget(INode value)
        {
            BeforeTargetSet();
            if (value is HydroNode)
                target = value;
            else if (value is Manhole manhole)
            {
                if (manhole?.Compartments != null && manhole.Compartments.Any())
                {
                    target = value;
                    if (targetCompartment == null || !manhole.ContainsCompartmentWithName(targetCompartment.Name))
                    {
                        if (manhole.ContainsCompartmentWithName(TargetCompartmentName))
                        {
                            targetCompartment = manhole.Compartments.FirstOrDefault(c =>
                                c.Name.Equals(TargetCompartmentName, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else if (targetCompartment != null &&
                                 manhole.ContainsCompartmentWithName(targetCompartment.Name))
                        {
                            targetCompartment = manhole.Compartments.FirstOrDefault(c =>
                                c.Name.Equals(targetCompartment.Name, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else
                        {
                            targetCompartment = manhole.Compartments.LastOrDefault();
                        }

                        UpdateTarget(targetCompartment);
                        UpdateTargetCompartmentId();
                        UpdateGeometryBasedOnSourceAndTargetCompartments();
                    }
                }
                else
                {
                    if (network is IHydroNetwork hydroNetwork && manhole.Compartments != null)
                    {
                        var uniqueCompartmentName = NetworkHelper.GetUniqueName("Compartment{0:D3}",
                            hydroNetwork.Manholes.SelectMany(m => m.Compartments), "Compartment");
                        var newCompartment = new Compartment(uniqueCompartmentName);
                        lock (manhole.Compartments)
                        {
                            manhole.Compartments.Add(newCompartment);
                        }
                        TargetCompartment = newCompartment;
                    }
                    else
                    {
                        target = null;
                    }
                }
            }

            AfterTargetSet();
        }

        private void BeforeTargetSet()
        {
            target?.IncomingBranches.Remove(this);
        }

        private void AfterTargetSet()
        {
            if(target == null || target.IncomingBranches.Contains(this)) return;
            lock (target.IncomingBranches)
            {
                target?.IncomingBranches.Add(this);
            }
        }

        #endregion

        [DisplayName("From compartment")]
        [FeatureAttribute(ExportName = "From compartment", Order = 4)]
        [DynamicReadOnly]
        public ICompartment SourceCompartment
        {
            get { return sourceCompartment; }
            set
            {
                if (value == null)
                {
                    sourceCompartment = null;
                    return;
                }

                if (value.ParentManhole == null)
                {
                    log.WarnFormat(Resources.SewerConnection_TargetCompartment_We_cannot_add_compartment_as_source
                        , value.Name, Name);
                    return;
                }

                sourceCompartment = value;
                UpdateSource(sourceCompartment);
                UpdateSourceCompartmentId();
                UpdateGeometryBasedOnSourceAndTargetCompartments();
            }
        }

        [DisplayName("To compartment")]
        [FeatureAttribute(ExportName = "To compartment", Order = 4)]
        [DynamicReadOnly]
        public ICompartment TargetCompartment
        {
            get { return targetCompartment; }
            set
            {
                if (value == null)
                {
                    targetCompartment = null;
                    return;
                }

                if (value.ParentManhole == null)
                {
                    log.WarnFormat(Resources.SewerConnection_TargetCompartment_We_cannot_add_compartment_as_target
                        , value.Name, Name);
                    return;
                }

                targetCompartment = value;
                UpdateTarget(targetCompartment);
                UpdateTargetCompartmentId();
                UpdateGeometryBasedOnSourceAndTargetCompartments();
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == nameof(SourceCompartment))
            {
                return !(Source is IManhole sourceManhole) || sourceManhole.Compartments.Count <= 1;
            }
            if (propertyName == nameof(TargetCompartment))
            {
                return Source == null || !(Target is IManhole targetManhole) || targetManhole.Compartments.Count <= 1;
            }

            return false;
        }

        protected virtual void UpdateGeometryBasedOnSourceAndTargetCompartments()
        {
            if (Source == null || Target == null) return;

            var sourceCoordinate = SourceCompartment?.Geometry?.Coordinate ?? Source?.Geometry?.Coordinate;
            var targetCoordinate = TargetCompartment?.Geometry?.Coordinate ?? Target?.Geometry?.Coordinate;

            if (sourceCoordinate == null || targetCoordinate == null) return;
            if (sourceCoordinate.Equals(targetCoordinate))
                targetCoordinate = new Coordinate(targetCoordinate.X+1,targetCoordinate.Y);
            Geometry = new LineString(new[] { sourceCoordinate, targetCoordinate });
            SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake();
        }

        public override bool IsLengthCustom
        {
            get { return true; }
        }

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
                AddBranchFeatureWhenEmpty(value);
                if (base.BranchFeatures != null)
                {
                    base.BranchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
                }
            }
        }

        private void AddBranchFeatureWhenEmpty(IEventedList<IBranchFeature> value)
        {
            //For the sewer connection we only allow one branch feature per sewer connection
            if (value != null && value.Count <= 1)
            {
                base.BranchFeatures = value;
            }
            else
            {
                log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, Name);
            }
        }
        
        private void UpdateTarget(ICompartment compartment)
        {
            var parent = compartment?.ParentManhole;
            BeforeTargetSet();
            if (!this.IsInternalConnection() && (IManhole) Target != parent)
            {
                target = parent;
            }
            AfterTargetSet();
        }

        private void UpdateSource(ICompartment compartment)
        {
            var parent = compartment?.ParentManhole;
            BeforeSetSource();
            if (!this.IsInternalConnection() && (IManhole) Source != parent)
            {
                source = parent;
            }
            AfterSetSource();

        }


        private void UpdateSourceCompartmentId()
        {
            if(sourceCompartment != null) sourceCompartmentName = sourceCompartment.Name;
        }
        
        private void UpdateTargetCompartmentId()
        {
            if(targetCompartment != null) targetCompartmentName = targetCompartment.Name;
        }

        private SewerConnectionSpecialConnectionType GetConnectionType()
        {
            if(!this.IsSpecialConnection()) return SewerConnectionSpecialConnectionType.None;

            if (!BranchFeatures.Any()) return SewerConnectionSpecialConnectionType.None;

            if(BranchFeatures.Any(bf => bf.GetType().Implements(typeof(IPump)))) return SewerConnectionSpecialConnectionType.Pump;
            if(BranchFeatures.Any(bf => bf.GetType().Implements(typeof(IWeir)))) return SewerConnectionSpecialConnectionType.Weir;

            return SewerConnectionSpecialConnectionType.None;
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action != NotifyCollectionChangeAction.Add) return;
            if (!BranchFeatures.Any() || e.Item is HydroLink || e.Item is LateralSource|| e.Item is CompositeBranchStructure) return;

            var compositeSubStructures = BranchFeatures
                                         .OfType<CompositeBranchStructure>()
                                         .SelectMany(f => f.Structures)
                                         .Where(s => s != e.Item)
                                         .ToList();

            var otherFeatures = BranchFeatures
                                .Where(f => !(f is LateralSource) && !(f is CompositeBranchStructure))
                                .Except(compositeSubStructures.Plus(e.Item));

            if (!compositeSubStructures.Any() && !otherFeatures.Any())
            {
                return;
            }

            log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
            e.Cancel = true;
        }

        #region IHydroNetworkFeature

        public virtual IHydroRegion Region { get { return HydroNetwork; } }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource { get { return false; } }

        public virtual bool CanBeLinkTarget { get { return false; } }

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            throw new NotSupportedException();
        }

        public virtual void UnlinkFrom(IHydroObject target)
        {
            throw new NotSupportedException();
        }

        public virtual bool CanLinkTo(IHydroObject target)
        {
            return false; // no linking to/from pipe yet
        }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return (IHydroNetwork) Network; }
        }

        public virtual string LongName { get; set; }

        #endregion


        #region Network is visiting us
        
        public void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            AddOrUpdateGeometry(hydroNetwork, helper);
            AddCrossSectionDefinition(hydroNetwork, helper);
            if (helper != null && helper.SewerConnectionsByName.ContainsKey(Name))
            {
                log.Warn($"SewerConnection {Name} already created");
                return;
            }
            lock (hydroNetwork.Branches)
            {
                hydroNetwork.Branches.Add(this);
            }
            helper?.SewerConnectionsByName?.AddOrUpdate(Name, this, (existingName, oldSewerConnection) =>
            {
                return Geometry == null ? oldSewerConnection : this;
            });


        }

        public void AddOrUpdateGeometry(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            IManhole sourceManhole = null;
            if (helper != null && !string.IsNullOrEmpty(SourceCompartmentName) &&
                !helper.ManholesByCompartmentName.TryGetValue(SourceCompartmentName, out sourceManhole))
                sourceManhole = null;

            IManhole targetManhole = null;
            if (helper != null && !string.IsNullOrEmpty(TargetCompartmentName) &&
                !helper.ManholesByCompartmentName.TryGetValue(TargetCompartmentName, out targetManhole))
                targetManhole = null;

            if (sourceManhole == null || targetManhole == null)
                hydroNetwork.FindAndConnectManholesInNetwork(this);
            else
            {
                ConnectSourceCompartment(sourceManhole);
                ConnectTargetCompartment(targetManhole);
            }

            SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake();
            if (Geometry != null)
            {
                foreach (var branchFeature in BranchFeatures.Where(bf => bf.Geometry == null))
                {
                    branchFeature.Geometry = HydroNetworkHelper.GetStructureGeometry(this, branchFeature.Chainage);
                    if (branchFeature is ICompositeBranchStructure compositeBranchStructure)
                    {
                        foreach (var structure1D in compositeBranchStructure.Structures.Where(s => s.Geometry == null))
                        {
                            structure1D.Geometry = HydroNetworkHelper.GetStructureGeometry(this, structure1D.Chainage);
                        }
                    }
                }
            }
            if(Geometry == null)
                Geometry = new LineString(new []{new Coordinate(0,0),new Coordinate(0,1)}); //stupid placeholder.
            UpdateBranchFeatureGeometries();
        }

        public void SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake()
        {
            if (Math.Abs(Length) < 1 && SourceCompartment?.Geometry?.Coordinate != null &&
                TargetCompartment?.Geometry?.Coordinate != null)
            {
                var distance = SourceCompartment.Geometry.Coordinate.Distance(TargetCompartment.Geometry.Coordinate);
                if (distance < 1)
                {
                    IsLengthCustom = true;
                    Length = 1;
                }
                else
                    Length = distance;
            }
        }

        private void AddCrossSectionDefinition(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            if (CrossSectionDefinitionName == null)
            {
                CrossSection = CrossSections.CrossSection.CreateDefault(CrossSectionType.Standard, this, Length / 2, true, hydroNetwork);
            }
            else
            {
                lock (hydroNetwork.SharedCrossSectionDefinitions)
                {
                    var crossSectionDefinition = hydroNetwork.SharedCrossSectionDefinitions.FirstOrDefault(cs => cs.Name == CrossSectionDefinitionName) as CrossSectionDefinitionStandard;
                    if (crossSectionDefinition != null)
                    {
                        var pipeCrossSection = CrossSections.CrossSection.CreateDefault(CrossSectionType.Standard, this, Length / 2);
                        pipeCrossSection.Name = NamingHelper.GetUniqueName("SewerProfile_{0}", hydroNetwork.CrossSections.Concat(hydroNetwork.Pipes.Select(p => p.CrossSection)), typeof(ICrossSection), true);
                        pipeCrossSection.UseSharedDefinition(crossSectionDefinition);
                        helper?.PipeCrossSections?.Enqueue(pipeCrossSection);
                        CrossSection = pipeCrossSection;
                        CrossSectionDefinitionName = CrossSection.Definition.Name;

                        OnAddCrossSectionDefinition(crossSectionDefinition);
                    }
                }
            }
        }

        protected virtual void OnAddCrossSectionDefinition(CrossSectionDefinitionStandard crossSectionDefinitionStandard)
        {

        }

        private void ConnectSourceCompartment(IManhole manhole)
        {
            if(manhole == null) return;
            var sourceCompartmentToAdd = manhole.GetCompartmentByName(SourceCompartmentName);
            sourceCompartment = sourceCompartmentToAdd;
            UpdateSource(sourceCompartment);
            UpdateSourceCompartmentId();
            UpdateGeometryBasedOnSourceAndTargetCompartments();
        }
        
        private void ConnectTargetCompartment(IManhole manhole)
        {
            if (manhole == null) return;
            var targetCompartmentToAdd = manhole.GetCompartmentByName(TargetCompartmentName);
            targetCompartment = targetCompartmentToAdd;
            UpdateTarget(targetCompartment);
            UpdateTargetCompartmentId();
            UpdateGeometryBasedOnSourceAndTargetCompartments();
        }

        public string SourceCompartmentName
        {
            get { return sourceCompartmentName; }
            set { sourceCompartmentName = value; }
        }

        public string TargetCompartmentName
        {
            get { return targetCompartmentName; }
            set { targetCompartmentName = value; }
        }

        public void UpdateBranchFeatureGeometries()
        {
            if (SourceCompartment != null && TargetCompartment != null)
                BranchFeatures.ForEach(bf =>
                {
                    bf.Geometry = GetBranchFeatureGeometry();
                    bf.Chainage = this.IsInternalConnection() ? 0 : Length / 2;
                });
        }

        private Point GetBranchFeatureGeometry()
        {
            var sewerConnectionCoordinates = new List<Coordinate>
            {
                SourceCompartment.Geometry.Coordinate,
                TargetCompartment.Geometry.Coordinate
            };
            var averageX = sewerConnectionCoordinates.Select(sc => sc.X).Average();
            var averageY = sewerConnectionCoordinates.Select(sc => sc.Y).Average();
            return new Point(averageX, averageY);
        }

        public string CrossSectionDefinitionName { get; set; }

        [DisplayName("Definition")]
        [FeatureAttribute(Order = 32, ExportName = "DefName")]
        public virtual string DefinitionName
        {
            get { return CrossSection.Definition.Name; }
            set
            {
                if (value == null || DefinitionName == value) return;
                var newDefinition = HydroNetwork?.SharedCrossSectionDefinitions.SingleOrDefault(scd => scd.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
                if(newDefinition == null) return;
                crossSection?.UseSharedDefinition(newDefinition);
            }
        }

        public virtual ICrossSection CrossSection
        {
            get { return crossSection; }
            set
            {
                crossSection = value;
                
                if (crossSection?.Definition != null)
                    CrossSectionDefinitionName = crossSection?.Definition?.Name;

                if (crossSection == null)
                {
                    return;
                }

                crossSection.Branch = this;
                crossSection.Chainage = Length / 2;

                if (HydroNetwork == null)
                {
                    return;
                }

                ICrossSectionDefinition sharedCrossSectionDefinition;
                lock (HydroNetwork.SharedCrossSectionDefinitions)
                {
                    sharedCrossSectionDefinition = HydroNetwork?.SharedCrossSectionDefinitions?
                        .FirstOrDefault(scsd => string.Equals(scsd.Name, crossSection.Definition.Name, StringComparison.InvariantCultureIgnoreCase));
                }

                if (sharedCrossSectionDefinition != null)
                {
                    crossSection?.UseSharedDefinition(sharedCrossSectionDefinition);
                }
                else
                {
                    lock (HydroNetwork.SharedCrossSectionDefinitions)
                    {
                        crossSection?.ShareDefinitionAndChangeToProxy();
                    }
                }
            }
        }

        public CrossSectionDefinitionStandard Profile
        {
            get { return CrossSection?.Definition.GetBaseDefinition() as CrossSectionDefinitionStandard; }
        }

        #endregion
    }
}