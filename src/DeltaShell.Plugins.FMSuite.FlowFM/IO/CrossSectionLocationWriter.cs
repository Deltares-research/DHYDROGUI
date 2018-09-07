using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class CrossSectionLocationWriter
    {
        public static void WriteFile(string filePath, WaterFlowFMModel model)
        {
            var pipes = model.Network.Pipes;
            var categories = CreateIniCategories(pipes);

            new DelftIniWriter().WriteDelftIniFile(categories, filePath, false);
        }

        private static List<DelftIniCategory> CreateIniCategories(IEnumerable<IPipe> pipes)
        {
            var categories = new List<DelftIniCategory>();
            foreach (var pipe in pipes)
            {
                var sourceiniCategory = CreateSourceIniCategory(pipe);
                var targetIniCategory = CreateTargetIniCategory(pipe);

                categories.Add(sourceiniCategory);
                categories.Add(targetIniCategory);
            }

            return categories;
        }

        private static DelftIniCategory CreateTargetIniCategory(IPipe pipe)
        {
            var targetIniCategory = new DelftIniCategory("CrossSection");
            targetIniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Id, pipe.Name + "_target", string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Branch, pipe.Name, string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Chainage, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", pipe.Length), string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Shift, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", pipe.LevelTarget), string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Definition, pipe.CrossSectionDefinitionName, string.Empty));
            return targetIniCategory;
        }

        private static DelftIniCategory CreateSourceIniCategory(IPipe pipe)
        {
            var sourceiniCategory = new DelftIniCategory("CrossSection");
            sourceiniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Id, pipe.Name + "_source", string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Branch, pipe.Name, string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Chainage, "0.00", string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Shift, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", pipe.LevelSource), string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Definition, pipe.CrossSectionDefinitionName, string.Empty));
            return sourceiniCategory;
        }

        private static class KnownPropertyNames
        {
            public const string Id = "Id";
            public const string Branch = "Branch";
            public const string Chainage = "Chainage";
            public const string Shift = "Shift";
            public const string Definition = "Definition";
        }
    }
}
