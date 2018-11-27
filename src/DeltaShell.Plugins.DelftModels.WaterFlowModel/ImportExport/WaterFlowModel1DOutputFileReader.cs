using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Store1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DOutputFileReader : Output1DFileReader<LocationMetaData, WaterFlow1DTimeDependentVariableMetaData>
    {
        public WaterFlowModel1DOutputFileReader()
        {
            timeVariableNameInNetCDFFile = WaterFlowModel1DOutputFileConstants.VariableNames.Time;
            timeDimensionNameInNetCdfFile = WaterFlowModel1DOutputFileConstants.DimensionKeys.Time;
            cfRoleAttributeNameInNetCdfFile = WaterFlowModel1DOutputFileConstants.AttributeKeys.CfRole;
            cfRoleAttributeValueInNetCdfFile = WaterFlowModel1DOutputFileConstants.AttributeValues.CfRole;
            branchidVariableNameInNetCDFFile = WaterFlowModel1DOutputFileConstants.VariableNames.BranchId;
            chainageVariableNameInNetCDFFile = WaterFlowModel1DOutputFileConstants.VariableNames.Chainage;
            xCoordinateVariableNameInNetCDFFile = WaterFlowModel1DOutputFileConstants.VariableNames.XCoordinate;
            yCoordinateVariableNameInNetCDFFile = WaterFlowModel1DOutputFileConstants.VariableNames.YCoordinate;
            unitsAttributeKeyNameInNetCdfFile = WaterFlowModel1DOutputFileConstants.AttributeKeys.Units;
            timeVariableUnitValuePrefixInNetCdfFile = WaterFlowModel1DOutputFileConstants.TimeVariableUnitValuePrefix;
            dateTimeFormat = WaterFlowModel1DOutputFileConstants.DateTimeFormat;
            longNameAttributeKeyNameInNetCdfFile = WaterFlowModel1DOutputFileConstants.AttributeKeys.LongName;
        }

        private string aggregationOptionAttributeKeyNameInNetCdfFile = WaterFlowModel1DOutputFileConstants.AttributeKeys.AggregationOption;

        public override OutputFile1DMetaData<LocationMetaData, WaterFlow1DTimeDependentVariableMetaData> ReadMetaData(string path, bool doValidation = true)
        {
            if (doValidation)
            {
                var validationReport = new WaterFlowModel1DOutputFileValidator().Validate(path);
                if (validationReport.Severity() == ValidationSeverity.Error)
                {
                    var errorMessage = string.Format("Failed to read water flow model 1d output file meta data: {0}",
                        string.Join("\n", Enumerable.Where<ValidationIssue>(validationReport.GetAllIssuesRecursive(), i => i.Severity == ValidationSeverity.Error)
                            .Select(i => string.Format("\t{0}: {1}", i.Subject, i.Message)).ToArray()));

                    throw new FileReadingException(errorMessage);
                }
            }

            return base.ReadMetaData(path, doValidation);
        }

        protected override WaterFlow1DTimeDependentVariableMetaData ParseVariableMetaData(string variableName, Dictionary<string, object> attributes)
        {
            var waterFlow1DTimeDependentVariableMetaData = base.ParseVariableMetaData(variableName, attributes);
            
           var aggregationOption = attributes.FirstOrDefault(a => a.Key == aggregationOptionAttributeKeyNameInNetCdfFile).Value;

           AggregationOptions option = AggregationOptions.Current;// default to Current
           if (aggregationOption != null)
           {
               var aggregationOptionString = aggregationOption.ToString();
               aggregationOptionString = aggregationOptionString.First().ToString().ToUpper() + string.Join("", aggregationOptionString.Skip(1));
               if (!Enum.TryParse(aggregationOptionString, true, out option))
               {
                   option = AggregationOptions.Current;// default to Current
               }
           }
            waterFlow1DTimeDependentVariableMetaData.AggregationOption = option;
            return waterFlow1DTimeDependentVariableMetaData;
        }
    }
}
