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

            line.Append($"{NwrwKeywords.Pluv_alg_PLVG} ");
            line.Append($"{NwrwKeywords.Pluv_id} '{DFEAULT_GENERAL_ID}' ");

            AppendRunoffDelayFactor(line, nwrwDefinitions);
            AppendMaximumStorage(line, nwrwDefinitions);
            AppendMaximumInfiltrationCapacity(line, nwrwDefinitions);
            AppendMinimumInfiltrationCapacity(line, nwrwDefinitions);
            AppendDecreaseInfiltrationCapacity(line, nwrwDefinitions);
            AppendIncreaseInfiltrationCapacity(line, nwrwDefinitions);

            line.Append($"{NwrwKeywords.Pluv_alg_od} {DEFAULT_INFILTRATION_FROM_DEPRESSIONS} ");
            line.Append($"{NwrwKeywords.Pluv_alg_or} {DEFAULT_INFILTRATION_FROM_RUNOFF} ");
            line.Append($"{NwrwKeywords.Pluv_alg_plvg}");

            return line.ToString();
        }

        private void AppendRunoffDelayFactor(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            line.Append($"{NwrwKeywords.Pluv_alg_rf} ");

            foreach (NwrwDefinition nwrwDefinition in nwrwDefinitions)
            {
                line.Append($"{nwrwDefinition.RunoffDelay} ");
            }
        }

        private void AppendMaximumStorage(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            line.Append($"{NwrwKeywords.Pluv_alg_ms} ");
            foreach (NwrwSurfaceType nwrwSurfaceType in NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder)
            {
                line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(nwrwSurfaceType))?.SurfaceStorage} ");
            }
        }

        private void AppendMaximumInfiltrationCapacity(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            line.Append($"{NwrwKeywords.Pluv_alg_ix} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityMax} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityMax} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityMax} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityMax} ");
        }

        private void AppendMinimumInfiltrationCapacity(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // minimum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append($"{NwrwKeywords.Pluv_alg_im} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityMin} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityMin} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityMin} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityMin} ");
        }

        private void AppendDecreaseInfiltrationCapacity(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // decrease in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append($"{NwrwKeywords.Pluv_alg_ic} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityReduction} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityReduction} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityReduction} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityReduction} ");
        }

        private void AppendIncreaseInfiltrationCapacity(StringBuilder line,
            IList<NwrwDefinition> nwrwDefinitions)
        {
            // increase in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append($"{NwrwKeywords.Pluv_alg_dc} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityRecovery} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityRecovery} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityRecovery} ");
            line.Append($"{nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityRecovery} ");
        }
    }
}