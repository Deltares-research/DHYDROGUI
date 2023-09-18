using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Filewriter for initial condition quantity.
    /// </summary>
    public static class ChannelInitialConditionDefinitionFileWriter
    {
        private const string DefaultUnit = "m";
        
        /// <summary>
        /// Writes a collection of <see cref="ChannelInitialConditionDefinition"/> to a file.
        /// </summary>
        /// <param name="filename">The filename to write to.</param>
        /// <param name="channelInitialConditionDefinitions">A collection of <see cref="ChannelInitialConditionDefinition"/> to write.</param>
        /// <param name="initialConditionQuantity">The quantity to write.</param>
        /// <param name="initialConditionValue">The quantity value to write.</param>
        /// <exception cref="InvalidOperationException">When an invalid <see cref="InitialConditionQuantity"/> is provided.</exception>
        public static void WriteFile(
            string filename,
            IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
            InitialConditionQuantity initialConditionQuantity,
            double initialConditionValue)
        {
            if (channelInitialConditionDefinitions == null) throw new ArgumentNullException();

            // [General]
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.InitialConditionDataMajorVersion,
                    GeneralRegion.InitialConditionDataMinorVersion,
                    GeneralRegion.FileTypeName.InitialConditionQuantity)
            };

            // [Global]
            iniSections.Add(CreateGlobalInitialConditionIniSection(initialConditionQuantity, initialConditionValue));

            // [Branch]
            var channelInitialConditionDefinitionArray = channelInitialConditionDefinitions.ToArray();
            foreach (var channelInitialConditionDefinition in channelInitialConditionDefinitionArray)
            {
                ChannelInitialConditionSpecificationType quantity = channelInitialConditionDefinition.SpecificationType;

                switch (quantity)
                {
                    case ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition:
                        iniSections.Add(CreateConstantChannelInitialCondition(channelInitialConditionDefinition));
                        break;
                    case ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition:
                        iniSections.Add(CreateSpatialChannelInitialConditionIniSection(channelInitialConditionDefinition));
                        break;
                    case ChannelInitialConditionSpecificationType.ModelSettings: // not written, takes global value by default
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(iniSections, filename);
        }

        private static IniSection CreateSpatialChannelInitialConditionIniSection(
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            var iniSection = new IniSection(InitialConditionRegion.ChannelInitialConditionDefinitionIniHeader);

            var spatialDefinition = channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;

            var branch = channelInitialConditionDefinition.Channel;
            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.BranchId.Key, branch.Name);

            var locationCount = spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Count;
            iniSection.AddProperty(InitialConditionRegion.NumLocations.Key, locationCount);

            IEnumerable<double> chainages =
                spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Select(definition => definition.Chainage);
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(InitialConditionRegion.Chainage.Key, chainages, "", InitialConditionRegion.Chainage.Format);

            IEnumerable<double> values =
                spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Select(definition => definition.Value);
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(InitialConditionRegion.Values.Key, values, InitialConditionRegion.Values.Description, InitialConditionRegion.Values.Format);

            return iniSection;
        }

        private static IniSection CreateConstantChannelInitialCondition(
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            var iniSection = new IniSection(InitialConditionRegion.ChannelInitialConditionDefinitionIniHeader);

            var branch = channelInitialConditionDefinition.Channel;
            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.BranchId.Key, branch.Name);

            double value = channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value;
            iniSection.AddPropertyWithOptionalCommentAndFormat(InitialConditionRegion.Values.Key, value, InitialConditionRegion.Values.Description, InitialConditionRegion.Values.Format);

            return iniSection;
        }

        private static IniSection CreateGlobalInitialConditionIniSection(
            InitialConditionQuantity initialConditionQuantity, 
            double initialConditionValue)
        {
            var iniSection = new IniSection(InitialConditionRegion.GlobalDefinitionIniHeader);
            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.Quantity.Key, initialConditionQuantity.ToString());
            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.Unit.Key, DefaultUnit);
            iniSection.AddPropertyWithOptionalCommentAndFormat(InitialConditionRegion.Value.Key, initialConditionValue, InitialConditionRegion.Value.Description, InitialConditionRegion.Value.Format);

            return iniSection;
        }
    }
}