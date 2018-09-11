using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
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

        protected IEventedList<IBranchFeature> branchFeatures;
        private INode source;
        private INode target;
        private Compartment sourceCompartment;
        private Compartment targetCompartment;

        public SewerConnection() : this(string.Empty)
        {
        }

        public SewerConnection(string name)
            : base(name, null, null, 0)
        {
        }

        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        
        public SewerConnectionWaterType WaterType { get; set; }

        // This property is used in the NetworkLayerStyleFactory, do not remove :)
        public SewerConnectionSpecialConnectionType SpecialConnectionType { get { return GetConnectionType(); } }

        #region Source and Target

        public override INode Source
        {
            get { return source; }
            set
            {
                var manhole = value as Manhole;
                if(manhole == null || !manhole.Compartments.Any() || source == value) return;

                BeforeSetSource();
                source = value;
                if (sourceCompartment == null) SourceCompartment = manhole.Compartments.FirstOrDefault();
                AfterSetSource();
            }
        }

        [EditAction]
        private void BeforeSetSource()
        {
            source?.OutgoingBranches.Remove(this);
        }

        [EditAction]
        private void AfterSetSource()
        {
            source?.OutgoingBranches.Add(this);
        }

        public override INode Target
        {
            get { return target; }
            set
            {
                var manhole = value as Manhole;
                if (manhole == null || !manhole.Compartments.Any() || target == value) return;

                BeforeTargetSet();
                target = value;
                if (targetCompartment == null) TargetCompartment = manhole.Compartments.FirstOrDefault();
                AfterTargetSet();
            }
        }

        [EditAction]
        private void BeforeTargetSet()
        {
            target?.IncomingBranches.Remove(this);
        }

        [EditAction]
        private void AfterTargetSet()
        {
            target?.IncomingBranches.Add(this);
        }

        #endregion

        public Compartment SourceCompartment
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
                UpdateGeometry();
            }
        }

        public Compartment TargetCompartment
        {
            get { return targetCompartment; }
            set
            {
                if (value == null)
                {
                    sourceCompartment = null;
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
                UpdateGeometry();
            }
        }

        private void UpdateGeometry()
        {
            if (Source != null && Target != null)
            {
                Geometry = new LineString(new[] { Source.Geometry.Coordinate, Target.Geometry.Coordinate });
            }
        }

        public override bool IsLengthCustom
        {
            get { return true; }
        }
        
        public override IEventedList<IBranchFeature> BranchFeatures
        {
            get { return branchFeatures; }
            set
            {
                if (branchFeatures != null) branchFeatures.CollectionChanging -= BranchFeaturesOnCollectionChanging;
                AddBranchFeatureWhenEmpty(value);
                if (branchFeatures != null) branchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
            }
        }

        private void AddBranchFeatureWhenEmpty(IEventedList<IBranchFeature> value)
        {
            //For the sewer connection we only allow one branch feature per sewer connection
            if (value != null && value.Count <= 1)
            {
                branchFeatures = value;
            }
            else
            {
                Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
            }
        }

        private void UpdateTarget(Compartment compartment)
        {
            var parent = compartment?.ParentManhole;
            if (this.IsInternalConnection()) return;
            if ((Manhole) Target != parent)
            {
                Target = parent;
            }
        }

        private void UpdateSource(Compartment compartment)
        {
            var parent = compartment?.ParentManhole;
            if (this.IsInternalConnection()) return;
            if ((Manhole) Source != parent)
            {
                Source = parent;
            }
        }

        private void UpdateSourceCompartmentId()
        {
            if(sourceCompartment != null) SourceCompartmentName = sourceCompartment.Name;
        }

        private void UpdateTargetCompartmentId()
        {
            if(targetCompartment != null) TargetCompartmentName = targetCompartment.Name;
        }

        private SewerConnectionSpecialConnectionType GetConnectionType()
        {
            if(!this.IsSpecialConnection()) return SewerConnectionSpecialConnectionType.None;

            if (!BranchFeatures.Any()) return SewerConnectionSpecialConnectionType.None;

            if(BranchFeatures.Any(bf => bf.GetType() == typeof(Pump))) return SewerConnectionSpecialConnectionType.Pump;
            if(BranchFeatures.Any(bf => bf.GetType() == typeof(Weir))) return SewerConnectionSpecialConnectionType.Weir;

            return SewerConnectionSpecialConnectionType.None;
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action != NotifyCollectionChangeAction.Add) return;
            if (!branchFeatures.Any()) return;

            var compositeStructures = branchFeatures.OfType<CompositeBranchStructure>().ToList();
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
        
        public void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            hydroNetwork.Branches.RemoveAllWhere(sc => sc.Name == Name && sc is SewerConnection);

            var sourceManhole = hydroNetwork.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(SourceCompartmentName));
            var targetManhole = hydroNetwork.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(TargetCompartmentName));

            ConnectSourceCompartment(sourceManhole);
            ConnectTargetCompartment(targetManhole);
            AddCrossSectionDefinition(hydroNetwork);
            hydroNetwork.Branches.Add(this);
        }

        protected virtual void AddCrossSectionDefinition(IHydroNetwork hydroNetwork)
        {
        }

        private void ConnectSourceCompartment(IManhole manhole)
        {
            if(manhole == null) return;
            var sourceCompartmentToAdd = manhole.GetCompartmentByName(SourceCompartmentName);
            SourceCompartment = sourceCompartmentToAdd;
        }

        private void ConnectTargetCompartment(IManhole manhole)
        {
            if (manhole == null) return;
            var targetCompartmentToAdd = manhole.GetCompartmentByName(TargetCompartmentName);
            TargetCompartment = targetCompartmentToAdd;
        }

        public string SourceCompartmentName { get; set; }
        public string TargetCompartmentName { get; set; }
        public void UpdateBranchFeatureGeometries()
        {
            if (SourceCompartment != null || TargetCompartment != null)
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
