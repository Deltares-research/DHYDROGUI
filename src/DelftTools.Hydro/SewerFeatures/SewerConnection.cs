using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(SewerConnection));

        private INode source;
        private INode target;
        private ICompartment sourceCompartment;
        private ICompartment targetCompartment;
        private string sourceCompartmentName;
        private string targetCompartmentName;

        public SewerConnection() : this("SewerConnection")
        {
        }

        public SewerConnection(string name)
            : base(name, null, null, 0)
        {
        }

        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }

        [DisplayName("Sewer type")]
        [FeatureAttribute(ExportName = "Sewer type", Order = 20)]
        public SewerConnectionWaterType WaterType { get; set; }

        // This property is used in the NetworkLayerStyleFactory, do not remove :)
        [DisplayName("Sewer Special Connection type")]
        [FeatureAttribute(ExportName = "Sewer Special Connection type", Order = 21)]
        [InvokeRequired]
        public SewerConnectionSpecialConnectionType SpecialConnectionType { get { return GetConnectionType(); } }

        #region Source and Target

        [DisplayName("From manhole")]
        [FeatureAttribute(ExportName = "From manhole", Order = 10)]
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
            var manhole = value as Manhole;
            if (manhole?.Compartments != null && manhole.Compartments.Any())
            {
                source = value;
                if (sourceCompartment == null || !manhole.ContainsCompartmentWithName(sourceCompartment.Name))
                {
                    sourceCompartment = manhole.Compartments.FirstOrDefault();
                    UpdateSource(sourceCompartment);
                    UpdateSourceCompartmentId();
                    UpdateGeometryBasedOnSourceAndTargetCompartments();
                }

            }
            else
            {
                source = null;
            }

            AfterSetSource();
        }

        private void BeforeSetSource()
        {
            source?.OutgoingBranches.Remove(this);
        }

        private void AfterSetSource()
        {
            source?.OutgoingBranches.Add(this);
        }

        [DisplayName("To manhole")]
        [FeatureAttribute(ExportName = "To manhole", Order = 11)]
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

            var manhole = value as IManhole;
            if (manhole?.Compartments != null && manhole.Compartments.Any())
            {
                target = value;
                if (targetCompartment == null || !manhole.ContainsCompartmentWithName(targetCompartment.Name))
                {
                    targetCompartment = manhole.Compartments.FirstOrDefault();
                    UpdateTarget(targetCompartment);
                    UpdateTargetCompartmentId();
                    UpdateGeometryBasedOnSourceAndTargetCompartments();
                }
            }
            else
            {
                target = null;
            }

            AfterTargetSet();
        }

        private void BeforeTargetSet()
        {
            target?.IncomingBranches.Remove(this);
        }

        private void AfterTargetSet()
        {
            target?.IncomingBranches.Add(this);
        }

        #endregion

        [DisplayName("From compartment")]
        [FeatureAttribute(ExportName = "From compartment", Order = 12)]
        [ReadOnly(true)]
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
                    Log.WarnFormat(Resources.SewerConnection_TargetCompartment_We_cannot_add_compartment_as_source
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
        [FeatureAttribute(ExportName = "To compartment", Order = 13)]
        [ReadOnly(true)]
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
                    Log.WarnFormat(Resources.SewerConnection_TargetCompartment_We_cannot_add_compartment_as_target
                        , value.Name, Name);
                    return;
                }

                targetCompartment = value;
                UpdateTarget(targetCompartment);
                UpdateTargetCompartmentId();
                UpdateGeometryBasedOnSourceAndTargetCompartments();
            }
        }

        
        protected virtual void UpdateGeometryBasedOnSourceAndTargetCompartments()
        {
            if (Source == null || Target == null) return;

            var sourceCoordinate = SourceCompartment.Geometry.Coordinate;
            var targetCoordinate = TargetCompartment.Geometry.Coordinate;
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
                Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, Name);
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

            var compositeStructures = BranchFeatures.Where(bf => !(bf is LateralSource)).OfType<CompositeBranchStructure>().ToList();
            if (!compositeStructures.Any() ||
                (compositeStructures.First().Structures.Any() &&
                 !compositeStructures.First().Structures.Contains(e.Item)))
            {
                Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
                e.Cancel = true;
            }
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

        public virtual IHydroNetwork HydroNetwork { get { return (IHydroNetwork) Network; } }
        public virtual string LongName { get; set; }

        #endregion


        #region Network is visiting us
        
        public void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            hydroNetwork.Branches.RemoveAllWhere(sc => sc.Name == Name && sc is SewerConnection);

            //var hydroNetworkManholes = hydroNetwork.Manholes.ToDictionary(m=>m.Compartments.SelectMany(c =>c.Name));

            IManhole sourceManhole = null;
            if (helper != null && !string.IsNullOrEmpty(SourceCompartmentName) && !helper.ManholesByCompartmentName.TryGetValue(SourceCompartmentName, out sourceManhole))
                sourceManhole = null;

            IManhole targetManhole= null;
            if (helper != null && !string.IsNullOrEmpty(TargetCompartmentName) && !helper.ManholesByCompartmentName.TryGetValue(TargetCompartmentName, out targetManhole))
                targetManhole = null;
            
            if (sourceManhole == null && targetManhole == null)
                hydroNetwork.FindAndConnectManholesInNetwork(this);
            else
            {
                ConnectSourceCompartment(sourceManhole);
                ConnectTargetCompartment(targetManhole);
            }
            SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake();

            AddCrossSectionDefinition(hydroNetwork);
            hydroNetwork.Branches.Add(this);
        }

        private void SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake()
        {
            if (Math.Abs(Length) < 10e-6 && SourceCompartment?.Geometry?.Coordinate != null &&
                TargetCompartment?.Geometry?.Coordinate != null)
            {
                if (SourceCompartment.Geometry.Coordinate.Equals(TargetCompartment.Geometry.Coordinate))
                {
                    Length = SourceCompartment.Geometry.Coordinate.Distance(
                        new Coordinate(TargetCompartment.Geometry.Coordinate.X + 1,
                            TargetCompartment.Geometry.Coordinate.Y));
                }
                else
                {
                    Length = SourceCompartment.Geometry.Coordinate.Distance(TargetCompartment.Geometry.Coordinate);
                }
            }
        }

        protected virtual void AddCrossSectionDefinition(IHydroNetwork hydroNetwork)
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
                BranchFeatures.ForEach(bf => bf.Geometry = GetBranchFeatureGeometry());
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

        #endregion
    }
}
