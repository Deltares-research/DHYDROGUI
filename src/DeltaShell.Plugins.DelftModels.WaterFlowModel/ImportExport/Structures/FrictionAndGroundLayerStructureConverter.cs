using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public abstract class FrictionAndGroundLayerStructureConverter : StructureConverter
    {
        protected static void SetFrictionValues(IFrictionData frictionDataStructure)
        {
            var bedFriction = Category.ReadProperty<double>(StructureRegion.BedFriction.Key);

            frictionDataStructure.FrictionDataType = (Friction)Category.ReadProperty<int>(StructureRegion.BedFrictionType.Key);
            frictionDataStructure.Friction = bedFriction;
        }

        protected static void SetGroundLayerValues(IGroundLayer groundLayerStructure)
        {
            var groundFriction = Category.ReadProperty<double>(StructureRegion.GroundFriction.Key);
            var bedFriction = Category.ReadProperty<double>(StructureRegion.BedFriction.Key);

            if (Math.Abs(groundFriction - bedFriction) > double.Epsilon)
            {
                groundLayerStructure.GroundLayerRoughness = groundFriction;
            }
        }
    }
}