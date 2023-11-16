using System;
using System.Collections.Generic;
using System.Linq;

namespace DelftTools.Hydro.CrossSections
{
    public static class CrossSectionZWExtensions
    {
        /// <summary>
        /// Updates a HW crosssection with the given hfsw data.
        /// </summary>
        /// <param name="heightFlowStorageWidth"></param>
        /// <returns>The updated crossection</returns>
        public static CrossSectionDefinitionZW SetWithHfswData(this CrossSectionDefinitionZW crossSectionDefinitionZW, IEnumerable<HeightFlowStorageWidth> heightFlowStorageWidth,bool addClosingTopRow = false)
        {
            crossSectionDefinitionZW.ZWDataTable.Clear();

            crossSectionDefinitionZW.BeginEdit("Updates a HW crosssection with the given hfsw data");

            foreach (var hfsw in heightFlowStorageWidth)
            {
                crossSectionDefinitionZW.ZWDataTable.AddCrossSectionZWRow(hfsw.Height, hfsw.TotalWidth, hfsw.StorageWidth);
            }

            if (addClosingTopRow)
            {
                //add a top row of width 0 when not already closed
                var height = heightFlowStorageWidth.Max(h => h.Height);
                crossSectionDefinitionZW.ZWDataTable.AddCrossSectionZWRow(height + 0.000001, 0, 0);
            }

            crossSectionDefinitionZW.EndEdit();

            return crossSectionDefinitionZW;
        }

        /// <summary>
        /// Updates the ZW crossection as a rectangle
        /// </summary>
        /// <param name="crossSectionDefinitionZW"></param>
        /// <param name="bedLevel">Bottom of the rectangle</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>Updated crossection</returns>
        public static CrossSectionDefinitionZW SetAsRectangle(this CrossSectionDefinitionZW crossSectionDefinitionZW,double bedLevel,double width,double height,bool closeProfile = false)
        {
            crossSectionDefinitionZW.ZWDataTable.Clear();
            crossSectionDefinitionZW.ZWDataTable.AddCrossSectionZWRow(bedLevel, width, 0);
            crossSectionDefinitionZW.ZWDataTable.AddCrossSectionZWRow(bedLevel + height, width, 0);
            if (closeProfile)
            {
                crossSectionDefinitionZW.ZWDataTable.AddCrossSectionZWRow(bedLevel + height+0.000001, 0.0, 0);
            }

            return crossSectionDefinitionZW;
        }

        public static bool IsProfileMonotonous(this CrossSectionDefinitionZW crossSectionDefinitionZw)
        {
            var profile = crossSectionDefinitionZw.GetProfile().ToList();
            var midPoint = (int) Math.Ceiling(profile.Count/2.0);
            
            for(int i = 1; i < midPoint; i++)
            {
                if (profile[i].X <= profile[i-1].X)
                {
                    return false;
                }
            }

            return true;
        }
    }
}