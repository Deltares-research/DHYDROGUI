using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class BranchFile
    {
        private static class KnownPropertyNames
        {
            public const string Name = "Name";
            public const string BranchType = "BranchType";
            public const string WaterType = "WaterType";
            public const string Material = "Material";
        }

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
                var iniCategory = new DelftIniCategory("Branch");
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Name, branch.Name, string.Empty));
                
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.BranchType, GetBranchType(branch), 
                    "2 = Pipe, 3 = SewerConnection"));
                /*
                 var sewerConnection = branch as ISewerConnection;
                var waterType = (int) (sewerConnection?.WaterType ?? SewerConnectionWaterType.None);
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.WaterType, waterType.ToString(),
                    "0 = None, 1 = StormWater, 2 = DryWater, 3 = Combined"));
                */
                var sewerConnection = branch as ISewerConnection;
                if (sewerConnection == null) continue;
                var pipe = branch as Pipe;
                if (pipe == null)
                {
                    categories.Add(iniCategory);
                    continue;
                }

                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Material, ((int) pipe.Material).ToString(),
                    "0 = Unknown, 1 = Concrete, 2 = CastIron, 3 = StoneWare, 4 = Hdpe, 5 = Masonry, 6 = SheetMetal, 7 = Polyester, 8 = Polyvinylchlorid, 9 = Steel"));

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
                    Name = category.GetPropertyValue(KnownPropertyNames.Name),
                    BranchType = category.GetEnumValueByKey<BranchType>(KnownPropertyNames.BranchType),
                    /* WaterType = category.GetEnumValueByKey<SewerConnectionWaterType>(KnownPropertyNames.WaterType),*/
                    Material = category.GetEnumValueByKey<SewerProfileMapping.SewerProfileMaterial>(KnownPropertyNames.Material)
                };
                propertiesPerBranch.Add(branchProperties);
            }
            
            if (!File.Exists(netFilePath)) return propertiesPerBranch;
            var file = NetCdfFile.OpenExisting(netFilePath);
            try
            {
                var branchIds = file.GetVariableByName($"{GridApiDataSet.DataSetNames.Network}_{GridApiDataSet.UGridApiConstants.BranchIds}"); ;
                if (branchIds == null) return propertiesPerBranch;
                var branchTypes = file.GetVariableByName($"{GridApiDataSet.DataSetNames.Network}_{GridApiDataSet.UGridApiConstants.BranchType}");
                if (branchTypes == null) return propertiesPerBranch;
                var branchIdValues = file.Read(branchIds)
                    .Cast<char[]>()
                    .SelectMany(s => s.Select((character, index) => new { character, index })
                                     .GroupBy(y => y.index / GridWrapper.idssize)
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
                case (int) NetworkUGridDataModel.BranchType.DryWeatherFlow:
                    return SewerConnectionWaterType.DryWater;
                case (int) NetworkUGridDataModel.BranchType.MixedFlow:
                    return SewerConnectionWaterType.Combined;
                case (int) NetworkUGridDataModel.BranchType.StormWaterFlow:
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
            public string Name;
            public BranchType BranchType;
            public SewerConnectionWaterType WaterType;
            public SewerProfileMapping.SewerProfileMaterial Material;
        }
    }
}
