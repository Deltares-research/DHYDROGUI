using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
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
    }
}