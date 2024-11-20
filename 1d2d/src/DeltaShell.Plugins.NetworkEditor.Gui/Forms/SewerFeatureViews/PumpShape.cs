using System;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PumpShape : InternalConnectionShape
    {
        public IPump Pump { get; set; }

        public override object Source
        {
            get { return Pump; }
            set { Pump = value as IPump; }
        }
        
        public double StartDeliveryLevel
        {
            get { return Pump?.StartDelivery ?? double.NaN; }
            set { }
        }

        public double StopDeliveryLevel
        {
            get { return Pump?.StopDelivery ?? double.NaN; }
            set { }
        }
        public double StartSuctionLevel
        {
            get { return Pump?.StartSuction ?? double.NaN; }
            set { }
        }
        public double StopSuctionLevel
        {
            get { return Pump?.StopSuction ?? double.NaN; }
            set { }
        }

        public override double TopLevel
        {
            get { return Math.Max(StartDeliveryLevel, StartSuctionLevel); }
            set { }
        }
        public override double BottomLevel
        {
            get { return Math.Min(StopDeliveryLevel, StopSuctionLevel); }
            set { }
        }
    }
}