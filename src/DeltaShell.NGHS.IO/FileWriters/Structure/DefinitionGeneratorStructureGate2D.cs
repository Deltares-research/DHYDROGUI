using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureGate2D : DefinitionGeneratorStructure2D
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Gate);

            var gate = (IGate) hydroObject;
            AddSillLevelProperty(gate);
            AddLowerEdgeLevelProperty(gate);
            AddOpeningWidthProperty(gate);
            AddHorizontalDirectionProperty(gate);
            AddSillWidthProperty(gate);

            return IniCategory;
        }

        private void AddSillLevelProperty(IGate gate)
        {
            if (gate.UseSillLevelTimeSeries)
            {
                var timeSeriesFileName = $"{gate.Name}_{KnownStructureProperties.GateSillLevel}.tim";
                IniCategory.AddProperty(StructureRegion.GateSillLevel.Key, timeSeriesFileName, StructureRegion.GateSillLevel.Description);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.GateSillLevel.Key, gate.SillLevel, StructureRegion.GateSillLevel.Description, StructureRegion.GateSillLevel.Format);
            }
        }

        private void AddLowerEdgeLevelProperty(IGate gate)
        {
            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                var timeSeriesFileName = $"{gate.Name}_{KnownStructureProperties.GateLowerEdgeLevel}.tim";
                IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, timeSeriesFileName, StructureRegion.GateLowerEdgeLevel.Description);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, gate.LowerEdgeLevel, StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);
            }
        }

        private void AddOpeningWidthProperty(IGate gate)
        {
            if (gate.UseOpeningWidthTimeSeries)
            {
                var timeSeriesFileName = $"{gate.Name}_{KnownStructureProperties.GateOpeningWidth}.tim";
                IniCategory.AddProperty(StructureRegion.GateOpeningWidth.Key, timeSeriesFileName, StructureRegion.GateOpeningWidth.Description);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.GateOpeningWidth.Key, gate.OpeningWidth, StructureRegion.GateOpeningWidth.Description, StructureRegion.GateOpeningWidth.Format);
            }
        }

        private void AddHorizontalDirectionProperty(IGate gate)
        {
            string horizontalDirection;
            switch (gate.HorizontalOpeningDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDirection = "symmetric";
                    break;
                case GateOpeningDirection.FromLeft:
                    horizontalDirection = "from_left";
                    break;
                case GateOpeningDirection.FromRight:
                    horizontalDirection = "from_right";
                    break;
                default:
                    throw new ArgumentException("We can't write " + gate.HorizontalOpeningDirection);
            }

            IniCategory.AddProperty(StructureRegion.GateHorizontalOpeningDirection.Key, horizontalDirection, StructureRegion.GateHorizontalOpeningDirection.Description);
        }

        private void AddSillWidthProperty(IGate gate)
        {
            if (gate.SillWidth > 0.0)
            {
                IniCategory.AddProperty(StructureRegion.GateSillWidth.Key, gate.SillWidth, StructureRegion.GateSillWidth.Description);
            }
        }
    }
}
