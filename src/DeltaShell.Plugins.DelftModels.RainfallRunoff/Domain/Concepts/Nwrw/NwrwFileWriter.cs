using System;
using System.Collections.Generic;
using System.Text;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwFileWriter : NGHSFileBase
    {
        public void WriteNwrwFile(IEnumerable<CatchmentModelData> nwrwDataList, string filePath, bool append = false)
        {
            OpenOutputFile(filePath, append);
            try
            {
                foreach (var catchmentModelData in nwrwDataList)
                {
                    var nwrwData = catchmentModelData as NwrwData;
                    if (nwrwData == null) return; 
                    WriteNwrwProperties(nwrwData);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteNwrwProperties(NwrwData nwrwData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(AddOpeningTag());
            sb.Append(AddIdProperty(nwrwData));
            //sb.Append(AddSurfaceLevelProperty(nwrwData));
            sb.Append(AddSpecialAreas(nwrwData));
            //sb.Append(AddAreaProperty(nwrwData));
            sb.Append(AddNumberOfPeopleProperty(nwrwData));
            sb.Append(AddDryWeatherFlowIdProperty(nwrwData));
            sb.Append(AddMeteostationIdProperty(nwrwData));
            sb.Append(AddClosingTag());

            WriteLine(sb.ToString());
        }

        private string AddClosingTag()
        {
            return $"{NwrwRegion.NwrwKey}";
        }

        private string AddSpecialAreas(NwrwData nwrwData)
        {
            if (nwrwData.NumberOfSpecialAreas == 0) return String.Empty;
            
            StringBuilder sb = new StringBuilder();
            sb.Append($"{NwrwRegion.NumberOfSpecialAreasKey} {nwrwData.NumberOfSpecialAreas} ");
            sb.Append($"{NwrwRegion.SpecialAreaKey} ");

            for (int i = 0; i < nwrwData.NumberOfSpecialAreas; i++)
            {
                sb.Append($"{nwrwData.SpecialAreas[i].Area} ");
            }
            sb.Append($"{NwrwRegion.SpecialInflowReferenceKey} ");
            for (int i = 0; i < nwrwData.NumberOfSpecialAreas; i++)
            {
                sb.Append($"'{nwrwData.SpecialAreas[i].SpecialInflowReference}' ");
            }

            return sb.ToString();
        }

        private string AddMeteostationIdProperty(NwrwData nwrwData)
        {
            return $"{NwrwRegion.MeteostationIdKey} '{nwrwData.MeteoStationName}' ";
        }

        private string AddDryWeatherFlowIdProperty(NwrwData nwrwData)
        {
            return $"{NwrwRegion.DryWeatherFlowIdKey} '{nwrwData.DryWeatherFlowIdInhabitant}' ";
        }

        private string AddNumberOfPeopleProperty(NwrwData nwrwData)
        {
            return $"{NwrwRegion.NumberOfPeopleKey} {nwrwData.NumberOfPeople} ";
        }

        //private string AddAreaProperty(CatchmentModelData nwrwData)
        //{
        //    return 
        //        $"{NwrwRegion.AreaKey} {nwrwData.ClosedPavedWithSlope} {nwrwData.ClosedPavedFlat} {nwrwData.ClosedPavedFlatStretched} " +
        //        $"{nwrwData.OpenPavedWithSlope} {nwrwData.OpenPavedFlat} {nwrwData.OpenPavedFlatStretched} {nwrwData.RoofWithSlope} {nwrwData.RoofFlat} " +
        //        $"{nwrwData.RoofFlatStretched} {nwrwData.UnpavedWithSlope} {nwrwData.UnpavedFlat} {nwrwData.UnpavedFlatStretched} ";
        //}

        //private string AddSurfaceLevelProperty(CatchmentModelData nwrwData)
        //{
        //    return $"{NwrwRegion.SurfaceLevelKey} {nwrwData.SurfaceLevel} ";
        //}

        private string AddIdProperty(CatchmentModelData nwrwData)
        {
            return $"{NwrwRegion.IdKey} '{nwrwData.Name}' ";
        }

        private string AddOpeningTag()
        {
            return $"{NwrwRegion.NwrwKey.ToUpper()} ";
        }
    }
}
