using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwFileWriter : NGHSFileBase
    {
        private const string NWRW3BFILENAME = "pluvius.3b";
        private const string NWRWALGFILENAME = "pluvius.alg";

        public void WriteNwrwFiles(IHydroModel model, string path)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null || path == null) return;

            WriteNwrw3bFile(rrModel, path);
            WriteNwrwAlgFile(rrModel, path);
            //WriteNwrwDwaFile();
            //WriteNwrwTableFile();
        }

        private void WriteNwrwAlgFile(RainfallRunoffModel rrModel, string path)
        {
            if (rrModel == null || path == null) return;
            var filePath = Path.Combine(Path.GetFullPath(path), NWRWALGFILENAME);
            OpenOutputFile(filePath);
            try
            {
                
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteNwrw3bFile(RainfallRunoffModel rrModel, string path)
        {
            if (rrModel == null || path == null) return;

            var filePath = Path.Combine(Path.GetFullPath(path), NWRW3BFILENAME);

            OpenOutputFile(filePath);
            try
            {
                foreach (NwrwData nwrwData in rrModel.GetAllModelData().OfType<NwrwData>())
                {
                    StringBuilder line = CreateNwrwPropertiesLine(nwrwData);
                    WriteLine(line.ToString());
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private StringBuilder CreateNwrwPropertiesLine(NwrwData nwrwData)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagToPropertiesLine(line); // 'NWRW'
            AppendIdToPropertiesLine(line, nwrwData.NodeOrBranchId); // 'id'
            AppendSurfaceLevelToPropertiesLine(line, nwrwData.LateralSurface); // 'sl'
            AppendAreaToPropertiesLine(line, nwrwData.SurfaceLevelDict); // 'ar'
            AppendDryWeatherFlowsToPropertiesLine(line, nwrwData.DryWeatherFlows); // 'np' 'dw' 'np2' 'dw2'
            AppendMeteoStationIdToPropertiesLine(line, nwrwData.MeteoStationId); // 'ms'
            AppendSpecialAreasToPropertiesLine(line, nwrwData.NumberOfSpecialAreas, nwrwData.SpecialAreas); // 'na'
            AppendClosingTagToPropertiesLine(line); // 'nwrw'

            return line;
        }

        

        private void AppendOpeningTagToPropertiesLine(StringBuilder line)
        {
            // 'NWRW' opening keyword
            line.Append(NwrwKeywords.NwrwOpeningKey);
            line.Append(" ");
        }

        private void AppendIdToPropertiesLine(StringBuilder line, string id)
        {
            // 'id' + node identification
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(id);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendSurfaceLevelToPropertiesLine(StringBuilder line, double surfaceLevel)
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

        private void AppendAreaToPropertiesLine(StringBuilder line, IDictionary<NwrwSurfaceType, double> surfaceLevelDict)
        {
            // 'ar' + area (12 types) as combination of 3 kind of slopes and 4 types of surfaces
            line.Append(NwrwKeywords.AreaKey);
            line.Append(" ");

            NwrwSurfaceType[] surfaceTypesInCorrectOrder =
            {
                NwrwSurfaceType.ClosedPavedWithSlope,   // a1
                NwrwSurfaceType.ClosedPavedFlat,        // a2
                NwrwSurfaceType.ClosedPavedFlatStretch, // a3
                NwrwSurfaceType.OpenPavedWithSlope,     // a4
                NwrwSurfaceType.OpenPavedFlat,          // a5
                NwrwSurfaceType.OpenPavedFlatStretched, // a6
                NwrwSurfaceType.RoofWithSlope,          // a7
                NwrwSurfaceType.RoofFlat,               // a8
                NwrwSurfaceType.RoofFlatStretched,      // a9
                NwrwSurfaceType.UnpavedWithSlope,       // a10
                NwrwSurfaceType.UnpavedFlat,            // a11
                NwrwSurfaceType.UnpavedFlatStretched    // a12
            };

            foreach (NwrwSurfaceType surfaceType in surfaceTypesInCorrectOrder)
            {
                if (surfaceLevelDict.ContainsKey(surfaceType))
                {
                    line.Append(surfaceLevelDict[surfaceType]);

                }
                else
                {
                    line.Append("0");
                }
                line.Append(" ");
            }
        }

        private void AppendDryWeatherFlowsToPropertiesLine(StringBuilder line,
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

        private void AppendMeteoStationIdToPropertiesLine(StringBuilder line, string meteostationId)
        {
            // 'ms' + identification of the meteostation
            line.Append(NwrwKeywords.MeteostationIdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(meteostationId);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendSpecialAreasToPropertiesLine(StringBuilder line, int numberOfSpecialAreas, IList<NwrwSpecialArea> specialAreas)
        {
            if (numberOfSpecialAreas > 0)
            {
                AppendNumberOfSpecialAreasToPropertiesLine(line, numberOfSpecialAreas);
                AppendAllSpecialAreasToPropertiesLine(line, specialAreas);
            }
        }

        private void AppendNumberOfSpecialAreasToPropertiesLine(StringBuilder line, int numberOfSpecialAreas)
        {
            // 'na' + number of special areas with special inflow characteristics
            line.Append(NwrwKeywords.NumberOfSpecialAreasKey);
            line.Append(" ");
            line.Append(numberOfSpecialAreas);
            line.Append(" ");
        }

        private void AppendAllSpecialAreasToPropertiesLine(StringBuilder line, IList<NwrwSpecialArea> specialAreas)
        {
            // 'aa' + special area in m2 (for number of areas as specified after the 'na' keyword
            line.Append(NwrwKeywords.SpecialAreaKey);
            line.Append(" ");
            foreach (NwrwSpecialArea specialArea in specialAreas)
            {
                line.Append(specialArea.Area);
                line.Append(" ");
            }
        }

        private void AppendClosingTagToPropertiesLine(StringBuilder line)
        {
            line.Append("nwrw");
        }
    }
}
