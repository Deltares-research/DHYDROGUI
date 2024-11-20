using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class Nwrw3BComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_3B_FILENAME = "pluvius.3b";

        public Nwrw3BComponentFileWriter(RainfallRunoffModel model) : base(model, NWRW_3B_FILENAME)
        {
        }

        private string CreateNwrw3bLine(NwrwData nwrwData)
        {
            var line = new StringBuilder();

            line.Append($"{NwrwKeywords.Pluv_3b_NWRW} ");
            line.Append($"{NwrwKeywords.Pluv_id} '{nwrwData.NodeOrBranchId}' ");

            var surfaceLevel = nwrwData.LateralSurface;
            if (Math.Abs(surfaceLevel) > 0.001)
            {
                line.Append($"{NwrwKeywords.Pluv_3b_sl} {surfaceLevel} ");
            }

            AppendArea(line, nwrwData.SurfaceLevelDict);
            AppendDryWeatherFlows(line, nwrwData.DryWeatherFlows);

            line.Append($"{NwrwKeywords.Pluv_3b_ms} '{nwrwData.MeteoStationId}' ");

            AppendSpecialAreas(line, nwrwData.NumberOfSpecialAreas, nwrwData.SpecialAreas);

            line.Append($"{NwrwKeywords.Pluv_3b_nwrw}");

            return line.ToString();
        }


        private void AppendArea(StringBuilder line, IDictionary<NwrwSurfaceType, double> surfaceLevelDict)
        {
            // 'ar' + area (12 types) as combination of 3 kind of slopes and 4 types of surfaces
            line.Append($"{NwrwKeywords.Pluv_3b_ar} ");

            foreach (var surfaceType in NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder)
            {
                if (surfaceLevelDict.ContainsKey(surfaceType))
                    line.Append(surfaceLevelDict[surfaceType]);
                else
                    line.Append("0");

                line.Append(" ");
            }
        }

        private void AppendDryWeatherFlows(StringBuilder line,
            IList<DryWeatherFlow> nwrwDataDryWeatherFlows)
        {
            var dryWeatherFlowCount = nwrwDataDryWeatherFlows.Count;
            if (dryWeatherFlowCount >= 1)
            {
                line.Append($"{NwrwKeywords.Pluv_3b_np} {nwrwDataDryWeatherFlows[0].NumberOfUnits} ");
                line.Append($"{NwrwKeywords.Pluv_3b_dw} '{nwrwDataDryWeatherFlows[0].DryWeatherFlowId}' ");
            }

            if (dryWeatherFlowCount >= 2)
            {
                line.Append($"{NwrwKeywords.Pluv_3b_np2} {nwrwDataDryWeatherFlows[1].NumberOfUnits} ");
                line.Append($"{NwrwKeywords.Pluv_3b_dw2} '{nwrwDataDryWeatherFlows[1].DryWeatherFlowId}' ");
            }
        }
        
        private void AppendSpecialAreas(StringBuilder line, int numberOfSpecialAreas,
            IList<NwrwSpecialArea> specialAreas)
        {
            if (numberOfSpecialAreas > 0)
            {
                line.Append($"{NwrwKeywords.Pluv_3b_na} {numberOfSpecialAreas} ");
                line.Append($"{NwrwKeywords.Pluv_3b_aa} ");
                foreach (var specialArea in specialAreas)
                {
                    line.Append($"{specialArea.Area} ");
                }
            }
        }
        
        protected override IEnumerable<string> CreateContentLine(RainfallRunoffModel model)
        {
            var nwrwData = model.GetAllModelData().OfType<NwrwData>();
            foreach (var data in nwrwData)
            {
                yield return CreateNwrw3bLine(data);
            }

        }
    }
}