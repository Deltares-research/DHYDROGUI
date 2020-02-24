using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwAlgComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_ALG_FILENAME = "pluvius.alg";

        private const string DFEAULT_GENERAL_ID = "-1";
        private const string DEFAULT_INFILTRATION_FROM_DEPRESSIONS = "1";
        private const string DEFAULT_INFILTRATION_FROM_RUNOFF = "0";

        public NwrwAlgComponentFileWriter(RainfallRunoffModel model) : base(model, NWRW_ALG_FILENAME)
        {
        }

        protected override IEnumerable<string> CreateContentLine(RainfallRunoffModel model)
        {
            var definitions = model.NwrwDefinitions;

            yield return CreateNwrwAlgLine(definitions);
        }

        private string CreateNwrwAlgLine(IList<NwrwDefinition> nwrwDefinitions)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagToAlgLine(line);
            AppendIdToAlgLine(line);
            AppendRunoffDelayFactorToAlgLine(line, nwrwDefinitions);
            AppendMaximumStorageToAlgLine(line, nwrwDefinitions);
            AppendMaximumInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendMinimumInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendDecreaseInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendIncreaseInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendInfiltrationFromDepressionToAlgLine(line, nwrwDefinitions);
            AppendInfiltrationFromRunoffToAlgLine(line, nwrwDefinitions);
            AppendClosingTagToAlgLine(line);

            return line.ToString();
        }

        private void AppendOpeningTagToAlgLine(StringBuilder line)
        {
            // opening tag
            line.Append("PLVG");
            line.Append(" ");
        }

        private void AppendIdToAlgLine(StringBuilder line)
        {
            // id
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(DFEAULT_GENERAL_ID);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendRunoffDelayFactorToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // runoff-delay factor
            line.Append(NwrwKeywords.RunoffDelayFactor);
            line.Append(" ");
            foreach (NwrwDefinition nwrwDefinition in nwrwDefinitions)
            {
                line.Append(nwrwDefinition.RunoffDelay);
                line.Append(" ");
            }
        }

        private void AppendMaximumStorageToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // maximum storage for 12 types
            line.Append(NwrwKeywords.MaximumStorage);
            line.Append(" ");
            foreach (NwrwSurfaceType nwrwSurfaceType in SurfaceTypesInCorrectOrder)
            {
                line.Append(
                    nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(nwrwSurfaceType))?.SurfaceStorage);
                line.Append(" ");
            }
        }

        private void AppendMaximumInfiltrationCapacityToAlgLine(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // maximum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.MaximumInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions
                .FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))
                ?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))
                ?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))
                ?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))
                ?.InfiltrationCapacityMax);
            line.Append(" ");
        }

        private void AppendMinimumInfiltrationCapacityToAlgLine(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // minimum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.MinimumInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions
                .FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))
                ?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))
                ?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))
                ?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))
                ?.InfiltrationCapacityMin);
            line.Append(" ");
        }

        private void AppendDecreaseInfiltrationCapacityToAlgLine(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // decrease in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.DecreaseInInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions
                .FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))
                ?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))
                ?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))
                ?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))
                ?.InfiltrationCapacityReduction);
            line.Append(" ");
        }

        private void AppendIncreaseInfiltrationCapacityToAlgLine(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // increase in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.IncreaseInInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions
                .FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))
                ?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))
                ?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))
                ?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))
                ?.InfiltrationCapacityRecovery);
            line.Append(" ");
        }

        private void AppendInfiltrationFromDepressionToAlgLine(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // option for infiltration from depressions
            line.Append(NwrwKeywords.InfiltrationFromDepressions);
            line.Append(" ");
            line.Append(DEFAULT_INFILTRATION_FROM_DEPRESSIONS);
            line.Append(" ");
        }

        private void AppendInfiltrationFromRunoffToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // option for infiltration from runoff
            line.Append(NwrwKeywords.InfiltrationFromRunoff);
            line.Append(" ");
            line.Append(DEFAULT_INFILTRATION_FROM_RUNOFF);
            line.Append(" ");
        }

        private void AppendClosingTagToAlgLine(StringBuilder line)
        {
            // closing tag
            line.Append("plvg");
        }
    }
}