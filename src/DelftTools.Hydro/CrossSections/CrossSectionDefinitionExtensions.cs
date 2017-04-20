using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections
{
    public static class CrossSectionDefinitionExtensions
    {
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
