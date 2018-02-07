using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Utils.Collections;
using GeoAPI.Geometries;
using log4net;

namespace DelftTools.Hydro.CrossSections
{
    public static class CrossSectionDefinitionExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionDefinitionExtensions));

        /// <summary>
        /// Returns a level shifted clone of the crossection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static T AddLevel<T>(this T instance, double level) where T : ICrossSectionDefinition
        {
            var clone = (T)instance.Clone();
            clone.ShiftLevel(level);
            return clone;
        }
        
        public static double FlowWidth(this CrossSectionDefinition crossSectionDefinition)
        {
            IEnumerable<Coordinate> coordinates = crossSectionDefinition.FlowProfile.ToList();
            return coordinates.Count() == 0 ? 0.0 : coordinates.Max(c => c.X) - coordinates.Min(c => c.X);
        }

        /// <summary>
        /// Returns the cumulative width of all the sections in this Cross Section Definition.
        /// </summary>
        /// <returns></returns>
        public static double SectionsTotalWidth(this CrossSectionDefinition crossSectionDefinition)
        {
            var sectionsTotalWidth = 0.0;
            crossSectionDefinition.Sections.ForEach(s =>
            {
                sectionsTotalWidth += 2 * (s.MaxY - s.MinY);
            });
            return sectionsTotalWidth;
        }

        /// <summary>
        /// Add a new section to the cross section definition. The new section will get have the appropriate MinY such that
        /// no two sections will overlap and that all sections are adjecent.
        /// </summary>
        /// <param name="crossSectionDefinition">The cross section definition.</param>
        /// <param name="crossSectionType">The cross section type of the new section.</param>
        /// <param name="sectionWidth">The desired width of the new section.</param>
        public static void AddSection(this ICrossSectionDefinition crossSectionDefinition, CrossSectionSectionType crossSectionType, double sectionWidth)
        {
            if(sectionWidth < 0.0)
            {
                Log.WarnFormat(Resources.CrossSectionDefinitionExtensions_AddCrossSectionSection_Could_not_add_CrossSectionSection_with_negative_length__0__to_cross_section_definition___1___, 
                sectionWidth, crossSectionDefinition.Name);
                return;
            }

            var sections = crossSectionDefinition.Sections;
            var sectionNames = sections.Select(s => s.SectionType.Name);
            if (sectionNames.Contains(crossSectionType.Name))
            {
                Log.WarnFormat(Resources.CrossSectionDefinitionExtensions_AddCrossSectionSection_Could_not_add_CrossSectionSection_with_duplicate_name___0__, crossSectionType.Name);
                return;
            }

            var newMinY = sections.Any() ? sections.Select(s => s.MaxY).Max() : 0.0;
            crossSectionDefinition.Sections.Add(new CrossSectionSection
            {
                SectionType = crossSectionType,
                MinY = newMinY,
                MaxY = newMinY + sectionWidth / 2
            });
        }

        public static IList<ICrossSection> FindUsage(this ICrossSectionDefinition definition, IHydroNetwork network)
        {
            var result = new List<ICrossSection>();

            foreach(var cs in network.CrossSections)
            {
                if (cs.Definition.IsProxy)
                {
                    var proxy = (CrossSectionDefinitionProxy) cs.Definition;

                    if (proxy.InnerDefinition == definition)
                    {
                        result.Add(cs);
                    }
                }
            }
            
            return result;
        }
    }
}
