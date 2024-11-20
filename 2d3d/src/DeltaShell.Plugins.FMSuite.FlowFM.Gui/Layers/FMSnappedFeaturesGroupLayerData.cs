using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    public class FMSnappedFeaturesGroupLayerData
    {
        private readonly WaterFlowFMModel model;
        private IList<SnappedFeatureCollection> childData;

        public FMSnappedFeaturesGroupLayerData(WaterFlowFMModel model)
        {
            this.model = model;
            SnapVersion = model.SnapVersion;
        }

        public IEnumerable<SnappedFeatureCollection> ChildData
        {
            get
            {
                if (childData == null || SnapVersion < model.SnapVersion)
                {
                    if (childData != null)
                    {
                        foreach (SnappedFeatureCollection coll in childData)
                        {
                            coll.Dispose();
                        }
                    }

                    childData = GetChildDataForModel(model).ToList();
                }

                return childData;
            }
        }

        private int SnapVersion { get; set; }

        private static IEnumerable<SnappedFeatureCollection> GetChildDataForModel(WaterFlowFMModel model)
        {
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem,
                                                      (IList)model.Area.ObservationPoints,
                                                      HydroAreaLayerStyles.ObservationPointStyle,
                                                      "Observation points (snapped)", UnstrucGridOperationApi.ObsPoint);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.ThinDams,
                                                      HydroAreaLayerStyles.ThinDamStyle, "Thin dams (snapped)",
                                                      UnstrucGridOperationApi.ThinDams);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.FixedWeirs,
                                                      HydroAreaLayerStyles.FixedWeirStyle, "Fixed weirs (snapped)",
                                                      UnstrucGridOperationApi.FixedWeir);
            yield return
                new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.DryPoints,
                                             HydroAreaLayerStyles.DryPointStyle, "Dry points (snapped)",
                                             UnstrucGridOperationApi.ObsPoint);
            yield return
                new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.DryAreas,
                                             HydroAreaLayerStyles.DryAreaStyle, "Dry areas (snapped)",
                                             UnstrucGridOperationApi.ObsCrossSection);
            yield return
                new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Enclosures,
                                             HydroAreaLayerStyles.EnclosureStyle, "Enclosure (snapped)",
                                             UnstrucGridOperationApi.ObsCrossSection);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Pumps,
                                                      HydroAreaLayerStyles.PumpStyle, "Pumps (snapped)",
                                                      UnstrucGridOperationApi.Pump);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Structures,
                                                      HydroAreaLayerStyles.WeirStyle, "Structures (snapped)",
                                                      UnstrucGridOperationApi.Weir);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem,
                                                      (IList)model.Area.ObservationCrossSections,
                                                      HydroAreaLayerStyles.ObsCrossSectionStyle,
                                                      "Observation cross sections (snapped)",
                                                      UnstrucGridOperationApi.ObsCrossSection);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.SourcesAndSinks,
                                                      HydroAreaLayerStyles.SnappedSourcesAndSinksStyle,
                                                      "Sources and sinks (snapped)",
                                                      UnstrucGridOperationApi.SourceSink);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Boundaries,
                                                      HydroAreaLayerStyles.BoundariesStyle, "Boundaries (snapped)",
                                                      UnstrucGridOperationApi.Boundary);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Boundaries,
                                                      HydroAreaLayerStyles.BoundariesWaterLevelPointsStyle,
                                                      "Water level boundary points",
                                                      UnstrucGridOperationApi.WaterLevelBnd);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Boundaries,
                                                      HydroAreaLayerStyles.BoundariesVelocityPointsStyle,
                                                      "Discharge / velocity boundary points",
                                                      UnstrucGridOperationApi.VelocityBnd);
        }
    }
}