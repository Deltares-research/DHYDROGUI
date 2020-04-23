using System;
using System.ComponentModel;
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
        private IManhole parentManhole;
        private string name;
        private static ILog Log = LogManager.GetLogger(typeof(Compartment));

        public Compartment() : this("compartment")
        {
            
        }

        public Compartment(string name)
        {
            Name = name;
        }

        [FeatureAttribute]
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
        [FeatureAttribute]
        public CompartmentShape Shape { get; set; }

        /// <summary>
        /// Length of manhole (m).
        /// </summary>
        [FeatureAttribute]
        public double ManholeLength { get; set; }

        /// <summary>
        /// Width of manhole (m).
        /// </summary>
        [FeatureAttribute]
        public double ManholeWidth { get; set; }

        /// <summary>
        /// The area at surface level that this manhole can flood (m2).
        /// </summary>
        [FeatureAttribute]
        public double FloodableArea { get; set; }

        /// <summary>
        /// The bottom level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute]
        public double BottomLevel { get; set; }

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute]
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
        
        public virtual void AddToHydroNetwork(IHydroNetwork network, SewerImporterHelper helper)
        {
            AssignParentManholeNameIfMissing(network);
            IManhole manhole = null;
            if (helper != null && !helper.ManholesByManholeName.TryGetValue(ParentManholeName, out manhole))
            {
                if (!helper.ManholesByCompartmentName.TryGetValue(Name, out manhole))
                {
                    manhole = null;
                }
            }

            if (helper == null && manhole == null)
                manhole = network.GetManhole(this);

            if (manhole != null)
            {
                //Log.Info("replacing compartments");
                var existingCompartment = manhole.Compartments.FirstOrDefault(c => c.Name.Equals(Name,StringComparison.InvariantCultureIgnoreCase));
                CopyToExistingCompartmentPropertyValues(existingCompartment);
                ReplaceCompartmentInManhole(existingCompartment, manhole, helper);
                ReconnectSewerConnections(existingCompartment, network);
            }
            else
            {
                lock (network.Nodes)
                {
                    var newManhole = CreateManholeWithCompartment(network, helper);
                    network.Nodes.Add(newManhole);
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
