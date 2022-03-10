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
        private IEnumerable<Coordinate> profile;
        public TestCrossSectionDefinition()
        {
            
        }
        public TestCrossSectionDefinition(string name) : base(name)
        {
        }
        
        public override bool GeometryBased
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<Coordinate> FlowProfile
        {
            get { return profile ?? new List<Coordinate>(); }
        }

        
        public override CrossSectionType CrossSectionType
        {
            get { throw new NotImplementedException(); }
        }

        public override LightDataTable RawData
        {
            get { return null; }
        }

        public override IEnumerable<Coordinate> GetProfile()
        {
            return profile ?? new List<Coordinate>();
        }

        public override void ShiftLevel(double delta)
        {
            // do nothing
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

        public override object Clone()
        {
            var clone = (TestCrossSectionDefinition)base.Clone();
            clone.profile = this.profile;

            return clone;
        }
    }
}