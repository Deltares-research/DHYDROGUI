using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.IO
{
    public static class BranchFile
    {
        public static class KnownPropertyNames
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
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.BranchType, GetBranchType(branch), string.Empty));

                var sewerConnection = branch as ISewerConnection;
                var waterType = sewerConnection?.WaterType ?? SewerConnectionWaterType.None;
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.WaterType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType), string.Empty));

                var pipe = branch as Pipe;
                var material = pipe?.Material ?? SewerProfileMapping.SewerProfileMaterial.Unknown;
                iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Material, EnumDescriptionAttributeTypeConverter.GetEnumDescription(material), string.Empty));

                categories.Add(iniCategory);
            }

            // write branch file
            new DelftIniWriter().WriteDelftIniFile(categories, filePath, false);
        }

        public static IList<BranchProperties> Read(string filePath)
        {
            var propertiesPerBranch = new List<BranchProperties>();
            var categories = new DelftIniReader().ReadDelftIniFile(filePath).ToList();
            foreach (var category in categories)
            {
                var branchProperties = new BranchProperties();
                branchProperties.Name = category.GetPropertyValue(KnownPropertyNames.Name);
                branchProperties.BranchType = (BranchType) int.Parse(category.GetPropertyValue(KnownPropertyNames.BranchType));
                branchProperties.WaterType = EnumerableExtensions.GetValueFromDescription<SewerConnectionWaterType>(category.GetPropertyValue(KnownPropertyNames.WaterType));
                branchProperties.Material = EnumerableExtensions.GetValueFromDescription<SewerProfileMapping.SewerProfileMaterial>(category.GetPropertyValue(KnownPropertyNames.Material));
                propertiesPerBranch.Add(branchProperties);
            }

            return propertiesPerBranch;
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
