using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Reflection;
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
                case Friction.Manning:
                    return "Manning";
                case Friction.StricklerNikuradse:
                    return "StricklerNikuradse";
                case Friction.Strickler:
                    return "Strickler";
                case Friction.WhiteColebrook:
                    return "WhiteColebrook";
                case Friction.DeBosBijkerk:
                    return "deBosBijkerk";
                case Friction.WallLawNikuradse:
                    return "wallLawNikuradse";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static DelftIniCategory GenerateBranchProperties(RoughnessSection roughnessSection, IBranch branch,
            RoughnessFunction roughnessFunctionType)
        {

            var branchProperties = new DelftIniCategory(RoughnessDataRegion.BranchPropertiesIniHeader);
            branchProperties.AddProperty(SpatialDataRegion.BranchId.Key, branch.Name,
                SpatialDataRegion.BranchId.Description);
            var roughnessType =
                FrictionTypeConverter.ConvertFrictionType(
                    roughnessSection.EvaluateRoughnessType(new NetworkLocation(branch,
                        0)));
            branchProperties.AddProperty(RoughnessDataRegion.RoughnessType.Key, roughnessType.GetDisplayName(),
                RoughnessDataRegion.RoughnessType.Description);

            branchProperties.AddProperty(RoughnessDataRegion.FunctionType.Key, roughnessFunctionType.GetDescription(),
                RoughnessDataRegion.FunctionType.Description);


            var levels = GetLevels(roughnessSection, branch);

            if (roughnessFunctionType != RoughnessFunction.Constant && levels != null)
            {
                branchProperties.AddProperty(RoughnessDataRegion.NumberOfLevels.Key, levels.Count, RoughnessDataRegion.NumberOfLevels.Description);
                branchProperties.AddProperty(RoughnessDataRegion.Levels.Key, levels, null, RoughnessDataRegion.Levels.Format);
            }

            var locations = GetLocations(roughnessSection, branch);
            if (locations != null)
            {
                // Write functions of Discharge or Water level
                branchProperties.AddProperty(RoughnessDataRegion.NumberOfLocations.Key,locations.Count);
                var nrOfLevels = levels != null ? levels.Count : 1;
                var values = new double[nrOfLevels][];
                for (int j = 0; j < nrOfLevels; j++)
                {
                    values[j] = new double[locations.Count];
                }
                for (int i = 0; i < locations.Count; i++)
                {
                    var branchFricValuesPerLevel = GetValues(roughnessSection, branch, locations[i], levels).ToArray();
                    for (int j = 0; j < nrOfLevels; j++)
                    {
                        values[j][i] = branchFricValuesPerLevel[j]; 
                    }
                }

                branchProperties.AddProperty(SpatialDataRegion.Chainage.Key, locations, null, SpatialDataRegion.Chainage.Format);
                var valuesAsString = string.Empty;
                for (int j = 0; j < nrOfLevels; j++)
                {
                    valuesAsString += string.Join(" ", values[j].Select(v => v.ToString(RoughnessDataRegion.Values.Format, CultureInfo.InvariantCulture)));
                    valuesAsString += Environment.NewLine;
                }
                
                branchProperties.AddProperty(RoughnessDataRegion.Values.Key, valuesAsString);
            }
            else
            {
                // Write constant
                if (roughnessSection.RoughnessNetworkCoverage == null) return branchProperties;
                var networkLocations = roughnessSection.RoughnessNetworkCoverage.GetLocationsForBranch(branch);
                if (networkLocations == null) return branchProperties;

                branchProperties.AddProperty(RoughnessDataRegion.NumberOfLocations.Key, networkLocations.Count);

                branchProperties.AddProperty(SpatialDataRegion.Chainage.Key, networkLocations.Select(nl => nl.Branch.GetBranchSnappedChainage(nl.Chainage)), SpatialDataRegion.Chainage.Description, SpatialDataRegion.Chainage.Format);
                branchProperties.AddProperty(RoughnessDataRegion.Values.Key, networkLocations.Select(nl => roughnessSection.RoughnessNetworkCoverage.GetValues<double>(new VariableValueFilter<INetworkLocation>(roughnessSection.RoughnessNetworkCoverage.Locations, nl)).FirstOrDefault()), RoughnessDataRegion.Values.Description, RoughnessDataRegion.Values.Format);
            }
            return branchProperties;
        }
        
        private static IEnumerable<double> GetValues(RoughnessSection roughnessSection, IBranch branch, double location, List<double> levels)
        {
            var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(branch);
            
            List<double> retValues = new List<double>();
            switch (roughnessFunctionType)
            {
                case RoughnessFunction.FunctionOfQ:
                    retValues.AddRange(levels.Select(level => branch.GetBranchSnappedChainage(roughnessSection.FunctionOfQ(branch).Evaluate<double>(location, level))));
                    break;
                case RoughnessFunction.FunctionOfH:
                    retValues.AddRange(levels.Select(level => branch.GetBranchSnappedChainage(roughnessSection.FunctionOfH(branch).Evaluate<double>(location, level))));
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

            if (returnLocations != null)
            {
                for (int i = 0; i < returnLocations.Count; i++)
                {
                    returnLocations[i] = branch.GetBranchSnappedChainage(returnLocations[i]);
                }
            }

            return returnLocations;
        }
    }
}
