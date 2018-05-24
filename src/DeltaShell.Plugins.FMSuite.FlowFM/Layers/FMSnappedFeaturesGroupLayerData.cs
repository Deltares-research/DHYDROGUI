using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.NetworkEditor.MapLayers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers
{
    public class FMSnappedFeaturesGroupLayerData
    {
        private readonly WaterFlowFMModel model;
        private IList<SnappedFeatureCollection> childData;
        private int SnapVersion { get; set; }

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
                        foreach(var coll in childData)
                            coll.Dispose();

                    childData = GetChildDataForModel(model).ToList();
                }
                return childData;
            }
        }

        private static IEnumerable<SnappedFeatureCollection> GetChildDataForModel(WaterFlowFMModel model)
        {
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList) model.Area.ObservationPoints,
                                                      AreaLayerStyles.ObservationPointStyle, "Observation points (snapped)", UnstrucGridOperationApi.ObsPoint);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.ThinDams,
                                                      AreaLayerStyles.ThinDamStyle, "Thin dams (snapped)", UnstrucGridOperationApi.ThinDams);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.FixedWeirs,
                                                      AreaLayerStyles.FixedWeirStyle, "Fixed weirs (snapped)", UnstrucGridOperationApi.FixedWeir);
            yield return new SnappedDamBreakCollection(model, model.CoordinateSystem, (IList)model.Area.LeveeBreaches,
                                          AreaLayerStyles.DamBreakStyle, "Levee breach / breach locations (snapped)", UnstrucGridOperationApi.DamBreakLine);
            yield return
                new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.DryPoints,
                    AreaLayerStyles.DryPointStyle, "Dry points (snapped)", UnstrucGridOperationApi.ObsPoint);
            yield return
                new SnappedFeatureCollection(model, model.CoordinateSystem, (IList) model.Area.DryAreas,
                    AreaLayerStyles.DryAreaStyle, "Dry areas (snapped)", UnstrucGridOperationApi.ObsCrossSection);
            yield return
                new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Enclosures,
                    AreaLayerStyles.EnclosureStyle, "Enclosure (snapped)", UnstrucGridOperationApi.ObsCrossSection);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Pumps,
                                                      AreaLayerStyles.PumpStyle, "Pumps (snapped)", UnstrucGridOperationApi.Pump);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Weirs,
                                                      AreaLayerStyles.WeirStyle, "Weirs (snapped)", UnstrucGridOperationApi.Weir);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Gates,
                                                      AreaLayerStyles.GateStyle, "Gates (snapped)", UnstrucGridOperationApi.Gate);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.ObservationCrossSections,
                                                      AreaLayerStyles.ObsCrossSectionStyle, "Observation cross sections (snapped)", UnstrucGridOperationApi.ObsCrossSection);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Area.Embankments,
                                                      AreaLayerStyles.EmbankmentStyle, "Embankments (snapped)", UnstrucGridOperationApi.Embankment);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.SourcesAndSinks,
                                                      AreaLayerStyles.SnappedSourcesAndSinksStyle, "Sources and sinks (snapped)", UnstrucGridOperationApi.SourceSink);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Boundaries,
                                                      AreaLayerStyles.BoundariesStyle, "Boundaries (snapped)", UnstrucGridOperationApi.Boundary);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList) model.Boundaries,
                                                      AreaLayerStyles.BoundariesWaterLevelPointsStyle,
                                                      "Water level boundary points", UnstrucGridOperationApi.WaterLevelBnd);
            yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList) model.Boundaries,
                                                      AreaLayerStyles.BoundariesVelocityPointsStyle,"Discharge / velocity boundary points",
                                                      UnstrucGridOperationApi.VelocityBnd);
        }
    }
}