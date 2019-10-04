using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileReaders.Roughness
{
    
    public static class RoughnessDataFileReader
    {
        public static void ReadFile(string filename, INetwork network, IList<RoughnessSection> RoughnessSections, bool isCalibratedRoughness = false)
        {
            if (!File.Exists(filename)) throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist_, filename));
            var categories = new DelftIniReader().ReadDelftIniFile(filename);
            if (categories.Count == 0) throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadFile_Could_not_read_file__0__properly__it_seems_empty, filename));
            var contentSections = categories.Where(category => category.Name == RoughnessDataRegion.GlobalIniHeader).ToList();
            if (contentSections.Count() > 1 && contentSections.Any()) throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadFile_Could_not_read_content_section__0__properly, filename));
            
            var roughnessSection = ReadRoughnessSection(network, RoughnessSections, contentSections[0], isCalibratedRoughness);
            
            var readRoughnessBranchData = ReadRoughnessBranchData(network, categories);

            var definitionData = ReadDefinitionData(network, categories, readRoughnessBranchData);


            //Reading went fine add to the model now!
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            foreach (var roughnessBranchData in readRoughnessBranchData)
            {
                try
                {
                    var branch = roughnessBranchData.Branch;

                    switch (roughnessBranchData.RoughnessFunctionType)
                    {
                        case RoughnessFunction.FunctionOfQ:
                        {
                            var function = RoughnessSection.DefineFunctionOfQ();
                            function.FillFunctionWithTableData(definitionData, branch, roughnessBranchData);
                            roughnessSection.AddQRoughnessFunctionToBranch(branch, function);
                            roughnessSection.UpdateCoverageForFunction(branch, function,
                                roughnessBranchData.RoughnessType);
                            break;
                        }

                        case RoughnessFunction.FunctionOfH:
                        {
                            var function = RoughnessSection.DefineFunctionOfH();
                            function.FillFunctionWithTableData(definitionData, branch, roughnessBranchData);
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
            if (fileReadingExceptions.Count != 0)
            {
                var innerExceptionMessages =
                    fileReadingExceptions.Select(
                        fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format(
                    (string) Resources.RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_, Environment.NewLine,
                    string.Join(Environment.NewLine, innerExceptionMessages)));
            }
            
            if(!isCalibratedRoughness)
                RoughnessSections.Add(roughnessSection);
        }

        private static void FillFunctionWithTableData(this IFunction function, IList<RoughnessDefinitionData> roughnessDefinitionData, IBranch branch, RoughnessBranchData roughnessBranchData)
        {
            foreach (var definitionData in roughnessDefinitionData.OfType<QorHRoughnessDefinitionData>().Where(dd => dd.Branch == branch))
            {
                var branchData = roughnessBranchData as QorHRoughnessBranchData;
                if (branchData == null)
                    throw new FileReadingException(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed_);
                
                var levels = branchData.Levels;
                if (levels.Count != definitionData.Values.Count)
                    throw new FileReadingException(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed__values_count_doesn_t_match_the_defined_levels_count_);
                
                for (int index = 0; index < levels.Count; index++)
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
                        throw new FileReadingException( string.Format((string) Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model, branchid));
                    var branchRoughnessType = FrictionTypeConverter.ConvertToRoughnessFrictionType(branchCategory.ReadProperty<Friction>(RoughnessDataRegion.RoughnessType.Key));
                    var functionType = (RoughnessFunction) branchCategory.ReadProperty<int>(RoughnessDataRegion.FunctionType.Key);
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
                                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_The_length_of_the_number_of_levels___0___and_the_defined_number_of_levels_of_the_branch_property__1__are_not_the_same_of_branch_properties____2__,levels.Count,numLevels, branch.Name));
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
            throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadRoughnessBranchData_While_reading_branches_for_roughness_section_an_error_occured___0___1_, Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
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
                        throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model,branchid));

                    var chainage = definitionCategory.ReadProperty<double>(SpatialDataRegion.Chainage.Key);
                    var roughnessBranchData = branchData.FirstOrDefault(bd => bd.Branch == branch);
                    if (roughnessBranchData == null)
                        throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model,branchid));

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
            throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_, Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
        }

        private static RoughnessSection ReadRoughnessSection(INetwork network, IList<RoughnessSection> roughnessSections, IDelftIniCategory contentSection, bool isCalibratedRoughness)
        {
            var sectionId = contentSection.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);
            var isReversed = contentSection.ReadProperty<bool>(RoughnessDataRegion.FlowDirection.Key);
            var interpolationType = (InterpolationType)contentSection.ReadProperty<int>(RoughnessDataRegion.Interpolate.Key);

            RoughnessType globalType = RoughnessType.Chezy; 
            double? globalValue = null;
            
            RoughnessSection roughnessSection;
            if (isCalibratedRoughness)
            {
                roughnessSection = !isReversed 
                    ? roughnessSections.FirstOrDefault(rs => rs.Name == sectionId)
                    : roughnessSections.OfType<ReverseRoughnessSection>().FirstOrDefault(rs => rs.NormalSection.Name == sectionId);

                if (roughnessSection == null)
                    throw new FileReadingException(string.Format((string) Resources.RoughnessDataFileReader_ReadRoughnessSection_Could_not_import_calibrated_roughness_section__0__because_the_calibrated_roughness_section_you_want_to_import_doesn_t_exist_in_the_model,sectionId));

                //cleanup old roughnessdata
                foreach (var branch in roughnessSection.Network.Branches)
                {
                    roughnessSection.RemoveRoughnessFunctionsForBranch(branch);
                }
                roughnessSection.RoughnessNetworkCoverage.Clear();

                globalType = FrictionTypeConverter.ConvertToRoughnessFrictionType(contentSection.ReadProperty<Friction>(RoughnessDataRegion.GlobalType.Key));
                globalValue = contentSection.ReadProperty<double>(RoughnessDataRegion.GlobalValue.Key);
            }
            else
            {
                var hasGlobalType = contentSection.Properties.Any(p => p.Name == RoughnessDataRegion.GlobalType.Key);

                if (isReversed)
                {
                    var normalSectionExists = roughnessSections.FirstOrDefault(rs => rs.Name == sectionId);
                    if (normalSectionExists != null)
                    {
                        roughnessSection = new ReverseRoughnessSection(normalSectionExists){UseNormalRoughness = !hasGlobalType};
                    }
                    else
                    {
                        var message = Resources.RoughnessDataFileReader_ReadRoughnessSection_When_reading_reverse_roughness_section___0___the_referring__linked___normal__roughness_section___1___is_not_found__The_normal_section___1___should_be_imported_first_;
                        throw new FileReadingException(string.Format((string) message,sectionId + " (Reversed)", sectionId));
                    }
                }
                else
                {
                    var crossSectionSection = new CrossSectionSectionType { Name = sectionId };
                    roughnessSection = new RoughnessSection(crossSectionSection, network);
                }

                if (!isReversed || hasGlobalType)
                {
                    globalType = FrictionTypeConverter.ConvertToRoughnessFrictionType(contentSection.ReadProperty<Friction>(RoughnessDataRegion.GlobalType.Key));
                    globalValue = contentSection.ReadProperty<double>(RoughnessDataRegion.GlobalValue.Key);
                }
                
                roughnessSection.Name = sectionId;
            }

            if (roughnessSection.RoughnessNetworkCoverage == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_roughness_network_coverage_is_not_created);

            var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var firstNWCArgument = roughnessNetworkCoverage.Arguments.FirstOrDefault();
            
            if (firstNWCArgument == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);
            
            firstNWCArgument.InterpolationType = interpolationType;
            if (globalValue.HasValue)
                roughnessSection.SetDefaults(globalType, globalValue.Value);

            return roughnessSection;
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