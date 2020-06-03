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
        private static string GetBranchType(IBranch branch)
        {
            var value = BranchType.Channel;
            if (branch is IPipe)
            {
                value = BranchType.Pipe;
            }
            else if (branch is ISewerConnection)
            {
                value = BranchType.SewerConnection;
            }

            return ((int)value).ToString();
        }

        public static void Write(string filePath, IEnumerable<IBranch> branches)
        {
            var categories = new List<DelftIniCategory>();
            foreach (var branch in branches)
            {
                var iniCategory = new DelftIniCategory(NetworkRegion.BranchIniHeader);
                iniCategory.AddProperty(NetworkRegion.BranchId, branch.Name);
                
                iniCategory.AddProperty(NetworkRegion.BranchType, GetBranchType(branch));
                iniCategory.AddProperty(NetworkRegion.IsLengthCustom, branch.IsLengthCustom);
                
                /*
                 var sewerConnection = branch as ISewerConnection;
                var waterType = (int) (sewerConnection?.WaterType ?? SewerConnectionWaterType.None);
                iniCategory.AddProperty(NetworkRegion.BranchWaterType, waterType);
                */
                var sewerConnection = branch as ISewerConnection;
                if (sewerConnection == null)
                {
                    if (branch.IsLengthCustom)
                    {
                        categories.Add(iniCategory);
                    }
                    continue;
                }
                iniCategory.AddProperty(NetworkRegion.SourceCompartmentName, sewerConnection.SourceCompartment?.Name);
                iniCategory.AddProperty(NetworkRegion.TargetCompartmentName, sewerConnection.TargetCompartment?.Name);

                var pipe = branch as Pipe;
                if (pipe == null)
                {
                    categories.Add(iniCategory);
                    continue;
                }

                iniCategory.AddProperty(NetworkRegion.BranchMaterial, (int) pipe.Material);

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

        public class BranchProperties
        {
            public string Name { get; set; }
            public BranchType BranchType { get; set; }
            public bool IsCustomLength { get; set; }
            public SewerConnectionWaterType WaterType { get; set; }
            public SewerProfileMapping.SewerProfileMaterial Material { get; set; }
            public string SourceCompartmentName { get; set; }
            public string TargetCompartmentName { get; set; }
        }
    }
}
