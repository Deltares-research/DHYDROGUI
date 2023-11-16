using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Drawing;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using SharpMap.UI.Forms;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class HydroRegionEditorMapTool : MapTool, IHydroNetworkEditorMapTool
    {
        public const string AddPointCrossSectionToolName = "add point cross-section";
        public const string AddLineCrossSectionToolName = "add line cross-section";
        public const string AddChannelScribleToolName = "add branch (scribble way)";
        public const string AddChannelToolName = "add channel";
        public const string AddPipeToolName = "add pipe";
        public const string AddSewerConnectionToolName = "add sewerConnection";
        public const string AddCatchmentToolName = "add catchment";
        public const string InsertNodeToolName = "insert new node";
        public const string InsertManholeToolName = "insert new manhole";
        public const string AddWasteWaterTreatmentPlantToolName = "add waste water treatment plant";
        public const string AddRunoffBoundaryToolName = "add runoff boundary";
        public const string AddCompositeStructureToolName = "add compound structure";
        public const string AddPumpToolName = "add pump";
        public const string AddLateralSourceToolName = "add lateral source";
        public const string AddDiffuseLateralSourceToolName = "add diffuse lateral source";
        public const string AddRetentionToolName = "add retention";
        public const string AddObservationPointToolName = "add observation point";
        public const string AddWeirToolName = "add weir";
        public const string AddOrificeToolName = "add orifice";
        public const string AddCulvertToolName = "add culvert";
        public const string AddBridgeToolName = "add bridge";
        public const string AddNetworkLocationToolName = "add new network location";
        public const string AddInterpolatedCrossSectionToolName = "add interpolated cross-section";

        public const string ThinDamToolName = "Thin dam tool (2D)";
        public const string FixedWeirToolName = "Fixed weir tool (2D)";
        public const string ObservationPointToolName = "Observation point tool (2D)";
        public const string ObservationCrossSectionToolName = "Observation cross section tool (2D)";
        public const string PumpToolName = "Pump tool (2D)";
        public const string WeirToolName = "Weir tool (2D)";
        public const string GateToolName = "Gate tool (2D)";
        public const string LandBoundaryToolName = "Land boundary tool";
        public const string DryPointToolName = "Dry point tool";
        public const string DryAreaToolName = "Dry area tool";
        public const string LeveeBreachToolName = "Levee breach tool";
        public const string BreachLocationToolName = "Breach location tool";
        public const string RoofAreaToolName = "Roof area tool";
        public const string GullyToolName = "Gully tool";
        public const string EmbankmentToolName = "Embankment tool";
        public const string EnclosureToolName = "Enclosure tool";
        public const string BridgePillarToolName = "Bridge pillar tool";


        private static readonly ILog log = LogManager.GetLogger(typeof (HydroRegionEditorMapTool));
        
        private readonly List<IMapTool> mapTools = new List<IMapTool>();

        private Coordinate contextMenuWorldPosition;

        private IMap map;

        private static bool TopologyRulesEnabledState;

        private INetworkCoverageGroupLayer activeNetworkCoverageGroupLayer;

        private static readonly Cursor PointCrossSectionCuror = MapCursors.CreateArrowOverlayCuror(Resources.CrossSectionSmall);
        private static readonly Cursor NewInsertNodeCursor = MapCursors.CreateArrowOverlayCuror(Resources.NodeOnMultipleBranches);
        private static readonly Cursor NewLateralSourceCursor = MapCursors.CreateArrowOverlayCuror(Resources.LateralSourceSmall);
        private static readonly Cursor NewPumpCursor = MapCursors.CreateArrowOverlayCuror(Resources.PumpSmall);
        private static readonly Cursor AddCompositeStructureCursor = MapCursors.CreateArrowOverlayCuror(Resources.StructureFeatureSmall);
        private static readonly Cursor NewLineCrossSectionCursor = MapCursors.CreateArrowOverlayCuror(Resources.CrossSectionSmallXYZ);
        private static readonly Cursor NewRetentionToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.Retention);
        private static readonly Cursor NewObservationPointToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.Observation);
        private static readonly Cursor AddNewWeirCursor = MapCursors.CreateArrowOverlayCuror(Resources.WeirSmall);
        private static readonly Cursor AddNewOrificeCursor = MapCursors.CreateArrowOverlayCuror(Resources.Gate);
        private static readonly Cursor NewCulvertToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.CulvertSmall);
        private static readonly Cursor NewBridgeToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.BridgeSmall);
        private static readonly Cursor NewWwtpToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.wwtp);
        private static readonly Cursor NewRunoffBoundaryToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.runoff);
        private static readonly Cursor AddInterpolatedCrossSectionToolCursor = MapCursors.CreateArrowOverlayCuror(Resources.AddInterpolatedCrossSection);
        private static readonly Cursor AddNewPipeCursor = MapCursors.CreateArrowOverlayCuror(Resources.Pipe_Small);
        private static readonly Cursor AddNewSewerConnectionCursor = MapCursors.CreateArrowOverlayCuror(Resources.Pipe_Small);

        private bool FeatureTypeLayerFilter<T>(ILayer layer)
        {
            if (layer.DataSource == null || layer is LabelLayer)
            {
                return false;
            }

            return typeof(T).IsAssignableFrom(layer.DataSource.FeatureType);
        }
        /// <summary>
        /// T1 datatype should not be found
        /// </summary>
        /// <typeparam name="T">layer with this type of data source</typeparam>
        /// <typeparam name="T1">layer but not of this type of data source</typeparam>
        /// <param name="layer">generated maplayer(s)</param>
        /// <returns></returns>
        private bool FeatureTypeLayerFilter<T, T1>(ILayer layer)
        {
            // T1 datatype should not be found
            if (layer.DataSource == null || layer is LabelLayer || typeof(T1).IsAssignableFrom(layer.DataSource.FeatureType))
            {
                return false;
            }

            return typeof(T).IsAssignableFrom(layer.DataSource.FeatureType);
        }
        
        private void AddNetworkEditorTools()
        {
            // HydroNetwork
            var newLineTool = new NewLineTool(FeatureTypeLayerFilter<Channel>, AddChannelToolName)
                                  {
                                      AutoCurve = false,
                                      MinDistance = 0,
                                      IsActive = false,
                                      Cursor = MapCursors.CreateArrowOverlayCuror(Resources.new_branch_small)
                                  };
            AddMapTool(newLineTool);

            var newLineTool2 = new NewLineTool(FeatureTypeLayerFilter<Channel>, AddChannelScribleToolName)
                                   {
                                       AutoCurve = true,
                                       MinDistance = 15,
                                       IsActive = false,
                                       Cursor = MapCursors.CreateArrowOverlayCuror(Resources.new_autobranch_small)
                                   };
            AddMapTool(newLineTool2);

            var newInsertNodeTool = new NewPointFeatureTool<HydroNode>(InsertNodeToolName) { Cursor = NewInsertNodeCursor };
            AddMapTool(newInsertNodeTool);
            
            var newPipeTool = new NewLineTool(FeatureTypeLayerFilter<Pipe>, AddPipeToolName)
            {
                AutoCurve = false,
                MinDistance = 0,
                IsActive = false,
                Cursor = AddNewPipeCursor,
                MaxPoints = 2
            };
            AddMapTool(newPipeTool);

            var newSewerConnectionTool = new NewLineTool(FeatureTypeLayerFilter<SewerConnection, IPipe>, AddSewerConnectionToolName)
            {
                AutoCurve = false,
                MinDistance = 0,
                IsActive = false,
                Cursor = AddNewSewerConnectionCursor,
                MaxPoints = 2
            };
            AddMapTool(newSewerConnectionTool);

            var newInsertManholeTool = new NewPointFeatureTool<Manhole>(InsertManholeToolName) { Cursor = AddNewPipeCursor };
            AddMapTool(newInsertManholeTool);

            var newPointCrossSectionTool = new NewPointFeatureTool<CrossSection>(AddPointCrossSectionToolName) { Cursor = PointCrossSectionCuror };
            AddMapTool(newPointCrossSectionTool);

            var newLineCrossSectionTool = new NewLineTool(FeatureTypeLayerFilter<CrossSection>, AddLineCrossSectionToolName)
            {
                AutoCurve = true,
                Cursor = NewLineCrossSectionCursor
            };
            AddMapTool(newLineCrossSectionTool);

            var newStructureFeatureTool = new NewPointFeatureTool<CompositeBranchStructure>(AddCompositeStructureToolName) { Cursor = AddCompositeStructureCursor };
            AddMapTool(newStructureFeatureTool);

            var newPumpTool = new NewPointFeatureTool(layer => layer.DataSource != null && !(layer is LabelLayer)
                  && layer.DataSource.FeatureType == typeof(Pump) && (layer.DataSource is HydroNetworkFeatureCollection), AddPumpToolName) { Cursor = NewPumpCursor };

            AddMapTool(newPumpTool);

            var newLateralSourceTool = new NewPointFeatureTool<LateralSource>(AddLateralSourceToolName) { Cursor = NewLateralSourceCursor };
            AddMapTool(newLateralSourceTool);

            var newDiffuseLateralSourceTool = new NewLineTool(FeatureTypeLayerFilter<LateralSource>, AddDiffuseLateralSourceToolName)
                                                  {
                                                      AutoCurve = true,
                                                      Cursor = NewLateralSourceCursor
                                                  };
            AddMapTool(newDiffuseLateralSourceTool);

            var newRetentionTool = new NewPointFeatureTool<Retention>(AddRetentionToolName) { Cursor = NewRetentionToolCursor };
            AddMapTool(newRetentionTool);

            var newObservationPointTool = new NewPointFeatureTool<ObservationPoint>(AddObservationPointToolName) { Cursor = NewObservationPointToolCursor };
            AddMapTool(newObservationPointTool);

            var newWeirTool = new NewPointFeatureTool(layer => layer.DataSource != null && !(layer is LabelLayer)
                  && layer.DataSource.FeatureType == typeof(Weir) && (layer.DataSource is HydroNetworkFeatureCollection), AddWeirToolName) { Cursor = AddNewWeirCursor };
            AddMapTool(newWeirTool);

            var newOrificeTool = new NewPointFeatureTool(layer => layer.DataSource != null && !(layer is LabelLayer)
                  && layer.DataSource.FeatureType == typeof(IOrifice) && (layer.DataSource is HydroNetworkFeatureCollection), AddOrificeToolName) { Cursor = AddNewOrificeCursor };
            AddMapTool(newOrificeTool);

            var newCulvertTool = new NewPointFeatureTool<Culvert>(AddCulvertToolName) { Cursor = NewCulvertToolCursor };
            AddMapTool(newCulvertTool);

            var newBridgeTool = new NewPointFeatureTool<Bridge>(AddBridgeToolName) { Cursor = NewBridgeToolCursor };
            AddMapTool(newBridgeTool);

            var outletCompartmentContextMenuMapTool = new OutletCompartmentContextMenuMapTool();
            AddMapTool(outletCompartmentContextMenuMapTool);

            var addNwrwCatchmentContextMenuMapTool = new AddNWRWCatchmentContextMenuMapTool();
            AddMapTool(addNwrwCatchmentContextMenuMapTool);

            // DrainageBasin
            Func<ILayer, bool> isCatchmentLayer = FeatureTypeLayerFilter<Catchment>;
            var newLineToolCatchment = new NewLineTool(isCatchmentLayer, AddCatchmentToolName)
            {
                CloseLine = true,
                KeepDuplicates = true,
                AutoCurve = true,
                MinDistance = 10,
                IsActive = false,
                ActualSnapping = false
            };
            AddMapTool(newLineToolCatchment);

            var newWwtpTool = new NewPointFeatureTool<WasteWaterTreatmentPlant>(AddWasteWaterTreatmentPlantToolName) { Cursor = NewWwtpToolCursor };
            AddMapTool(newWwtpTool);

            var newRunoffBoundaryTool = new NewPointFeatureTool<RunoffBoundary>(AddRunoffBoundaryToolName) { Cursor = NewRunoffBoundaryToolCursor };
            AddMapTool(newRunoffBoundaryTool);

            var newLinkTool = new AddHydroLinkMapTool(FeatureTypeLayerFilter<HydroLink>);
            AddMapTool(newLinkTool);
            
            var importBranchesMapTool = new ImportBranchesFromSelectionMapTool(FeatureTypeLayerFilter<Channel>)
                                            {
                                                Name = "import branches"
                                            };
            AddMapTool(importBranchesMapTool);

            var importStructuresMapTool = new ImportBranchFeaturesFromSelectedFeaturesMapTool(HydroNetworkFilter)
                                              {
                                                  Name = "import structures",
                      
                                              };
            AddMapTool(importStructuresMapTool);

            var copyBranchFeaturesMapTool = new CopyBranchFeaturesMapTool()
                                                {
                                                    Name = "Copy Branch Feature",

                                                };
            AddMapTool(copyBranchFeaturesMapTool);

            var pasteBranchFeaturesMapTool = new PasteBranchFeaturesMapTool(HydroNetworkFilter)
                                                 {
                                                     Name = "Paste Branch Feature",
                                                 };
            AddMapTool(pasteBranchFeaturesMapTool);

            var pasteIntoBranchFeaturesMapTool = new PasteIntoBranchFeaturesMapTool(HydroNetworkFilter)
                                                     {
                                                         Name = "Paste Into Branch Feature"
                                                     };
            AddMapTool(pasteIntoBranchFeaturesMapTool);

            var importCrossSectionFromCsvMapTool = new ImportCrossSectionsFromCsvMapTool(HydroNetworkFilter)
                                                       {
                                                           Name = "Import cross sections from csv"
                                                       };
            AddMapTool(importCrossSectionFromCsvMapTool);

            var exportCrossSectionToCsvMapTool = new ExportCrossSectionToCsvMapTool(HydroNetworkFilter)
                                                     {
                                                         Name = "Export cross sections to csv"
                                                     };
            AddMapTool(exportCrossSectionToCsvMapTool);

            AddMapTool(new Feature2DLineTool(HydroArea.ThinDamsPluralName, ThinDamToolName, Resources.thindam));
            AddMapTool(new Feature2DLineTool(HydroArea.FixedWeirsPluralName, FixedWeirToolName, Resources.fixedweir));
            AddMapTool(new Feature2DPointTool(HydroArea.ObservationPointsPluralName, ObservationPointToolName, Resources.Observation));
            AddMapTool(new Feature2DLineTool(HydroArea.ObservationCrossSectionsPluralName, ObservationCrossSectionToolName, Resources.observationcs2d));
            AddMapTool(new Feature2DLineTool(HydroArea.PumpsPluralName, PumpToolName, Resources.pump));
            AddMapTool(new Feature2DLineTool(HydroArea.WeirsPluralName, WeirToolName, Resources.Weir) { MaxPoints = 2 });
            AddMapTool(new Feature2DLineTool(HydroArea.GatesPluralName, GateToolName, Resources.Gate) { MaxPoints = 2 });
            AddMapTool(new Feature2DLineTool(HydroArea.LandBoundariesPluralName, LandBoundaryToolName, Resources.landboundary));
            AddMapTool(new Feature2DPointTool(HydroArea.DryPointsPluralName, DryPointToolName, Resources.dry_point));
            AddMapTool(new Feature2DLineTool(HydroArea.DryAreasPluralName, DryAreaToolName, Resources.dry_area) { CloseLine = true });
            AddMapTool(new Feature2DLineTool(HydroArea.EmbankmentsPluralName, EmbankmentToolName, Resources.Embankment));
            AddMapTool(new LeveeBreachMapTool(HydroArea.LeveeBreachName, LeveeBreachToolName, Resources.LeveeBreach));
            AddMapTool(new Feature2DLineTool(HydroArea.RoofAreaName, RoofAreaToolName, Resources.Roof){CloseLine = true});
            AddMapTool(new Feature2DPointTool(HydroArea.GullyName, GullyToolName, Resources.Gully));
            AddMapTool(new SingleFeature2DLineTool(HydroArea.EnclosureName, EnclosureToolName, Resources.enclosure) { CloseLine = true });
            AddMapTool(new Feature2DLineTool(HydroArea.BridgePillarsPluralName, BridgePillarToolName, Resources.BridgeSmall));

            var addInterpolatedCrossSectionTool = new NewPointFeatureTool(FeatureTypeLayerFilter<CrossSection>, AddInterpolatedCrossSectionToolName) { Cursor = AddInterpolatedCrossSectionToolCursor };
            AddMapTool(addInterpolatedCrossSectionTool);
        }

        private static bool HydroNetworkFilter(ILayer layer)
        {
            var hydroRegionLayer = layer as HydroRegionMapLayer;
            if (hydroRegionLayer != null)
            {
                return hydroRegionLayer.Region is IHydroNetwork;
            }
            return false;
        }

        private IMapTool NetworkLocationTool { get; set; }


        public HydroRegionEditorMapTool()
        {
            Tolerance = 1;
        }

        /// <summary>
        /// The active coveragelayer used by the NetworkLocationTool.
        /// </summary>
        public INetworkCoverageGroupLayer ActiveNetworkCoverageGroupLayer
        {
            get { return activeNetworkCoverageGroupLayer; }
            set
            {
                bool activateTool = false;
                if (activeNetworkCoverageGroupLayer != null)
                {
                    if (null != NetworkLocationTool)
                    {
                        activateTool = NetworkLocationTool.IsActive;
                    }
                    ResetCurrentNetworkCoverageEditor(activeNetworkCoverageGroupLayer);
                }

                activeNetworkCoverageGroupLayer = value;

                if (activeNetworkCoverageGroupLayer != null)
                {
                    SetCurrentNetworkCoverageEditor(activeNetworkCoverageGroupLayer, activateTool);
                }
            }
        }

        public override IMapControl MapControl
        {
            get { return base.MapControl; }
            set
            {
                if (MapControl != null)
                {
                    RemoveNetworkEditorTools();

                    if (map != null)
                    {
                        map.CollectionChanged -= LayersCollectionChanged;
                    }

                    var mapControl = MapControl as MapControl;

                    if (mapControl != null)
                    {
                        mapControl.MouseUp -= MapControlMouseUp;
                        mapControl.KeyDown -= MapControlKeyDown;
                        mapControl.KeyUp -= MapControlKeyUp;
                    }
                }

                base.MapControl = value;

                if (null != MapControl)
                {
                    AddNetworkEditorTools();

                    MapControl.Map.CollectionChanged += LayersCollectionChanged;
                    map = MapControl.Map;

                    var mapControl = MapControl as MapControl;

                    if (mapControl != null)
                    {
                        mapControl.MouseUp += MapControlMouseUp;
                        mapControl.KeyDown += MapControlKeyDown;
                        mapControl.KeyUp += MapControlKeyUp;
                    }

                    var networkCovergeGroupLayer = Map.GetAllVisibleLayers(true).OfType<NetworkCoverageGroupLayer>().FirstOrDefault();

                    if(networkCovergeGroupLayer != null)
                    {
                        ActiveNetworkCoverageGroupLayer = networkCovergeGroupLayer;
                    }
                }
            }
        }

        private void MapControlKeyDown(object sender, KeyEventArgs e)
        {
            TopologyRulesEnabledState = TopologyRulesEnabled;
            TopologyRulesEnabled = true;
        }

        private void MapControlKeyUp(object sender, KeyEventArgs e)
        {
            // remember topologyrule state; to reset properly. 
            // When tool is active (eg drawing branch) keydown/up should not reset TopologyRulesEnabled
            TopologyRulesEnabled = TopologyRulesEnabledState;

            if ((e.KeyCode == Keys.Control || e.KeyCode == Keys.C && MapControl.SelectedFeatures.Any()) && (MapControl.SelectedFeatures.First() is IChannel || MapControl.SelectedFeatures.First() is IBranchFeature))
            {
                HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard((INetworkFeature)MapControl.SelectedFeatures.First());
            }

            if (e.KeyCode == Keys.Control || e.KeyCode == Keys.V)
            {
                if (!MapControl.SelectedFeatures.Any())
                {
                    if (HydroNetworkCopyAndPasteHelper.IsChannelSetToClipBoard())
                    {
                        PasteBranch();
                    }
                }
                else
                {
                    if (MapControl.SelectedFeatures.First() is IChannel && HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard())
                    {
                        var pasteBranchFeaturesMapTool = mapTools.OfType<PasteBranchFeaturesMapTool>().FirstOrDefault();

                        if (pasteBranchFeaturesMapTool != null)
                        {
                            pasteBranchFeaturesMapTool.Execute();
                        }
                    }

                    if (MapControl.SelectedFeatures.First() is IBranchFeature && HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard())
                    {
                        var pasteIntoBranchFeaturesMapTool = mapTools.OfType<PasteIntoBranchFeaturesMapTool>().FirstOrDefault();

                        if (pasteIntoBranchFeaturesMapTool != null)
                        {
                            pasteIntoBranchFeaturesMapTool.Execute();
                        }
                    }
                }
            }
        }

        void MapControlMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TopologyRulesEnabled = false;
            }
        }

        public override bool IsActive
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// All topology rules work only when user is editing data (currently it is between mouse down and mouse up).
        /// </summary>
        public bool TopologyRulesEnabled { get; set; }

        public virtual float Tolerance { get; set; }

        public IEnumerable<IHydroRegion> HydroRegions
        {
            get { return Map.GetAllLayers(true).OfType<HydroRegionMapLayer>().Select(l => l.Region); }
        }

        private void RemoveNetworkEditorTools()
        {
            foreach (IMapTool mapTool in mapTools)
            {
                MapControl.Tools.Remove(mapTool);
            }
            
            mapTools.Clear();
        }

        private void AddMapTool(IMapTool mapTool)
        {
            mapTools.Add(mapTool);
            MapControl.Tools.Add(mapTool);
        }

        private void RemoveMapTool(IMapTool mapTool)
        {
            mapTools.Remove(mapTool);
            MapControl.Tools.Remove(mapTool);
        }

        /// <summary>
        /// Called when a networklocation is to be added to the active ActiveNetworkCoverageLayer.
        /// See AddNewFeatureFromGeometryDelegate for the handling of the default network features.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private IFeature AddNetworkLocationGeometryDelegate(IFeatureProvider provider, IGeometry geometry)
        {
            var branch = (IBranch) MapControl.SnapTool.SnapResult.SnappedFeature;
            double offset = GeometryHelper.Distance((ILineString) branch.Geometry, geometry.Coordinates[0]);

            offset *= branch.Length / branch.Geometry.Length;

            var location = new NetworkLocation(branch, offset) {Geometry = geometry};

            //cannot add double locations..should maybe include a miminal distance based on the current zoom level.
            if (!ActiveNetworkCoverageGroupLayer.NetworkCoverage.Locations.Values.Contains(location))
            {
                ActiveNetworkCoverageGroupLayer.NetworkCoverage.BeginEdit(string.Format("Add new network location to {0}", ActiveNetworkCoverageGroupLayer.NetworkCoverage));
                ActiveNetworkCoverageGroupLayer.NetworkCoverage.Locations.Values.Add(location);    
                ActiveNetworkCoverageGroupLayer.NetworkCoverage.EndEdit();
            }
            
            ActiveNetworkCoverageGroupLayer.RenderRequired = true;

            return location;
        }
        
        private void LayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.GetRemovedOrAddedItem() is INetworkCoverageGroupLayer)
            {
                var networkCoverageLayer = (INetworkCoverageGroupLayer) e.GetRemovedOrAddedItem();
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException();

                    case NotifyCollectionChangedAction.Add:
                        // if a network coverage is already in editing mode; reset interactor
                        ActiveNetworkCoverageGroupLayer = networkCoverageLayer;
                        SetCoverageLayerTheme(networkCoverageLayer);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        ActiveNetworkCoverageGroupLayer = null;

                        // should we try to set editing to another networkcoverage if available?
                        break;
                }
            }
            if (e.GetRemovedOrAddedItem() is ILayer && HydroNetworkFilter((ILayer)e.GetRemovedOrAddedItem()))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException();

                    case NotifyCollectionChangedAction.Add:
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        break;
                }
            }
        }

        private void SetCoverageLayerTheme(INetworkCoverageGroupLayer networkCoverageGroupLayer)
        {
            int count = MapControl.Map.GetAllLayers(true).Count(l => l is INetworkCoverageGroupLayer);

            if (networkCoverageGroupLayer.NetworkCoverage.SegmentGenerationMethod !=
                     SegmentGenerationMethod.RouteBetweenLocations)
            {
                // Create a different style for each added coverage layer to prevent all layers look the same
                ShapeType shapeType = VectorRenderingHelper.GetIndexedShapeType(count);
                Brush brush = new SolidBrush(ColorHelper.GetIndexedColor(255, count));
                networkCoverageGroupLayer.LocationLayer.Style.Shape = shapeType;
                networkCoverageGroupLayer.LocationLayer.Style.Fill = brush;
                if (networkCoverageGroupLayer.LocationLayer.Theme is CustomTheme)
                {
                    ((VectorStyle) ((CustomTheme) networkCoverageGroupLayer.LocationLayer.Theme).DefaultStyle).Shape
                        = shapeType;
                    ((VectorStyle) ((CustomTheme) networkCoverageGroupLayer.LocationLayer.Theme).DefaultStyle).Fill =
                        brush;
                }
            }
        }
        
        private void SetCurrentNetworkCoverageEditor(INetworkCoverageGroupLayer networkCoverageGroupLayer, bool activateTool)
        {
            if (null == networkCoverageGroupLayer)
                return;

            NetworkLocationTool = new NewPointFeatureTool(l => l.Equals(networkCoverageGroupLayer.LocationLayer), AddNetworkLocationToolName)
                                      {
                                          IsActive = activateTool,
                                          Cursor = MapCursors.CreateArrowOverlayCuror(Resources.NetworkLocationSmall)
                                      };
            AddMapTool(NetworkLocationTool);

            networkCoverageGroupLayer.LocationLayer.DataSource.AddNewFeatureFromGeometryDelegate = AddNetworkLocationGeometryDelegate;
        }

        /// <summary>
        /// Clears the network coverage interactor.
        /// </summary>
        /// <param name="networkCoverageGroupLayer"></param>
        /// 
        private void ResetCurrentNetworkCoverageEditor(INetworkCoverageGroupLayer networkCoverageGroupLayer)
        {
            if (null == NetworkLocationTool || null == networkCoverageGroupLayer)
                return;
            RemoveMapTool(NetworkLocationTool);
            NetworkLocationTool = null;
            if (networkCoverageGroupLayer.LocationLayer.DataSource != null)
                networkCoverageGroupLayer.LocationLayer.DataSource.AddNewFeatureFromGeometryDelegate = null;
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TopologyRulesEnabled = true;
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            // select the nearest object, maybe this is redundant and select tool should always do it?
            MapControl.SelectTool.OnMouseDown(worldPosition, e);
        }

        private void RemoveBranchSegments(IDiscretization discretization, IEnumerable<IChannel> channels)
        {
            foreach (var channel in channels)
            {
                NetworkHelper.ClearLocations(discretization, channel);
            }
            MapControl.SelectTool.RefreshSelection();
            MapControl.Refresh();
        }

        private void GenerateBranchSegments(IDiscretization discretization, IEnumerable<IChannel> channels)
        {
            HydroNetworkEditorMapToolHelper.RunCalculationGridWizard(channels != null ? channels.ToList() : null, discretization);
            MapControl.SelectTool.RefreshSelection();
            MapControl.Refresh();
        }

        private void RemoveNetworkSegments(IDiscretization discretization)
        {
            if (discretization == null) return;

            discretization.ClearRuralLocations();

            MapControl.SelectTool.RefreshSelection();
            MapControl.Refresh();
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            // HydroNetworkEditorMapTool is alwas added to a map, even if there is no network.
            if (!Map.GetAllVisibleLayers(true).Any(HydroNetworkFilter))
                yield break;

            contextMenuWorldPosition = worldPosition;

            var firstDiscretizationLayer = MapControl.Map.GetAllLayers(true)
                .OfType<INetworkCoverageGroupLayer>()
                .FirstOrDefault(cl => cl.Coverage is IDiscretization);

            var discretization = firstDiscretizationLayer != null ? (IDiscretization)firstDiscretizationLayer.NetworkCoverage : null;
            if (discretization != null)
            {
                if (discretization.Locations.Values.Count > 0)
                {
                    yield return new MapToolContextMenuItem
                    {
                        Priority = 4,
                        MenuItem = new ToolStripMenuItem("Remove computational grid nodes", null, (s, e) =>
                            {
                                RemoveNetworkSegments(discretization);
                                firstDiscretizationLayer.Visible = true;
                            })
                    };
                }

                yield return new MapToolContextMenuItem
                    {
                        Priority = 4,
                        MenuItem = new ToolStripMenuItem("Generate computational grid nodes", null, (s, e) =>
                            {
                                GenerateBranchSegments(discretization, null);
                                firstDiscretizationLayer.Visible = true;
                            })
                    };
            }

            if (HydroNetworkCopyAndPasteHelper.IsChannelSetToClipBoard())
            {
                yield return new MapToolContextMenuItem
                {
                    Priority = 2,
                    MenuItem = new ToolStripMenuItem("Paste channel", null, (s, e) => PasteBranch())
                        {
                            ShortcutKeys = Keys.Control | Keys.V
                        }
                };
            }

            if (MapControl.SelectedFeatures == null) yield break;

            var channels = MapControl.SelectedFeatures.OfType<IChannel>().ToList();
            if (channels.Count > 0)
            {
                if (discretization != null)
                {
                    if (discretization.Locations.Values.Any(nl => channels.Contains(nl.Branch)))
                    {
                        yield return new MapToolContextMenuItem
                            {
                                Priority = 3,
                                MenuItem = new ToolStripMenuItem("Remove computational grid nodes in selected branch(es)", null,
                                    (sender, args) =>
                                        {
                                            RemoveBranchSegments(discretization, channels);
                                            firstDiscretizationLayer.Visible = true;
                                        })
                            };
                    }

                    yield return new MapToolContextMenuItem
                        {
                            Priority = 3,
                            MenuItem = new ToolStripMenuItem("Generate computational grid nodes in selected branch(es)", null,
                                (s, e) =>
                                    {
                                        GenerateBranchSegments(discretization, channels);
                                        firstDiscretizationLayer.Visible = true;
                                    })
                        };
                }

                yield return new MapToolContextMenuItem
                    {
                        Priority = 3,
                        MenuItem = new ToolStripMenuItem("Insert Node", null, (s, e) => InsertNode(channels))
                    };
                
                yield return new MapToolContextMenuItem
                    {
                        Priority = 3,
                        MenuItem = new ToolStripMenuItem("Reverse direction", null, (s,e) => ReverseBranch(channels))
                    };

                yield return new MapToolContextMenuItem
                    {
                        Priority = 2,
                        MenuItem = new ToolStripMenuItem("Copy channel", null, (s, e) => CopyBranch(channels))
                            {
                                ShortcutKeys = Keys.Control | Keys.C
                            }
                    };
            }

            var nodes = MapControl.SelectedFeatures.OfType<INode>().ToList();
            if (nodes.Count > 0)
            {
                yield return new MapToolContextMenuItem
                    {
                        Priority = 3,
                        MenuItem = new ToolStripMenuItem("Remove Node", null, (s,e) => RemoveNode(nodes))
                            {
                                Enabled = nodes.Any(n => n.IncomingBranches.Count == 1 && n.OutgoingBranches.Count == 1)
                            }
                    };
            }

            var networkLocations = MapControl.SelectedFeatures.OfType<INetworkLocation>().ToList();
            if (networkLocations.Count > 0 && discretization != null && discretization.Locations.Values.Any(networkLocations.Contains))
            {
                yield return new MapToolContextMenuItem
                {
                    Priority = 3,
                    MenuItem = new ToolStripMenuItem("Fixed gridpoint", null, (s, e) => ToggleFixedGridPoint(discretization, networkLocations))
                    {
                        Checked = networkLocations.Any(discretization.IsFixedPoint)
                    }
                };
            }

            var crossSections = MapControl.SelectedFeatures.OfType<ICrossSection>().ToList();
            if (crossSections.Count > 0)
            {
                yield return new MapToolContextMenuItem
                    {
                        Priority = 3,
                        MenuItem = new ToolStripMenuItem("Shift Level...", null, (s, e) => LevelShift(crossSections))
                    };
            }
        }

        private void InsertNode(IList<IChannel> channels)
        {
            if (contextMenuWorldPosition == null)
                return;

            if (channels == null || !channels.Any())
            {
                return;
            }

            IChannel branch;
            if (channels.Count == 1)
            {
                branch = channels.First();
            }
            else
            {
                var point = new Point(contextMenuWorldPosition);
                var distanceLookup = channels.ToDictionary(c => c, c => c.Geometry.Distance(point));
                var lowestDistance = distanceLookup.Min(kvp => kvp.Value);

                branch = distanceLookup.First(kvp => Math.Abs(kvp.Value - lowestDistance) < 0.000000001).Key;
            }
            
            HydroNetworkHelper.SplitChannelAtNode(branch, contextMenuWorldPosition);
            MapControl.SelectTool.RefreshSelection();
            MapControl.Refresh();
        }

        private void PasteBranch()
        {
            var layer = Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().FirstOrDefault(HydroNetworkFilter);
            if (layer == null)
            {
                return;
            }

            var network = (IHydroNetwork)layer.Region;
            if(network == null)
            {
                return;
            }

            string errorMessage;
            if (!HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(network, out errorMessage))
            {
                MessageBox.Show(errorMessage, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MapControl.Refresh();
        }

        private static void CopyBranch(IEnumerable<IChannel> channels)
        {
            var channel = channels.FirstOrDefault();
            if (channel == null) return;

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(channel);
        }

        private static bool IsOriented(IBranchFeature branchFeature)
        {
            if (branchFeature is ICompositeBranchStructure)
            {
                return ((ICompositeBranchStructure) branchFeature).Structures.Any(IsOriented);
            }
            if (branchFeature is BranchStructure)
            {
                return true;
            }
            var crossSection = branchFeature as ICrossSection;
            if (crossSection != null)
            {
                return crossSection.CrossSectionType == CrossSectionType.YZ ||
                       crossSection.CrossSectionType == CrossSectionType.GeometryBased;
            }
            return false;
        }

        private void ReverseBranch(IEnumerable<IChannel> channels)
        {
            if (channels.Any(c => c.BranchFeatures.Any(IsOriented)))
            {
                var result = MessageBox.Show(
                    "Your branch contains oriented structures and cross sections, which will not be reversed upon reversal of the flow direction. Continue?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;
            }
            foreach (var channel in channels)
            {
                HydroNetworkHelper.ReverseBranch(channel);
            }
            MapControl.Refresh();
        }

        private static void ToggleFixedGridPoint(IDiscretization discretization, IEnumerable<INetworkLocation> networkLocations)
        {
            foreach (var networkLocation in networkLocations)
            {
                discretization.ToggleFixedPoint(networkLocation);
            }
        }

        private void LevelShift(IEnumerable<ICrossSection> crossSections)
        {
            var formLevelShift = new FormLevelShift();
            if (formLevelShift.ShowDialog() == DialogResult.OK)
            {
                foreach (var crossSection in crossSections)
                {
                    crossSection.Definition.ShiftLevel(formLevelShift.Shift);
                }
            }
        }
        
        private void RemoveNode(IEnumerable<INode> nodes)
        {
            INode currentNode = null;
            try
            {
                foreach (var node in nodes.Where(n => n.IncomingBranches.Count == 1 && n.OutgoingBranches.Count == 1))
                {
                    currentNode = node;
                    NetworkHelper.MergeNodeBranches(node, node.Network);
                }
            }
            catch (ArgumentException ex)
            {
                //isn't a dialog more appropriate..
                log.ErrorFormat("An error occured while removing node '{0}': {1}", currentNode?.Name, ex.Message);
            }

            MapControl.SelectTool.RefreshSelection();
            MapControl.Refresh();
        }
    }
}