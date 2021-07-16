using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public class NetworkSideViewShapeHandler
    {
        private readonly IChart chart;
        private ShapeModifyTool shapeModifyTool;
        private StructurePresenter structurePresenter;
        private readonly IDictionary<IShapeFeature, IFeature> shapesToFeature = new Dictionary<IShapeFeature, IFeature>();

        public NetworkSideViewShapeHandler(IChart chart)
        {
            this.chart = chart;
            AddToolsToChart();
        }

        public IFeature SelectedFeature
        {
            get
            {
                return null != ModifyTool.SelectedShape ? shapesToFeature[ModifyTool.SelectedShape] : null;
            }
            set
            {
                UpdateShapeSelection(value);
            }
        }

        public Action OnSelectedFeatureChanged { get; set; }

        public ShapeModifyTool ModifyTool
        {
            get { return shapeModifyTool; }
        }

        public object StructurePresenter
        {
            get { return structurePresenter; }
        }

        public Route NetworkRoute { get; set; }

        internal IDictionary<IFeature, IShapeFeature> FeatureToShape { get; } = new Dictionary<IFeature, IShapeFeature>();

        public NetworkSideViewDataController NetworkSideViewDataController { get; set; }
        public bool AllowFeatureVisibilityChanges { get; set; }

        public bool ShowCrossSections { get; set; } = false;

        public bool ShowStructures { get; set; } = true;

        public void UpdateStyles(IBranchFeature branchFeature, VectorStyle normalStyle, VectorStyle selectedStyle)
        {
            if (FeatureToShape.TryGetValue(branchFeature, out IShapeFeature shapeFeature))
            {
                shapeFeature.NormalStyle = normalStyle;
                shapeFeature.SelectedStyle = selectedStyle;
            }
        }

        private void AddToolsToChart()
        {
            shapeModifyTool = new ShapeModifyTool(chart)
            {
                ShapeEditMode = (ShapeEditMode.ShapeSelect)
            };

            ModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;

            ModifyTool.SelectStyle = NetworkSideViewStyles.SelectStyle;
            ModifyTool.DefaultStyle = NetworkSideViewStyles.DefaultStyle;

            structurePresenter = new StructurePresenter(ModifyTool);
        }

        private void UpdateShapeSelection(IFeature feature)
        {
            //TODO: clean up refactor etc..this code is repeated all over.
            if (feature == null)
            {
                shapeModifyTool.SelectionChanged -= ShapeModifyToolSelectionChanged;
                shapeModifyTool.SelectedShape = null;
                shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
            }
            else
            {
                if (FeatureToShape.ContainsKey(feature))
                {
                    shapeModifyTool.SelectionChanged -= ShapeModifyToolSelectionChanged;
                    shapeModifyTool.SelectedShape = FeatureToShape[feature];
                    shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
                }
            }
        }

        private void ShapeModifyToolSelectionChanged(object sender, ShapeEventArgs e)
        {
            if ((null != e.ShapeFeature) && (shapesToFeature.ContainsKey(e.ShapeFeature)))
            {
                SelectedFeature = shapesToFeature[e.ShapeFeature];
                OnSelectedFeatureChanged?.Invoke();
            }
            else
            {
                SelectedFeature = null;
                OnSelectedFeatureChanged?.Invoke();
            }
        }

        private void AddStructuresToChart(ICompositeBranchStructure compositeStructure, bool active)
        {
            IShapeFeature shape = null;
            foreach (var structure in compositeStructure.Structures)
            {
                if (!FeatureToShape.TryGetValue(structure, out shape))
                {
                    if (structure is IWeir)
                    {
                        shape = GetWeirShape((IWeir)structure);
                    }
                    else if (structure is IPump)
                    {
                        shape = GetPumpShape((IPump)structure);
                    }
                    else if (structure is IBridge)
                    {
                        shape = GetBrigdeShape((IBridge)structure);
                    }
                    else if (structure is ICulvert)
                    {
                        shape = GetCulvertShape((ICulvert)structure);
                    }
                    else if (structure is IExtraResistance)
                    {
                        shape = GetExtraResistanceShape((IExtraResistance)structure);
                    }
                }

                shape.Active = active;
                AddStructureAndShape(structure, shape);
            }
        }

        private CulvertInSideViewShape GetCulvertShape(ICulvert culvert)
        {
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, culvert);

            return new CulvertInSideViewShape(chart, offset, culvert, NetworkSideViewHelper.GetReversed(NetworkRoute, culvert));
        }

        private BridgeInSideViewShape GetBrigdeShape(IBridge bridge)
        {
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, bridge);

            return new BridgeInSideViewShape(chart, offset, bridge);
        }

        private WeirInSideViewShape GetWeirShape(IWeir weir)
        {
            double offsetInSideView = RouteHelper.GetRouteChainage(NetworkRoute, weir);
            //pump levels etc.
            var weirInSideViewShape = new WeirInSideViewShape(chart, offsetInSideView, weir);
            return weirInSideViewShape;
        }

        private PumpInSideViewShape GetPumpShape(IPump pump)
        {
            Route route = NetworkRoute;
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, pump);
            // pump is reversed in route if segment of pump has reversed start and endofset in underlying 
            // branch.
            bool reversed = NetworkSideViewHelper.GetReversed(route, pump);
            //pump levels etc.
            return new PumpInSideViewShape(chart, offset, pump, reversed);
        }

        private ExtraResistanceInSideViewShape GetExtraResistanceShape(IExtraResistance extraResistance)
        {
            double offset = RouteHelper.GetRouteChainage(NetworkRoute, extraResistance);
            return new ExtraResistanceInSideViewShape(chart, offset, extraResistance);
        }

        private void AddStructureAndShape(IStructure1D structure, IShapeFeature symbolShapeFeature)
        {
            var hoverText = new HoverText(structure.Name, null, symbolShapeFeature, Color.Black, HoverPosition.Top, ArrowHeadPosition.None);
            if (symbolShapeFeature is IHover hoverShape)
            {
                hoverShape.ClearHovers();
                hoverShape.AddHover(new HoverRectangle(symbolShapeFeature, Color.FromArgb(50, Color.DarkTurquoise)));
                hoverShape.AddHover(hoverText);
            }

            if (structure is IWeir)
            {
                hoverText.BackColor = Color.LightPink;
            }
            else if (structure is IPump)
            {
                hoverText.BackColor = Color.LightGoldenrodYellow;
            }
            else if (structure is IBridge)
            {
                hoverText.BackColor = Color.LightGray;
            }
            else if (structure is ICulvert)
            {
                hoverText.BackColor = Color.Thistle;
            }
            if (!shapeModifyTool.ShapeFeatures.Contains(symbolShapeFeature))
                shapeModifyTool.AddShape(symbolShapeFeature);
            FeatureToShape[structure] = symbolShapeFeature;
            shapesToFeature[symbolShapeFeature] = structure;
        }

        private void AddImageShape(Image image, IBranchFeature structure, double minY)
        {
            if (!FeatureToShape.TryGetValue(structure, out var symbolShapeFeature))
            {
                double offset = RouteHelper.GetRouteChainage(NetworkRoute, structure);
                symbolShapeFeature = new SymbolShapeFeature(chart, offset, minY,
                                                            SymbolShapeFeatureHorizontalAlignment.Center,
                                                            SymbolShapeFeatureVerticalAlignment.Center)
                    { Image = image };
                shapeModifyTool.AddShape(symbolShapeFeature);
                if (symbolShapeFeature is IHover hoverShape)
                {
                    hoverShape.AddHover(new HoverRectangle(symbolShapeFeature, Color.FromArgb(100, Color.Cyan)));
                    var hoverText = new HoverText(structure.Name, null, symbolShapeFeature, Color.Black, HoverPosition.Bottom, ArrowHeadPosition.None) { BackColor = Color.WhiteSmoke };
                    hoverShape.AddHover(hoverText);
                }
            }
            if (!shapeModifyTool.ShapeFeatures.Contains(symbolShapeFeature))
                shapeModifyTool.AddShape(symbolShapeFeature);
            FeatureToShape[structure] = symbolShapeFeature;
            shapesToFeature[symbolShapeFeature] = structure;
        }

        private void AddDiffuseLateralSourceShape(IBranchFeature structure, double minY)
        {
            if (!FeatureToShape.TryGetValue(structure, out var symbolShapeFeature))
            {
                double offset = RouteHelper.GetRouteChainage(NetworkRoute, structure);
                double length = structure.Length;

                var renderStyle = new VectorStyle
                {
                    Line = new Pen(Color.MediumVioletRed, 2),
                    Outline = new Pen(Color.MediumVioletRed, 2)
                };

                renderStyle.Outline.DashStyle = DashStyle.Dash;
                renderStyle.Line.DashStyle = DashStyle.Dash;

                var hoverStyle = renderStyle;
                hoverStyle.Outline.Color = Color.LightBlue;

                symbolShapeFeature = new FixedRectangleShapeFeature(chart, offset, minY, length, 6, true, false)
                {
                    NormalStyle = renderStyle,
                };
                if (symbolShapeFeature is IHover hoverShape)
                {
                    hoverShape.AddHover(new HoverRectangle(symbolShapeFeature, hoverStyle));
                    var hoverText = new HoverText(structure.Name, null, symbolShapeFeature, Color.Black,
                                                  HoverPosition.Bottom, ArrowHeadPosition.None)
                        { BackColor = Color.WhiteSmoke };
                    hoverShape.AddHover(hoverText);
                }
            }
            if (!shapeModifyTool.ShapeFeatures.Contains(symbolShapeFeature))
                shapeModifyTool.AddShape(symbolShapeFeature);

            FeatureToShape[structure] = symbolShapeFeature;
            shapesToFeature[symbolShapeFeature] = structure;
        }

        private void AddCrossSectionShape(Route route, ICrossSection crossSection)
        {
            double offset = RouteHelper.GetRouteChainage(route, crossSection);

            var crossSectionShape = new CrossSectionInSideViewShape(chart, offset, 6, crossSection.Definition)
            {
                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                VerticalShapeAlignment = VerticalShapeAlignment.Top,
                NormalStyle = NetworkSideViewStyles.NormalCrossSectionStyle,
                SelectedStyle = NetworkSideViewStyles.SelectedCrossSectionStyle
            };
            crossSectionShape.AddHover(new HoverRectangle(crossSectionShape, Color.FromArgb(50, Color.Blue)));
            crossSectionShape.AddHover(new HoverText(crossSection.Name, null, crossSectionShape, Color.Black,
                                                     HoverPosition.Top, ArrowHeadPosition.LeftRight)
                                           { BackColor = Color.LightCyan });
            shapeModifyTool.AddShape(crossSectionShape);
            FeatureToShape[crossSection] = crossSectionShape;
            shapesToFeature[crossSectionShape] = crossSection;
        }

        private void AddManholeShape(IManhole manhole, double offset)
        {
            var compartmentShape = new ManHoleSideViewShape(chart, offset, manhole)
            {
                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                VerticalShapeAlignment = VerticalShapeAlignment.Top,
                SelectedStyle = NetworkSideViewStyles.SelectedCrossSectionStyle
            };

            compartmentShape.AddHover(new HoverRectangle(compartmentShape, Color.LightGray));
            compartmentShape.AddHover(new HoverText("ManHole:" + manhole.Name, "",
                                                    compartmentShape, Color.Black, HoverPosition.Top, ArrowHeadPosition.None));

            shapeModifyTool.AddShape(compartmentShape);
            FeatureToShape[manhole] = compartmentShape;
            shapesToFeature[compartmentShape] = manhole;
        }

        internal void UpdateAllShapes()
        {
            if (NetworkSideViewDataController == null)
            {
                // do not remove; see comment in Data set
                return;
            }

            UpdateCompartmentShapes();
            UpdateCrossSectionShapes();
            UpdateStructureShapes();
        }

        private void UpdateCompartmentShapes()
        {
            var manHolesInRoute = NetworkSideViewDataController.ActiveManholes;

            foreach (var tuple in manHolesInRoute)
            {
                var manhole = tuple.Item1;
                var offset = tuple.Item2;

                AddManholeShape(manhole, offset);
            }
        }

        private void UpdateCrossSectionShapes()
        {
            if (AllowFeatureVisibilityChanges && !ShowCrossSections)
            {
                return;
            }

            foreach (var crossSection in NetworkSideViewDataController.ActiveBranchFeatures.OfType<ICrossSection>())
            {
                AddCrossSectionShape(NetworkRoute, crossSection);
            }
        }

        private void UpdateStructureShapes()
        {
            if (AllowFeatureVisibilityChanges && !ShowStructures)
            {
                return;
            }

            double minY = chart.LeftAxis.Minimum;

            foreach (var branchFeature in NetworkSideViewDataController.ActiveBranchFeatures)
            {
                switch (branchFeature)
                {
                    case ICompositeBranchStructure branchStructure:
                        AddStructuresToChart(branchStructure, true);
                        break;
                    case LateralSource source when source.IsDiffuse:
                        AddDiffuseLateralSourceShape(source, minY);
                        break;
                    case LateralSource source:
                        AddImageShape(NetworkSideViewStyles.LateralSourceSmallIcon, source, minY);
                        break;
                    case Retention _:
                        AddImageShape(NetworkSideViewStyles.RetentionIcon, branchFeature, minY);
                        break;
                    case ObservationPoint _:
                        AddImageShape(NetworkSideViewStyles.ObservationIcon, branchFeature, minY);
                        break;
                }
            }

            foreach (var branchFeature in NetworkSideViewDataController.InactiveBranchFeatures.OfType<ICompositeBranchStructure>())
            {
                AddStructuresToChart(branchFeature, false);
            }
        }
    }
}