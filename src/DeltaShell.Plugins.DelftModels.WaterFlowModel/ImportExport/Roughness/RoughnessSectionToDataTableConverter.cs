using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RoughnessSectionToDataTableConverter
    {
        public DataTable GetDataTable(IEnumerable<RoughnessSection> roughnessSections)
        {
            var table = GetCsvRoughnessSchemaTable();
            
            var network = roughnessSections.First().RoughnessNetworkCoverage.Network;

            foreach (var branch in network.Branches)
            {
                foreach (var roughnessSection in roughnessSections)
                {
                    var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);

                    if (roughnessFunctionType == RoughnessFunction.FunctionOfQ)
                    {
                        AddFunctionOfQorHToTable(true, roughnessSection, roughnessSection.FunctionOfQ(branch), branch, table);
                        continue;
                    }
                    if (roughnessFunctionType == RoughnessFunction.FunctionOfH)
                    {
                        AddFunctionOfQorHToTable(false, roughnessSection, roughnessSection.FunctionOfH(branch), branch, table);
                        continue;
                    }

                    AddConstantToTable(roughnessSection, branch, table);
                }
            }
            return table;
        }

        /// <summary>
        /// create cross section schema for csv
        /// </summary>
        /// <returns></returns>
        private static DataTable GetCsvRoughnessSchemaTable()
        {
            var table = new DataTable();//TODO: why not use a typed datatable to specify the iso this settings stuff.
            var t = new RoughnessCSvSettings();
            foreach (var roughnessCSvSetting in t)
            {
                table.Columns.Add(roughnessCSvSetting.HeaderText, roughnessCSvSetting.Type);
            }
            return table;
        }

        private static void AddConstantToTable(RoughnessSection roughnessSection, IBranch branch, DataTable table)
        {
            var networkLocations = roughnessSection.RoughnessNetworkCoverage.Locations.Values.Where(nl => nl.Branch == branch);
            
            foreach (var networkLocation in networkLocations)
            {
                table.Rows.Add(new object[]
                                   {
                                       branch.Name, // Name
                                       networkLocation.Chainage, // Chainage
                                       RoughnessTypeCsvConverter.ToString(roughnessSection.EvaluateRoughnessType(new NetworkLocation(branch, 0))), // RoughnessType
                                       roughnessSection.Name,  // SectionType
                                       RoughnessFunction.Constant, //Dependance 
                                       InterpolationType.Linear, // constant? Interpolation
                                       RoughnessNegativeIsPositiveCsvConverter.ToString(true),//Pos/neg 
                                       (double)roughnessSection.RoughnessNetworkCoverage[networkLocation], // R_pos_constant
                                       null, // Q_pos
                                       null, // R_pos_f(Q)
                                       null, //H_pos
                                       null, //R_pos__f(h)
                                       null, //R_neg_constant
                                       null, //Q_neg
                                       null, //R_neg_f(Q)
                                       null, //H_neg
                                       null //R_neg_f(h)
                                   });
            }
        }

        private static void AddFunctionOfQorHToTable(bool q, RoughnessSection roughnessSection, IFunction functionOfQorH, IBranch branch, DataTable table)
        {
            var chainages = functionOfQorH.Arguments[0].Values;
            var qOrHs = functionOfQorH.Arguments[1].Values;

            foreach (var chainage in chainages)
            {
                foreach (var qOrH in qOrHs)
                {
                    table.Rows.Add(new[]
                                       {
                                           branch.Name, // Name
                                           (double)chainage, // Chainage
                                           RoughnessTypeCsvConverter.ToString(roughnessSection.EvaluateRoughnessType(new NetworkLocation(branch, 0))), // RoughnessType
                                           roughnessSection.Name,  // SectionType
                                           RoughnessFunctionCsvConverter.ToString(q ? RoughnessFunction.FunctionOfQ : RoughnessFunction.FunctionOfH),
                                           functionOfQorH.Arguments[0].InterpolationType, // Interpolation
                                           RoughnessNegativeIsPositiveCsvConverter.ToString(true),//Pos/neg 
                                           null,//"", // R_pos_constant
                                           q ? qOrH : null, // Q_pos
                                           q ? functionOfQorH[chainage, qOrH] : null, // R_pos_f(Q)
                                           !q ? qOrH : null, //H_pos
                                           !q ? functionOfQorH[chainage, qOrH] : null, //R_pos__f(h)
                                           null, //R_neg_constant
                                           null, //Q_neg
                                           null, //R_neg_f(Q)
                                           null, //H_neg
                                           null //R_neg_f(h)
                                       });
                }
            }
        }

    }
}