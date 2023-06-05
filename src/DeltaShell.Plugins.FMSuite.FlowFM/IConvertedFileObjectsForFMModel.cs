using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Grids;
using System.Collections.Generic;
using DelftTools.Hydro.Link1d2d;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface IConvertedFileObjectsForFMModel : IConvertedUgridFileObjects
    {
        HydroArea HydroArea { get; set; }
        WaterFlowFMModelDefinition ModelDefinition { get; set; }
        IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D { get; set; }
        IEventedList<Model1DLateralSourceData> LateralSourcesData { get; set; }
        IList<ModelFeatureCoordinateData<FixedWeir>> AllFixedWeirsAndCorrespondingProperties { get; set; }
        IList<ModelFeatureCoordinateData<BridgePillar>> AllBridgePillarsAndCorrespondingProperties { get; set; }
    }

    public class ConvertedFileObjectsForFMModel : ConvertedUgridFileObjects, IConvertedFileObjectsForFMModel
    {
        public HydroArea HydroArea { get; set; }
        public WaterFlowFMModelDefinition ModelDefinition { get; set; }
        public IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D { get; set; }
        public IEventedList<Model1DLateralSourceData> LateralSourcesData { get; set; }
        public IList<ModelFeatureCoordinateData<FixedWeir>> AllFixedWeirsAndCorrespondingProperties { get; set; }
        public IList<ModelFeatureCoordinateData<BridgePillar>> AllBridgePillarsAndCorrespondingProperties { get; set; }
    }
}