using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public interface IDrawingShape
    {
        double TopLevel { get; set; }

        double BottomLevel { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        double LeftOffset { get; set; }

        double TopOffset { get; set; }
    }

    [Entity]
    public class CompartmentShape : IDrawingShape
    {
        public double TopOffset { get; set; }

        public double LeftOffset { get; set; }

        public Compartment Compartment { get; set; }

        public double TopLevel
        {
            get { return Compartment?.SurfaceLevel ?? double.NaN; }
            set { }
        }

        public double BottomLevel
        {
            get { return Compartment?.BottomLevel ?? double.NaN; }
            set { }
        }

        public double Width
        {
            get { return Compartment?.ManholeWidth / 1000 ?? double.NaN; }
            set { }
        }

        public double Height
        {
            get { return Compartment?.SurfaceLevel - Compartment?.BottomLevel ?? double.NaN; }
            set { }
        }
    }

    [Entity]
    public class PipeShape : IDrawingShape
    {
        private CompartmentShape connectedCompartmentShape;

        public CompartmentShape ConnectedCompartmentShape
        {
            get { return connectedCompartmentShape; }
            set
            {
                if (connectedCompartmentShape != null)
                {
                    ResetProperties();
                }

                connectedCompartmentShape = value;

                if (connectedCompartmentShape != null)
                {
                    SetProperties();
                }
            }
        }

        private void ResetProperties()
        {
            BottomLevel = 0;
            TopLevel = 0;
        }

        private void SetProperties()
        {
            CalculateBottomLevel();
            TopLevel = CalculateTopLevel();
        }

        private void CalculateBottomLevel()
        {
            var connectedCompartment = connectedCompartmentShape?.Compartment;

            if (Pipe == null || connectedCompartment == null) return;

            if (connectedCompartment == Pipe.SourceCompartment)
            {
                BottomLevel = Pipe.LevelSource;
            }
            else if (connectedCompartment == Pipe.TargetCompartment)
            {
                BottomLevel = Pipe.LevelTarget;
            }
        }

        private double CalculateTopLevel()
        {
            return BottomLevel + Height;
        }

        public double TopLevel { get; set; }

        public double BottomLevel { get; set; }

        public double Width { get { return 0.25; } set { } }

        public double Height { get { return 0.25; } set { } }

        public double LeftOffset { get; set; }

        public double TopOffset { get; set; }

        public IPipe Pipe { get; set; }
    }

    [Entity]
    public class ConnectionShape : IDrawingShape
    {
        protected Compartment SourceCompartment;
        protected Compartment TargetCompartment;
        private CompartmentShape sourceCompartmentShape;
        private CompartmentShape targetCompartmentShape;
        public double TopOffset { get; set; }

        public CompartmentShape SourceCompartmentShape
        {
            get { return sourceCompartmentShape; }
            set
            {
                sourceCompartmentShape = value;
                SourceCompartment = sourceCompartmentShape?.Compartment;
            }
        }

        public CompartmentShape TargetCompartmentShape
        {
            get { return targetCompartmentShape; }
            set
            {
                targetCompartmentShape = value;
                TargetCompartment = targetCompartmentShape?.Compartment;
            }
        }

        public virtual double TopLevel { get; set; }

        public virtual double BottomLevel { get; set; }

        public virtual double Width
        {
            get { return GetWidthBasedOnCompartments(); }
            set { }
        }

        public double Height
        {
            get { return TopLevel - BottomLevel; }
            set { }
        }

        public double LeftOffset { get; set; }


        protected double GetWidthBasedOnCompartments()
        {
            var sourceIsLeft = sourceCompartmentShape.LeftOffset < targetCompartmentShape.LeftOffset;

            var leftShape = sourceIsLeft ? sourceCompartmentShape : targetCompartmentShape;
            var rightShape = sourceIsLeft ? targetCompartmentShape : sourceCompartmentShape;

            return rightShape.LeftOffset - (leftShape.LeftOffset + leftShape.Width);
        }
    }

    [Entity]
    public class PumpShape : ConnectionShape
    {
        public IPump Pump { get; set; }

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

        public override double Width
        {
            get { return GetWidthBasedOnCompartments() * 4.0; }
            set { }
        }
    }

    [Entity]
    public class OrificeShape : ConnectionShape
    {
        public SewerConnectionOrifice Orifice { get; set; }

        public override double BottomLevel
        {
            get { return Orifice?.Bottom_Level ?? double.NaN; }
            set { }
        }

        public override double TopLevel
        {
            get { return GetTopLevelBasedOnCompartments(); }
            set { }
        }

        private double GetTopLevelBasedOnCompartments()
        {
            if (SourceCompartment == null || TargetCompartment == null)
            {
                return double.NaN;
            }

            return Math.Min(SourceCompartment.SurfaceLevel, TargetCompartment.SurfaceLevel);

        }
    }
}