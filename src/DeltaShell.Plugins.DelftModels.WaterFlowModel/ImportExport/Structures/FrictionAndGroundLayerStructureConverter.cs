using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This structure converter sets is responsible for setting values on objects that inherit from
    /// <see cref="IFrictionData"/> and <see cref="IGroundLayer"/>.
    /// </summary>
    /// <seealso cref="StructureConverter" />
    public abstract class FrictionAndGroundLayerStructureConverter : StructureConverter
    {
        protected static void SetFrictionValuesFromCategory(IFrictionData frictionDataStructure)
        {
            var bedFriction = Category.ReadProperty<double>(StructureRegion.BedFriction.Key);

            frictionDataStructure.FrictionDataType = (Friction)Category.ReadProperty<int>(StructureRegion.BedFrictionType.Key);
            frictionDataStructure.Friction = bedFriction;
        }

        protected static void SetGroundLayerValuesFromCategory(IGroundLayer groundLayerStructure)
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