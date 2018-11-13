using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public abstract class RoughnessConverter
    {
        protected abstract RoughnessSection ReadRoughnessSection(IDelftIniCategory roughnessSectionCategory, IEnumerable<RoughnessSection> roughnessSections);

        public RoughnessSection Convert(IList<DelftIniCategory> categories, IHydroNetwork network, IEnumerable<RoughnessSection> roughnessSections, IList<string> errorMessages)
        {
            var roughnessSectionCategory = categories.FirstOrDefault(category => category.Name == RoughnessDataRegion.ContentIniHeader);
            var roughnessSection = ReadRoughnessSection(roughnessSectionCategory, roughnessSections);
            if (roughnessSection.Network == null) roughnessSection.Network = network;

            var fileReadingExceptions = ReadBranchRoughnessData(categories, network, roughnessSection);

            if (fileReadingExceptions.Count != 0)
            {
                var innerExceptionMessages =
                    fileReadingExceptions.Select(
                        fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format(
                    Resources.RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_, Environment.NewLine,
                    string.Join(Environment.NewLine, innerExceptionMessages)));
            }

            return roughnessSection;
        }

        private static IList<FileReadingException> ReadBranchRoughnessData(IList<DelftIniCategory> categories, IHydroNetwork network, RoughnessSection roughnessSection)
        {
            var readRoughnessBranchData = ReadRoughnessBranchData(network, categories);
            var definitionData = ReadDefinitionData(network, categories, readRoughnessBranchData);

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            foreach (var roughnessBranchData in readRoughnessBranchData)
            {
                AddBranchRoughnessDataToRoughnessSection(roughnessBranchData, definitionData, roughnessSection, fileReadingExceptions);
            }

            return fileReadingExceptions;
        }

        private static void AddBranchRoughnessDataToRoughnessSection(RoughnessBranchData roughnessBranchData, IList<RoughnessDefinitionData> definitionData, RoughnessSection roughnessSection, IList<FileReadingException> fileReadingExceptions)
        {
            try
            {
                var branch = roughnessBranchData.Branch;

                switch (roughnessBranchData.RoughnessFunctionType)
                {
                    case RoughnessFunction.FunctionOfQ:
                    {
                        var function = RoughnessSection.DefineFunctionOfQ();
                        FillFunctionWithTableData(function, definitionData, branch, roughnessBranchData);
                        roughnessSection.AddQRoughnessFunctionToBranch(branch, function);
                        roughnessSection.UpdateCoverageForFunction(branch, function,
                            roughnessBranchData.RoughnessType);
                        break;
                    }

                    case RoughnessFunction.FunctionOfH:
                    {
                        var function = RoughnessSection.DefineFunctionOfH();
                        FillFunctionWithTableData(function, definitionData, branch, roughnessBranchData);
                        roughnessSection.AddHRoughnessFunctionToBranch(branch, function);
                        roughnessSection.UpdateCoverageForFunction(branch, function,
                            roughnessBranchData.RoughnessType);
                        break;
                    }

                    case RoughnessFunction.Constant:
                    {
                        foreach (
                            var roughnessContantData in
                            definitionData.OfType<ConstantRoughnessDefinitionData>()
                                .Where(dd => dd.Branch == branch))
                        {
                            roughnessSection.RoughnessNetworkCoverage[
                                new NetworkLocation(branch, roughnessContantData.Chainage)] = new object[]
                                {roughnessContantData.Value, roughnessBranchData.RoughnessType};
                        }

                        break;
                    }
                    default:
                        throw new FileReadingException(Resources.RoughnessDataFileReader_ReadFile_adding_to_network_went_wrong_);
                }
            }
            catch (FileReadingException fileReadingException)
            {
                fileReadingExceptions.Add(new FileReadingException(Resources.RoughnessDataFileReader_ReadFile_Could_not_set_roughness_data_in_model, fileReadingException));
            }
        }

        private static void FillFunctionWithTableData(IFunction function, IList<RoughnessDefinitionData> roughnessDefinitionData, IBranch branch, RoughnessBranchData roughnessBranchData)
        {
            foreach (var definitionData in roughnessDefinitionData.OfType<QorHRoughnessDefinitionData>().Where(dd => dd.Branch == branch))
            {
                var branchData = roughnessBranchData as QorHRoughnessBranchData;
                if (branchData == null)
                    throw new FileReadingException(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed_);

                var levels = branchData.Levels;
                if (levels.Count != definitionData.Values.Count)
                    throw new FileReadingException(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed__values_count_doesn_t_match_the_defined_levels_count_);

                for (var index = 0; index < levels.Count; index++)
                {
                    var level = levels[index];
                    function[definitionData.Chainage, level] = definitionData.Values[index];
                }
            }
        }

        private static IList<RoughnessBranchData> ReadRoughnessBranchData(INetwork network, IList<DelftIniCategory> categories)
        {
            IList<RoughnessBranchData> branchData = new List<RoughnessBranchData>();
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            foreach (var branchCategory in categories.Where(category => category.Name == RoughnessDataRegion.BranchPropertiesIniHeader))
            {
                try
                {
                    var branchid = branchCategory.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
                    var branch = network.Branches.FirstOrDefault(b => b.Name == branchid);
                    if (branch == null)
                        throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model, branchid));
                    var branchRoughnessType = FrictionTypeConverter.ConvertToRoughnessFrictionType(branchCategory.ReadProperty<Friction>(RoughnessDataRegion.RoughnessType.Key));
                    var functionType = (RoughnessFunction)branchCategory.ReadProperty<int>(RoughnessDataRegion.FunctionType.Key);
                    switch (functionType)
                    {
                        case RoughnessFunction.Constant:
                            branchData.Add(new RoughnessBranchData
                            {
                                Branch = branch,
                                RoughnessType = branchRoughnessType,
                                RoughnessFunctionType = functionType
                            });
                            break;
                        case RoughnessFunction.FunctionOfQ:
                        case RoughnessFunction.FunctionOfH:
                            var numLevels = branchCategory.ReadProperty<int>(RoughnessDataRegion.NumberOfLevels.Key);
                            var levels = branchCategory.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Levels.Key);
                            if (levels.Count != numLevels)
                                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_The_length_of_the_number_of_levels___0___and_the_defined_number_of_levels_of_the_branch_property__1__are_not_the_same_of_branch_properties____2__, levels.Count, numLevels, branch.Name));
                            branchData.Add(new QorHRoughnessBranchData
                            {
                                Branch = branch,
                                RoughnessType = branchRoughnessType,
                                RoughnessFunctionType = functionType,
                                Levels = levels
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_Could_not_read_roughness_branch_data,
                        fileReadingException));
                }
            }

            if (fileReadingExceptions.Count == 0) return branchData;

            var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
            throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_While_reading_branches_for_roughness_section_an_error_occured___0___1_, Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
        }

        private static IList<RoughnessDefinitionData> ReadDefinitionData(INetwork network, IList<DelftIniCategory> categories, IList<RoughnessBranchData> branchData)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            IList<RoughnessDefinitionData> definitionData = new List<RoughnessDefinitionData>();
            foreach (var definitionCategory in categories.Where(category => category.Name == RoughnessDataRegion.DefinitionIniHeader))
            {
                try
                {
                    var branchid = definitionCategory.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
                    var branch = network.Branches.FirstOrDefault(b => b.Name == branchid);
                    if (branch == null)
                        throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model, branchid));

                    var chainage = definitionCategory.ReadProperty<double>(SpatialDataRegion.Chainage.Key);
                    var roughnessBranchData = branchData.FirstOrDefault(bd => bd.Branch == branch);
                    if (roughnessBranchData == null)
                        throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model, branchid));

                    switch (roughnessBranchData.RoughnessFunctionType)
                    {
                        //read definitions for this type
                        case RoughnessFunction.Constant:
                            var value = definitionCategory.ReadProperty<double>(SpatialDataRegion.Value.Key);
                            definitionData.Add(new ConstantRoughnessDefinitionData
                            {
                                Branch = branch,
                                Chainage = chainage,
                                Value = value
                            });
                            break;
                        case RoughnessFunction.FunctionOfQ:
                        case RoughnessFunction.FunctionOfH:
                            var values = definitionCategory.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Values.Key);
                            definitionData.Add(new QorHRoughnessDefinitionData
                            {
                                Branch = branch,
                                Chainage = chainage,
                                Values = values
                            });
                            break;
                        default:
                            throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadDefinitionData_Couldn_t_read_roughness_section_with_a_function_type_of____0_, roughnessBranchData.RoughnessFunctionType));
                    }
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException(Resources.RoughnessDataFileReader_ReadDefinitionData_Could_not_read_roughness_definition_data, fileReadingException));
                }
            }

            if (fileReadingExceptions.Count == 0) return definitionData;

            var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
            throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_, Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
        }

        private class RoughnessBranchData
        {
            public IBranch Branch { get; set; }
            public RoughnessType RoughnessType { get; set; }
            public RoughnessFunction RoughnessFunctionType { get; set; }
        }
        private class QorHRoughnessBranchData : RoughnessBranchData
        {
            public IList<double> Levels { get; set; }
        }

        private abstract class RoughnessDefinitionData
        {
            public IBranch Branch { get; set; }
            public double Chainage { get; set; }
        }
        private class ConstantRoughnessDefinitionData : RoughnessDefinitionData
        {
            public double Value { get; set; }
        }
        private class QorHRoughnessDefinitionData : RoughnessDefinitionData
        {
            public IList<double> Values { get; set; }
        }
    }
}
