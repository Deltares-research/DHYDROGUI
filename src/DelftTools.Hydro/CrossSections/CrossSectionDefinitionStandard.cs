using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange = false)]
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public class CrossSectionDefinitionStandard : CrossSectionDefinition
    {
        private ICrossSectionStandardShape shape;

        public CrossSectionDefinitionStandard() : this(CrossSectionStandardShapeRectangle.CreateDefault()) {}

        public CrossSectionDefinitionStandard(ICrossSectionStandardShape shape)
        {
            Shape = shape;
        }

        public override bool GeometryBased => false;

        public override IEnumerable<Coordinate> Profile
        {
            get
            {
                return Shape.Profile.Select(c => new Coordinate(c.X, c.Y + LevelShift));
            }
        }

        public override IEnumerable<Coordinate> FlowProfile => Profile;

        public override CrossSectionType CrossSectionType => CrossSectionType.Standard;

        public override LightDataTable RawData => null;

        public virtual double LevelShift { get; set; }

        public virtual ICrossSectionStandardShape Shape
        {
            get => shape;
            protected set => shape = value;
        }

        public virtual CrossSectionStandardShapeType ShapeType
        {
            get => shape.Type;
            set
            {
                if (shape.Type == value)
                {
                    return;
                }

                SetShapeFromType(value);
            }
        }

        public static ICrossSectionDefinition CreateDefault()
        {
            return new CrossSectionDefinitionStandard();
        }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionStandard) base.Clone();
            clone.LevelShift = LevelShift;
            clone.Shape = (ICrossSectionStandardShape) Shape.Clone();
            return clone;
        }

        public override void ShiftLevel(double delta)
        {
            BeginEdit(new DefaultEditAction("Shift level"));
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

        [EditAction]
        private void SetShapeFromType(CrossSectionStandardShapeType shapeType)
        {
            Shape = GetDefaultShape(shapeType);
        }

        private static ICrossSectionStandardShape GetDefaultShape(CrossSectionStandardShapeType value)
        {
            switch (value)
            {
                case CrossSectionStandardShapeType.Rectangle:
                    return CrossSectionStandardShapeRectangle.CreateDefault();
                // TODO: Re-enable once Enclosed branches are supported
                case CrossSectionStandardShapeType.Round:
                    return CrossSectionStandardShapeRound.CreateDefault();
                case CrossSectionStandardShapeType.Arch:
                    return CrossSectionStandardShapeArch.CreateDefault();
                case CrossSectionStandardShapeType.Cunette:
                    return CrossSectionStandardShapeCunette.CreateDefault();
                // TODO: Re-enable once Enclosed branches are supported
                case CrossSectionStandardShapeType.Egg:
                    return CrossSectionStandardShapeEgg.CreateDefault();
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
    }
}