using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public static class CrossSectionLocationFileReader
    {
        public static IList<ICrossSectionLocation> Read(string path)
        {
            if (!File.Exists(path)) throw new FileReadingException($"Could not read file {path} properly, it doesn't exist.");

            var categories = DelftIniFileParser.ReadFile(path);

            var crossSectionLocations = CrossSectionLocationConverter.Convert(categories);

            if (crossSectionLocations == null || crossSectionLocations.Count == 0)
                throw new FileReadingException("Could not read cross section definitions.");

            if (!crossSectionLocations.Select(csl => csl.Name).HasUniqueValues())
                throw new FileReadingException("There are duplicate cross section IDs in the location file, must be unique!");

            return crossSectionLocations;
        }
    }
}
