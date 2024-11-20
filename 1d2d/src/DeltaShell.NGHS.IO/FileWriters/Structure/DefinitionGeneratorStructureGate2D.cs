using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureGate2D : DefinitionGeneratorTimeSeriesStructure2D
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Gate);

            var gate = (IGate) hydroObject;
            AddCrestLevelProperty(gate);
            AddLowerEdgeLevelProperty(gate);
            AddOpeningWidthProperty(gate);
            AddHorizontalDirectionProperty(gate);
            AddSillWidthProperty(gate);
            AddGateHeightProperty(gate);
            
            return IniSection;
        }

        private void AddCrestLevelProperty(IGate gate)
        {
            if (gate.UseSillLevelTimeSeries)
            {
                var timeSeriesFileName = $"{gate.Name}_{StructureRegion.GateCrestLevel.Key}{FileSuffices.TimFile}";
                IniSection.AddPropertyWithOptionalComment(StructureRegion.GateCrestLevel.Key, timeSeriesFileName, StructureRegion.GateCrestLevel.Description);
            }
            else
            {
                IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateCrestLevel.Key, gate.SillLevel, StructureRegion.GateCrestLevel.Description, StructureRegion.GateCrestLevel.Format);
            }
        }

        private void AddLowerEdgeLevelProperty(IGate gate)
        {
            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                var timeSeriesFileName = $"{gate.Name}_{StructureRegion.GateLowerEdgeLevel.Key}{FileSuffices.TimFile}";
                IniSection.AddPropertyWithOptionalComment(StructureRegion.GateLowerEdgeLevel.Key, timeSeriesFileName, StructureRegion.GateLowerEdgeLevel.Description);
            }
            else
            {
                IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateLowerEdgeLevel.Key, gate.LowerEdgeLevel, StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);
            }
        }

        private void AddOpeningWidthProperty(IGate gate)
        {
            if (gate.UseOpeningWidthTimeSeries)
            {
                var timeSeriesFileName = $"{gate.Name}_{StructureRegion.GateOpeningWidth.Key}{FileSuffices.TimFile}";
                IniSection.AddPropertyWithOptionalComment(StructureRegion.GateOpeningWidth.Key, timeSeriesFileName, StructureRegion.GateOpeningWidth.Description);
            }
            else
            {
                IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateOpeningWidth.Key, gate.OpeningWidth, StructureRegion.GateOpeningWidth.Description, StructureRegion.GateOpeningWidth.Format);
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
                    horizontalDirection = "fromLeft";
                    break;
                case GateOpeningDirection.FromRight:
                    horizontalDirection = "fromRight";
                    break;
                default:
                    throw new ArgumentException("We can't write " + gate.HorizontalOpeningDirection);
            }

            IniSection.AddPropertyWithOptionalComment(StructureRegion.GateHorizontalOpeningDirection.Key, horizontalDirection, StructureRegion.GateHorizontalOpeningDirection.Description);
        }

        private void AddSillWidthProperty(IGate gate)
        {
            if (gate.SillWidth > 0.0)
            {
                IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateCrestWidth.Key, gate.SillWidth, StructureRegion.GateCrestWidth.Description);
            }
        }

        private void AddGateHeightProperty(IGate gate)
        {
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateHeight.Key, gate.DoorHeight,StructureRegion.GateHeight.Description, StructureRegion.GateHeight.Format);

        }
    }
}
