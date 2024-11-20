using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class PumpInStructureViewShape : CompositeShapeFeature
    {
        private readonly IPump pump;
        private readonly IChart chart;
        private SymbolShapeFeature symbolShapeFeature;
        private  VectorStyle normalLineStyle;
        private  VectorStyle normalLineSelectedStyle;
        private  VectorStyle offLevelLineSelectedStyle;
        private readonly bool horizontalAxisIsReversed;
        private static readonly Bitmap PumpSmallNegativeIcon = Properties.Resources.PumpSmallNegative;
        private static readonly Bitmap PumpSmallPositiveIcon = Properties.Resources.PumpSmallPositive;

        public PumpInStructureViewShape(IChart chart, IPump pump, bool horizontalAxisIsReversed)
            : base(chart)
        {
            this.horizontalAxisIsReversed = horizontalAxisIsReversed;
            this.chart = chart;
            this.pump = pump;

            CreateLineStyles();
            CalculateShapeFeatures();
        }

        private void CreateLineStyles()
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


        /// <summary>
        /// Custom paint method since x of level lines is dependend of zoom-level
        /// </summary>
        /// <param name="vectorStyle"></param>
        public override void Paint(VectorStyle  vectorStyle)
        {
            //custom paint logic :)
            CalculateShapeFeatures();
            base.Paint(vectorStyle);
        }

        public override bool Contains(int x, int y)
        {
            CalculateShapeFeatures();
            return base.Contains(x, y);
            //get current shapes
        }

        private double GetImageWidthInWorld()
        {
            var imageWidth = symbolShapeFeature.Image.Width - 2;
            return GetWorldWith(imageWidth);
        }

        private double GetWorldWith(int deviceWidth)
        {
            return ChartCoordinateService.ToWorldWidth(chart, deviceWidth);
        }

        /// <summary>
        /// delivery is left if we pump along the branch and the axis is reversed
        /// or we pump against the branch and the axis was not reversed
        /// </summary>
        private bool DeliverySideIsLeft
        {
            get { return pump.DirectionIsPositive == horizontalAxisIsReversed;}
        }
        private void CalculateShapeFeatures()
        {
            //keep selection
            bool wasSelected = Selected;
            ShapeFeatures.Clear();
            ClearHovers();

            Image image = DeliverySideIsLeft
                              ? PumpSmallNegativeIcon
                              : PumpSmallPositiveIcon;
            symbolShapeFeature = new SymbolShapeFeature(chart, pump.OffsetY, pump.OffsetZ,
                                                        SymbolShapeFeatureHorizontalAlignment.Left,
                                                        SymbolShapeFeatureVerticalAlignment.Center) { Image = image };


            
            //we should draw a line left if we have delivery control and delivery is left
            //or we have suction control and suction is left
            //or we have both side control
            if ((pump.ControlDirection ==  PumpControlDirection.DeliverySideControl)||
                (pump.ControlDirection == PumpControlDirection.SuctionAndDeliverySideControl))
            {
                ShapeFeatures.Add(GetHorizontalLine(pump.StopDelivery, true, HoverPosition.Right, Color.Red, "switch off delivery"));
                ShapeFeatures.Add(GetHorizontalLine(pump.StartDelivery, false, HoverPosition.Right, Color.Black, "switch on delivery"));
            }

            //we should draw a line right if...see above
            if ((pump.ControlDirection == PumpControlDirection.SuctionSideControl) ||
                (pump.ControlDirection == PumpControlDirection.SuctionAndDeliverySideControl))
            {
                ShapeFeatures.Add(GetHorizontalLine(pump.StartSuction, false, HoverPosition.Left, Color.Black, "switch on suction"));
                ShapeFeatures.Add(GetHorizontalLine(pump.StopSuction, true, HoverPosition.Left, Color.Red, "switch off suction"));
            }
            ShapeFeatures.Add(symbolShapeFeature);
            //reset selection as it was
            Selected = wasSelected;
        }

        /// <summary>
        /// Draws a horizontal line at y
        /// </summary>
        /// <param name="y"></param>
        /// <param name="isStopLevel">Stop level allows custom style</param>
        /// <param name="hoverText"></param>
        /// <returns></returns>
        private IShapeFeature GetHorizontalLine(double y, bool isStopLevel, HoverPosition hoverPosition, 
            Color hoverColor, string hoverText)
        {
            var left = symbolShapeFeature.Left;
            var right = left + GetImageWidthInWorld();

            //if delivery is left this is suction so the bottom is a off line

            var line = new FixedRectangleShapeFeature(chart,
                                                  left,
                                                  y,
                                                  right - left, 2,
                                                  true,
                                                  false);

            UpdateLineStyle(line, isStopLevel);
            AddHover(new HoverText(hoverText, string.Format("{0:f2}m.", y),
                                   line, hoverColor, hoverPosition,
                                   ArrowHeadPosition.None) { ShowLine = false, BackColor = Color.WhiteSmoke, HoverType = HoverType.Selected});

            return line;
        }

        private void UpdateLineStyle(FixedRectangleShapeFeature line, bool isOffLevel)
        {
            if (isOffLevel)
            {
                line.SelectedStyle = offLevelLineSelectedStyle;
                line.NormalStyle = normalLineStyle;
            }
            else
            {
                line.SelectedStyle = normalLineSelectedStyle;
                line.NormalStyle = normalLineStyle;    
            }
        }
    }
}
