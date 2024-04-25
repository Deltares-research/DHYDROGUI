using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileReaders.Roughness
{
    /// <summary>
    /// Provides a reader for roughness files.
    /// </summary>
    public sealed class RoughnessDataFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RoughnessDataFileReader));

        private const bool isCalibratedRoughness = false;
        
        private readonly IFileSystem fileSystem;
        
        private IHydroNetwork network;
        private IList<RoughnessSection> roughnessSections;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoughnessDataFileReader"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public RoughnessDataFileReader(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));
            
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Reads roughness data from the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to read from.</param>
        /// <param name="hydroNetwork"></param>
        /// <param name="sections"></param>
        /// <exception cref="ArgumentException">When <paramref name="fileName"/> is <c>null</c> or whitespace.</exception>
        /// <exception cref="FormatException ">When the specified file has an invalid format.</exception>
        /// <exception cref="FileReadingException">When the specified file does not contain roughness data or when the data is invalid.</exception>
        public void ReadFile(string fileName, IHydroNetwork hydroNetwork, IList<RoughnessSection> sections)
        {
            Ensure.NotNullOrWhiteSpace(fileName, nameof(fileName));
            Ensure.NotNull(hydroNetwork, nameof(hydroNetwork));
            Ensure.NotNull(sections, nameof(sections));
            
            network = hydroNetwork;
            roughnessSections = sections;
            
            if (!fileSystem.File.Exists(fileName))
            {
                throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_doesnt_exist, fileName));
            }
            
            log.InfoFormat(Resources.RoughnessDataFileReader_ReadFile_Reading_roughness_data_from__0__, fileName);

            IniData iniData;
            IniParser iniParser = new IniParser();
            iniParser.Configuration.AllowMultiLineValues = true;

            using (Stream stream = fileSystem.File.OpenRead(fileName))
            {
                iniData = iniParser.Parse(stream);
            }

            if (iniData.SectionCount == 0)
            {
                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadFile_Could_not_read_file__0__properly__it_seems_empty, fileName));
            }
            
            var contentSections = iniData.Sections.Where(iniSection => iniSection.IsNameEqualTo(RoughnessDataRegion.GlobalIniHeader)).ToList();
            if (!contentSections.Any())
            {
                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadFile_Could_not_read_content_section__0__properly, fileName));
            }
            
            var roughnessSection = ReadRoughnessSection(contentSections[0]);
            
            var readRoughnessBranchData = ReadRoughnessBranchData(hydroNetwork, iniData);
            
            //Reading went fine add to the model now!
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            foreach (var roughnessBranchData in readRoughnessBranchData)
            {
                try
                {
                    roughnessSection.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = true;

                    var branch = roughnessBranchData.Branch;

                    switch (roughnessBranchData.RoughnessFunctionType)
                    {
                        case RoughnessFunction.FunctionOfQ:
                        {
                            var function = RoughnessSection.DefineFunctionOfQ();
                            FillFunctionWithTableData(function, roughnessBranchData);
                            roughnessSection.AddQRoughnessFunctionToBranch(branch, function);
                            roughnessSection.UpdateCoverageForFunction(branch, function,
                                roughnessBranchData.RoughnessType);
                            break;
                        }

                        case RoughnessFunction.FunctionOfH:
                        {
                            var function = RoughnessSection.DefineFunctionOfH();
                            FillFunctionWithTableData(function, roughnessBranchData);
                            roughnessSection.AddHRoughnessFunctionToBranch(branch, function);
                            roughnessSection.UpdateCoverageForFunction(branch, function,
                                roughnessBranchData.RoughnessType);
                            break;
                        }

                        case RoughnessFunction.Constant:
                        {

                            for (int i = 0; i < roughnessBranchData.Chainages.Length; i++)
                            {
                                var offset = branch.GetBranchSnappedChainage(roughnessBranchData.Chainages[i]);
                                roughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, offset)] = new object[] {roughnessBranchData.Values[0][i], roughnessBranchData.RoughnessType};
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
                finally
                {
                    roughnessSection.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = false;
                }
            }
            if (fileReadingExceptions.Count != 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format(
                    Resources.RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_, Environment.NewLine,
                    string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }
        
        private static void FillFunctionWithTableData(IFunction function, RoughnessBranchData roughnessBranchData)
        {
            var branchData = roughnessBranchData as QorHRoughnessBranchData;
            if (branchData == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed_);
            
            var levels = branchData.Levels;
            if (levels.Count != roughnessBranchData.Values.Length)
                throw new FileReadingException(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed__values_count_doesn_t_match_the_defined_levels_count_);
            
            for (int levelIndex = 0; levelIndex < levels.Count; levelIndex++)
            {
                var level = levels[levelIndex];
                for(int chainageIndex =0; chainageIndex < roughnessBranchData.Chainages.Length; chainageIndex++)
                {
                    var chainage = branchData.Branch.GetBranchSnappedChainage(roughnessBranchData.Chainages[chainageIndex]);
                    function[chainage, level] = roughnessBranchData.Values[levelIndex][chainageIndex];
                }
            }
        }

        private static IList<RoughnessBranchData> ReadRoughnessBranchData(INetwork network, IniData iniData)
        {
            IList<RoughnessBranchData> branchData = new List<RoughnessBranchData>();
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            foreach (var branchIniSection in iniData.Sections.Where(iniSection => iniSection.IsNameEqualTo(RoughnessDataRegion.BranchPropertiesIniHeader)))
            {
                try
                {
                    var branchId = branchIniSection.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
                    if (branchId == null)
                        throw new FileReadingException(string.Format((string)Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model, branchId));
                    var branch = network.Branches.FirstOrDefault(b => b.Name == branchId);
                    if (branch == null)
                        throw new FileReadingException( string.Format((string) Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model, branchId));

                    var branchRoughnessType = FrictionTypeConverter.ConvertToRoughnessFrictionType(branchIniSection.ReadProperty<Friction>(RoughnessDataRegion.RoughnessType.Key));

                    RoughnessFunction functionType;
                    var functionTypeString = branchIniSection.ReadProperty<string>(RoughnessDataRegion.FunctionType.Key);

                    try
                    {
                        functionType = RoughnessHelper.ConvertStringToRoughnessFunction(functionTypeString);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FileReadingException(string.Format("The function type {0} is unknown!", functionTypeString));
                    }

                    var chainages = branchIniSection.ReadPropertiesToListOfType<double>(SpatialDataRegion.Chainage.Key).ToArray();
                    for (int i = 0; i < chainages.Length; i++)
                    {
                        chainages[i] = branch.GetBranchSnappedChainage(chainages[i]);
                    }
                    
                    var numLocations = branchIniSection.ReadProperty<int>(RoughnessDataRegion.NumberOfLocations.Key);
                    var values = branchIniSection.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Values.Key).ToList();

                    switch (functionType)
                    {
                        case RoughnessFunction.Constant:
                            var dataDefinition = new RoughnessBranchData
                            {
                                Branch = branch,
                                RoughnessType = branchRoughnessType,
                                RoughnessFunctionType = functionType,
                                Chainages = chainages,
                                Values = new double[1][] 
                            };
                            dataDefinition.Values[0] = values.ToArray();
                            branchData.Add(dataDefinition);
                            break;
                        case RoughnessFunction.FunctionOfQ:
                        case RoughnessFunction.FunctionOfH:
                            var numLevels = branchIniSection.ReadProperty<int>(RoughnessDataRegion.NumberOfLevels.Key);
                            var levels = branchIniSection.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Levels.Key);
                            if (levels.Count != numLevels)
                                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_The_length_of_the_number_of_levels___0___and_the_defined_number_of_levels_of_the_branch_property__1__are_not_the_same_of_branch_properties____2__,levels.Count,numLevels, branch.Name));
                            var qorHRoughnessBranchData = new QorHRoughnessBranchData
                            {
                                Branch = branch,
                                RoughnessType = branchRoughnessType,
                                RoughnessFunctionType = functionType,
                                Levels = levels,
                                Chainages = chainages,
                                Values = new double[numLevels][]
                            };
                            for (int i = 0; i < numLevels; i++)
                            {
                                qorHRoughnessBranchData.Values[i] = values.GetRange(i * numLocations, numLocations).ToArray(); 
                            }
                            branchData.Add(qorHRoughnessBranchData);
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

        private RoughnessSection ReadRoughnessSection(IniSection contentSection)
        {
            var sectionId = contentSection.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);
            var isReversed = false;
            
            RoughnessType? globalType = null;
            double? globalValue = null;
            RoughnessSection roughnessSection = !isReversed
                                                   ? roughnessSections.FirstOrDefault(rs => rs.Name == sectionId)
                                                   : roughnessSections.OfType<ReverseRoughnessSection>()
                                                                      .FirstOrDefault(rs => rs.NormalSection.Name == sectionId);


            if (isCalibratedRoughness)
            {
                
                if (roughnessSection == null)
                    throw new FileReadingException(string.Format(
                        (string) Resources
                            .RoughnessDataFileReader_ReadRoughnessSection_Could_not_import_calibrated_roughness_section__0__because_the_calibrated_roughness_section_you_want_to_import_doesn_t_exist_in_the_model,
                        sectionId));


                //cleanup old roughnessdata
                foreach (var branch in roughnessSection.Network.Branches)
                {
                    roughnessSection.RemoveRoughnessFunctionsForBranch(branch);
                }

                roughnessSection.RoughnessNetworkCoverage.Clear();
            }
            else
            {
                var hasGlobalType = contentSection.Name.Equals(RoughnessDataRegion.GlobalIniHeader, StringComparison.InvariantCultureIgnoreCase);

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
                    if (roughnessSection == null)
                    {
                        var crossSectionSection = new CrossSectionSectionType {Name = sectionId};
                        roughnessSection = new RoughnessSection(crossSectionSection, network);
                        roughnessSections.Add(roughnessSection);
                    }
                }

                if (!isReversed || hasGlobalType)
                {
                    if (contentSection.ContainsProperty(RoughnessDataRegion.FrictionType.Key))
                    {
                        globalType = FrictionTypeConverter.ConvertToRoughnessFrictionType(contentSection.ReadProperty<Friction>(RoughnessDataRegion.FrictionType.Key));
                    }
                    if (contentSection.ContainsProperty(RoughnessDataRegion.FrictionValue.Key))
                    {
                        globalValue = contentSection.ReadProperty<double>(RoughnessDataRegion.FrictionValue.Key);
                    }
                }
                
                roughnessSection.Name = sectionId;
            }

            if (roughnessSection.RoughnessNetworkCoverage == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_roughness_network_coverage_is_not_created);

            var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var firstNWCArgument = roughnessNetworkCoverage.Arguments.FirstOrDefault();
            
            if (firstNWCArgument == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);
            
            if (globalType.HasValue)
                roughnessSection.SetDefaultRoughnessType(globalType.Value);
            if (globalValue.HasValue)
                roughnessSection.SetDefaultRoughnessValue(globalValue.Value);

            return roughnessSection;
        }

        private class RoughnessBranchData
        {
            public IBranch Branch { get; set; }
            public RoughnessType RoughnessType { get; set; }
            public RoughnessFunction RoughnessFunctionType { get; set; }
            public double[] Chainages { get; set; }
            public double[][] Values { get; set; }
        }
        private class QorHRoughnessBranchData : RoughnessBranchData
        {
            public IList<double> Levels { get; set; }
        }
    }
}