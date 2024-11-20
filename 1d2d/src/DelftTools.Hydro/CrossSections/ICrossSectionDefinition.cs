using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections
{
    public interface ICrossSectionDefinition : ICloneable, IUnique<long>, ICopyFrom, INameable, IEditableObject
    {
        /// <summary>
        /// The crossSection is based on a linestring geometry.
        /// Default is true, otherwise cs is a point but will be shown
        /// as a linestring in the network/map.
        /// </summary>
        bool GeometryBased { get; }

        ///<summary>
        /// YZ representation of the current geometry. Y is distance perpendicular to the branch
        ///</summary>
        IEnumerable<Coordinate> GetProfile();

        /// <summary>
        /// YZ representation of the current flow geometry.
        /// </summary>
        IEnumerable<Coordinate> FlowProfile { get; }

        /// <summary>
        /// Defines the datatable that contains cross-section raw data.
        /// </summary>
        LightDataTable RawData { get; }

        ///<summary>
        ///</summary>
        double LowestPoint { get; }

        ///<summary>
        ///</summary>
        double HighestPoint { get; }

        double LeftEmbankment { get; }

        double RightEmbankment { get; }

        /// <summary>
        /// Roughness sections
        /// </summary>
        IEventedList<CrossSectionSection> Sections { get; }

        /// <summary>
        /// The type of cross section
        /// </summary>
        CrossSectionType CrossSectionType { get; }

        /// <summary>
        /// The width of the cross section as shown in the y'z plane (y'max - y'min)
        /// </summary>
        double Width { get; }

        ///<summary>
        /// For cross sections of GeometryBased this is exactly the position where cross section intersect channel.
        /// For YZ and ZW this is user defined value.
        /// The thalWeg currently has no meaning for the calculation. It is used for drawing the cross section on the map.
        /// The thalWeg is the intersection with the channel.
        ///</summary>
        double Thalweg { get; set; }

        /// <summary>
        /// The left-most point along the crosssection in the y' plane
        /// </summary>
        double Left { get; }

        /// <summary>
        /// The right-most point along the crosssection in the y' plane
        /// </summary>
        double Right { get; }

        [FeatureAttribute]
        string Description { get; set; }
        
        /// <summary>
        /// Indicates whether or not sections should be shrunk and/or expanded to keep spanning the entire cross 
        /// section width, each time a change to the cross section is made.
        /// </summary>
        bool ForceSectionsSpanFullWidth { get; set; }

        ///<summary>
        /// Adds delta to all z-levels of the cross-section
        ///</summary>
        ///<param name="delta"></param>
        ///<exception cref="ArgumentException"></exception>
        void ShiftLevel(double delta);

        IGeometry GetGeometry(ICrossSection crossSection);

        void SetGeometry(IGeometry value);

        bool IsProxy { get; }

        /// <summary>
        /// Clear the cached definition geometry
        /// </summary>
        void RefreshGeometry();

        /// <summary>
        /// Validates the cell value.
        /// </summary>
        /// <param name="rowIndex">The row index (unsorted)</param>
        /// <param name="columnIndex">The column index</param>
        /// <param name="cellValue">The value to be validated.</param>
        /// <returns>The validation result.</returns>
        Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue);
    }
}