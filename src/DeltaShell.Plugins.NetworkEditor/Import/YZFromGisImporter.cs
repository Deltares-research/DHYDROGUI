using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// YZ from Gis importer, used for Gis importers using YZ.
    /// </summary>
    public class YzFromGisImporter
    {
        /// <summary>
        /// Convert string from property List Y or Z into a list of doubles.
        /// </summary>
        /// <param name="propertyListAsStringWithSeparators">string from property List Y or Z.</param>
        /// <returns>List of doubles based on the string data in <see cref="propertyListAsStringWithSeparators"/>.</returns>
        public IList<double> ConvertPropertyMappingToList(string propertyListAsStringWithSeparators)
        {
            List<string> stringCoordinates = propertyListAsStringWithSeparators.Split(',').ToList();
            return stringCoordinates.Select(coordinate => Convert.ToDouble(coordinate, CultureInfo.InvariantCulture)).ToList();
        }

        /// <summary>
        /// Convert Y and Z coordinates into YZ coordinates and updates it in the <see cref="CrossSectionDefinition"/> YZ table.
        /// </summary>
        /// <param name="crossSectionDefinition">Cross section to update.</param>
        /// <param name="yCoordinates">Y coordinates used to setup the YZ coordinates.</param>
        /// <param name="zCoordinates">Z coordinates used to setup the YZ coordinates.</param>
        public void ConvertYzProperties(CrossSectionDefinitionYZ crossSectionDefinition, IList<double> yCoordinates, IList<double> zCoordinates)
        {
            List<Coordinate> yzCoordinates = yCoordinates.Select((t, i) => new Coordinate(t, zCoordinates[i])).ToList();
            SetYzValues(crossSectionDefinition, yzCoordinates, 0);
        }

        private void SetYzValues(CrossSectionDefinitionYZ crossSectionDefinition, List<Coordinate> yzCoordinates, double shiftLevel)
        {
            crossSectionDefinition.BeginEdit(new DefaultEditAction("Set YZ values"));

            crossSectionDefinition.YZDataTable.Clear();

            yzCoordinates = yzCoordinates.OrderBy(c => c.X).ToList();

            foreach (Coordinate coordinate in yzCoordinates)
            {
                crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(coordinate.X, coordinate.Y + shiftLevel);
            }

            crossSectionDefinition.EndEdit();
        }
    }
}