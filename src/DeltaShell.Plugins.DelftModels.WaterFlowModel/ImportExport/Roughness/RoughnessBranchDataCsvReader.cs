using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using log4net;
using LumenWorks.Framework.IO.Csv;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    /// <summary>
    /// Importer for comma separated files containing roughness for branches
    /// Import result in a list of RoughessCsvBranchData records and import can be done without reference
    /// to a network.
    /// Use MergeIntoRoughnessSections to set the RoughessCsvBranchData into the roughness sections
    /// of a WaterFlowModel1D.
    /// </summary>
    public class RoughnessBranchDataCsvReader 
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoughnessBranchDataCsvReader));

      
        public IList<RoughessCsvBranchData> GetBranchData(string path)
        {
            var records = ReadCsvRecords(path);

            return ConvertToBranchData(records);
        }
        

        //TODO: factor this into a separate object ? Is this not already existing?
        /// <summary>
        /// Parses the reocrs in the CSV file and puts them 1 on 1 in an array
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IList<RoughnessCsvRecord> ReadCsvRecords(string path)
        {
            IList<RoughnessCsvRecord> roughnessCommaseparatedData = new List<RoughnessCsvRecord>();
            const char delimiter = ',';
            using (var textReader = new StreamReader(path))
            {
                using (var reader = new CsvReader(textReader, true /* firstRowIsHeaderRow */, delimiter))
                {
                    reader.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    //obtain the schema

                    var roughnessCSvSettings = new RoughnessCSvSettings();
        
                    var fieldHeaders = ((CsvReader)reader).GetFieldHeaders();
                    foreach (var fieldheader in fieldHeaders)
                    {
                        var processed = false;
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.BranchNameIndex, RoughnessCSvSettings.BranchNameHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.ChainageIndex, RoughnessCSvSettings.ChainageHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.RoughnessTypeIndex, RoughnessCSvSettings.RoughnessTypeHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.SectionTypeIndex, RoughnessCSvSettings.SectionTypeHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.RoughnessFunctionIndex, RoughnessCSvSettings.RoughnessFunctionHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.InterpolationTypeIndex, RoughnessCSvSettings.InterpolationTypeHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.NegativeIsPositiveIndex, RoughnessCSvSettings.NegativeIsPositiveHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.PositiveConstantIndex, RoughnessCSvSettings.PositiveConstantHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.PositiveQIndex, RoughnessCSvSettings.PositiveQHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.PositiveQRoughnessIndex, RoughnessCSvSettings.PositiveQRoughnessHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.PositiveHIndex, RoughnessCSvSettings.PositiveHHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.PositiveHRoughnessIndex, RoughnessCSvSettings.PositiveHRoughnessHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.NegativeConstantIndex, RoughnessCSvSettings.NegativeConstantHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.NegativeQIndex, RoughnessCSvSettings.NegativeQHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.NegativeQRoughnessIndex, RoughnessCSvSettings.NegativeQRoughnessHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.NegativeHIndex, RoughnessCSvSettings.NegativeHHeaderText);
                        processed |= CsvImporterHelper.CheckIndexInHeader(reader, fieldheader, roughnessCSvSettings.NegativeHRoughnessIndex, RoughnessCSvSettings.NegativeHRoughnessHeaderText);
                        if (!processed)
                        {
                            throw new ArgumentException(
                                string.Format("Unknown column header '{0}' in file {1}; import canceled.", fieldheader, path));
                        }
                    }

                    var currentField = 0;
                    try
                    {
                        while (((IDataReader)reader).Read())
                        {
                            currentField = 0;
                            var branchName = reader[roughnessCSvSettings.BranchNameIndex];
                            if ((branchName == null) || (branchName.Trim() == ""))
                            {
                                Log.InfoFormat("Skipped empty line {0}", reader.CurrentRecordIndex);
                                continue;
                            }

                            var roughnessCsvData = new RoughnessCsvRecord { BranchName = branchName };

                            currentField++;
                            roughnessCsvData.Chainage = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.ChainageIndex);
                            currentField++;
                            roughnessCsvData.RoughnessType = RoughnessTypeCsvConverter.Fromstring(reader[roughnessCSvSettings.RoughnessTypeIndex]);
                            currentField++;
                            roughnessCsvData.SectionType = reader[roughnessCSvSettings.SectionTypeIndex];
                            currentField++;
                            roughnessCsvData.RoughnessFunction = RoughnessFunctionCsvConverter.Fromstring(reader[roughnessCSvSettings.RoughnessFunctionIndex]);
                            currentField++;
                            roughnessCsvData.InterpolationType = (InterpolationType)Enum.Parse(typeof(InterpolationType), reader[roughnessCSvSettings.InterpolationTypeIndex]);
                            currentField++;
                            roughnessCsvData.NegativeIsPositive = RoughnessNegativeIsPositiveCsvConverter.Fromstring(reader[roughnessCSvSettings.NegativeIsPositiveIndex]);
                            currentField++;
                            roughnessCsvData.PositiveConstant = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.PositiveConstantIndex);
                            currentField++;
                            roughnessCsvData.PositiveQ = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.PositiveQIndex);
                            currentField++;
                            roughnessCsvData.PositiveQRoughness = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.PositiveQRoughnessIndex);
                            currentField++;
                            roughnessCsvData.PositiveH = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.PositiveHIndex);
                            currentField++;
                            roughnessCsvData.PositiveHRoughness = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.PositiveHRoughnessIndex);
                            currentField++;
                            roughnessCsvData.NegativeConstant = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.NegativeConstantIndex);
                            currentField++;
                            roughnessCsvData.NegativeQ = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.NegativeQIndex);
                            currentField++;
                            roughnessCsvData.NegativeQRoughness = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.NegativeQRoughnessIndex);
                            currentField++;
                            roughnessCsvData.NegativeH = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.NegativeHIndex);
                            currentField++;
                            roughnessCsvData.NegativeHRoughness = CsvImporterHelper.ParseToDouble(reader, roughnessCSvSettings.NegativeHRoughnessIndex);

                            roughnessCommaseparatedData.Add(roughnessCsvData);
                        }
                    }
                    catch (Exception exception)
                    {
                        var message = string.Format("Error parsing field[record={0}, column={1}] = '{2}' : {3}",
                                                    reader.CurrentRecordIndex, currentField, reader[currentField],
                                                    exception.Message);

                        throw new ArgumentException(message);
                    }
                }
            }
            return roughnessCommaseparatedData;
        }

        /// <summary>
        /// Processed the records from the csv an collect all records for a branch / sectiontype combination.
        /// The records for a branch / sectiontype are processed to a RoughessCsvBranchData object. This can 
        /// easily be put into a RoughnessSection.
        /// 
        /// Current assumption is a sorted CSV file.
        /// </summary>
        /// <param name="csvRecords"></param>
        /// <returns></returns>
        private static IList<RoughessCsvBranchData> ConvertToBranchData(IEnumerable<RoughnessCsvRecord> csvRecords)
        {
            string branchName = null;
            string sectionTypeName = null;
            var t = new List<RoughessCsvBranchData>();
            IList<RoughnessCsvRecord> tmpBranchData = new List<RoughnessCsvRecord>();

            //the record are sorted on branch. and the section type. 
            //since the sord order (probably is relevant we cannot do a group by?
            foreach (var row in csvRecords)
            {
                if ((row.BranchName != branchName) || (row.SectionType != sectionTypeName))
                {
                    //next branch
                    if (branchName!= null)
                    {
                        t.Add(GetBranchSectionTypeInfo(tmpBranchData));
                        tmpBranchData.Clear();
                    }
                    branchName = row.BranchName;
                    sectionTypeName = row.SectionType;
                }
                tmpBranchData.Add(row);
            }
            //do not add empty data
            if (tmpBranchData.Count != 0)
            {
                t.Add(GetBranchSectionTypeInfo(tmpBranchData));    
            }
            
            return t;
        }

        /// <summary>
        /// Puts all records for a branch / sectiontype combination in a RoughessCsvBranchData object.
        /// Per branch roughness if of 1 RoughnessType (Chezy, etc.) and 1 RoughnessFunction (Constant, Discharge,
        /// WaterLevel). Multiple chainages are allowed for all of RoughnessFunction.
        /// </summary>
        /// <param name="branchData"></param>
        /// <returns></returns>
        private static RoughessCsvBranchData GetBranchSectionTypeInfo(IList<RoughnessCsvRecord> branchData)
        {
            if (branchData.Count == 0)
            {
                return null;
            }
            var roughessImportedData = new RoughessCsvBranchData();
            roughessImportedData.BranchName = branchData[0].BranchName;
            roughessImportedData.SectionType = branchData[0].SectionType;
            roughessImportedData.Chainage = branchData[0].Chainage;
            roughessImportedData.RoughnessType = branchData[0].RoughnessType;
            switch (branchData[0].RoughnessFunction)
            {
                case RoughnessFunction.Constant:
                    roughessImportedData.ConstantRoughness = RoughnessBranchDataMerger.DefineConstantFunction();
                    branchData.ForEach(
                        bd =>
                        roughessImportedData.ConstantRoughness[bd.Chainage] =
                        bd.PositiveConstant);
                    break;
                case RoughnessFunction.FunctionOfQ:
                    roughessImportedData.RoughnessFunctionOfQ = RoughnessSection.DefineFunctionOfQ();
                    branchData.ForEach(
                        bd =>
                        roughessImportedData.RoughnessFunctionOfQ[bd.Chainage, bd.PositiveQ] =
                        bd.PositiveQRoughness);
                    break;
                case RoughnessFunction.FunctionOfH:
                    roughessImportedData.RoughnessFunctionOfH = RoughnessSection.DefineFunctionOfH();
                    branchData.ForEach(
                        bd =>
                        roughessImportedData.RoughnessFunctionOfH[bd.Chainage, bd.PositiveH] =
                        bd.PositiveHRoughness);
                    break;
            }
            return roughessImportedData;
        }
    }
}




