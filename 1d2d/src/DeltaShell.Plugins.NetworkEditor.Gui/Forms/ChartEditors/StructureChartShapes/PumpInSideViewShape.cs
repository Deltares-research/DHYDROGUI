using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class PumpInSideViewShape : StructureSideViewShape<IPump>
    {
        //5 pixel line
        private const int horizontalLineLength = 10;
        private  VectorStyle normalLineStyle;
        private  VectorStyle normalLineSelectedStyle;
        private  VectorStyle offLevelLineSelectedStyle;
        private readonly bool horizontalAxisIsReversed;
        private static readonly Bitmap pumpSmallLeftIcon = Properties.Resources.PumpSmallLeft;
        private static readonly Bitmap pumpSmallRightIcon = Properties.Resources.PumpSmallRight;
        private readonly double iconLocationY;

        public PumpInSideViewShape(IChart chart, 
                                   double offset, 
                                   double iconLocationY,
                                   IPump pump, 
                                   bool horizontalAxisIsReversed)
            : base(chart, offset, pump)
        {
            this.iconLocationY = iconLocationY;
            this.horizontalAxisIsReversed = horizontalAxisIsReversed;
        }

        protected override void CreateStyles()
        {
            var alpha = 40;
  
            offLevelLineSelectedStyle = new VectorStyle
                                            {
                                                Fill = new SolidBrush(Color.Red),
                                                Line = new Pen(Color.Red)
                                            };

            normalLineStyle = new VectorStyle
                                  {
                                      Fill = new SolidBrush(Color.FromArgb(alpha,Color.Black)),
                                      Line = new Pen(Color.FromArgb(alpha,Color.Black))
                                  };

            normalLineSelectedStyle = new VectorStyle
                                          {
                                              //solid black
                                              Fill = new SolidBrush(Color.Black),
                                              Line = new Pen(Color.Black)
                                          };
        }

        private double GetImageWidthInWorld()
        {
            Image image = DeliverySideIsLeft
                              ? pumpSmallLeftIcon
                              : pumpSmallRightIcon;
            var imageWidth = image.Width - 2;
            return GetWorldWidth(imageWidth);
        }

        /// <summary>
        /// delivery is left if we pump along the branch and the axis is reversed
        /// or we pump against the branch and the axis was not reversed
        /// </summary>
        private bool DeliverySideIsLeft => Structure.DirectionIsPositive == horizontalAxisIsReversed;

        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            
            //we should draw a line left if we have delivery control and delivery is left
            //or we have suction control and suction is left
            //or we have both side control
            if (((DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.DeliverySideControl)) ||
                (!DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.SuctionSideControl) ||
                Structure.ControlDirection == PumpControlDirection.SuctionAndDeliverySideControl)
            {
                yield return GetLeftBottomLine();
                yield return GetLeftTopLine();
                yield return GetLeftVerticalLine();
            }

            //we should draw a line right if...see above
            if (((DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.SuctionSideControl)) ||
                (!DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.DeliverySideControl) ||
                Structure.ControlDirection == PumpControlDirection.SuctionAndDeliverySideControl)
            {
                yield return GetRightBottomLine();
                yield return GetRightTopLine();
                yield return GetRightVerticalLine();
            }
            Image image = DeliverySideIsLeft
                  ? pumpSmallLeftIcon
                  : pumpSmallRightIcon;
            var symbolShapeFeature = new SymbolShapeFeature(Chart, 
                                                            OffsetInSideView, 
                                                            iconLocationY,
                                                            SymbolShapeFeatureHorizontalAlignment.Center,
                                                            SymbolShapeFeatureVerticalAlignment.Center)
                                         {Image = image};

            yield return symbolShapeFeature;
        }

        /// <summary>
        /// Since the axis of a sideview is always in the direction of the branch. The
        /// pump direction controls what is left suction of deliverty.
        /// If direction is positive the pump goes in the direction of the branch and hence 
        /// suction is the left line.
        /// </summary>
        /// <param name="imageWidthInWorld"></param>
        /// <returns></returns>
        private FixedRectangleShapeFeature GetLeftVerticalLine()
        {
            var x = OffsetInSideView - GetImageWidthInWorld()/2;
            var relevantZValues = GetRelevantZValuesLeft();
            relevantZValues.Add(Structure.OffsetZ);

            double top = relevantZValues.Max();
            double bottom = relevantZValues.Min();

            var height = (top - bottom);
            var line = new FixedRectangleShapeFeature(Chart,
                                                         x,
                                                         top,
                                                         2, height,
                                                         false,
                                                         true);
            UpdateLineStyle(line,false);
            return line;
        }

        /// <summary>
        /// Gets line style for a givens
        /// </summary>
        /// <param name="line">SharpFeature to style</param>
        /// <param name="isStopLevel">Determines whether the line is a stop line. Allows custom styling</param>
        private void UpdateLineStyle(ShapeFeatureBase line, bool isStopLevel)
        {
            if (isStopLevel)
            {
                line.SelectedStyle = offLevelLineSelectedStyle;
                line.NormalStyle = normalLineStyle;
                line.AddHover(new HoverText("stop", $"{56:f2}m.",
                          line, offLevelLineSelectedStyle.Line.Color, HoverPosition.Left,
                          ArrowHeadPosition.None));

            }
            else
            {
                line.SelectedStyle = normalLineSelectedStyle;
                line.NormalStyle = normalLineStyle;
                line.AddHover(new HoverText("start", $"{56:f2}m.",
                          line, offLevelLineSelectedStyle.Line.Color, HoverPosition.Left,
                          ArrowHeadPosition.None));
            }
        }

        /// <summary>
        /// Determines the left Z values.
        /// </summary>
        /// <returns></returns>
        private IList<double> GetRelevantZValuesLeft() =>
            DeliverySideIsLeft
                ? new List<double>(new[] { Structure.StartDelivery, Structure.StopDelivery })
                : new List<double>(new[] { Structure.StartSuction, Structure.StopSuction });

        private IList<double> GetRelevantZValuesRight() =>
            DeliverySideIsLeft
                ? new List<double>(new[] { Structure.StartSuction, Structure.StopSuction })
                : new List<double>(new[] { Structure.StartDelivery, Structure.StopDelivery });

        private double GetHorizontalLineLengthInWorld() => 
            GetWorldWidth(horizontalLineLength);

        private FixedRectangleShapeFeature GetRightVerticalLine()
        {
            //move a little to the left so we fall into the symbol
            var x = OffsetInSideView - GetWorldWidth(2) + GetImageWidthInWorld() / 2;
            var relevantZValues = GetRelevantZValuesRight();
            relevantZValues.Add(Structure.OffsetZ);
            
            double top = relevantZValues.Max();
            double bottom = relevantZValues.Min();

            var height = top - bottom;
            
            var line = new FixedRectangleShapeFeature(Chart,
                                                         x,
                                                         top,
                                                         2, height,
                                                         false,
                                                         true);
            
            UpdateLineStyle(line,false);
            return line;
        }

        

        private FixedRectangleShapeFeature GetLeftTopLine()
        {
            var right = GetWorldWidth(2) + OffsetInSideView- GetImageWidthInWorld() / 2;
            var left = right - GetHorizontalLineLengthInWorld();
            var relevantZValues = GetRelevantZValuesLeft();

            double y = relevantZValues.Max();

            var line = new FixedRectangleShapeFeature(Chart,
                                                         left,
                                                         y,
                                                         right - left, 2,
                                                         true,
                                                         false);
 
            //suction left so bottom is stop level
            UpdateLineStyle(line,DeliverySideIsLeft);
            
            return line;
        }
        private FixedRectangleShapeFeature GetLeftBottomLine()
        {
            var right = GetWorldWidth(2)+ OffsetInSideView- GetImageWidthInWorld() / 2;
            var left = right - GetHorizontalLineLengthInWorld();
            var relevantZValues = GetRelevantZValuesLeft();

            double y = relevantZValues.Min() ;
            
            var line = new FixedRectangleShapeFeature(Chart,
                                                         left,
                                                         y,
                                                         right - left, 2,
                                                         true,
                                                         false);

            //suction left so bottom is stop level
            UpdateLineStyle(line,!DeliverySideIsLeft);
            
            return line;

        }

        private FixedRectangleShapeFeature GetRightTopLine()
        {
            var left = OffsetInSideView + GetImageWidthInWorld() / 2;
            var right = left + GetHorizontalLineLengthInWorld();

            var relevantZValues = GetRelevantZValuesRight();

            double y = relevantZValues.Max();

            var line = new FixedRectangleShapeFeature(Chart,
                                                         left,
                                                         y,
                                                         right - left, 2,
                                                         true,
                                                         false);

            //delivery is right so this is a stop line
            UpdateLineStyle(line,!DeliverySideIsLeft);
            
            return line;
        }

        private FixedRectangleShapeFeature GetRightBottomLine()
        {
            var left = -GetWorldWidth(2) + OffsetInSideView+ GetImageWidthInWorld() / 2;
            var right = left + GetHorizontalLineLengthInWorld();

            var relevantZValues = GetRelevantZValuesRight();

            double y = relevantZValues.Min();

            //if delivery is left this is suction so the bottom is a off line

            var line =  new FixedRectangleShapeFeature(Chart,
                                                  left,
                                                  y,
                                                  right - left, 2,
                                                  true,
                                                  false);
            
            UpdateLineStyle(line,DeliverySideIsLeft);    
            
            return line;
        }
    }
}
