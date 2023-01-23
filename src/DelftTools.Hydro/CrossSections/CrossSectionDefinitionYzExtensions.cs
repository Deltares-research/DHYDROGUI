using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using log4net;

namespace DelftTools.Hydro.CrossSections
{
    public static class CrossSectionDefinitionYzExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionDefinitionYzExtensions));
        /// <summary>
        /// Convert Y and Z coordinates into YZ coordinates and updates it in the <see cref="CrossSectionDefinition"/> YZ table.
        /// </summary>
        /// <param name="crossSectionDefinition">Cross section to update.</param>
        /// <param name="yCoordinates">Y coordinates used to setup the YZ coordinates.</param>
        /// <param name="zCoordinates">Z coordinates used to setup the YZ coordinates.</param>
        public static void SetYzValues(this CrossSectionDefinitionYZ crossSectionDefinition, IList<double> yCoordinates, IList<double> zCoordinates)
        {
            List<Coordinate> orderedYzCoordinates = yCoordinates
                                                    .Select((t, i) => new Coordinate(t, zCoordinates[i]))
                                                    .OrderBy(c => c.X)
                                                    .ToList();
            crossSectionDefinition.BeginEdit(new DefaultEditAction("Set YZ values"));

            crossSectionDefinition.YZDataTable.Clear();

            var table = new FastYZDataTable();
            table.BeginLoadData();
            foreach (Coordinate coordinate in orderedYzCoordinates)
            {
                table.AddCrossSectionYZRow(coordinate.X, coordinate.Y);
            }
            table.EndLoadData();
            crossSectionDefinition.YZDataTable = table; //triggers only 1 event!

            crossSectionDefinition.EndEdit();
        }
    }
}