using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Compartment : IPointFeature, ICompartment
    {
        private IManhole parentManhole;
        private string name;
        
        public Compartment() : this("compartment")
        {
            
        }
        public Compartment(ICompartment compartment) : this("outletCompartment")
        {
            Name = compartment.Name;
            ParentManhole = compartment.ParentManhole;
            ParentManholeName = compartment.ParentManholeName;
            SurfaceLevel = compartment.SurfaceLevel;
            ManholeLength = compartment.ManholeLength;
            ManholeWidth = compartment.ManholeWidth;
            FloodableArea = compartment.FloodableArea;
            BottomLevel = compartment.BottomLevel;
            Geometry = compartment.Geometry;
            Shape = compartment.Shape;
        }
        public Compartment(string name)
        {
            Name = name;
            SurfaceLevel = 0.0;
            BottomLevel = -2.0;
            FloodableArea = 100.0;
            ManholeLength = 0.64;
            ManholeWidth = 0.64;
        }

        [FeatureAttribute(Order = 1)]
        [DisplayName("Name")]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
        }

        /// <summary>
        /// The manhole that contains this compartment.
        /// </summary>
        [NoNotifyPropertyChange]
        public IManhole ParentManhole
        {
            get { return parentManhole; }
            set
            {
                parentManhole = value;
                if (parentManhole != null)
                {
                    ParentPointFeature = parentManhole;
                }
            }
        }

        /// <summary>
        /// The shape of the manhole (either square or rectangular).
        /// </summary>
        [FeatureAttribute(Order = 2)]
        [DisplayName("Shape")]
        public virtual CompartmentShape Shape { get; set; }

        /// <summary>
        /// The storage type of the compartment.
        /// </summary>
        [FeatureAttribute(Order = 3)]
        [DisplayName("Compartment Storage Type")]
        public CompartmentStorageType CompartmentStorageType { get; set; } = CompartmentStorageType.Reservoir;

        /// <summary>
        /// Length of manhole (m).
        /// </summary>
        [FeatureAttribute(Order = 3)]
        [DisplayName("Length")]
        public virtual double ManholeLength { get; set; }

        /// <summary>
        /// Width of manhole (m).
        /// </summary>
        [FeatureAttribute(Order = 4)]
        [DisplayName("Width")]
        public virtual double ManholeWidth { get; set; }

        /// <summary>
        /// The area at surface level that this manhole can flood (m2).
        /// </summary>
        [FeatureAttribute(Order = 5)]
        [DisplayName("Floodable area")]
        public virtual double FloodableArea { get; set; }

        /// <summary>
        /// The bottom level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute(Order = 6)]
        [DisplayName("Bottom level")]
        public virtual double BottomLevel { get; set; }

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute(Order = 7)]
        [DisplayName("Surface level")]
        public virtual double SurfaceLevel { get; set; }

        /// <summary>
        /// Returns the name of the Compartment object.
        /// </summary>
        /// <returns>The object name.</returns>
        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            return new Compartment();
        }

        public Type GetEntityType()
        {
            return typeof(Compartment);
        }

        public long Id { get; set; }

        public IGeometry Geometry { get; set; }

        public IFeatureAttributeCollection Attributes { get; set; }

        [NoNotifyPropertyChange]
        public ICompositeNetworkPointFeature ParentPointFeature { get; set; }

        #region Network is visiting us

        public string ParentManholeName { get; set; }
        
        public virtual void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            AssignParentManholeNameIfMissing(hydroNetwork);
            IManhole manhole = null;
            if (helper != null && 
                !helper.ManholesByManholeName.TryGetValue(ParentManholeName, out manhole) && 
                !helper.ManholesByCompartmentName.TryGetValue(Name, out manhole))
            {
                manhole = null;
            }

            if (helper == null && manhole == null)
                manhole = hydroNetwork.GetManhole(this);

            if (manhole != null)
            {
                var existingCompartment = manhole.Compartments.FirstOrDefault(c => c.Name.Equals(Name,StringComparison.InvariantCultureIgnoreCase));
                CopyToExistingCompartmentPropertyValues(existingCompartment);
                ReplaceCompartmentInManhole(existingCompartment, manhole, helper);
                ReconnectSewerConnections(existingCompartment, hydroNetwork);
            }
            else
            {
                lock (hydroNetwork.Nodes)
                {
                    var newManhole = CreateManholeWithCompartment(hydroNetwork, helper);
                    hydroNetwork.Nodes.Add(newManhole);
                    if (helper != null) helper.ManholesByManholeName.AddOrUpdate(ParentManholeName, newManhole, (orgParentManholeName, oldManhole) => newManhole);
                }
            }
        }

        private Manhole CreateManholeWithCompartment(IHydroNetwork network, SewerImporterHelper helper)
        {
            var newManhole = new Manhole(ParentManholeName);
            if (Name == null)
            {
                Name = HydroNetworkHelper.CreateUniqueCompartmentNameInNetwork(network);
            }
            
            lock(newManhole.Compartments)
            {
                newManhole.Compartments.Add(this);
            }
            if (helper != null) helper.ManholesByCompartmentName.AddOrUpdate(Name, newManhole, (orgManholeName, oldManhole) => newManhole);
            return newManhole;
        }
        
        private void AssignParentManholeNameIfMissing(IHydroNetwork network)
        {
            if (ParentManholeName == null)
                ParentManholeName = HydroNetworkHelper.GetUniqueManholeIdInNetwork(network);
        }

        protected void ReplaceCompartmentInManhole(ICompartment oldCompartment, IManhole manhole, SewerImporterHelper helper)
        {
            lock (manhole.Compartments)
            {
                manhole.Compartments.Remove(oldCompartment);
                manhole.Compartments.Add(this);
            }

            if (helper != null) helper.ManholesByCompartmentName.AddOrUpdate(name, manhole, (orgManholeName, oldManhole) => manhole);
        }
        public virtual void TakeConnectionsOverFrom(ICompartment compartment)
        {
            var hydroNetwork = ParentManhole?.HydroNetwork;
            if (hydroNetwork != null)
            {
                ReconnectSewerConnections(compartment, hydroNetwork);
            }

        }
        protected void ReconnectSewerConnections(ICompartment oldCompartment, IHydroNetwork network)
        {
            ReconnectSources(oldCompartment, network);
            ReconnectTargets(oldCompartment, network);
        }

        private void ReconnectSources(ICompartment oldCompartment, IHydroNetwork network)
        {
            lock (network.Branches)
            {
                var sewerConnectionsToReconnectToSource = network.SewerConnections.Where(sc =>
                    sc.SourceCompartment != null && sc.SourceCompartment.Equals(oldCompartment));
                sewerConnectionsToReconnectToSource.ForEach(sc => sc.SourceCompartment = this);
            }
        }

        private void ReconnectTargets(ICompartment oldCompartment, IHydroNetwork network)
        {
            lock (network.Branches)
            {
                var sewerConnectionsToReconnectToTarget = network.SewerConnections.Where(sc =>
                    sc.TargetCompartment != null && sc.TargetCompartment.Equals(oldCompartment));
                sewerConnectionsToReconnectToTarget.ForEach(sc => sc.TargetCompartment = this);
            }
        }

        protected virtual void CopyToExistingCompartmentPropertyValues(ICompartment existingCompartment)
        {
        }

        #endregion
    }
}
