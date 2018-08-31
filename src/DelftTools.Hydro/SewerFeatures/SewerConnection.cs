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
        private Compartment sourceCompartment;
        private Compartment targetCompartment;

        public SewerConnection() : this(null, null)
        {
        }

        public SewerConnection(string name)
            : this(name, null, null, 0)
        {
        }

        public SewerConnection(IManhole sourceManhole, IManhole targetManhole)
            : this("sewerConnection", sourceManhole, targetManhole, 0)
        {
        }

        public SewerConnection(IManhole sourceManhole, IManhole targetManhole, double length)
            : this("sewerConnection", sourceManhole, targetManhole, length)
        {
        }

        public SewerConnection(string name, IManhole sourceManhole, IManhole targetManhole, double length) :
            base(name, sourceManhole, targetManhole, length)
        {
            if (sourceManhole?.Geometry == null || targetManhole?.Geometry == null) return;

            if (sourceManhole.Geometry.IsValid && targetManhole.Geometry.IsValid)
            {
                Geometry = new LineString(new[] { sourceManhole.Geometry.Coordinate, targetManhole.Geometry.Coordinate });
            }
        }

        public double LevelSource { get; set; }

        public double LevelTarget { get; set; }
        
        public SewerConnectionWaterType WaterType { get; set; }

        // This property is used in the NetworkLayerStyleFactory, do not remove :)
        public SewerConnectionSpecialConnectionType SpecialConnectionType { get { return GetConnectionType(); } }

        public Compartment SourceCompartment
        {
            get { return sourceCompartment; }
            set
            {
                sourceCompartment = value;
                UpdateSource(sourceCompartment);
                UpdateSourceCompartmentId();
            }
        }

        public Compartment TargetCompartment
        {
            get { return targetCompartment; }
            set
            {
                targetCompartment = value;
                UpdateTarget(targetCompartment);
                UpdateTargetCompartmentId();
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
            UpdateGeometry(sourceManhole, targetManhole);
            AddCrossSectionDefinition(hydroNetwork);
            hydroNetwork.Branches.Add(this);
        }

        protected virtual void AddCrossSectionDefinition(IHydroNetwork hydroNetwork)
        {
        }

        private void UpdateGeometry(IManhole sourceManhole, IManhole targetManhole)
        {
            if (sourceManhole != null && targetManhole != null)
            {
                Geometry = new LineString(new[] {sourceManhole.Geometry.Coordinate, targetManhole.Geometry.Coordinate});
            }
            else if (sourceManhole != null)
            {
                Geometry = new LineString(new[] {sourceManhole.Geometry.Coordinate, sourceManhole.Geometry.Coordinate});
            }
            else if (targetManhole != null)
            {
                Geometry = new LineString(new[] {targetManhole.Geometry.Coordinate, targetManhole.Geometry.Coordinate});
            }
            else
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 0)});
            }
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

        public string CrossSectionDefinitionId { get; set; }

        #endregion
    }
}
