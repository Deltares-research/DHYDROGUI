using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public interface IDrawingShape
    {
        double TopLevel { get; set; }

        double BottomLevel { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        double WidthPix { get; set; }

        double HeightPix { get; set; }

        #region Offset

        double TopOffset { get; set; }

        double TopOffsetPix { get; set; }

        double LeftOffset { get; set; }

        double LeftOffsetPix { get; set; }

        #endregion

        void SetPixelValues(double minX, double maxX, double minY, double maxY, double actualWidth, double actualHeight);

    }

    [Entity]
    public class DrawingShape : IDrawingShape
    {
        public virtual double TopLevel { get; set; }
        public virtual double BottomLevel { get; set; }
        public virtual double Width { get; set; }
        public virtual double Height { get; set; }
        public virtual double WidthPix { get; set; }
        public virtual double HeightPix { get; set; }
        public virtual double TopOffset { get; set; }
        public virtual double TopOffsetPix { get; set; }
        public virtual double LeftOffset { get; set; }
        public virtual double LeftOffsetPix { get; set; }
        public void SetPixelValues(double minX, double maxX, double minY, double maxY, double actualWidth, double actualHeight)
        {
            LeftOffsetPix = CoordinateScalingHelper.ScaleX(LeftOffset, minX, maxX, actualWidth);
            TopOffsetPix = CoordinateScalingHelper.ScaleY(TopLevel, minY, maxY, actualHeight);
            WidthPix = CoordinateScalingHelper.ScaleWidth(Width, minX, maxX, actualWidth);
            HeightPix = CoordinateScalingHelper.ScaleHeight(Height, minY, maxY, actualHeight);
        }
    }

    [Entity]
    public class CompartmentShape : DrawingShape
    {
        private Compartment compartment;

        public Compartment Compartment
        {
            get { return compartment; }
            set
            {
                compartment = value;

                if (compartment != null)
                {
                    SetProperties();
                }
            }
        }

        private void SetProperties()
        {
            TopLevel = compartment.SurfaceLevel;
            BottomLevel = compartment.BottomLevel;
            Width = compartment.ManholeWidth;
            Height = compartment.SurfaceLevel - compartment.BottomLevel;
        }
    }

    [Entity]
    public class PipeShape : DrawingShape
    {
        public IPipe Pipe { get; set; }

        public CompartmentShape ConnectedCompartmentShape { get; set; }

        public override double TopLevel
        {
            get { return BottomLevel + Height; }
            set { }
        }
        public override double BottomLevel
        {
            get { return CalculateBottomLevel(); }
            set { }
        }

        public override double Width
        {
            get { return GetPipeWidth(); }
            set { }
        }

        public override double Height
        {
            get { return GetPipeHeight(); }
            set { }
        }

        private double CalculateBottomLevel()
        {
            var connectedCompartment = ConnectedCompartmentShape?.Compartment;

            if (Pipe == null || connectedCompartment == null) return double.NaN;

            if (connectedCompartment == Pipe.SourceCompartment)
            {
                return Pipe.LevelSource;
            }

            return connectedCompartment == Pipe.TargetCompartment ? Pipe.LevelTarget : double.NaN;
        }

        private double GetPipeWidth()
        {
            var shape = Pipe?.SewerProfileDefinition?.Shape;
            if (shape == null) return 0;
            var rectangleShape = shape as CrossSectionStandardShapeWidthHeightBase;
            if (rectangleShape != null)
            {
                return rectangleShape.Width;
            }

            var roundShape = shape as CrossSectionStandardShapeRound;
            if (roundShape != null)
            {
                return roundShape.Diameter;
            }

            throw new ArgumentException($"Pipe shape {shape?.Type} is not yet supported");
        }

        private double GetPipeHeight()
        {
            var shape = Pipe?.SewerProfileDefinition?.Shape;
            if (shape == null) return 0;
            var rectangleShape = shape as CrossSectionStandardShapeWidthHeightBase;
            if (rectangleShape != null)
            {
                return rectangleShape.Height;
            }

            var roundShape = shape as CrossSectionStandardShapeRound;
            if (roundShape != null)
            {
                return roundShape.Diameter;
            }

            throw new ArgumentException($"Sewer pipe shape {shape?.Type} is not yet supported");
        }
    }

    [Entity]
    public class ConnectionShape : DrawingShape
    {
        private Compartment sourceCompartment;
        private Compartment targetCompartment;
        private CompartmentShape sourceCompartmentShape;
        private CompartmentShape targetCompartmentShape;

        public CompartmentShape SourceCompartmentShape
        {
            get { return sourceCompartmentShape; }
            set
            {
                sourceCompartmentShape = value;
                sourceCompartment = sourceCompartmentShape?.Compartment;
            }
        }

        public CompartmentShape TargetCompartmentShape
        {
            get { return targetCompartmentShape; }
            set
            {
                targetCompartmentShape = value;
                targetCompartment = targetCompartmentShape?.Compartment;
            }
        }
     
        public override double Width
        {
            get { return GetWidthBasedOnCompartments(); }
            set { }
        }
        
        public override double Height
        {
            get { return TopLevel - BottomLevel; }
            set { }
        }

        protected double GetWidthBasedOnCompartments()
        {
            var sourceIsLeft = sourceCompartmentShape.LeftOffset < targetCompartmentShape.LeftOffset;

            var leftShape = sourceIsLeft ? sourceCompartmentShape : targetCompartmentShape;
            var rightShape = sourceIsLeft ? targetCompartmentShape : sourceCompartmentShape;

            return rightShape.LeftOffset - (leftShape.LeftOffset + leftShape.Width);
        }

        protected double GetTopLevelBasedOnCompartments()
        {
            if (sourceCompartment == null || targetCompartment == null)
            {
                return double.NaN;
            }

            return Math.Min(sourceCompartment.SurfaceLevel, targetCompartment.SurfaceLevel);
        }

        protected double GetBottomLevelBasedOnCompartments()
        {
            if (sourceCompartment == null || targetCompartment == null)
            {
                return double.NaN;
            }

            return Math.Max(sourceCompartment.BottomLevel, targetCompartment.BottomLevel);
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
    public class WeirShape : ConnectionShape
    {
        public Weir Weir { get; set; }

        public override double BottomLevel
        {
            get { return Weir?.CrestLevel ?? double.NaN; }
            set { }
        }

        public override double TopLevel
        {
            get { return GetTopLevelBasedOnCompartments(); }
            set { }
        }
    }

    [Entity]
    public class OrificeShape : ConnectionShape
    {
        public SewerConnectionOrifice Orifice { get; set; }

        public override double BottomLevel
        {
            get { return GetBottomLevelBasedOnCompartments(); }
            set { }
        }

        public override double TopLevel
        {
            get { return Orifice?.Bottom_Level ?? double.NaN; }
            set { }
        }
    }
}