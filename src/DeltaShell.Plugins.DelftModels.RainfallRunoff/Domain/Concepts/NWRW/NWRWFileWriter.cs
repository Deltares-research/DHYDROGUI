using System;
using System.Collections.Generic;
using System.Text;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    public class NWRWFileWriter : NGHSFileBase
    {
        public void WriteNWRWFile(IEnumerable<NWRWData> nwrwDataList, string filePath, bool append = false)
        {
            OpenOutputFile(filePath, append);
            try
            {
                foreach (var nwrwData in nwrwDataList)
                {
                    WriteNWRWProperties(nwrwData);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteNWRWProperties(NWRWData nwrwData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(AddOpeningTag());
            sb.Append(AddIdProperty(nwrwData));
            sb.Append(AddSurfaceLevelProperty(nwrwData));
            sb.Append(AddSpecialAreas(nwrwData));
            sb.Append(AddAreaProperty(nwrwData));
            sb.Append(AddNumberOfPeopleProperty(nwrwData));
            sb.Append(AddDryWeatherFlowIdProperty(nwrwData));
            sb.Append(AddMeteostationIdProperty(nwrwData));
            sb.Append(AddClosingTag());

            WriteLine(sb.ToString());
        }

        private string AddClosingTag()
        {
            return $"{NWRWRegion.NWRWKey}";
        }

        private string AddSpecialAreas(NWRWData nwrwData)
        {
            if (nwrwData.NumberOfSpecialAreas == 0) return String.Empty;
            
            StringBuilder sb = new StringBuilder();
            sb.Append($"{NWRWRegion.NumberOfSpecialAreasKey} {nwrwData.NumberOfSpecialAreas} ");
            sb.Append($"{NWRWRegion.SpecialAreaKey} ");

            for (int i = 0; i < nwrwData.NumberOfSpecialAreas; i++)
            {
                sb.Append($"{nwrwData.SpecialAreas[i].Area} ");
            }
            sb.Append($"{NWRWRegion.SpecialInflowReferenceKey} ");
            for (int i = 0; i < nwrwData.NumberOfSpecialAreas; i++)
            {
                sb.Append($"'{nwrwData.SpecialAreas[i].SpecialInflowReference}' ");
            }

            return sb.ToString();
        }

        private string AddMeteostationIdProperty(NWRWData nwrwData)
        {
            return $"{NWRWRegion.MeteostationIdKey} '{nwrwData.MeteoStationId}' ";
        }

        private string AddDryWeatherFlowIdProperty(NWRWData nwrwData)
        {
            return $"{NWRWRegion.DryWeatherFlowIdKey} '{nwrwData.DryWeatherFlowId}' ";
        }

        private string AddNumberOfPeopleProperty(NWRWData nwrwData)
        {
            return $"{NWRWRegion.NumberOfPeopleKey} {nwrwData.NumberOfPeople} ";
        }

        private string AddAreaProperty(NWRWData nwrwData)
        {
            return 
                $"{NWRWRegion.AreaKey} {nwrwData.ClosedPavedWithSlope} {nwrwData.ClosedPavedFlat} {nwrwData.ClosedPavedFlatStretched} " +
                $"{nwrwData.OpenPavedWithSlope} {nwrwData.OpenPavedFlat} {nwrwData.OpenPavedFlatStretched} {nwrwData.RoofWithSlope} {nwrwData.RoofFlat} " +
                $"{nwrwData.RoofFlatStretched} {nwrwData.UnpavedWithSlope} {nwrwData.UnpavedFlat} {nwrwData.UnpavedFlatStretched} ";
        }

        private string AddSurfaceLevelProperty(NWRWData nwrwData)
        {
            return $"{NWRWRegion.SurfaceLevelKey} {nwrwData.SurfaceLevel} ";
        }

        private string AddIdProperty(NWRWData nwrwData)
        {
            return $"{NWRWRegion.IdKey} '{nwrwData.NWRWDataId}' ";
        }

        private string AddOpeningTag()
        {
            return $"{NWRWRegion.NWRWKey.ToUpper()} ";
        }
    }
}
