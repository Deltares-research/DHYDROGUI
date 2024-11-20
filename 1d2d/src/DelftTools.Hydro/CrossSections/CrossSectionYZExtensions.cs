using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using log4net;

namespace DelftTools.Hydro.CrossSections
{
    public static class CrossSectionYZExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionYZExtensions));

        public static CrossSectionDefinitionYZ SetAsRectangle(this CrossSectionDefinitionYZ crossSectionDefinitionYz, double bedLevel, double width, double height)
        {
            crossSectionDefinitionYz.YZDataTable.Clear();
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2*-1, bedLevel);
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2*-1 - 0.000001, bedLevel + height);
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2, bedLevel + height);
            crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(width/2 + 0.000001, bedLevel);
            return crossSectionDefinitionYz;
        }
        public static CrossSectionDefinitionYZ ConvertZWDataTableToYZ(this CrossSectionDefinitionYZ crossSectionDefinitionYz, FastZWDataTable zWDataTable)
        {
            crossSectionDefinitionYz.YZDataTable.Clear();
            foreach (var zwRow in zWDataTable)
            {
                var zwRowYLeft = zwRow.Width / 2 * -1;
                while (crossSectionDefinitionYz.YZDataTable.Rows.Select(r => r.Yq).Contains(zwRowYLeft))
                    zwRowYLeft -= 0.000001;
                try
                {
                    crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(zwRowYLeft, zwRow.Z);
                }
                catch (Exception e)
                {
                    //gulp
                    log.Warn(e.Message);
                }
                var zwRowYRight = zwRow.Width / 2;
                while (crossSectionDefinitionYz.YZDataTable.Rows.Select(r => r.Yq).Contains(zwRowYRight))
                    zwRowYRight += 0.000001;
                try
                {
                    crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(zwRowYRight, zwRow.Z);
                }
                catch (Exception e)
                {
                    //gulp
                    log.Warn(e.Message);
                }
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

            crossSectionDefinitionYz.BeginEdit("Updates a YZ crosssection with the given hfsw data");

            foreach (var hfsw in heightFlowStorageWidth)
            {
                var hfswYLeft = -1*(hfsw.TotalWidth/2);
                while (crossSectionDefinitionYz.YZDataTable.Rows.Select(r => r.Yq).Contains(hfswYLeft))
                    hfswYLeft -= 0.000001;
                crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(hfswYLeft, hfsw.Height);

                var hfswYRight = hfsw.TotalWidth / 2; 
                while (crossSectionDefinitionYz.YZDataTable.Rows.Select(r => r.Yq).Contains(hfswYRight))
                    hfswYRight += 0.000001;

                
                crossSectionDefinitionYz.YZDataTable.AddCrossSectionYZRow(hfswYRight, hfsw.Height);
            }
            crossSectionDefinitionYz.EndEdit();
        }
    }
}