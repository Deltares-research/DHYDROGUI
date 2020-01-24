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

        private const string DFEAULT_GENERAL_ID = "-1";
        private const string DEFAULT_INFILTRATION_FROM_DEPRESSIONS = "1";
        private const string DEFAULT_INFILTRATION_FROM_RUNOFF = "0";
        private NwrwSurfaceType[] SurfaceTypesInCorrectOrder { get; } =
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

        public void WriteNwrwFiles(IHydroModel model, string path)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null || path == null) return;

            WriteNwrw3bFile(rrModel, path);
            WriteNwrwAlgFile(rrModel, path);
            //WriteNwrwDwaFile();
            //WriteNwrwTableFile();
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
                    StringBuilder line = CreateNwrw3bLine(nwrwData);
                    WriteLine(line.ToString());
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteNwrwAlgFile(RainfallRunoffModel rrModel, string path)
        {
            if (rrModel == null || path == null) return;
            var filePath = Path.Combine(Path.GetFullPath(path), NWRWALGFILENAME);
            OpenOutputFile(filePath);
            try
            {
                StringBuilder line = CreateNwrwAlgLine(rrModel.NwrwDefinitions);
                WriteLine(line.ToString());
            }
            finally
            {
                CloseOutputFile();
            }
        }
        

        #region .alg
        private StringBuilder CreateNwrwAlgLine(IList<NwrwDefinition> nwrwDefinitions)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagToAlgLine(line);
            AppendIdToAlgLine(line);
            AppendNameToAlgLine(line);
            AppendRunoffDelayFactorToAlgLine(line, nwrwDefinitions);
            AppendMaximumStorageToAlgLine(line, nwrwDefinitions);
            AppendMaximumInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendMinimumInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendDecreaseInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendIncreaseInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendInfiltrationFromDepressionToAlgLine(line, nwrwDefinitions);
            AppendInfiltrationFromRunoffToAlgLine(line, nwrwDefinitions);
            AppendClosingTagToAlgLine(line);

            return line;
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

        private void AppendNameToAlgLine(StringBuilder line)
        {
            // name
            line.Append(NwrwKeywords.NameKey);
            line.Append(" ");
            line.Append("'");
            line.Append(String.Empty); // empty
            line.Append("'");
            line.Append(" ");
        }

        private void AppendRunoffDelayFactorToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // runoff-delay factor for 3 types of slopes (with slope, flat, flat stretched)
            line.Append(NwrwKeywords.RunoffDelayFactor);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.RunoffSlope);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedFlat))?.RunoffSlope);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedFlatStretch))?.RunoffSlope);
            line.Append(" ");
        }

        private void AppendMaximumStorageToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // maximum storage for 12 types
            line.Append(NwrwKeywords.MaximumStorage);
            line.Append(" ");
            foreach (NwrwSurfaceType nwrwSurfaceType in SurfaceTypesInCorrectOrder)
            {
                line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(nwrwSurfaceType))?.SurfaceStorage);
                line.Append(" ");
            }
        }

        private void AppendMaximumInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // maximum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.MaximumInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
        }

        private void AppendMinimumInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // minimum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.MinimumInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
        }

        private void AppendDecreaseInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // decrease in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.DecreaseInInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
        }

        private void AppendIncreaseInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // increase in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.IncreaseInInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
        }

        private void AppendInfiltrationFromDepressionToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
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

        #endregion
        
        #region .3b
        private StringBuilder CreateNwrw3bLine(NwrwData nwrwData)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagTo3bLine(line); // 'NWRW'
            AppendIdTo3bLine(line, nwrwData.NodeOrBranchId); // 'id'
            AppendSurfaceLevelTo3bLine(line, nwrwData.LateralSurface); // 'sl'
            AppendAreaTo3bLine(line, nwrwData.SurfaceLevelDict); // 'ar'
            AppendDryWeatherFlowsTo3bLine(line, nwrwData.DryWeatherFlows); // 'np' 'dw' 'np2' 'dw2'
            AppendMeteoStationIdTo3bLine(line, nwrwData.MeteoStationId); // 'ms'
            AppendSpecialAreasTo3bLine(line, nwrwData.NumberOfSpecialAreas, nwrwData.SpecialAreas); // 'na'
            AppendClosingTagTo3bLine(line); // 'nwrw'

            return line;
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

            

            foreach (NwrwSurfaceType surfaceType in SurfaceTypesInCorrectOrder)
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

        private void AppendSpecialAreasTo3bLine(StringBuilder line, int numberOfSpecialAreas, IList<NwrwSpecialArea> specialAreas)
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
            foreach (NwrwSpecialArea specialArea in specialAreas)
            {
                line.Append(specialArea.Area);
                line.Append(" ");
            }
        }

        private void AppendClosingTagTo3bLine(StringBuilder line)
        {
            line.Append("nwrw");
        }
        #endregion
    }
}
