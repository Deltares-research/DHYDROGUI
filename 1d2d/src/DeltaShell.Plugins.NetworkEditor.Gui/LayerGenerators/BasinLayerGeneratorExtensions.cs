using System.Collections;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.LayerGenerators
{
    internal static class BasinLayerGeneratorExtensions
    {
        internal static ILayer GenerateDrainageBasinLayer(this IDrainageBasin drainageBasin, object data)
        {
            switch (data)
            {
                case IEventedList<WasteWaterTreatmentPlant> wasteWaterTreatmentPlants when drainageBasin != null:
                    return new VectorLayer("Wastewater Treatment Plants")
                    {
                        Style = NetworkLayerStyleFactory.CreateStyle(wasteWaterTreatmentPlants),
                        NameIsReadOnly = true,
                        DataSource = new ComplexFeatureCollection(drainageBasin, (IList)wasteWaterTreatmentPlants, typeof(WasteWaterTreatmentPlant)),
                        FeatureEditor = new WasteWaterTreatmentPlantFeatureEditor { DrainageBasin = drainageBasin }
                    };
                case IEventedList<RunoffBoundary> runoffBoundaries when drainageBasin != null:
                    return new VectorLayer("Runoff Boundaries")
                    {
                        Style = NetworkLayerStyleFactory.CreateStyle(runoffBoundaries),
                        NameIsReadOnly = true,
                        DataSource = new ComplexFeatureCollection(drainageBasin, (IList)runoffBoundaries, typeof(RunoffBoundary)),
                        FeatureEditor = new RunoffBoundaryFeatureEditor { DrainageBasin = drainageBasin }
                    };
                case IEventedList<Catchment> catchments when drainageBasin != null:
                {
                    var centerLayers = new VectorLayer
                    {
                        Name = "Catchments (centers)",
                        DataSource = new ComplexFeatureCollection(drainageBasin, (IList)catchments, typeof(Catchment)),
                        FeatureEditor = new CatchmentFeatureEditor(true) { DrainageBasin = drainageBasin },
                        CustomRenderers = { new CatchmentAnchorPointRenderer() },
                        NameIsReadOnly = true
                    };
                    var catchmentLayer = new VectorLayer
                    {
                        Name = "Catchments (Polygons)",
                        Style = NetworkLayerStyleFactory.CreateStyle(catchments),
                        DataSource =
                            new ComplexFeatureCollection(drainageBasin,
                                                         (IList)catchments, typeof(Catchment))
                            {
                                CoordinateSystem = drainageBasin.CoordinateSystem
                            },
                        FeatureEditor = new CatchmentFeatureEditor { DrainageBasin = drainageBasin },
                        NameIsReadOnly = true
                    };

                    var groupLayer = new GroupLayer("Catchments") { NameIsReadOnly = true };
                    groupLayer.Layers.AddRange(new[] { centerLayers, catchmentLayer });
                    groupLayer.LayersReadOnly = true;

                    return groupLayer;
                }
                default:
                    return null;
            }
        }
    }
}