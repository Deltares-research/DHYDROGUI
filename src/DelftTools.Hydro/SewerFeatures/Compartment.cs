using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Compartment : ACompartment, IPointFeature, ICompartment, ICopyFrom, IItemContainer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Compartment));
        /// <summary>
        /// The default floodable area for a reservoir compartment (m²).
        /// </summary>
        public const double DefaultReservoirFloodableArea = 500.0;

        private IManhole parentManhole;
        private readonly NameValidator nameValidator;

        public Compartment(ILogHandler logHandler, string name):this(name)
        {
            LogHandler = logHandler;
        }
        
        public Compartment() : this("compartment")
        {
            
        }

        public Compartment(string name)
        {
            Name = name;
            InterpolationType = InterpolationType.Linear;
            nameValidator = NameValidator.CreateDefault();
        }

        [FeatureAttribute(Order = 1)]
        [DisplayName("Name")]
        public string Name { get; set; }
        
        [FeatureAttribute(Order = 0)]
        [DisplayName("Manhole name")]
        public string ManholeName => ParentManhole?.Name;

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
        public CompartmentShape Shape { get; set; }

        /// <summary>
        /// The storage type of the compartment.
        /// </summary>
        [FeatureAttribute(Order = 3)]
        [DisplayName("Compartment Storage Type")]
        public CompartmentStorageType CompartmentStorageType { get; set; } = CompartmentStorageType.Reservoir;

        /// <summary>
        /// Length of manhole (m).
        /// </summary>
        [FeatureAttribute(Order = 4)]
        [DynamicReadOnly]
        [DisplayName("Length")]
        public double ManholeLength { get; set; } = 0.64;

        /// <summary>
        /// Width of manhole (m).
        /// </summary>
        [FeatureAttribute(Order = 5)]
        [DynamicReadOnly]
        [DisplayName("Width")]
        public double ManholeWidth { get; set; } = 0.64;

        /// <summary>
        /// The area at surface level that this manhole can flood (m2).
        /// </summary>
        [FeatureAttribute(Order = 6)]
        [DynamicReadOnly]
        [DisplayName("Floodable area")]
        public double FloodableArea { get; set; } = DefaultReservoirFloodableArea;

        /// <summary>
        /// The bottom level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute(Order = 7)]
        [DynamicReadOnly]
        [DisplayName("Bottom level")]
        public double BottomLevel { get; set; } = -10.0;

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute(Order = 8)]
        [DynamicReadOnly]
        [DisplayName("Surface level")]
        public double SurfaceLevel { get; set; } = 0.0;

        /// <summary>
        /// The surface level of the manhole compared to Dutch NAP (m).
        /// </summary>
        [FeatureAttribute(Order = 9)]
        [DisplayName("Use a storage table")] 
        public bool UseTable { get; set; }

        /// <summary>
        /// The storage table in this compartment.
        /// </summary>
        [FeatureAttribute(Order = 10)]
        [DynamicReadOnly]
        [DisplayName("Storage table")]
        public IFunction Storage { get; set; } = FunctionHelper.Get1DFunction<double, double>("Storage Table", "Height", "Storage");

        [Description("Interpolate")]
        [FeatureAttribute(Order = 11)]
        [DynamicReadOnly]

        public InterpolationType InterpolationType
        {
            get { return Storage.Arguments[0].InterpolationType; }
            set { Storage.Arguments[0].InterpolationType = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsFieldReadOnly(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SurfaceLevel):
                case nameof(FloodableArea):
                case nameof(ManholeLength):
                case nameof(ManholeWidth):
                case nameof(BottomLevel):
                    return UseTable;
                case nameof(Storage):
                case nameof(InterpolationType):
                    return !UseTable;
                default:
                    return true;
            }
        }

        public IEnumerable<object> GetDirectChildren()
        {
            if (Storage != null)
                yield return Storage;
        }

        /// <summary>
        /// Returns the name of the Compartment object.
        /// </summary>
        /// <returns>The object name.</returns>
        public override string ToString()
        {
            return Name;
        }

        public override ILogHandler LogHandler { get; }

        public override ACompartment ProcessInput(object gwswElement)
        {
            return this;
        }

        public object Clone()
        {
            var compartment = new Compartment();
            compartment.CopyFrom(this);
            return compartment;
        }
        public void CopyFrom(object source)
        {
            if (!(source is ICompartment compartment)) 
                return;

            Name = compartment.Name;
            ParentManhole = compartment.ParentManhole;
            ParentManholeName = compartment.ParentManholeName;
            SurfaceLevel = compartment.SurfaceLevel;
            ManholeLength = compartment.ManholeLength;
            ManholeWidth = compartment.ManholeWidth;
            CompartmentStorageType = compartment.CompartmentStorageType;
            FloodableArea = compartment.FloodableArea;
            BottomLevel = compartment.BottomLevel;
            Geometry = compartment.Geometry;
            Shape = compartment.Shape;
            UseTable = compartment.UseTable;
            Storage = (IFunction)compartment.Storage.Clone(true);
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
        
        public void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
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

            if (helper != null) helper.ManholesByCompartmentName.AddOrUpdate(Name, manhole, (orgManholeName, oldManhole) => manhole);
        }
        public void TakeConnectionsOverFrom(ICompartment compartment)
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

        /// <inheritdoc/>
        public void SetNameIfValid(string name)
        {
            if (ValidateName(name))
            {
                Name = name;
            }
        }

        private bool ValidateName(string name)
        {
            ValidationResult result = nameValidator.Validate(name);
            if (result.Valid)
            {
                return true;
            }

            log.Warn(result.Message);
            return false;
        }

        /// <inheritdoc/>
        public virtual void AttachNameValidator(IValidator<string> subValidator)
        {
            Ensure.NotNull(subValidator, nameof(subValidator));
            nameValidator.AddValidator(subValidator);
        }

        /// <inheritdoc/>
        public virtual void DetachNameValidator(IValidator<string> subValidator)
        {
            Ensure.NotNull(subValidator, nameof(subValidator));
            nameValidator.RemoveValidator(subValidator);
        }
    }
}
