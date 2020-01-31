using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class Nwrw3BComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_3B_FILENAME = "pluvius.3b";
        private static ILog Log = LogManager.GetLogger(typeof(Nwrw3BComponentFileWriter));

        public Nwrw3BComponentFileWriter(RainfallRunoffModel model) : base(model, NWRW_3B_FILENAME)
        {
        }

        private string CreateNwrw3bLine(NwrwData nwrwData)
        {
            var line = new StringBuilder();

            AppendOpeningTagTo3bLine(line); // 'NWRW'
            AppendIdTo3bLine(line, nwrwData.NodeOrBranchId); // 'id'
            AppendSurfaceLevelTo3bLine(line, nwrwData.LateralSurface); // 'sl'
            AppendAreaTo3bLine(line, nwrwData.SurfaceLevelDict); // 'ar'
            AppendDryWeatherFlowsTo3bLine(line, nwrwData.DryWeatherFlows); // 'np' 'dw' 'np2' 'dw2'
            AppendMeteoStationIdTo3bLine(line, nwrwData.MeteoStationId); // 'ms'
            AppendSpecialAreasTo3bLine(line, nwrwData.NumberOfSpecialAreas, nwrwData.SpecialAreas); // 'na'
            AppendClosingTagTo3bLine(line); // 'nwrw'

            return line.ToString();
        }

        private void AppendOpeningTagTo3bLine(StringBuilder line)
        {
            // 'NWRW' opening keyword
            line.Append(NwrwKeywords.NwrwOpeningKey);
            line.Append(" ");
        }

        private void AppendIdTo3bLine(StringBuilder line, string id)
        {
            // 'id' + node identification
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(id);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendSurfaceLevelTo3bLine(StringBuilder line, double surfaceLevel)
        {
            // 'sl' + surface level (in m) (optional input data)
            if (Math.Abs(surfaceLevel) > 0.001)
            {
                line.Append(NwrwKeywords.SurfaceLevelKey);
                line.Append(" ");
                line.Append(surfaceLevel);
                line.Append(" ");
            }
        }

        private void AppendAreaTo3bLine(StringBuilder line, IDictionary<NwrwSurfaceType, double> surfaceLevelDict)
        {
            // 'ar' + area (12 types) as combination of 3 kind of slopes and 4 types of surfaces
            line.Append(NwrwKeywords.AreaKey);
            line.Append(" ");


            foreach (var surfaceType in SurfaceTypesInCorrectOrder)
            {
                if (surfaceLevelDict.ContainsKey(surfaceType))
                    line.Append(surfaceLevelDict[surfaceType]);
                else
                    line.Append("0");

                line.Append(" ");
            }
        }

        private void AppendDryWeatherFlowsTo3bLine(StringBuilder line,
            IList<DryWeatherFlow> nwrwDataDryWeatherFlows)
        {
            var numberOfDryWeatherFlows = nwrwDataDryWeatherFlows.Count;
            if (numberOfDryWeatherFlows >= 1)
            {
                line.Append(NwrwKeywords.FirstNumberOfUnitsKey);
                line.Append(" ");
                line.Append(nwrwDataDryWeatherFlows[0].NumberOfUnits);
                line.Append(" ");
                line.Append(NwrwKeywords.FirstDryWeatherFlowIdKey);
                line.Append(" ");
                line.Append("'");
                line.Append(nwrwDataDryWeatherFlows[0].DryWeatherFlowId);
                line.Append("'");
                line.Append(" ");
            }

            if (numberOfDryWeatherFlows >= 2)
            {
                line.Append(NwrwKeywords.SecondNumberOfUnitsKey);
                line.Append(" ");
                line.Append(nwrwDataDryWeatherFlows[1].NumberOfUnits);
                line.Append(" ");
                line.Append(NwrwKeywords.SecondDryWeatherFlowIdKey);
                line.Append(" ");
                line.Append("'");
                line.Append(nwrwDataDryWeatherFlows[1].DryWeatherFlowId);
                line.Append("'");
                line.Append(" ");
            }
        }

        private void AppendMeteoStationIdTo3bLine(StringBuilder line, string meteostationId)
        {
            // 'ms' + identification of the meteostation
            line.Append(NwrwKeywords.MeteostationIdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(meteostationId);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendSpecialAreasTo3bLine(StringBuilder line, int numberOfSpecialAreas,
            IList<NwrwSpecialArea> specialAreas)
        {
            if (numberOfSpecialAreas > 0)
            {
                AppendNumberOfSpecialAreasTo3bLine(line, numberOfSpecialAreas);
                AppendAllSpecialAreasTo3bLine(line, specialAreas);
            }
        }

        private void AppendNumberOfSpecialAreasTo3bLine(StringBuilder line, int numberOfSpecialAreas)
        {
            // 'na' + number of special areas with special inflow characteristics
            line.Append(NwrwKeywords.NumberOfSpecialAreasKey);
            line.Append(" ");
            line.Append(numberOfSpecialAreas);
            line.Append(" ");
        }

        private void AppendAllSpecialAreasTo3bLine(StringBuilder line, IList<NwrwSpecialArea> specialAreas)
        {
            // 'aa' + special area in m2 (for number of areas as specified after the 'na' keyword
            line.Append(NwrwKeywords.SpecialAreaKey);
            line.Append(" ");
            foreach (var specialArea in specialAreas)
            {
                line.Append(specialArea.Area);
                line.Append(" ");
            }
        }

        private void AppendClosingTagTo3bLine(StringBuilder line)
        {
            line.Append("nwrw");
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