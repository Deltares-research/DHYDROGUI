using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileWriters.Roughness
{
    public static class RoughnessDataFileWriter 
    {
        public static void WriteFile(string filename, RoughnessSection roughnessSection)
        {
            var networkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.RoughnessDataMajorVersion, 
                                                             GeneralRegion.RoughnessDataMinorVersion, 
                                                             GeneralRegion.FileTypeName.RoughnessData),
                GenerateGlobal(roughnessSection, networkCoverage)
            };

            var branches = roughnessSection.Network.Branches.Where(branch => HasSpatialRoughnessDefinedOnBranch(roughnessSection, branch)).ToList();
            
            // Add branch properties
            foreach (var branch in branches)
            {
                var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);
                switch (roughnessFunctionType)
                {
                    case RoughnessFunction.FunctionOfQ:
                        if (roughnessSection.FunctionOfQ(branch).Components[0].Values.Count > 0)
                            categories.Add(GenerateBranchProperties(roughnessSection, branch, RoughnessFunction.FunctionOfQ));
                        break;
                    case RoughnessFunction.FunctionOfH:
                        if (roughnessSection.FunctionOfH(branch).Components[0].Values.Count > 0)
                            categories.Add(GenerateBranchProperties(roughnessSection, branch, RoughnessFunction.FunctionOfH));
                        break;
                    case RoughnessFunction.Constant:
                        categories.Add(GenerateBranchProperties(roughnessSection, branch, RoughnessFunction.Constant));
                        break;
                }
            }
            
            // Add definitions
            foreach (var branch in branches)
            {
                var locations = GetLocations(roughnessSection, branch);
                if (locations != null)
                {
                    // Write functions of Discharge or Water level
                    categories.AddRange(
                        locations.Select(location => GenerateRoughnessDefinition(roughnessSection, branch, location)));
                }
                else
                {
                    // Write constant
                    foreach (var networkLocation in networkCoverage.GetLocationsForBranch(branch))
                    {
                        var roughnessValues =
                            roughnessSection.RoughnessNetworkCoverage.GetValues(
                                new VariableValueFilter<INetworkLocation>(
                                    roughnessSection.RoughnessNetworkCoverage.Locations, networkLocation));
                        foreach (double roughnessValue in roughnessValues)
                        {
                            categories.Add(SpatialDataFileWriter.GenerateSpatialDataDefinition(networkLocation, roughnessValue));
                        }
                    }
                }
            }
            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(categories, filename);
        }

        private static bool HasSpatialRoughnessDefinedOnBranch(RoughnessSection roughnessSection, IBranch branch)
        {
            var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);
            switch (roughnessFunctionType)
            {
                case RoughnessFunction.FunctionOfQ:
                    return roughnessSection.FunctionOfQ(branch) != null;
                case RoughnessFunction.FunctionOfH:
                    return roughnessSection.FunctionOfH(branch) != null;
            }
            var locations = roughnessSection.RoughnessNetworkCoverage.Locations.AllValues;
            return locations.Any(networkLocation => networkLocation.Branch == branch);
        }

        private static DelftIniCategory GenerateGlobal(RoughnessSection roughnessSection, INetworkCoverage networkCoverage)
        {
            var reversedRoughnessSection = roughnessSection as ReverseRoughnessSection;

            var content = new DelftIniCategory(RoughnessDataRegion.GlobalIniHeader);

            var roughnessSectionId = reversedRoughnessSection?.NormalSection.Name ?? roughnessSection.Name;

            content.AddProperty(RoughnessDataRegion.SectionId.Key, roughnessSectionId);
            //content.AddProperty(RoughnessDataRegion.FlowDirection.Key, roughnessSection.Reversed.ToString(), RoughnessDataRegion.FlowDirection.Description);
            //var interpolationIsLinear = networkCoverage != null && (networkCoverage.Arguments.FirstOrDefault() != null && networkCoverage.Arguments.First().InterpolationType == InterpolationType.Linear) ? 1 : 0;
            //content.AddProperty(RoughnessDataRegion.Interpolate.Key, interpolationIsLinear, RoughnessDataRegion.Interpolate.Description);

            var frictionType = ConvertFrictionTypeToTextForFM(roughnessSection);
            var frictionValue = roughnessSection.GetDefaultRoughnessValue();

            if (reversedRoughnessSection == null || !reversedRoughnessSection.UseNormalRoughness)
            {
                content.AddProperty(RoughnessDataRegion.FrictionType.Key, frictionType, RoughnessDataRegion.FrictionType.Description);
                content.AddProperty(RoughnessDataRegion.FrictionValue.Key, frictionValue, RoughnessDataRegion.FrictionValue.Description, RoughnessDataRegion.GlobalValue.Format);
            }

            return content;
        }

        private static string ConvertFrictionTypeToTextForFM(RoughnessSection roughnessSection)
        {
            switch (FrictionTypeConverter.ConvertFrictionType(roughnessSection.GetDefaultRoughnessType()))
            {
                case Friction.Chezy:
                    return "Chezy";
                case Friction.Mannings:
                    return "Manning";
                case Friction.Nikuradse:
                    return "StricklerNikuradse";
                case Friction.Strickler:
                    return "Strickler";
                case Friction.WhiteColebrook:
                    return "WhiteColebrook";
                case Friction.BosBijkerk:
                    return "deBosBijkerk";
                case Friction.WallLawNikuradse:
                    return "wallLawNikuradse";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static DelftIniCategory GenerateBranchProperties(RoughnessSection roughnessSection, IBranch branch, RoughnessFunction roughnessFunctionType)
        {

            var branchProperties = new DelftIniCategory(RoughnessDataRegion.BranchPropertiesIniHeader);
            branchProperties.AddProperty(SpatialDataRegion.BranchId.Key, branch.Name, SpatialDataRegion.BranchId.Description);
            var roughnessType = (int)FrictionTypeConverter.ConvertFrictionType(roughnessSection.EvaluateRoughnessType(new NetworkLocation(branch, 0))); // TODO are we sure about this? 0 can be not set in UI
            branchProperties.AddProperty(RoughnessDataRegion.RoughnessType.Key, roughnessType, RoughnessDataRegion.RoughnessType.Description);

            branchProperties.AddProperty(RoughnessDataRegion.FunctionType.Key, (int)roughnessFunctionType, RoughnessDataRegion.FunctionType.Description);

            var levels = GetLevels(roughnessSection, branch);
            if (levels == null) return branchProperties;

            branchProperties.AddProperty(RoughnessDataRegion.NumberOfLevels.Key, levels.Count, RoughnessDataRegion.NumberOfLevels.Description);
            branchProperties.AddProperty(RoughnessDataRegion.Levels.Key, levels, null, RoughnessDataRegion.Levels.Format);

            return branchProperties;
        }

        private static DelftIniCategory GenerateRoughnessDefinition(RoughnessSection roughnessSection, IBranch branch, double location)
        {
            var definition = new DelftIniCategory(RoughnessDataRegion.DefinitionIniHeader);
            definition.AddProperty(SpatialDataRegion.BranchId.Key, branch.Name);
            definition.AddProperty(SpatialDataRegion.Chainage.Key, location, null, SpatialDataRegion.Chainage.Format);
            var levels = GetLevels(roughnessSection, branch);
            if (levels == null) return definition;

            var values = GetValues(roughnessSection, branch, location, levels);
            definition.AddProperty(RoughnessDataRegion.Values.Key, values, null, RoughnessDataRegion.Values.Format);

            return definition;
        }
        
        private static IEnumerable<double> GetValues(RoughnessSection roughnessSection, IBranch branch, double location, List<double> levels)
        {
            var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);
            
            List<double> retValues = new List<double>();
            switch (roughnessFunctionType)
            {
                case RoughnessFunction.FunctionOfQ:
                    retValues.AddRange(levels.Select(level => roughnessSection.FunctionOfQ(branch).Evaluate<double>(location, level)));
                    break;
                case RoughnessFunction.FunctionOfH:
                    retValues.AddRange(levels.Select(level => roughnessSection.FunctionOfH(branch).Evaluate<double>(location, level)));
                    break;
            }
            return retValues;
        }

        private static List<double> GetLevels(RoughnessSection roughnessSection, IBranch branch)
        {
            var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);
            List<double> roughnessLevels = null;
            switch (roughnessFunctionType)
            {
                case RoughnessFunction.FunctionOfQ:
                    roughnessLevels =
                        new List<double>(
                            roughnessSection.FunctionOfQ(branch).Arguments[1].GetValues<double>());
                    break;
                case RoughnessFunction.FunctionOfH:
                    roughnessLevels =
                        new List<double>(
                            roughnessSection.FunctionOfH(branch).Arguments[1].GetValues<double>());
                    break;
            }
            return roughnessLevels;
        }

        private static List<double> GetLocations(RoughnessSection roughnessSection, IBranch branch)
        {
            var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);
            List<double> returnLocations = null;
            switch (roughnessFunctionType)
            {
                case RoughnessFunction.FunctionOfQ:
                    returnLocations =
                        new List<double>(roughnessSection.FunctionOfQ(branch).Arguments[0].GetValues<double>());
                    break;
                case RoughnessFunction.FunctionOfH:
                    returnLocations =
                        new List<double>(roughnessSection.FunctionOfH(branch).Arguments[0].GetValues<double>());
                    break;
            }
            return returnLocations;
        }
    }
}
