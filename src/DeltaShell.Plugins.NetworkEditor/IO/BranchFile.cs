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
        }

        public enum BranchTypes
        {
            Unkown = 0, Channel = 1, SewerConnection = 2, Pipe = 3
        }

        private static string GetBranchType(IBranch branch)
        {
            var value = BranchTypes.Unkown;
            if (branch is IChannel)
            {
                value = BranchTypes.Channel;
            }
            else if (branch is IPipe)
            {
                value = BranchTypes.Pipe;
            }
            else if (branch is ISewerConnection)
            {
                value = BranchTypes.SewerConnection;
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
                categories.Add(iniCategory);
            }

            // write branch file
            new DelftIniWriter().WriteDelftIniFile(categories, filePath, false);
        }

        public static List<DelftIniCategory> Read(string filePath)
        {
            return new DelftIniReader().ReadDelftIniFile(filePath).ToList();
        }
    }
}
