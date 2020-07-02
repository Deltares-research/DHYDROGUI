using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

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
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.InitialConditionDataMajorVersion,
                    GeneralRegion.InitialConditionDataMinorVersion,
                    GeneralRegion.FileTypeName.InitialConditionQuantity)
            };

            // [Global]
            categories.Add(CreateGlobalInitialConditionCategory(initialConditionQuantity, initialConditionValue));

            // [Branch]
            var channelInitialConditionDefinitionArray = channelInitialConditionDefinitions.ToArray();
            foreach (var channelInitialConditionDefinition in channelInitialConditionDefinitionArray)
            {
                ChannelInitialConditionSpecificationType quantity = channelInitialConditionDefinition.SpecificationType;

                switch (quantity)
                {
                    case ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition:
                        categories.Add(CreateConstantChannelInitialCondition(channelInitialConditionDefinition));
                        break;
                    case ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition:
                        categories.Add(CreateSpatialChannelInitialConditionCategory(channelInitialConditionDefinition));
                        break;
                    case ChannelInitialConditionSpecificationType.ModelSettings: // not written, takes global value by default
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(categories, filename);
        }

        private static DelftIniCategory CreateSpatialChannelInitialConditionCategory(
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            var category = new DelftIniCategory(InitialConditionRegion.ChannelInitialConditionDefinitionIniHeader);

            var spatialDefinition = channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;

            var branch = channelInitialConditionDefinition.Channel;
            category.AddProperty(InitialConditionRegion.BranchId.Key, branch.Name);

            var locationCount = spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Count;
            category.AddProperty(InitialConditionRegion.NumLocations.Key, locationCount);

            IEnumerable<double> chainages =
                spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Select(definition => definition.Chainage);
            category.AddProperty(InitialConditionRegion.Chainage.Key, chainages, "", InitialConditionRegion.Chainage.Format);

            IEnumerable<double> values =
                spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Select(definition => definition.Value);
            category.AddProperty(InitialConditionRegion.Values.Key, values, InitialConditionRegion.Values.Description, InitialConditionRegion.Values.Format);

            return category;
        }

        private static DelftIniCategory CreateConstantChannelInitialCondition(
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            var category = new DelftIniCategory(InitialConditionRegion.ChannelInitialConditionDefinitionIniHeader);

            var branch = channelInitialConditionDefinition.Channel;
            category.AddProperty(InitialConditionRegion.BranchId.Key, branch.Name);

            double value = channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value;
            category.AddProperty(InitialConditionRegion.Values.Key, value, InitialConditionRegion.Values.Description, InitialConditionRegion.Values.Format);

            return category;
        }

        private static DelftIniCategory CreateGlobalInitialConditionCategory(
            InitialConditionQuantity initialConditionQuantity, 
            double initialConditionValue)
        {
            var category = new DelftIniCategory(InitialConditionRegion.GlobalDefinitionIniHeader);
            category.AddProperty(InitialConditionRegion.Quantity.Key, initialConditionQuantity.ToString());
            category.AddProperty(InitialConditionRegion.Unit.Key, DefaultUnit);
            category.AddProperty(InitialConditionRegion.Value.Key, initialConditionValue, InitialConditionRegion.Value.Description, InitialConditionRegion.Value.Format);

            return category;
        }
    }
}