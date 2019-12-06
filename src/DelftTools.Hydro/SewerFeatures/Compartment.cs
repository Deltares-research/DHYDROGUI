using System;
using System.Collections.Generic;
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
        [EditAction]
        public virtual void AddToHydroNetwork(IHydroNetwork network, SewerImporterHelper helper)
        {
            AssignParentManholeNameIfMissing(network);
            IManhole manhole;
            if (!helper.ManholesByManholeName.TryGetValue(ParentManholeName, out manhole))
            {
                if (!helper.ManholesByCompartmentName.TryGetValue(Name, out manhole))
                {
                    manhole = null;
                }
            }

            if (manhole != null)
            {
                var duplicateCompartment = manhole.Compartments.FirstOrDefault(c => c.Name == Name);
                CopyExistingCompartmentPropertyValuesToNewCompartment(duplicateCompartment);
                ReplaceCompartmentInManhole(duplicateCompartment, manhole, helper);
                ReconnectSewerConnections(duplicateCompartment, network);
            }
            else
            {
                var newManhole = CreateManholeWithCompartment(network, helper);
                network.Nodes.Add(newManhole);
                helper.ManholesByManholeName[ParentManholeName] = newManhole;
            }
        }

        private Manhole CreateManholeWithCompartment(IHydroNetwork network, SewerImporterHelper helper)
        {
            var newManhole = new Manhole(ParentManholeName);
            if (Name == null)
            {
                Name = HydroNetworkHelper.CreateUniqueCompartmentNameInNetwork(network);
            }
            
            newManhole.Compartments.Add(this);
            helper.ManholesByCompartmentName[Name] = newManhole;
            return newManhole;
        }

        /*private Manhole GetManholeInNetworkToAddCompartmentTo(IHydroNetwork network)
        {
            var networkManholes = new HashSet<IManhole>(network.Manholes);
            var manhole = networkManholes.FirstOrDefault(m => m.Name.Equals(ParentManholeName,StringComparison.InvariantCultureIgnoreCase)) as Manhole;
            if (manhole == null)
            {
                manhole = networkManholes.FirstOrDefault(m => m.ContainsCompartmentWithName(Name)) as Manhole;
            }

            return manhole;
        }*/

        private void AssignParentManholeNameIfMissing(IHydroNetwork network)
        {
            if (ParentManholeName == null)
                ParentManholeName = HydroNetworkHelper.GetUniqueManholeIdInNetwork(network);
        }

        protected void ReplaceCompartmentInManhole(ICompartment oldCompartment, IManhole manhole, SewerImporterHelper helper)
        {
            manhole.Compartments.Remove(oldCompartment);
            manhole.Compartments.Add(this);
            helper.ManholesByCompartmentName[name] = manhole;
        }

        protected void ReconnectSewerConnections(ICompartment oldCompartment, IHydroNetwork network)
        {
            ReconnectSources(oldCompartment, network);
            ReconnectTargets(oldCompartment, network);
        }

        private void ReconnectSources(ICompartment oldCompartment, IHydroNetwork network)
        {
            var sewerConnectionsToReconnectToSource = network.SewerConnections.Where(sc => sc.SourceCompartment != null && sc.SourceCompartment.Equals(oldCompartment));
            sewerConnectionsToReconnectToSource.ForEach(sc => sc.SourceCompartment = this);
        }

        private void ReconnectTargets(ICompartment oldCompartment, IHydroNetwork network)
        {
            var sewerConnectionsToReconnectToTarget = network.SewerConnections.Where(sc => sc.TargetCompartment != null && sc.TargetCompartment.Equals(oldCompartment));
            sewerConnectionsToReconnectToTarget.ForEach(sc => sc.TargetCompartment = this);
        }

        protected virtual void CopyExistingCompartmentPropertyValuesToNewCompartment(ICompartment existingCompartment)
        {
        }

        #endregion
    }
}
