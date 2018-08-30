using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Definition
{
    public interface IDefinitionReader
    {
        ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category);
        IStructure1D ReadStructureDefinition(IDelftIniCategory category);
    }

    abstract class DefinitionReader : IDefinitionReader
    {
        public virtual ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category){return null;}
        public virtual IStructure1D ReadStructureDefinition(IDelftIniCategory category){ return null; }

        protected void SetCommonCrossSectionDefinitionsProperties(ICrossSectionDefinition crossSectionDefinition, IDelftIniCategory category)
        {
            crossSectionDefinition.Name = category.ReadProperty<string>(DefinitionPropertySettings.Id.Key);
            crossSectionDefinition.Thalweg = category.ReadProperty<double>(DefinitionPropertySettings.Thalweg.Key);
        }

        protected void SetCommonStructureDefinitionsProperties(IStructure1D structureDefinition, IDelftIniCategory category)
        {
            
        }

    }

    class CSDYZDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {
            var crossSectionDefinition = new CrossSectionDefinitionYZ();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition,category);
            
            var yList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.YValues.Key);
            var zList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.ZValues.Key);
            var deltaZList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.DeltaZStorage.Key);
            
            var yzCount = category.ReadProperty<int>(DefinitionPropertySettings.YZCount.Key);

            if (yzCount == yList.Count && yList.Count != zList.Count && zList.Count != deltaZList.Count)
            {
                var errorMessage = "yz count property is not equal to number of yvalues or zvalues or delta z storage";
                throw new FileReadingException(errorMessage);
            }

            var table = new FastYZDataTable();
            table.BeginLoadData();
            for (int i = 0; i < yList.Count; i++)
            {
                table.AddCrossSectionYZRow(yList[i], zList[i], deltaZList[i]);
            }
            table.EndLoadData();
            crossSectionDefinition.YZDataTable = table;
            return crossSectionDefinition;
        }
    }
    class CSDXYZDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {
            var crossSectionDefinition = new CrossSectionDefinitionXYZ();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            var xCoorList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.XCoors.Key);
            var yCoorList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.YCoors.Key);
            var zCoorList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.ZCoors.Key);
            
            var deltaZList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.DeltaZStorage.Key);

            var xyzCount = category.ReadProperty<int>(DefinitionPropertySettings.XYZCount.Key);

            if (xyzCount != deltaZList.Count || xyzCount != xCoorList.Count || xyzCount != yCoorList.Count ||
                xyzCount != zCoorList.Count)
            {
                var errorMessage = "xyz count property is not equal to number of x, y or z coordinates or delta z storage"; 
                throw new FileReadingException(errorMessage);
            }

            var geometryCoors = new Coordinate[xyzCount];
            for (var i = 0; i < xyzCount; i++)
            {
                geometryCoors[i] = new Coordinate(xCoorList[i], yCoorList[i], zCoorList[i]);
            }

            crossSectionDefinition.Geometry = new LineString(geometryCoors);

            //then add the deltaZStorage
            for (var i = 0; i < xyzCount; i++)
            {
                crossSectionDefinition.XYZDataTable[i].DeltaZStorage = deltaZList[i];
            }
            
            return crossSectionDefinition;
        }
    }

    class CSDZWDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {
            var crossSectionDefinition = new CrossSectionDefinitionZW();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            var numLevels = category.ReadProperty<int>(DefinitionPropertySettings.NumLevels.Key);
            var levels = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.Levels.Key);
            var flowWidths = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FlowWidths.Key);
            var totalWidths = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.TotalWidths.Key);

            if (numLevels == levels.Count
                && numLevels == flowWidths.Count
                && numLevels == totalWidths.Count)
            {
                var table = new FastZWDataTable();
                table.BeginLoadData();
                for (int i = 0; i < numLevels; i++)
                {
                    var storageWidth = totalWidths[i] - flowWidths[i];
                    table.AddCrossSectionZWRow(levels[i], totalWidths[i], storageWidth);
                }
                table.EndLoadData();
                crossSectionDefinition.ZWDataTable = table;   
            }
            else
            {
                var errorMessage = "num levels count property is not equal to number of level, flowWidths or totalWidths"; 
                throw new FileReadingException(errorMessage);
            }
            // summer dike
            var crestLevel = category.ReadProperty<double>(DefinitionPropertySettings.CrestSummerdike.Key);
            var flowArea = category.ReadProperty<double>(DefinitionPropertySettings.FlowAreaSummerdike.Key);
            var totalArea = category.ReadProperty<double>(DefinitionPropertySettings.TotalAreaSummerdike.Key);
            var baseLevel = category.ReadProperty<double>(DefinitionPropertySettings.BaseLevelSummerdike.Key);
            
            if (Math.Abs(flowArea) > double.Epsilon && Math.Abs(totalArea) > double.Epsilon)//(flowArea and totalArea are larger than 0, so you can do something with this
            {
                crossSectionDefinition.SummerDike = new SummerDike()
                {
                    Active = true,
                    CrestLevel = crestLevel,
                    FloodPlainLevel = baseLevel,
                    FloodSurface = flowArea,
                    TotalSurface = totalArea
                };
            }
            
            return crossSectionDefinition;
        }
    }

    class CSDRectangleDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {
            
            var width = category.ReadProperty<double>(DefinitionPropertySettings.RectangleWidth.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.RectangleHeight.Key);
            //var closed = category.ReadProperty<int>(DefinitionPropertySettings.Closed.Key); // NO CLUE!!
            
            var shape = new CrossSectionStandardShapeRectangle {Height = height, Width = width};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }

    class CSDEllipseDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {

            var width = category.ReadProperty<double>(DefinitionPropertySettings.EllipseWidth.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.EllipseHeight.Key);
            
            var shape = new CrossSectionStandardShapeElliptical{ Height = height, Width = width  };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            
            return crossSectionDefinition;
        }
    }
    class CSDCircleDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {

            var diameter = category.ReadProperty<double>(DefinitionPropertySettings.Diameter.Key);
            
            var shape = new CrossSectionStandardShapeCircle{ Diameter = diameter};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
    class CSDEggDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {

            var width = category.ReadProperty<double>(DefinitionPropertySettings.EggWidth.Key);

            var shape = new CrossSectionStandardShapeEgg { Width = width };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
    class CSDArchDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {

            var archHeight = category.ReadProperty<double>(DefinitionPropertySettings.ArchHeight.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            var width = category.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionWidth.Key);

            var shape = new CrossSectionStandardShapeArch { ArcHeight = archHeight, Width = width, Height = height};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
    class CSDCunetteDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {
            var width = category.ReadProperty<double>(DefinitionPropertySettings.CunetteWidth.Key);

            var shape = new CrossSectionStandardShapeCunette{ Width = width };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            return crossSectionDefinition;
        }
    }
    class CSDSteelCunetteDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {

            var height = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteHeight.Key);
            var radiusR = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR.Key);
            var radiusR1 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR1.Key);
            var radiusR2 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR2.Key);
            var radiusR3 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR3.Key);
            
            var angleA = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteA.Key);
            var angleA1 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteA1.Key);
            
            var shape = new CrossSectionStandardShapeSteelCunette {Height = height, 
                RadiusR = radiusR,
                RadiusR1 = radiusR1,
                RadiusR2 = radiusR2,
                RadiusR3 = radiusR3,
                AngleA = angleA,
                AngleA1 = angleA1
            };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            return crossSectionDefinition;
        }
    }
    class CSDTrapeziumDefinitionReader : DefinitionReader
    {
        public override ICrossSectionDefinition ReadCrossSectionDefinition(IDelftIniCategory category)
        {

            var slope = category.ReadProperty<double>(DefinitionPropertySettings.Slope.Key);
            var bottomWidth = category.ReadProperty<double>(DefinitionPropertySettings.BottomWidth.Key);
            var maximumFlowWidth = category.ReadProperty<double>(DefinitionPropertySettings.MaximumFlowWidth.Key);

            var shape = new CrossSectionStandardShapeTrapezium { Slope = slope, BottomWidthB = bottomWidth, MaximumFlowWidth = maximumFlowWidth };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            return crossSectionDefinition;
        }
    }
    class WeirDefinitionReader : DefinitionReader
    {
        public override IStructure1D ReadStructureDefinition(IDelftIniCategory category)
        {

            var slope = category.ReadProperty<double>(DefinitionPropertySettings.Slope.Key);
            var bottomWidth = category.ReadProperty<double>(DefinitionPropertySettings.BottomWidth.Key);
            var maximumFlowWidth = category.ReadProperty<double>(DefinitionPropertySettings.MaximumFlowWidth.Key);

            var shape = new CrossSectionStandardShapeTrapezium { Slope = slope, BottomWidthB = bottomWidth, MaximumFlowWidth = maximumFlowWidth };
            var structureDefinition = new Weir();
            SetCommonStructureDefinitionsProperties(structureDefinition, category);
            return structureDefinition;
        }
    }

}

