using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// Returns new crossection based on exising crossection. 
    /// Provides conversion between some types
    /// XYZ -> YZ 
    /// </summary>
    public static class CrossSectionConverter
    {
        /// <summary>
        /// Returns a new crossection based on the given crossection
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionYZ ConvertToYz(CrossSectionDefinitionXYZ source)
        {
            //var csWidth = source.Width;
            var result = new CrossSectionDefinitionYZ();
            foreach (var row in source.XYZDataTable)
            {
                result.YZDataTable.AddCrossSectionYZRow(row.Yq, row.Z);
            }

                          
            return result;
        }
    }
}