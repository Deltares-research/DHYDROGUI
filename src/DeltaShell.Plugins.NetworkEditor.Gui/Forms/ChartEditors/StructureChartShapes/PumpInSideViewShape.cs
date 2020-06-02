using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class PumpInSideViewShape : StructureSideViewShape<IPump>
    {
        //5 pixel line
        private const int HorizontalLineLength = 10;
        private static readonly Bitmap PumpSmallLeftIcon = Resources.PumpSmallLeft;
        private static readonly Bitmap PumpSmallRightIcon = Resources.PumpSmallRight;
        private readonly bool horizontalAxisIsReversed;
        private VectorStyle normalLineStyle;
        private VectorStyle normalLineSelectedStyle;
        private VectorStyle offLevelLineSelectedStyle;

        public PumpInSideViewShape(IChart chart, double offset, IPump pump, bool horizontalAxisIsReversed)
            : base(chart, offset, pump)
        {
            this.horizontalAxisIsReversed = horizontalAxisIsReversed;

            //get a shape for the image. Don't generate this one all the
            //time since it contains our selection
            /*Image image = DeliverySideIsLeft
                              ? Properties.Resources.PumpSmallLeft
                              : Properties.Resources.PumpSmallRight;
            symbolShapeFeature = new SymbolShapeFeature(chart, offset, pump.OffsetZ) { Image = image };*/
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
                Fill = new SolidBrush(Color.FromArgb(alpha, Color.Black)),
                Line = new Pen(Color.FromArgb(alpha, Color.Black))
            };

            normalLineSelectedStyle = new VectorStyle
            {
                //solid black
                Fill = new SolidBrush(Color.Black),
                Line = new Pen(Color.Black)
            };
        }

        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            //we should draw a line left if we have delivery control and delivery is left
            //or we have suction control and suction is left
            //or we have both side control
            if (DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.DeliverySideControl ||
                !DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.SuctionSideControl ||
                Structure.ControlDirection == PumpControlDirection.SuctionAndDeliverySideControl)
            {
                yield return GetLeftBottomLine();
                yield return GetLeftTopLine();
                yield return GetLeftVerticalLine();
            }

            //we should draw a line right if...see above
            if (DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.SuctionSideControl ||
                !DeliverySideIsLeft && Structure.ControlDirection == PumpControlDirection.DeliverySideControl ||
                Structure.ControlDirection == PumpControlDirection.SuctionAndDeliverySideControl)
            {
                yield return GetRightBottomLine();
                yield return GetRightTopLine();
                yield return GetRightVerticalLine();
            }

            Image image = DeliverySideIsLeft
                              ? PumpSmallLeftIcon
                              : PumpSmallRightIcon;
            var symbolShapeFeature = new SymbolShapeFeature(Chart, OffsetInSideView, Structure.OffsetZ,
                                                            SymbolShapeFeatureHorizontalAlignment.Center,
                                                            SymbolShapeFeatureVerticalAlignment.Center) {Image = image};

            yield return symbolShapeFeature;
        }

        /// <summary>
        /// delivery is left if we pump along the branch and the axis is reversed
        /// or we pump against the branch and the axis was not reversed
        /// </summary>
        private bool DeliverySideIsLeft
        {
            get
            {
                return Structure.DirectionIsPositive == horizontalAxisIsReversed;
            }
        }

        private double GetImageWidthInWorld()
        {
            Image image = DeliverySideIsLeft
                              ? PumpSmallLeftIcon
                              : PumpSmallRightIcon;
            int imageWidth = image.Width - 2;
            return GetWorldWidth(imageWidth);
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
            double x = OffsetInSideView - (GetImageWidthInWorld() / 2);
            IList<double> relevantZValues = GetRelevantZValuesLeft();
            relevantZValues.Add(Structure.OffsetZ);

            double top = relevantZValues.Max();
            double bottom = relevantZValues.Min();

            double height = top - bottom;
            var line = new FixedRectangleShapeFeature(Chart,
                                                      x,
                                                      top,
                                                      2, height,
                                                      false,
                                                      true);
            UpdateLineStyle(line, false);
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
                line.AddHover(new HoverText("stop", string.Format("{0:f2}m.", 56),
                                            line, offLevelLineSelectedStyle.Line.Color, HoverPosition.Left,
                                            ArrowHeadPosition.None));
            }
            else
            {
                line.SelectedStyle = normalLineSelectedStyle;
                line.NormalStyle = normalLineStyle;
                line.AddHover(new HoverText("start", string.Format("{0:f2}m.", 56),
                                            line, offLevelLineSelectedStyle.Line.Color, HoverPosition.Left,
                                            ArrowHeadPosition.None));
            }
        }

        /// <summary>
        /// Determines the left Z values.
        /// </summary>
        /// <returns></returns>
        private IList<double> GetRelevantZValuesLeft()
        {
            return DeliverySideIsLeft
                       ? new List<double>(new[]
                       {
                           Structure.StartDelivery,
                           Structure.StopDelivery
                       })
                       : new List<double>(new[]
                       {
                           Structure.StartSuction,
                           Structure.StopSuction
                       });
        }

        private IList<double> GetRelevantZValuesRight()
        {
            return DeliverySideIsLeft
                       ? new List<double>(new[]
                       {
                           Structure.StartSuction,
                           Structure.StopSuction
                       })
                       : new List<double>(new[]
                       {
                           Structure.StartDelivery,
                           Structure.StopDelivery
                       });
        }

        private double GetHorizontalLineLengthInWorld()
        {
            return GetWorldWidth(HorizontalLineLength);
        }

        private FixedRectangleShapeFeature GetRightVerticalLine()
        {
            //move a little to the left so we fall into the symbol
            double x = (OffsetInSideView - GetWorldWidth(2)) + (GetImageWidthInWorld() / 2);
            IList<double> relevantZValues = GetRelevantZValuesRight();
            relevantZValues.Add(Structure.OffsetZ);

            double top = relevantZValues.Max();
            double bottom = relevantZValues.Min();

            double height = top - bottom;

            var line = new FixedRectangleShapeFeature(Chart,
                                                      x,
                                                      top,
                                                      2, height,
                                                      false,
                                                      true);

            UpdateLineStyle(line, false);
            return line;
        }

        private FixedRectangleShapeFeature GetLeftTopLine()
        {
            double right = (GetWorldWidth(2) + OffsetInSideView) - (GetImageWidthInWorld() / 2);
            double left = right - GetHorizontalLineLengthInWorld();
            IList<double> relevantZValues = GetRelevantZValuesLeft();

            double y = relevantZValues.Max();

            //var height = top - bottom;
            var line = new FixedRectangleShapeFeature(Chart,
                                                      left,
                                                      y,
                                                      right - left, 2,
                                                      true,
                                                      false);

            //suction left so bottom is stop level
            UpdateLineStyle(line, DeliverySideIsLeft);

            return line;
        }

        private FixedRectangleShapeFeature GetLeftBottomLine()
        {
            double right = (GetWorldWidth(2) + OffsetInSideView) - (GetImageWidthInWorld() / 2);
            double left = right - GetHorizontalLineLengthInWorld();
            IList<double> relevantZValues = GetRelevantZValuesLeft();

            double y = relevantZValues.Min();

            //var height = top - bottom;
            var line = new FixedRectangleShapeFeature(Chart,
                                                      left,
                                                      y,
                                                      right - left, 2,
                                                      true,
                                                      false);

            //suction left so bottom is stop level
            UpdateLineStyle(line, !DeliverySideIsLeft);

            return line;
        }

        private FixedRectangleShapeFeature GetRightTopLine()
        {
            double left = OffsetInSideView + (GetImageWidthInWorld() / 2);
            double right = left + GetHorizontalLineLengthInWorld();

            IList<double> relevantZValues = GetRelevantZValuesRight();

            double y = relevantZValues.Max();

            //var height = top - bottom;
            var line = new FixedRectangleShapeFeature(Chart,
                                                      left,
                                                      y,
                                                      right - left, 2,
                                                      true,
                                                      false);

            //delivery is right so this is a stop line
            UpdateLineStyle(line, !DeliverySideIsLeft);

            return line;
        }

        private FixedRectangleShapeFeature GetRightBottomLine()
        {
            double left = -GetWorldWidth(2) + OffsetInSideView + (GetImageWidthInWorld() / 2);
            double right = left + GetHorizontalLineLengthInWorld();

            IList<double> relevantZValues = GetRelevantZValuesRight();

            double y = relevantZValues.Min();

            //if delivery is left this is suction so the bottom is a off line

            //var height = top - bottom;
            var line = new FixedRectangleShapeFeature(Chart,
                                                      left,
                                                      y,
                                                      right - left, 2,
                                                      true,
                                                      false);

            UpdateLineStyle(line, DeliverySideIsLeft);

            return line;
        }
    }
}