using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionDefinitionStandard : CrossSectionDefinition
    {
        private ICrossSectionStandardShape shape;

        public CrossSectionDefinitionStandard():this(CrossSectionStandardShapeRectangle.CreateDefault())
        {
            
        }

        public CrossSectionDefinitionStandard(ICrossSectionStandardShape shape)
        {
            Shape = shape;
        }

        public override bool GeometryBased
        {
            get { return false; }
        }

        public override IEnumerable<Coordinate> GetProfile()
        {
            return Shape.Profile.Select(c => new Coordinate(c.X, c.Y + LevelShift));
        }

        public override IEnumerable<Coordinate> FlowProfile
        {
            get { return GetProfile(); }
        }

        public override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.Standard; }
        }

        public override LightDataTable RawData
        {
            get { return null; }
        }

        public virtual double LevelShift { get; set; }

        public virtual ICrossSectionStandardShape Shape
        {
            get { return shape; }
            protected set
            {
                shape = value;
            }
        }

        public virtual CrossSectionStandardShapeType ShapeType
        {
            get { return shape.Type; }
            set
            {
                if (shape.Type == value)
                {
                    return;
                }
                SetShapeFromType(value);
            }
        }

        void SetShapeFromType(CrossSectionStandardShapeType shapeType)
        {
            Shape = GetDefaultShape(shapeType);
        }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionStandard)base.Clone();
            clone.LevelShift = LevelShift;
            clone.Shape = (ICrossSectionStandardShape) Shape.Clone();
            return clone;
        }

        public override void ShiftLevel(double delta)
        {
            BeginEdit("Shift level");
            LevelShift += delta;
            EndEdit();
        }

        public override Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue)
        {
            return new Utils.Tuple<string, bool>("", true);
        }

        public override IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage)
        {
            return CrossSectionHelper.CreatePerpendicularGeometry(branchGeometry, mapChainage, Width, Thalweg);
        }

        public override int GetRawDataTableIndex(int profileIndex)
        {
            throw new NotImplementedException();
        }

        private static ICrossSectionStandardShape GetDefaultShape(CrossSectionStandardShapeType value)
        {
            switch (value)
            {
                case CrossSectionStandardShapeType.Rectangle:
                    return CrossSectionStandardShapeRectangle.CreateDefault();
                case CrossSectionStandardShapeType.Circle: 
                    return CrossSectionStandardShapeCircle.CreateDefault();
                case CrossSectionStandardShapeType.Arch:
                    return CrossSectionStandardShapeArch.CreateDefault();
                case CrossSectionStandardShapeType.UShape:
                    return CrossSectionStandardShapeUShape.CreateDefault();
                case CrossSectionStandardShapeType.Cunette:
                    return CrossSectionStandardShapeCunette.CreateDefault();
                case CrossSectionStandardShapeType.Egg: 
                    return CrossSectionStandardShapeEgg.CreateDefault();
                case CrossSectionStandardShapeType.InvertedEgg: 
                    return CrossSectionStandardShapeInvertedEgg.CreateDefault();
                case CrossSectionStandardShapeType.Elliptical:
                    return CrossSectionStandardShapeElliptical.CreateDefault();
                case CrossSectionStandardShapeType.SteelCunette:
                    return CrossSectionStandardShapeSteelCunette.CreateDefault();
                case CrossSectionStandardShapeType.Trapezium:
                    return CrossSectionStandardShapeTrapezium.CreateDefault();
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        public static ICrossSectionDefinition CreateDefault()
        {
            return new CrossSectionDefinitionStandard();
        }
    }
}
