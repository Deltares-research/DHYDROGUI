using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Converters.Geometries;
using SharpMap.Styles;
using ValidationAspects;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public partial class StructureView : UserControl, IStructureView
    {
        private const string XValuesDataMember = "X";
        private const string YValuesDataMember = "Y";

        public event EventHandler<SelectedItemChangedEventArgs> SelectionChanged;

        private enum StructureType
        {
            Weir,
            Pump,
            Bridge,
            Culvert,
            ExtraResistance,
            Unknown
        }

        private ICrossSectionDefinition predecessorCrossSectionDefinition;
        private ICrossSectionDefinition successorCrossSectionDefinition;
        private Envelope structuresBoundingRect;

        private readonly StructurePresenter structurePresenter;

        private static readonly ILog log = LogManager.GetLogger(typeof(StructureView));

        private ILineChartSeries crossSectionDefinitionSeries;

        private readonly IChart chart;

        private ShapeModifyTool shapeModifyTool;

        private double minZValue = double.MaxValue;
        private double leftView = 0;
        private double rightView = 0;

        private readonly IDictionary<IShapeFeature, IStructure1D> Shapes2Structures = new Dictionary<IShapeFeature, IStructure1D>();
        private readonly IDictionary<IStructure1D, IShapeFeature> Structures2Shape = new Dictionary<IStructure1D, IShapeFeature>();

        private readonly VectorStyle selectStyle = new VectorStyle
                                                       {
                                                           Fill = new SolidBrush(Color.FromArgb(150, Color.Magenta)), 
                                                           Line = new Pen(Color.Black)
                                                       };

        private readonly VectorStyle defaultStyle = new VectorStyle
                                                        {
                                                            Fill = new SolidBrush(Color.FromArgb(100, Color.Gold)), 
                                                            Line = new Pen(Color.Black)
                                                        };

        private readonly VectorStyle errorStyle = new VectorStyle
                                                      {
                                                          Fill = new SolidBrush(Color.FromArgb(150, Color.DarkRed)), 
                                                          Line = new Pen(Color.Black)
                                                      };

        public StructureView()
        {
            InitializeComponent();

            chart = chartView.Chart;
            chart.LeftAxis.MinimumOffset = 10;
            chart.LeftAxis.MaximumOffset = 10;
            chart.BottomAxis.MaximumOffset = 10;
            chart.BottomAxis.MinimumOffset = 10;
            chart.BottomAxis.Title = "Offset in the cross section [m]";
            chart.LeftAxis.Title = "Level [m AD]";
            chart.BackGroundColor = Color.LightGray;
            chart.SurroundingBackGroundColor = SystemColors.ButtonFace;
            chartView.ViewPortChanged += ChartViewViewPortChanged;
            AddToolsToChart();
            structurePresenter = new StructurePresenter(shapeModifyTool);
        }

        private static StructureType StructureToStructureType(IStructure1D structure)
        {
            if (structure is IWeir)
            {
                return StructureType.Weir;
            }
            if (structure is IPump)
            {
                return StructureType.Pump;
            }
            if (structure is IBridge)
            {
                return StructureType.Bridge;
            }
            if (structure is ICulvert)
            {
                return StructureType.Culvert;
            }
            if (structure is IExtraResistance)
            {
                return StructureType.ExtraResistance;
            }
            return StructureType.Unknown;
        }

        #region ChartTools

        private void AddToolsToChart()
        {
            shapeModifyTool = CreateShapeModifyTool();
            if (shapeModifyTool != null)
            {
                chartView.Tools.Add(shapeModifyTool);
                shapeModifyTool.BeforeDraw += ShapeModifyToolBeforeDraw;
            }
        }

        private ShapeModifyTool CreateShapeModifyTool()
        {
            var modifyTool = new ShapeModifyTool(chart)
                                 {
                                     // to enable graphical editing use next lines.
                                     ShapeEditMode = (ShapeEditMode.ShapeSelect)
                                 };

            modifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
            modifyTool.GetCustomStyle += ShapeModifyToolGetCustomStyle;

            modifyTool.SelectStyle = selectStyle;
            modifyTool.DefaultStyle = defaultStyle;

            return modifyTool;
        }

        private void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool updateAxes = false;
            var crossSection = e.GetRemovedOrAddedItem() as ICrossSection;
            if (crossSection != null)
            {
                var removedDefinition = crossSection.Definition;
                if (CompareCrossSectionDefinitions(removedDefinition, predecessorCrossSectionDefinition) || 
                    CompareCrossSectionDefinitions(removedDefinition, successorCrossSectionDefinition))
                {
                    crossSectionDefinitionSeries.Clear();
                    Refresh();
                    updateAxes = true;
                }
            }
            if (updateAxes)
            {
                UpdateLeftAxis();
                UpdateBottomAxis();
            }
        }

        private void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool updateAxes = false;
            IStructure1D selectedStructure = null;
            switch (sender)
            {
                case IStructure1D structure when e.PropertyName == "OffsetY":
                    return;
                // Try to maintain the selected structure in the shapeModifyTool
                // for example when a weir is selected with Trackers and the user modifies the crest width in
                // the propertygrid the selection is maintained and the Trackers are updated.
                case IStructure1D structure when shapeModifyTool.SelectedShape == null:
                    return;
                case IStructure1D structure when !Shapes2Structures.ContainsKey(shapeModifyTool.SelectedShape):
                    // this is occasionally detected after changing the weir formula
                    return;
                case IStructure1D structure:
                {
                    selectedStructure = Shapes2Structures[shapeModifyTool.SelectedShape];

                    if (Structures2Shape.ContainsKey(structure))
                    {
                        switch (structure)
                        {
                            case IWeir weir:
                                UpdateWeirShapeWithWeir(weir);
                                break;
                            case IPump pump:
                                UpdatePumpShapeWithPump(pump);
                                break;
                            case IBridge bridge:
                                UpdateBridgeShapeWithBridge(bridge);
                                break;
                        }

                        updateAxes = true;
                        shapeModifyTool.ClearChache();
                        chartView.Invalidate();
                    }

                    break;
                }
                case IWeirFormula formula:
                    UpdateWeirShapeWithWeirFormula(formula);
                    updateAxes = true;
                    break;
                case ICrossSectionDefinition definition:
                {
                    if(CompareCrossSectionDefinitions(definition, predecessorCrossSectionDefinition) || 
                       CompareCrossSectionDefinitions(definition, successorCrossSectionDefinition))
                    {
                        crossSectionDefinitionSeries.Clear();
                        Refresh();
                        updateAxes = true;
                    }

                    break;
                }
                case ICrossSectionStandardShape shape when (CompareCrossSectionShapes(shape, predecessorCrossSectionDefinition) || 
                                                            CompareCrossSectionShapes(shape, successorCrossSectionDefinition)):
                    crossSectionDefinitionSeries.Clear();
                    Refresh();
                    updateAxes = true;
                    break;
            }

            if (null != selectedStructure && Structures2Shape.ContainsKey(selectedStructure))
            {
                IShapeFeature shapeFeature = Structures2Shape[selectedStructure];
                shapeModifyTool.ShapeSelectTool.SelectShape(shapeFeature);
            }
            if (sender is IHydroNetwork && e.PropertyName == "IsEditing" && !HydroNetwork.IsEditing)
            {
                updateAxes = true;
            }

            if (!updateAxes) return;

            UpdateLeftAxis();
            UpdateBottomAxis();
        }

        private static bool CompareCrossSectionDefinitions(ICrossSectionDefinition sender, ICrossSectionDefinition neighbor)
        {
            if (Equals(sender, neighbor))
            {
                return true;
            }
            if (neighbor is CrossSectionDefinitionProxy)
            {
                return Equals(sender, ((CrossSectionDefinitionProxy) neighbor).InnerDefinition);
            }
            return false;
        }

        private static bool CompareCrossSectionShapes(ICrossSectionStandardShape sender, ICrossSectionDefinition neighbor)
        {
            if(neighbor is CrossSectionDefinitionStandard standard)
            {
                return Equals(sender, standard.Shape);
            }
            if (neighbor is CrossSectionDefinitionProxy proxy && 
                proxy.InnerDefinition is CrossSectionDefinitionStandard innerDefinition)
            {
                return Equals(sender, innerDefinition.Shape);
            }
            return false;
        }

        void ChartViewViewPortChanged(object sender, EventArgs e)
        {
            UpdateStructureOffsetsY();
            shapeModifyTool.ClearChache();
        }

        private void UpdateWeirShapeWithWeirFormula(IWeirFormula weirFormula)
        {
            var weir = structureViewData.CompositeBranchStructure.Structures.FirstOrDefault(s => s is IWeir && ((IWeir)s).WeirFormula == weirFormula) as IWeir;
            
            if (weir == null || (!(weir.WeirFormula is FreeFormWeirFormula)))
            {
                return;
            }

            var freeFormWeirFormula = (FreeFormWeirFormula) weirFormula;
            
            if (freeFormWeirFormula.Shape.Coordinates.Length < 2)
            {
                freeFormWeirFormula.SetDefaultShape();
            }
            UpdateWeirShapeWithWeir(weir);
        }

        private void UpdateBridgeShapeWithBridge(IBridge bridge)
        {
            RemoveShapeFeature(bridge);
            AddBridge(bridge);
            chartView.Invalidate();
        }


        private void ShapeModifyToolSelectionChanged(object sender, ShapeEventArgs e)
        {
            if (null == e.ShapeFeature)
            {
                return;
            }
            if (!Shapes2Structures.ContainsKey(e.ShapeFeature))
            {
                return;
            }
            IStructure1D structure = Shapes2Structures[e.ShapeFeature];
            Validate(this);
            if (null != SelectionChanged)
            {
                SelectionChanged(this, new SelectedItemChangedEventArgs(structure));
            }
        }
        
        #endregion

        private void UpdatePumpShapeWithPump(IPump pump)
        {
            RemoveShapeFeature(pump);
            AddPumpShape(pump);
            chartView.Invalidate();
        }

        private void UpdateWeirShapeWithWeir(IWeir weir)
        {
            RemoveShapeFeature(weir);
            AddWeirShape(weir);
            Validate(this);
            chartView.Invalidate();
        }

        #region Chart drawing / chart series

        private ILineChartSeries MakeCrossSectionDefinitionSeries(ICrossSectionDefinition crossSectionDefinition)
        {
            crossSectionDefinitionSeries = ChartSeriesFactory.CreateLineSeries();

            if (crossSectionDefinitionSeries != null)
            {
                // Add crossSection definition series to chartView
                crossSectionDefinitionSeries.XValuesDataMember = XValuesDataMember;
                crossSectionDefinitionSeries.YValuesDataMember = YValuesDataMember;
                crossSectionDefinitionSeries.DataSource = crossSectionDefinition.Profile.OfType<ICoordinate>().ToList();
            }

            return crossSectionDefinitionSeries;
        }

        /// <summary>
        /// Creates a series that contains all structure. This is used when there are no crossections available.
        /// </summary>
        /// <returns></returns>
        private ILineChartSeries GetSeriesThatContainsAllStuctures()
        {
            structuresBoundingRect = GetStructuresBoundingRect();
            ExpandToIncludeCrossSection(structuresBoundingRect, predecessorCrossSectionDefinition);
            ExpandToIncludeCrossSection(structuresBoundingRect, successorCrossSectionDefinition);
            ILineChartSeries seriesToAdd = GetSeriesToAdd(structuresBoundingRect);
            return seriesToAdd;
        }

        private void ExpandToIncludeCrossSection(Envelope envelope, ICrossSectionDefinition sectionDefinition)
        {
            if (sectionDefinition != null)
            {
                envelope.ExpandToInclude(GetEnvelope(sectionDefinition.Profile, 0));
            }
        }

        /// <summary>
        /// Converts a bounding rect to series
        /// </summary>
        /// <param name="boundingRect"></param>
        /// <returns></returns>
        private static ILineChartSeries GetSeriesToAdd(Envelope boundingRect)
        {
            ILineChartSeries seriesToAdd = ChartSeriesFactory.CreateLineSeries();

            // Add ten percent for padding
            var tenPercLeft = Math.Abs(boundingRect.MinX/10);
            var tenPercRight = Math.Abs(boundingRect.MaxX/10);
            var tenPercTop = Math.Abs(boundingRect.MaxY/10);
            var tenPercBottom = Math.Abs(boundingRect.MinY/10);

            seriesToAdd.Add(boundingRect.MinX - tenPercLeft, boundingRect.MaxY + tenPercTop);
            seriesToAdd.Add(boundingRect.MinX - tenPercLeft, boundingRect.MinY - tenPercBottom);
            seriesToAdd.Add(boundingRect.MaxX + tenPercRight, boundingRect.MinY - tenPercBottom);
            seriesToAdd.Add(boundingRect.MaxX + tenPercRight, boundingRect.MaxY + tenPercTop);
            return seriesToAdd;
        }


        void ShapeModifyToolBeforeDraw(object sender, EventArgs e)
        {
            UpdateStructureOffsetsY();
        }

        private void UpdateStructureOffsetsY()
        {
            if (null == CompositeStructure)
            {
                return;
            }
            IList<double> widths = new List<double>();
            double totalWidth = 0.0;

            double halluf = leftView + (rightView - leftView)/2;
            halluf = double.IsInfinity(halluf) || double.IsNaN(halluf) ? 0.0 : halluf;
            
            foreach (var structure in CompositeStructure.Structures)
            {
                var structureBoundingRect = GetBoundingRectStructure(structure);
                totalWidth += structureBoundingRect.Width;
                widths.Add(structureBoundingRect.Width); 
            }
            double offsetY = halluf - totalWidth / 2;
            int i = 0;
            foreach (var structure in CompositeStructure.Structures)
            {
                structure.OffsetY = offsetY;
                offsetY += widths[i++];
            }
        }

        private Envelope GetStructuresBoundingRect()
        {
            Envelope boundingRect = null;

            UpdateStructureOffsetsY();

            foreach (var structure in CompositeStructure.Structures)
            {
                var structureBoundingRect = GetBoundingRectStructure(structure);

                if (boundingRect == null)
                {
                    boundingRect = structureBoundingRect;
                }
                else
                {
                    boundingRect.ExpandToInclude(structureBoundingRect);
                }
            }
            return boundingRect;
        }

        private void AddStructuresToChart(ICompositeBranchStructure compositeStructure)
        {
            foreach (var structure in compositeStructure.Structures)
            {
                switch (StructureToStructureType(structure))
                {
                    case StructureType.Weir:
                        AddWeirShape((IWeir)structure);
                        break;
                    case StructureType.Pump:
                        AddPumpShape((IPump)structure);
                        break;
                    case StructureType.Bridge:
                        AddBridge((IBridge)structure);
                        break;
                    case StructureType.Culvert:
                        AddCulvert((ICulvert) structure);
                        break;
                    case StructureType.ExtraResistance:
                        AddExtraResistanceShape((IExtraResistance)structure);
                        break;
                }
            }
        }

        private void AddThalWayToChart()
        {
            var line = new ThalWegInStructureView(chart);
            shapeModifyTool.AddShape(line);
        }


        private void AddCulvert(ICulvert culvert)
        {
            var culvertShape = new CulvertInStructureViewShape(chart, culvert);
            AddStructureAndShape(culvert, culvertShape);
        }

        private void AddBridge(IBridge bridge)
        {
            var bridgeShape = new BridgeInStructureViewShape(chart, bridge);
            AddStructureAndShape(bridge,bridgeShape);
        }

        private void AddPumpShape(IPump pump)
        {
            var pumpShape = new PumpInStructureViewShape(chart, pump, false);
            AddStructureAndShape(pump, pumpShape);
        }

        private void AddExtraResistanceShape(IExtraResistance extraResistance)
        {
            var extraResistanceShape = new ExtraResistanceInStructureViewShape(chart, extraResistance);
            AddStructureAndShape(extraResistance, extraResistanceShape);
            
        }

        private void AddStructureAndShape(IStructure1D structure, IShapeFeature shape)
        {
            shapeModifyTool.AddShape(shape);
            Structures2Shape[structure] = shape;
            Shapes2Structures[shape] = structure;
        }

        private void AddWeirShape(IWeir weir)
        {
            IShapeFeature weirShape = new WeirInStructureViewShape(chart, weir);
            shapeModifyTool.AddShape(weirShape);
            Structures2Shape[weir] = weirShape;
            Shapes2Structures[weirShape] = weir;
        }

        private void RemoveShapeFeature(IStructure1D structure)
        {
            if (!Structures2Shape.ContainsKey(structure)) 
                return;
            IShapeFeature shapeFeature = Structures2Shape[structure];
            Structures2Shape.Remove(structure);
            Shapes2Structures.Remove(shapeFeature);
            shapeModifyTool.RemoveShape(shapeFeature);
        }

        private Envelope GetBoundingRectStructure(IStructure1D structure)
        {
            
            switch (StructureToStructureType(structure))
            {
                case StructureType.Weir: return GetBoundingRectWeir((IWeir)structure);
                case StructureType.Pump: return GetBoundingRectPump((IPump) structure);
                    
                case StructureType.Bridge: return GetBoundingRectBridge((IBridge)structure);
                case StructureType.Culvert: return GetBoundingRectCulver((ICulvert)structure);
                case StructureType.ExtraResistance: return GetBoundingRectExtraResistance((IExtraResistance)structure);
                default:
                    throw new NotImplementedException("Unsupported type");
            }
        }

        private Envelope GetBoundingRectWeir(IWeir weir)
        {
            double left, top, right, bottom;
            if (weir.WeirFormula is FreeFormWeirFormula)
            {
                FreeFormWeirFormula freeFormWeirFormula = (FreeFormWeirFormula) weir.WeirFormula;
                var coordinates = freeFormWeirFormula.Shape.Coordinates.ToArray();

                left = coordinates[0].X;
                top = coordinates.Max(c => c.Y);
                right = coordinates[coordinates.Length - 1].X;
                bottom = coordinates.Min(c => c.Y);
            }
            else
            {
                left = weir.OffsetY;
                top = weir.CrestLevel;
                right = weir.OffsetY + weir.CrestWidth;
                bottom = minZValue;

                top += 10; // gate
                if (bottom > top)
                {
                    bottom = top - 10 - 10;
                    minZValue = bottom;
                }
            }
            return GeometryFactory.CreateEnvelope(left, right, bottom, top);
        }
        
        private static Envelope GetBoundingRectBridge(IBridge bridge)
        {
            IList<Coordinate> yzValues = bridge.BridgeType == BridgeType.YzProfile 
            ? bridge.YZCrossSectionDefinition.FlowProfile.ToList()
            : bridge.EffectiveCrossSectionDefinition.FlowProfile.ToList();
            return GetEnvelope(yzValues, bridge.Shift);
        }

        private static Envelope GetEnvelope(IEnumerable<Coordinate> yzValues, double offSetY)
        {
            if (yzValues.Count() <= 1)
            {
                return GeometryFactory.CreateEnvelope(0, 0, 0, 0);
            }
            var yValues = yzValues.Select(yz => yz.X + offSetY);
            var zValues = yzValues.Select(yz => yz.Y);
            return GeometryFactory.CreateEnvelope(yValues.Min(), yValues.Max(), zValues.Min(), zValues.Max());
        }

        private static Envelope GetBoundingRectCulver(ICulvert culvert)
        {
            IList<Coordinate> yzValues = culvert.CrossSectionDefinitionAtInletAbsolute.FlowProfile.ToList();
            return GetEnvelope(yzValues, culvert.BottomLevel);
        }

        private Envelope GetBoundingRectPump(IPump pump)
        {
            //used in min max for axis
            var zValues = new[]
                              {
                                  pump.OffsetZ,
                                  pump.StartDelivery,
                                  pump.StartSuction,
                                  pump.StopDelivery,
                                  pump.StopSuction
                              };
            double minY = zValues.Min();
            double maxY = zValues.Max();
            double width =
                chartView.ChartCoordinateService.ToWorldWidth(PumpSmallIcon.Width);

            return GeometryFactory.CreateEnvelope(pump.OffsetY, pump.OffsetY + width, minY, maxY);
        }

        private Envelope GetBoundingRectExtraResistance(IExtraResistance extraResistance)
        {
            //used in min max for axis
            double minY = 10;
            double maxY = 10;
            double width = chartView.ChartCoordinateService.ToWorldWidth(ExtraResistanceSmallIcon.Width);
            return GeometryFactory.CreateEnvelope(extraResistance.OffsetY, extraResistance.OffsetY + width, minY, maxY);
        }
        #endregion

        #region Public properties

        private IStructureViewData structureViewData;
        private static readonly Bitmap PumpSmallIcon = Properties.Resources.PumpSmall;
        private static readonly Bitmap ExtraResistanceSmallIcon = Properties.Resources.ExtraResistanceSmall;

        public object Data
        {
            get
            { 
                return structureViewData; 
            }
            set
            {
                UnSubscribeToData();
                structureViewData = (IStructureViewData) value;
                SubscribeToData();
                RefreshViewData();
            }
        }

        private void SubscribeToData()
        {
            if (null != HydroNetwork)
            {
                ((INotifyPropertyChanged)HydroNetwork).PropertyChanged += NetworkPropertyChanged;
                ((INotifyCollectionChange) HydroNetwork).CollectionChanged += NetworkCollectionChanged;
            }
            if (CompositeStructure == null)
            {
                return;
            }
            DataBindings.Add("Text", CompositeStructure, "Name", false, DataSourceUpdateMode.OnPropertyChanged);
            Text = CompositeStructure.Name;
        }

        private void UnSubscribeToData()
        {
            if (null != HydroNetwork)
            {
                DataBindings.Clear();
                ((INotifyPropertyChanged)HydroNetwork).PropertyChanged -= NetworkPropertyChanged;
                ((INotifyCollectionChange)HydroNetwork).CollectionChanged -= NetworkCollectionChanged;
            }
        }

        public Image Image
        {
            get { return Properties.Resources.StructureFeatureSmall; }
            set { }
        }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        public IStructure1D SelectedStructure
        {
            get
            {
                return null != shapeModifyTool.SelectedShape ? Shapes2Structures[shapeModifyTool.SelectedShape] : null;
            }
            set
            {
                if (value is ICompositeBranchStructure)
                    return;

                if (value == null)
                {
                    shapeModifyTool.SelectionChanged -= ShapeModifyToolSelectionChanged;
                    shapeModifyTool.SelectedShape = null;
                    shapeModifyTool.ShapeFeatureEditor = null;
                    shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
                }
                if (value != null && Structures2Shape.ContainsKey(value))
                {
                    shapeModifyTool.SelectionChanged -= ShapeModifyToolSelectionChanged;
                    shapeModifyTool.SelectedShape = Structures2Shape[value];
                    shapeModifyTool.SelectionChanged += ShapeModifyToolSelectionChanged;
                }
                chartView.Refresh();
            }
        }

        private HydroNetwork HydroNetwork
        {
            get { return structureViewData != null ? structureViewData.HydroNetwork : null; }
        }

        private ICompositeBranchStructure CompositeStructure
        {
            get
            {
                return structureViewData != null ? structureViewData.CompositeBranchStructure:null;
            }
        }
        
        #endregion

        public override void Refresh()
        {
            base.Refresh();
            if (structureViewData != null)
            {
                structureViewData.ResetMinMaxZ();
                RefreshViewData();
            }
        }

        private void RefreshViewData()
        {
            Structures2Shape.Clear();
            Shapes2Structures.Clear();
            shapeModifyTool.Clear();
            // Do not update when no necessary to avoid flashing, context loss (selection loss), etc
            if (null == CompositeStructure ||
                (shapeModifyTool.ShapeFeatures.Count == CompositeStructure.Structures.Count))
            {
                return;
            }

            Text = CompositeStructure.Name;

            if (null != HydroNetwork)
            {
                chartView.Chart.Series.Clear();
                AddStructuresToChart(CompositeStructure);
                AddCrossSectionsOrBoundingSeriesToChart();
                //AddThalWayToChart(); //thalweg does nothing here
                chartView.Invalidate();
            }
            UpdateLeftAxis();
            UpdateBottomAxis();
        }

        private void UpdateBottomAxis()
        {
            double min, max;
            //y is horizontal
            GetYMinMax(out min, out max);

            chartView.Chart.BottomAxis.MinimumOffset = 15;
            chartView.Chart.BottomAxis.MaximumOffset = 15;
            chartView.Chart.BottomAxis.Minimum = min;
            chartView.Chart.BottomAxis.Maximum = max;
            chartView.Chart.BottomAxis.Automatic = false;
        }

        private void GetYMinMax(out double min, out double max)
        {
            double profileMin = double.MaxValue;
            double profileMax = double.MinValue;

            if (predecessorCrossSectionDefinition != null)
            {
                var yz = predecessorCrossSectionDefinition.Profile;
                profileMin = Math.Min(profileMin, yz.First().X);
                profileMax = Math.Max(profileMax, yz.Last().X);
            }
            if (successorCrossSectionDefinition != null)
            {
                var yz = successorCrossSectionDefinition.Profile;
                profileMin = Math.Min(profileMin, yz.First().X);
                profileMax = Math.Max(profileMax, yz.Last().X);
            }

            Envelope rectangle = GetStructuresBoundingRect();

            if ((successorCrossSectionDefinition == null) && (predecessorCrossSectionDefinition == null))
            {
                min = rectangle.MinX - rectangle.Width / 20;
                max = rectangle.MaxX + rectangle.Width / 20;
            }
            else
            {
                min = Math.Min(profileMin, (profileMax - profileMin) / 2 - rectangle.Width / 2);
                max = Math.Max(profileMax, (profileMax - profileMin) / 2 + rectangle.Width / 2);
            }
        }

        private void UpdateLeftAxis()
        {
            if (structureViewData == null)
                return;
            
            chartView.Chart.LeftAxis.MinimumOffset = 15;
            chartView.Chart.LeftAxis.MaximumOffset = 15;
            chartView.Chart.LeftAxis.Minimum = structureViewData.ZMinValue;
            chartView.Chart.LeftAxis.Maximum = structureViewData.ZMaxValue;
            chartView.Chart.LeftAxis.Automatic = false;
        }

        private void AddCrossSectionsOrBoundingSeriesToChart()
        {
            ICrossSection predecessor;
            ICrossSection successor;

            NetworkHelper.GetNeighboursOnBranch(CompositeStructure.Branch, CompositeStructure.Chainage,
                                                out predecessor, out successor);

            predecessorCrossSectionDefinition = (predecessor != null && predecessor.Definition.Profile.Any())
                                                    ? predecessor.Definition
                                                    : null;
            successorCrossSectionDefinition = (successor != null && successor.Definition.Profile.Any())
                                                  ? successor.Definition
                                                  : null;
            
            ILineChartSeries seriesToAdd;

            leftView = double.MaxValue;
            rightView = double.MinValue;

            if (predecessorCrossSectionDefinition != null)
            {
                // Add crossSectionDefinition to chart as IChartSeries
                seriesToAdd = MakeCrossSectionDefinitionSeries(predecessorCrossSectionDefinition);
                seriesToAdd.PointerVisible = false;
                seriesToAdd.Color = Color.Black;
                seriesToAdd.DashStyle = DashStyle.Dot;
                chartView.Chart.Series.Add(seriesToAdd);
                leftView = Math.Min(predecessorCrossSectionDefinition.Left, leftView);
                rightView = Math.Max(predecessorCrossSectionDefinition.Left + predecessorCrossSectionDefinition.Width, rightView);
            }

            if (successorCrossSectionDefinition != null)
            {
                // Add crossSectionDefinition to chart as IChartSeries
                seriesToAdd = MakeCrossSectionDefinitionSeries(successorCrossSectionDefinition);
                seriesToAdd.PointerVisible = false;
                seriesToAdd.Color = Color.DarkGray;
                seriesToAdd.DashStyle = DashStyle.Dot;
                chartView.Chart.Series.Add(seriesToAdd);
                leftView = Math.Min(successorCrossSectionDefinition.Left, leftView);
                    rightView = Math.Max(successorCrossSectionDefinition.Width + successorCrossSectionDefinition.Left,
                                         rightView);
            }

            if ((predecessorCrossSectionDefinition == null) && (successorCrossSectionDefinition == null))
            {
                seriesToAdd = GetSeriesThatContainsAllStuctures();
                seriesToAdd.PointerVisible = false;
                seriesToAdd.Color = Color.LightGray;
                seriesToAdd.DashStyle = DashStyle.Solid;
                chartView.Chart.Series.Add(seriesToAdd);
                leftView = Math.Min(seriesToAdd.XValues[seriesToAdd.XValues.Count - 1], leftView);
                rightView = Math.Max(seriesToAdd.XValues[0], rightView);
            }
        }

        private VectorStyle ShapeModifyToolGetCustomStyle(object sender, IShapeFeature shapeFeature)
        {
            if (Shapes2Structures.ContainsKey(shapeFeature))
            {
                IStructure1D structure = Shapes2Structures[shapeFeature];
                var result = structure.Validate();
                return result.IsValid ? (shapeFeature.Selected ? selectStyle : defaultStyle) : errorStyle;
            }
            return defaultStyle;
        }

        [ValidationMethod]
        public static void Validate(StructureView structureView)
        {
            foreach (var structure in structureView.CompositeStructure.Structures)
            {
                var result = structure.Validate();

                structureView.shapeModifyTool.SelectStyle = structureView.selectStyle;
                structureView.shapeModifyTool.SelectStyle = structureView.errorStyle;
                if (!result.IsValid)
                {
                    log.Error(string.Join(", ", result.Messages.ToArray()));
                }
            }
            structureView.chartView.Refresh();
        }

        public object CommandReceiver
        {
            get { return structurePresenter; }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            if (HydroNetwork == null)
            {
                return;
            }
            ((INotifyPropertyChanged)HydroNetwork).PropertyChanged -= NetworkPropertyChanged;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //validate
            foreach (IStructure1D s in Shapes2Structures.Values)
            {
                if (double.IsInfinity(s.OffsetY))
                {
                    log.Error("OffsetY in structure " + s.Name + " cannot be Infinity (type: " + StructureToStructureType(s) + "; chan: " +
                              s.Branch.Name + ")");
                }
                if (double.IsNaN(s.OffsetY))
                {
                    log.Error("OffsetY in structure " + s.Name + " cannot be NaN of (type: " + StructureToStructureType(s) + "; chan: " +
                              s.Branch.Name + ")");
                }
            }
        }
    }
}