using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
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
        /// <typeparam name="T"> </typeparam>
        /// <param name="instance"> </param>
        /// <param name="level"> </param>
        /// <returns> </returns>
        public static T AddLevel<T>(this T instance, double level) where T : ICrossSectionDefinition
        {
            var clone = (T) instance.Clone();
            clone.ShiftLevel(level);
            return clone;
        }

        public static double FlowWidth(this CrossSectionDefinition crossSectionDefinition)
        {
            IEnumerable<Coordinate> coordinates = crossSectionDefinition.FlowProfile.ToList();
            return !coordinates.Any() ? 0.0 : coordinates.Max(c => c.X) - coordinates.Min(c => c.X);
        }

        /// <summary>
        /// Returns the cumulative width of all the sections in this Cross Section Definition.
        /// </summary>
        /// <returns> </returns>
        public static double SectionsTotalWidth(this ICrossSectionDefinition crossSectionDefinition)
        {
            double widthFactor = GetWidthFactor(crossSectionDefinition);

            var sectionsTotalWidth = 0.0;
            crossSectionDefinition.Sections.ForEach(s => { sectionsTotalWidth += (s.MaxY - s.MinY) * widthFactor; });
            return sectionsTotalWidth;
        }

        /// <summary>
        /// Add a new section to the cross section definition. The new section will get have the appropriate MinY such that
        /// no two sections will overlap and that all sections are adjecent.
        /// </summary>
        /// <param name="crossSectionDefinition"> The cross section definition. </param>
        /// <param name="crossSectionType"> The cross section type of the new section. </param>
        /// <param name="sectionWidth"> The desired width of the new section. </param>
        public static void AddSection(this ICrossSectionDefinition crossSectionDefinition,
                                      CrossSectionSectionType crossSectionType, double sectionWidth)
        {
            double widthFactor = GetWidthFactor(crossSectionDefinition);

            if (sectionWidth < 0.0)
            {
                Log.WarnFormat(
                    Resources
                        .CrossSectionDefinitionExtensions_AddCrossSectionSection_Could_not_add_CrossSectionSection_with_negative_length__0__to_cross_section_definition___1___,
                    sectionWidth, crossSectionDefinition.Name);
                return;
            }

            IEventedList<CrossSectionSection> sections = crossSectionDefinition.Sections;
            IEnumerable<string> sectionNames = sections.Select(s => s.SectionType.Name);
            if (sectionNames.Contains(crossSectionType.Name))
            {
                Log.WarnFormat(
                    Resources
                        .CrossSectionDefinitionExtensions_AddCrossSectionSection_Could_not_add_CrossSectionSection_with_duplicate_name___0__,
                    crossSectionType.Name);
                return;
            }

            ICrossSectionDefinition definition = GetInnerOrCurrentCrossSectionDefinition(crossSectionDefinition);

            if (definition is CrossSectionDefinitionStandard)
            {
                double distanceToSymmetryAxis = sectionWidth / 2;
                crossSectionDefinition.Sections.Add(new CrossSectionSection
                {
                    SectionType = crossSectionType,
                    MinY = -distanceToSymmetryAxis,
                    MaxY = distanceToSymmetryAxis
                });
            }
            else
            {
                double newMinY = sections.Any() ? sections.Select(s => s.MaxY).Max() : 0.0;
                crossSectionDefinition.Sections.Add(new CrossSectionSection
                {
                    SectionType = crossSectionType,
                    MinY = newMinY,
                    MaxY = newMinY + (sectionWidth / widthFactor)
                });
            }
        }

        public static void AdjustSectionWidths(this CrossSectionDefinition crossSectionDefinition)
        {
            if (!crossSectionDefinition.Sections.Any())
            {
                return;
            }

            double actualCrossSectionWidth = crossSectionDefinition.FlowWidth();
            double widthDifference = actualCrossSectionWidth - crossSectionDefinition.SectionsTotalWidth();
            if (Math.Abs(widthDifference) < 1e-10)
            {
                return;
            }

            var sectionAndIndexToAdjust = crossSectionDefinition.Sections
                                                                .Select((section, index) => new
                                                                {
                                                                    section,
                                                                    index
                                                                })
                                                                .FirstOrDefault(
                                                                    s => s.section.SectionType.Name.Equals(
                                                                        CrossSectionDefinition.MainSectionName,
                                                                        StringComparison
                                                                            .InvariantCultureIgnoreCase))
                                          ?? new
                                          {
                                              section = crossSectionDefinition.Sections.First(),
                                              index = 0
                                          };

            CrossSectionSection adjustedSection = sectionAndIndexToAdjust.section;
            if (adjustedSection == null)
            {
                return;
            }

            // Get old Width for log message before updating!
            double widthFactor = GetWidthFactor(crossSectionDefinition);
            double oldWidth = (adjustedSection.MaxY - adjustedSection.MinY) * widthFactor;

            double nextMinY;
            double nextMaxY;
            crossSectionDefinition.GetCrossSectionDefinitionSectionBounds(out nextMinY, out nextMaxY);

            for (var i = 0; i <= sectionAndIndexToAdjust.index; i++)
            {
                CrossSectionSection section = crossSectionDefinition.Sections[i];
                double diff = nextMinY - section.MinY;

                section.MinY += diff;
                nextMinY = section.MaxY + diff;
            }

            for (int i = crossSectionDefinition.Sections.Count - 1; i >= sectionAndIndexToAdjust.index; i--)
            {
                CrossSectionSection section = crossSectionDefinition.Sections[i];
                double diff = nextMaxY - section.MaxY;

                section.MaxY += diff;
                nextMaxY = section.MinY + diff;
            }

            double newWidth = (adjustedSection.MaxY - adjustedSection.MinY) * widthFactor;

            Log.InfoFormat(
                Resources
                    .CrossSectionDefinitionExtensions_AdjustSectionWidths_The__0__section_width_of_cross_section__1__has_been_changed_from__2_m_to__3_m,
                adjustedSection.SectionType.Name, crossSectionDefinition.Name, oldWidth, newWidth);
        }

        public static IList<ICrossSection> FindUsage(this ICrossSectionDefinition definition, IHydroNetwork network)
        {
            var result = new List<ICrossSection>();

            foreach (ICrossSection cs in network.CrossSections)
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

        public static double GetWidthFactor(this ICrossSectionDefinition crossSectionDefinition)
        {
            ICrossSectionDefinition definition = GetInnerOrCurrentCrossSectionDefinition(crossSectionDefinition);

            return definition is CrossSectionDefinitionZW ? 2.0 : 1.0;
        }

        private static ICrossSectionDefinition GetInnerOrCurrentCrossSectionDefinition(
            ICrossSectionDefinition crossSectionDefinition)
        {
            var proxyDefinition = crossSectionDefinition as CrossSectionDefinitionProxy;

            return proxyDefinition != null ? proxyDefinition.InnerDefinition : crossSectionDefinition;
        }

        public static void GetCrossSectionDefinitionSectionBounds(this CrossSectionDefinition definition,
                                                                  out double minY, out double maxY)
        {
            /*
                 YZ - goes from 0 to Width
                 XYZ - goes from 0 to Width
                 ZW - goes from 0 to +1/2 width (width factor is 2)
                 standard - goes from -1/2 width to +1/2 width
            */

            if (definition is CrossSectionDefinitionZW)
            {
                minY = 0.0;
                maxY = definition.FlowWidth() / 2;
                return;
            }

            if (definition is CrossSectionDefinitionStandard)
            {
                minY = -definition.FlowWidth() / 2;
                maxY = definition.FlowWidth() / 2;
                return;
            }

            // default (YZ & XYZ)
            minY = 0.0;
            maxY = definition.FlowWidth();
        }
    }
}