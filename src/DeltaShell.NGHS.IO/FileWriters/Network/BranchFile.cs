using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class BranchFile
    {
        public enum BranchType
        {
            Channel = 0, SewerConnection = 1, Pipe = 2
        }

        public static BranchProperties GetBranchProperties(this IBranch branch)
        {
            var branchProperties = new BranchProperties
            {
                Name = branch.Name,
                IsCustomLength = branch.IsLengthCustom
            };

            switch (branch)
            {
                case Channel _:
                    branchProperties.BranchType = BranchType.Channel;
                    break;
                case IPipe pipe:
                    SetSewerConnectionProperties(branchProperties, pipe);
                    branchProperties.BranchType = BranchType.Pipe;
                    branchProperties.Material = pipe.Material;
                    break;
                case ISewerConnection sewerConnection:
                    SetSewerConnectionProperties(branchProperties, sewerConnection);
                    break;
            }

            return branchProperties;
        }

        public static void Write(string filePath, IEnumerable<IBranch> branches)
        {
            var categories = new List<DelftIniCategory>();
            foreach (var branch in branches)
            {
                var properties = branch.GetBranchProperties();

                var iniCategory = new DelftIniCategory(NetworkRegion.BranchIniHeader);
                iniCategory.AddProperty(NetworkRegion.BranchId, properties.Name);
                
                iniCategory.AddProperty(NetworkRegion.BranchType, ((int)properties.BranchType).ToString());
                iniCategory.AddProperty(NetworkRegion.IsLengthCustom, properties.IsCustomLength);

                if (properties.BranchType == BranchType.Channel && !properties.IsCustomLength)
                {
                    // do not write channels without custom length (no special properties need to be saved)
                    continue;
                }

                if (properties.SourceCompartmentName != null)
                {
                    iniCategory.AddProperty(NetworkRegion.SourceCompartmentName, properties.SourceCompartmentName);
                }

                if (properties.TargetCompartmentName != null)
                {
                    iniCategory.AddProperty(NetworkRegion.TargetCompartmentName, properties.TargetCompartmentName);
                }

                if (properties.BranchType == BranchType.Pipe)
                {
                    iniCategory.AddProperty(NetworkRegion.BranchMaterial, (int) properties.Material);
                }

                categories.Add(iniCategory);
            }

            // write branch file
            new DelftIniWriter().WriteDelftIniFile(categories, filePath);
        }

        public static IList<BranchProperties> Read(string filePath, string netFilePath)
        {
            var propertiesPerBranch = new List<BranchProperties>();
            var categories = new DelftIniReader().ReadDelftIniFile(filePath).ToList();
            foreach (var category in categories)
            {
                var branchProperties = new BranchProperties
                {
                    Name = category.ReadProperty<string>(NetworkRegion.BranchId.Key),
                    BranchType = category.GetEnumValueByKey<BranchType>(NetworkRegion.BranchType.Key),
                    IsCustomLength = category.ReadProperty<bool>(NetworkRegion.IsLengthCustom.Key, true),
                    /* WaterType = category.GetEnumValueByKey<SewerConnectionWaterType>(KnownPropertyNames.WaterType),*/
                    Material = category.GetEnumValueByKey<SewerProfileMapping.SewerProfileMaterial>(NetworkRegion.BranchMaterial.Key),
                    SourceCompartmentName = category.ReadProperty<string>(NetworkRegion.SourceCompartmentName.Key, true),
                    TargetCompartmentName = category.ReadProperty<string>(NetworkRegion.TargetCompartmentName.Key, true)
                };
                propertiesPerBranch.Add(branchProperties);
            }
            
            if (!File.Exists(netFilePath)) return propertiesPerBranch;
            var file = NetCdfFile.OpenExisting(netFilePath);
            try
            {
                var branchIds = file.GetVariableByName($"network_{UGridConstants.Naming.BranchIds}");
                if (branchIds == null) return propertiesPerBranch;
                var branchTypes = file.GetVariableByName($"network_{UGridConstants.Naming.BranchType}");
                if (branchTypes == null) return propertiesPerBranch;

                var branchIdValues = file.Read(branchIds)
                    .Cast<char[]>()
                    .SelectMany(s => s.Select((character, index) => new { character, index })
                                     .GroupBy(y => y.index / UGridFileHelper.IdsSize)
                                     .Select(y => new string(y.Select(z => z.character).ToArray()).Trim()))
                    .ToArray();
                var branchTypeValues = file.Read(branchTypes).Cast<int>().ToArray();
                if (branchIdValues.Length != branchTypeValues.Length) return propertiesPerBranch;
                for (int i = 0; i < branchIdValues.Length; i++)
                {
                    var branchProperty = propertiesPerBranch.FirstOrDefault(bp => bp.Name == branchIdValues[i]);
                    if (branchProperty == null) continue;
                    branchProperty.WaterType = ConvertBranchTypeToWaterType(branchTypeValues[i]);
                }

            }
            finally
            {
                file.Close();
            }

            return propertiesPerBranch;
        }

        private static SewerConnectionWaterType ConvertBranchTypeToWaterType(int branchTypeValue)
        {

            switch (branchTypeValue)
            {
                case (int) Grid.BranchType.DryWeatherFlow:
                    return SewerConnectionWaterType.DryWater;
                case (int) Grid.BranchType.MixedFlow:
                    return SewerConnectionWaterType.Combined;
                case (int) Grid.BranchType.StormWaterFlow:
                    return SewerConnectionWaterType.StormWater;
                default:
                    return SewerConnectionWaterType.None;

                
            }
        }

        private static T GetEnumValueByKey<T>(this IDelftIniCategory category, string propertyKey)
        {
            return (T) Enum.Parse(typeof(T), category.ReadProperty<int>(propertyKey, true).ToString());
        }

        private static void SetSewerConnectionProperties(BranchProperties branchProperties, ISewerConnection pipe)
        {
            branchProperties.BranchType = BranchType.SewerConnection;
            branchProperties.WaterType = pipe.WaterType;
            branchProperties.SourceCompartmentName = pipe.SourceCompartmentName;
            branchProperties.TargetCompartmentName = pipe.TargetCompartmentName;
        }
    }
}
