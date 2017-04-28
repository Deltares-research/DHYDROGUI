using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Tests.TestObjects
{
    /// <summary>
    /// Provides a dummy subclass of crossection
    /// </summary>
    public class TestCrossSectionDefinition:CrossSectionDefinition
    {
        public TestCrossSectionDefinition()
        {
            
        }
        public TestCrossSectionDefinition(string name, double offset) : base(name)
        {
        }
        
        public override bool GeometryBased
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<Coordinate> FlowProfile
        {
            get { throw new NotImplementedException(); }
        }

        
        public override CrossSectionType CrossSectionType
        {
            get { throw new NotImplementedException(); }
        }

        public override LightDataTable RawData
        {
            get { return null; }
        }

        public override IEnumerable<Coordinate> Profile
        {
            get { return new List<Coordinate>(); }
        }

        public override void ShiftLevel(double delta)
        {
            throw new NotImplementedException();
        }

        public override Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue)
        {
            throw new NotImplementedException();
        }

        public override IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage)
        {
            throw new NotImplementedException();
        }

        public override int GetRawDataTableIndex(int profileIndex)
        {
            throw new NotImplementedException();
        }
    }
}