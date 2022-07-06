using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    ///<summary>
    /// <see cref="Pump"/> Implements a 1D pump.
    /// Both the Sobek Pump (type 9) and River Pump (type 3) are implemented by this class.
    ///</summary>
    [Entity(FireOnCollectionChange=false)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Pump : BranchStructure, IPump, IHasSteerableProperties
    {
        /// <summary>
        /// Creates a new <see cref="Pump"/> without time-varying data and with a default name.
        /// </summary>
        public Pump() : this("Pump") {}

        /// <summary>
        /// Creates a new <see cref="Pump"/> with a default name.
        /// </summary>
        /// <param name="canBeTimeDependent">Whether time-varying data is allowed.</param>
        public Pump(bool canBeTimeDependent) :this("Pump", canBeTimeDependent) {}

        /// <summary>
        /// Creates a new <see cref="Pump"/> with the given name.
        /// </summary>
        /// <param name="name">The name of the new pump</param>
        /// <param name="canBeTimeDependent">Whether time-varying data is allowed.</param>
        public Pump(string name, bool canBeTimeDependent = false)
        {
            Name = name;
            StartDelivery = 0;
            StopDelivery = 0;
            StartSuction = 3.0;
            StopSuction = 2.0;
            DirectionIsPositive = true;
            ControlDirection = PumpControlDirection.SuctionSideControl;
            ReductionTable = FunctionHelper.Get1DFunction<double, double>("reduction", "difference", "factor");

            CanBeTimedependent = canBeTimeDependent;
            capacity = ConstructCapacitySteerableProperty();
        }

        private SteerableProperty ConstructCapacitySteerableProperty()
        {
            const double defaultValue = 1.0;
            return CanBeTimedependent
                       ? new SteerableProperty(defaultValue,
                                               "Capacity",
                                               "Capacity",
                                               "m3/s")
                       : new SteerableProperty(defaultValue);
        }

        private SteerableProperty capacity;

        public virtual bool CanBeTimedependent { get; protected set; }

        [DisplayName("Positive direction")]
        [FeatureAttribute(Order = 5, ExportName = "PosDir")]
        public virtual bool DirectionIsPositive { get; set; }

        [FeatureAttribute(Order = 6)]
        public virtual double Capacity
        {
            get => capacity.Constant; 
            set => capacity.Constant = value;
        }

        public virtual bool UseCapacityTimeSeries
        {
            get => capacity.CurrentDriver == SteerablePropertyDriver.TimeSeries;
            set => capacity.CurrentDriver = value ? SteerablePropertyDriver.TimeSeries : SteerablePropertyDriver.Constant;
        }

        public virtual TimeSeries CapacityTimeSeries { 
            get => capacity.TimeSeries; 
            protected set => capacity.TimeSeries = value;
        }

        [DisplayName("Start delivery")]
        [FeatureAttribute(Order = 7, ExportName = "OnDelivery")]
        public virtual double StartDelivery { get; set; }

        [DisplayName("Stop delivery")]
        [FeatureAttribute(Order = 8, ExportName = "OffDelivery")]
        public virtual double StopDelivery { get; set; }

        [DisplayName("Start suction")]
        [FeatureAttribute(Order = 9, ExportName = "OnSuction")]
        public virtual double StartSuction { get; set; }

        [DisplayName("Stop suction")]
        [FeatureAttribute(Order = 10, ExportName = "OffSuction")]
        public virtual double StopSuction { get; set; }

        [DisplayName("Control on")]
        [FeatureAttribute(Order = 11, ExportName = "ControlDir")]
        public virtual PumpControlDirection ControlDirection { get; set; }

        public virtual double OffsetZ
        {
            get
            {
                double[] relevantZValues;
                switch (ControlDirection)
                {
                    case PumpControlDirection.DeliverySideControl:
                        relevantZValues = new[] { StopDelivery, StartDelivery };
                        break;
                    case PumpControlDirection.SuctionSideControl:
                        relevantZValues = new[] { StartSuction, StopSuction };
                        break;
                    default:
                        relevantZValues = new[] { StartSuction, StopSuction, StartDelivery, StopDelivery };
                        break;
                }
                //return the middle between min and max
                return (relevantZValues.Min() + relevantZValues.Max()) / 2;    
            }
        }

        /// <summary>
        /// reduction table
        /// </summary>
        public virtual IFunction ReductionTable { get; set; }
        
        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var pump = (Pump) source;

            Attributes = (IFeatureAttributeCollection) pump.Attributes.Clone();

            CanBeTimedependent = pump.CanBeTimedependent;
            capacity = new SteerableProperty(pump.capacity);

            StopDelivery = pump.StopDelivery;
            StartDelivery = pump.StartDelivery;
            StartSuction = pump.StartSuction;
            StopSuction = pump.StopSuction;
            ControlDirection = pump.ControlDirection;
            ReductionTable = (IFunction) pump.ReductionTable.Clone(true);
            DirectionIsPositive = pump.DirectionIsPositive;
        }

        public override StructureType GetStructureType() => StructureType.Pump;

        public virtual void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            ISewerConnection sewerConnection = null;
            if (helper == null)
            {
                //read from network.
                sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(sc =>
                    sc.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            }

            if (sewerConnection == null &&
                (helper == null || !helper.SewerConnectionsByName.TryGetValue(Name, out sewerConnection)))
            {
                sewerConnection = GetNewSewerConnectionWithPump(hydroNetwork, helper);
                sewerConnection.AddToHydroNetwork(hydroNetwork, helper);
            }

            var pumps = sewerConnection?.BranchFeatures.OfType<IPump>().ToArray();
            if (pumps != null && pumps.Any())
            {
                SetSewerConnectionProperties(sewerConnection, hydroNetwork, helper);
                pumps.ForEach(CopyPropertyValuesToExistingPump);
            }
            else
            {
                //remove old bf and add this one
                lock (hydroNetwork.BranchFeatures)
                {
                    sewerConnection?.BranchFeatures.RemoveAllWhere(bf =>
                        bf.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                        || bf is ICompositeBranchStructure compositeBranchStructure &&
                        compositeBranchStructure.Structures.Any(s =>
                            s.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)));

                    var composite = sewerConnection.AddStructureToBranch(this, false);
                    composite.Name = "CompositeBranchStructure_1D_";
                    helper?.CompositeBranchStructures?.Enqueue(composite);
                }
            }

        }

        private ISewerConnection GetNewSewerConnectionWithPump(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            if (helper == null || !helper.SewerConnectionsByName.TryGetValue(Name, out var sewerConnection))
            {
                sewerConnection = new SewerConnection(Name);
                SetSewerConnectionProperties(sewerConnection, hydroNetwork, helper);
            }

            var composite = sewerConnection.AddStructureToBranch(this, false);
            composite.Name = "CompositeBranchStructure_1D_";
            helper?.CompositeBranchStructures?.Enqueue(composite);
            return sewerConnection;
        }

        protected virtual void CopyPropertyValuesToExistingPump(IPump pump)
        {
        }

        protected virtual void SetSewerConnectionProperties(ISewerConnection sewerConnection, IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
        }

        public virtual IEnumerable<SteerableProperty> RetrieveSteerableProperties()
        {
            yield return capacity;
        }
    }
}