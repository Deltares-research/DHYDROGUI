using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// File writer for 1D channel roughness.
    /// </summary>
    public static class ChannelFrictionDefinitionFileWriter
    {
        private const string CONSTANT_FUNCTION_TYPE = "constant";

        /// <summary>
        /// Conditionally writes a collection of ChannelFrictionDefinitions to the given filename.
        /// </summary>
        /// <param name="filename">The name of the file to save to.</param>
        /// <param name="channelFrictionDefinitions">A collection of ChannelFrictionDefinitions.</param>
        /// <exception cref="NotSupportedException">When an invalid <see cref="ChannelFrictionSpecificationType"/> is provided.</exception>
        public static void WriteFile(string filename, IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions, RoughnessType globalFrictionType, double globalFrictionValue)
        {
            // [General]
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.RoughnessDataMajorVersion,
                    GeneralRegion.RoughnessDataMinorVersion,
                    GeneralRegion.FileTypeName.RoughnessData),
            };

            // [Global]
            iniSections.Add(CreateGlobalFrictionIniSection(globalFrictionType, globalFrictionValue));

            // [Branch]
            foreach (var channelFrictionDefinition in channelFrictionDefinitions)
            {
                ChannelFrictionSpecificationType specificationType = channelFrictionDefinition.SpecificationType;

                switch (specificationType)
                {
                    case ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition:
                        iniSections.Add(CreateConstantFrictionIniSection(channelFrictionDefinition));
                        break;
                    case ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition:
                        iniSections.Add(CreateSpatialChannelFrictionIniSection(channelFrictionDefinition));
                        break;
                    case ChannelFrictionSpecificationType.ModelSettings: // not written, takes global value by default
                    case ChannelFrictionSpecificationType.RoughnessSections: // written in lane files
                    case ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions: // written in crosssection files
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(iniSections, filename);
        }

        private static IniSection CreateGlobalFrictionIniSection(RoughnessType frictionType, double frictionValue)
        {
            var iniSection = new IniSection(RoughnessDataRegion.GlobalIniHeader);
            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.SectionId.Key, RoughnessDataRegion.SectionId.DefaultValue, RoughnessDataRegion.SectionId.Description);
            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.FrictionType.Key, frictionType.ToString(), RoughnessDataRegion.FrictionType.Description);
            iniSection.AddPropertyWithOptionalCommentAndFormat(RoughnessDataRegion.FrictionValue.Key, frictionValue, RoughnessDataRegion.FrictionValue.Description, RoughnessDataRegion.GlobalValue.Format);
            return iniSection;
        }

        /// <summary>
        /// Creates a IniSection for a channel with a spatial friction specification.
        /// </summary>
        /// <param name="channelFrictionDefinition">A ChannelFrictionDefinition.</param>
        /// <returns>A IniSection for a channel with a spatial friction specification.</returns>
        /// <exception cref="NotSupportedException">When invalid <see cref="RoughnessFunction"/> is provided.</exception>
        private static IniSection CreateSpatialChannelFrictionIniSection(ChannelFrictionDefinition channelFrictionDefinition)
        {
            var iniSection = new IniSection(RoughnessDataRegion.BranchPropertiesIniHeader);

            var spatialDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;

            var channel = channelFrictionDefinition.Channel;
            iniSection.AddPropertyWithOptionalComment(SpatialDataRegion.BranchId.Key, channel.Name,
                SpatialDataRegion.BranchId.Description);

            var roughnessType = spatialDefinition.Type;
            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.RoughnessType.Key, roughnessType.ToString(),
                RoughnessDataRegion.RoughnessType.Description);

            var functionType = spatialDefinition.FunctionType;
            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.FunctionType.Key, RoughnessHelper.ConvertRoughnessFunctionToString(functionType),
                RoughnessDataRegion.FunctionType.Description);

            switch (functionType)
            {
                case RoughnessFunction.Constant:
                    AddConstantFunctionPropertiesToIniSection(spatialDefinition, iniSection);
                    break;
                case RoughnessFunction.FunctionOfH:
                case RoughnessFunction.FunctionOfQ:
                    AddFunctionPropertiesToIniSection(spatialDefinition, iniSection);
                    break;
                default:
                    throw new NotSupportedException();
            }
            
            return iniSection;
        }

        /// <summary>
        /// Create a IniSection for a channel with a constant friction specification.
        /// </summary>
        /// <param name="channelFrictionDefinition">A ChannelFrictionDefinition.</param>
        /// <returns>A IniSection for a channel with a constant friction specification.</returns>
        private static IniSection CreateConstantFrictionIniSection(ChannelFrictionDefinition channelFrictionDefinition)
        {
            var iniSection = new IniSection(RoughnessDataRegion.BranchPropertiesIniHeader);

            iniSection.AddPropertyWithOptionalComment(SpatialDataRegion.BranchId.Key, channelFrictionDefinition.Channel.Name,
                SpatialDataRegion.BranchId.Description);

            var roughnessType = channelFrictionDefinition.ConstantChannelFrictionDefinition.Type;
            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.RoughnessType.Key, roughnessType.ToString(),
                RoughnessDataRegion.RoughnessType.Description);

            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.FunctionType.Key, CONSTANT_FUNCTION_TYPE,
                RoughnessDataRegion.FunctionType.Description);

            double roughnessValue = channelFrictionDefinition.ConstantChannelFrictionDefinition.Value;
            iniSection.AddPropertyWithOptionalCommentAndFormat(RoughnessDataRegion.Values.Key, roughnessValue, 
                null, RoughnessDataRegion.Values.Format);

            return iniSection;
        }

        /// <summary>
        /// Adds FunctionOfQ and FunctionOfH related properties to a given INI section.
        /// </summary>
        /// <param name="spatialDefinition"></param>
        /// <param name="iniSection"></param>
        private static void AddFunctionPropertiesToIniSection(SpatialChannelFrictionDefinition spatialDefinition,
            IniSection iniSection)
        {
            IFunction function = spatialDefinition.Function;
            var levels = function.Arguments[1].GetValues<double>();
            iniSection.AddProperty(RoughnessDataRegion.NumberOfLevels.Key, levels.Count,
                RoughnessDataRegion.NumberOfLevels.Description);
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(RoughnessDataRegion.Levels.Key, levels, null,
                RoughnessDataRegion.Levels.Format);

            var locations = function.Arguments[0].GetValues<double>();
            iniSection.AddProperty(RoughnessDataRegion.NumberOfLocations.Key, locations.Count,
                RoughnessDataRegion.NumberOfLocations.Description);
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(SpatialDataRegion.Chainage.Key, locations,
                SpatialDataRegion.Chainage.Description, SpatialDataRegion.Chainage.Format);

            var values = new double[levels.Count][];
            values = GetFrictionValues(locations, levels, function);
            string valuesAsString = String.Empty;
            foreach (double[] chainageArray in values)
            {
                valuesAsString += string.Join(" ",
                    chainageArray.Select(v => v.ToString(RoughnessDataRegion.Values.Format, CultureInfo.InvariantCulture)));
                valuesAsString += Environment.NewLine;
                valuesAsString += new string(' ', 28);
            }

            iniSection.AddPropertyWithOptionalComment(RoughnessDataRegion.Values.Key, valuesAsString.TrimEnd(), 
                RoughnessDataRegion.Values.Description);
        }

        /// <summary>
        /// Adds ConstantFunction related properties to a given INI section.
        /// </summary>
        /// <param name="spatialDefinition">A SpatialChannelFrictionDefinition.</param>
        /// <param name="iniSection">A IniSection.</param>
        private static void AddConstantFunctionPropertiesToIniSection(SpatialChannelFrictionDefinition spatialDefinition,
            IniSection iniSection)
        {
            var locationCount = spatialDefinition.ConstantSpatialChannelFrictionDefinitions.Count;
            iniSection.AddProperty(RoughnessDataRegion.NumberOfLocations.Key, locationCount,
                RoughnessDataRegion.NumberOfLocations.Description);

            IEnumerable<double> chainages =
                spatialDefinition.ConstantSpatialChannelFrictionDefinitions.Select(cscfd => cscfd.Chainage);
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(SpatialDataRegion.Chainage.Key, chainages,
                SpatialDataRegion.Chainage.Description, SpatialDataRegion.Chainage.Format);

            IEnumerable<double> frictionValues =
                spatialDefinition.ConstantSpatialChannelFrictionDefinitions.Select(cscfd => cscfd.Value);
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(RoughnessDataRegion.Values.Key, frictionValues, null, RoughnessDataRegion.Values.Format);
        }

        /// <summary>
        /// Evaluates the roughness values for a collection of locations and a collection of levels.
        /// </summary>
        /// <param name="locations">All location values.</param>
        /// <param name="levels">All level values.</param>
        /// <param name="function">The SpatialChannelFrictionDefinition function.</param>
        /// <returns>A 2D array with all the friction values.</returns>
        private static double[][] GetFrictionValues(IMultiDimensionalArray<double> locations, IMultiDimensionalArray<double> levels, IFunction function)
        {
            var values = new double[levels.Count][];

            for (int i = 0; i < levels.Count; i++)
            {
                values[i] = new double[locations.Count];

                for (int j = 0; j < locations.Count; j++)
                {
                    values[i][j] = function.Evaluate<double>(locations[j], levels[i]);
                }
            }

            return values;
        }

        
    }
}