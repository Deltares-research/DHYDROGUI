using System;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Compartment : IPointFeature, ICompartment
    {
        private static ILog Log = LogManager.GetLogger(typeof(Compartment));
        private Manhole parentManhole;
        public Compartment() : this("compartment")
        {
            
        }

        public Compartment(string name)
        {
            Name = name;
        }

        

        [FeatureAttribute]
        public string Name { get; set; }

        /// <summary>
        /// The manhole that contains this compartment.
        /// </summary>
        [NoNotifyPropertyChange]
        public Manhole ParentManhole
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
        public CompartmentShape Shape { get; set; }

        /// <summary>
        /// Length of manhole (m).
        /// </summary>
        public double ManholeLength { get; set; }

        /// <summary>
        /// Width of manhole (m).
        /// </summary>
        public double ManholeWidth { get; set; }

        /// <summary>
        /// The area at surface level that this manhole can flood (m2).
        /// </summary>
        public double FloodableArea { get; set; }

        /// <summary>
        /// The bottom level of the manhole compared to Dutch NAP (m).
        /// </summary>
        public double BottomLevel { get; set; }

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        public double SurfaceLevel { get; set; }
        
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

        public virtual void AddToHydroNetwork(IHydroNetwork network)
        {
            AssignParentManholeNameIfMissing(network);
            var manhole = GetManholeInNetworkToAddCompartmentTo(network);
            if (manhole != null)
            {
                var duplicateCompartment = manhole.Compartments.FirstOrDefault(c => c.Name == Name);
                CopyExistingCompartmentPropertyValuesToNewCompartment(duplicateCompartment);
                ReplaceCompartmentInManhole(duplicateCompartment, manhole);
                ReconnectSewerConnections(duplicateCompartment, network);
            }
            else
            {
                var newManhole = CreateManholeWithCompartment(network);
                network.Nodes.Add(newManhole);
            }
        }

        private Manhole CreateManholeWithCompartment(IHydroNetwork network)
        {
            var newManhole = new Manhole(ParentManholeName);
            if (Name == null)
            {
                Name = HydroNetworkHelper.CreateUniqueCompartmentNameInNetwork(network);
            }

            newManhole.Compartments.Add(this);
            return newManhole;
        }

        private Manhole GetManholeInNetworkToAddCompartmentTo(IHydroNetwork network)
        {
            var manhole = network.Manholes.FirstOrDefault(m => m.Name == ParentManholeName) as Manhole;
            if (manhole == null)
            {
                manhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(Name)) as Manhole;
            }

            return manhole;
        }

        private void AssignParentManholeNameIfMissing(IHydroNetwork network)
        {
            if (ParentManholeName == null)
                ParentManholeName = HydroNetworkHelper.GetUniqueManholeIdInNetwork(network);
        }

        protected void ReplaceCompartmentInManhole(Compartment oldCompartment, IManhole manhole)
        {
            manhole.Compartments.Remove(oldCompartment);
            manhole.Compartments.Add(this);
        }

        protected void ReconnectSewerConnections(Compartment oldCompartment, IHydroNetwork network)
        {
            ReconnectSources(oldCompartment, network);
            ReconnectTargets(oldCompartment, network);
        }

        private void ReconnectSources(Compartment oldCompartment, IHydroNetwork network)
        {
            var sewerConnectionsToReconnectToSource = network.SewerConnections.Where(sc => sc.SourceCompartment != null && sc.SourceCompartment.Equals(oldCompartment));
            sewerConnectionsToReconnectToSource.ForEach(sc => sc.SourceCompartment = this);
        }

        private void ReconnectTargets(Compartment oldCompartment, IHydroNetwork network)
        {
            var sewerConnectionsToReconnectToTarget = network.SewerConnections.Where(sc => sc.TargetCompartment != null && sc.TargetCompartment.Equals(oldCompartment));
            sewerConnectionsToReconnectToTarget.ForEach(sc => sc.TargetCompartment = this);
        }

        protected virtual void CopyExistingCompartmentPropertyValuesToNewCompartment(Compartment existingCompartment)
        {
        }

        #endregion
    }
}
