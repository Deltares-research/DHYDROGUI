using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekSaltLocalDispersionReader : SobekReader<SobekSaltLocalDispersion>
    {
        // This file contains the location-dependant dispersion information. These are linked to a branch-id. This file is only 
        // necessary dispersion is entered as a function of place, in the gld-file. 
        // DSPN id '1' nm 'Dispersietak1' ci '1' ty 0 f1 11 f3 12 f4 13 dspn
        // or
        // DSPN id '2' nm 'Dispersietak2' ci '3' ty 2 dl lt 
        // TBLE .. 
        // tble 
        // dspn
        //         
        // Where:
        // id  = id dispersion
        // nm  = name 
        // ci  = carrier id (=branch id)
        // ty  = type of dispersion function:
        // 0 = constant
        // 1 = f(time)
        // 2 = f(place)
        // f1 tm f4 = see gld-file
        // dl lt  = dispersion table as function of the location on the branch
        // column 1 = location
        // the other columns any one of f1,f2,f3,f4 depending on the chosen dispersion formulation in the gld-file.

        public override IEnumerable<SobekSaltLocalDispersion> Parse(string text)
        {
            const string structurePattern = @"(DSPN (?'text'.*?) dspn)";

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, text))
            {
                SobekSaltLocalDispersion flowInitialCondition = GetLocalDispersion(structureMatch.Value);
                if (flowInitialCondition != null)
                {
                    yield return flowInitialCondition;
                }
            }
        }

        public static SobekSaltLocalDispersion GetLocalDispersion(string record)
        {
            var sobekSaltLocalDispersion = new SobekSaltLocalDispersion();

            sobekSaltLocalDispersion.Id = RegularExpression.ParseFieldAsString("id", record);
            sobekSaltLocalDispersion.Name = RegularExpression.ParseFieldAsString("nm", record);
            sobekSaltLocalDispersion.BranchId = RegularExpression.ParseFieldAsString("ci", record);
            sobekSaltLocalDispersion.DispersionType = (DispersionType) RegularExpression.ParseFieldAsInt("ty", record);
            sobekSaltLocalDispersion.F1 = RegularExpression.ParseFieldAsDouble("f1", record);
            sobekSaltLocalDispersion.F2 = RegularExpression.ParseFieldAsDouble("f2", record);
            sobekSaltLocalDispersion.F3 = RegularExpression.ParseFieldAsDouble("f3", record);
            sobekSaltLocalDispersion.F4 = RegularExpression.ParseFieldAsDouble("f4", record);
            if (sobekSaltLocalDispersion.DispersionType == DispersionType.FunctionOfPlace)
            {
                sobekSaltLocalDispersion.Data = SobekDataTableReader.GetTable(record, FunctionOfPlaceTable);
            }
            if (sobekSaltLocalDispersion.DispersionType == DispersionType.FunctionOfTime)
            {
                sobekSaltLocalDispersion.Data = SobekDataTableReader.GetTable(record, FunctionOfTimeTable);
            }
            return sobekSaltLocalDispersion;
        }

        private static DataTable FunctionOfPlaceTable
        {
            get
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("Location", typeof(double));
                dataTable.Columns.Add("F1", typeof(double));
                dataTable.Columns.Add("F2", typeof(double));
                dataTable.Columns.Add("F3", typeof(double));
                dataTable.Columns.Add("F4", typeof(double));
                return dataTable;
            }
        }

        private static DataTable FunctionOfTimeTable
        {
            get
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("Time", typeof(DateTime));
                dataTable.Columns.Add("F1", typeof(double));
                dataTable.Columns.Add("F2", typeof(double));
                dataTable.Columns.Add("F3", typeof(double));
                dataTable.Columns.Add("F4", typeof(double));
                return dataTable;
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "dspn";
        }
    }
}