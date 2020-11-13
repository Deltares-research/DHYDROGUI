using System.Collections.Generic;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Utils.Editing;

namespace DelftTools.Hydro.CrossSections
{
    public static class CrossSectionYZExtensions
    {
        public static CrossSectionDefinitionYZ SetAsRectangle(this CrossSectionDefinitionYZ crossSectionDefinitionYz, double bedLevel, double width, double height)
        {
            crossSectionDefinitionYz.YZDataTable.Clear();
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2*-1, bedLevel);
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2*-1 + 0.000001, bedLevel + height);
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2, bedLevel + height);
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2 + 0.000001, bedLevel);
            return crossSectionDefinitionYz;
        }
        public static CrossSectionDefinitionYZ ConvertZWDataTableToYZ(this CrossSectionDefinitionYZ crossSectionDefinitionYz, FastZWDataTable zWDataTable)
        {
            crossSectionDefinitionYz.YZDataTable.Clear();
            foreach (var zwRow in zWDataTable)
            {
                crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(zwRow.Width / 2 * -1, zwRow.Z);
                crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(zwRow.Width / 2, zwRow.Z);
            }
            return crossSectionDefinitionYz;
        }

        /// <summary>
        /// Updates a YZ cross section with the given hfsw data.
        /// </summary>
        /// <param name="crossSectionDefinitionYz"></param>
        /// <param name="heightFlowStorageWidth"></param>
        /// <param name="addClosingTopRow"></param>
        /// <returns>The updated crossection</returns>
        public static void SetWithHfswData(this CrossSectionDefinitionYZ crossSectionDefinitionYz, IEnumerable<HeightFlowStorageWidth> heightFlowStorageWidth)
        {
            crossSectionDefinitionYz.YZDataTable.Clear();

            crossSectionDefinitionYz.BeginEdit(new DefaultEditAction("Updates a YZ crosssection with the given hfsw data"));

            foreach (var hfsw in heightFlowStorageWidth)
            {
                crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(-1*(hfsw.TotalWidth/2), hfsw.Height);
                crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(hfsw.TotalWidth/2, hfsw.Height);
            }
            crossSectionDefinitionYz.EndEdit();
        }
    }
}