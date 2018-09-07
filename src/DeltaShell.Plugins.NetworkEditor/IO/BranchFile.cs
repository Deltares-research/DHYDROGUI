using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.IO
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
            Unkown = 0, Channel = 1, SewerConnection = 2, Pipe = 3
        }

        private static string GetBranchType(IBranch branch)
        {
            var value = BranchType.Unkown;
            if (branch is IChannel)
            {
                value = BranchType.Channel;
            }
            else if (branch is IPipe)
            {
                value = BranchType.Pipe;
            }
            else if (branch is ISewerConnection)
            {
                value = BranchType.SewerConnection;
            }

            return ((int)value).ToString();
        }

        public static void Write(IEnumerable<IBranch> branches, string filePath)
        {
            var categories = new List<DelftIniCategory>();
            foreach (var branch in branches)
            {
                var iniCategory = new DelftIniCategory("Branch");
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Name, branch.Name, string.Empty));
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.BranchType, GetBranchType(branch), 
                    "0 = Unknown, 1 = Channel, 2 = Pipe, 3 = SewerConnection"));

                var sewerConnection = branch as ISewerConnection;
                var waterType = (int) (sewerConnection?.WaterType ?? SewerConnectionWaterType.None);
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.WaterType, waterType.ToString(),
                    "0 = None, 1 = StromWater, 2 = DryWater, 3 = Combined"));

                var pipe = branch as Pipe;
                var material = (int) (pipe?.Material ?? SewerProfileMapping.SewerProfileMaterial.Unknown);
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Material, material.ToString(),
                    "0 = Unknown, 1 = Concrete, 2 = CastIron, 3 = StoneWare, 4 = Hdpe, 5 = Masonry, 6 = SheetMetal, 7 = Polyester, 8 = Polyvinylchlorid, 9 = Steel"));

                categories.Add(iniCategory);
            }

            // write branch file
            new DelftIniWriter().WriteDelftIniFile(categories, filePath);
        }

        public static IList<BranchProperties> Read(string filePath)
        {
            var propertiesPerBranch = new List<BranchProperties>();
            var categories = new DelftIniReader().ReadDelftIniFile(filePath).ToList();
            foreach (var category in categories)
            {
                var branchProperties = new BranchProperties
                {
                    Name = category.GetPropertyValue(KnownPropertyNames.Name),
                    BranchType = category.GetEnumValueByKey<BranchType>(KnownPropertyNames.BranchType),
                    WaterType = category.GetEnumValueByKey<SewerConnectionWaterType>(KnownPropertyNames.WaterType),
                    Material = category.GetEnumValueByKey<SewerProfileMapping.SewerProfileMaterial>(KnownPropertyNames.Material)
                };
                propertiesPerBranch.Add(branchProperties);
            }

            return propertiesPerBranch;
        }

        private static T GetEnumValueByKey<T>(this IDelftIniCategory category, string propertyKey)
        {
            return (T) Enum.Parse(typeof(T), category.GetPropertyValue(propertyKey));
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
