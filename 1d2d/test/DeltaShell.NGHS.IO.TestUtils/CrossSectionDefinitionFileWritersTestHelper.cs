using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class CrossSectionDefinitionFileWritersTestHelper
    {
        public static void AddCrossSectionYz(IBranch branch, double chainage)
        {
            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.YZ, chainage);
        }

        public static void AddCrossSectionXyz(IBranch branch, double chainage, double[] xCoors, double[] yCoors, double[] zCoors)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.GeometryBased, chainage);

            var csd = crossSection.Definition as CrossSectionDefinitionXYZ;
            if (csd != null)
            {
                var coordinates = new Coordinate[xCoors.Length];
                for (var i = 0; i < coordinates.Length; i++)
                    coordinates[i] = new Coordinate(xCoors[i], yCoors[i], zCoors[i]);

                csd.Geometry = new LineString(coordinates);  
            }
        }

        public static void AddCrossSectionZw(IBranch branch, double chainage, 
            double crestLevel, double floodSurface, double totalSurface, double floodPlainLevel)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.ZW, chainage);
            var csd = crossSection.Definition as CrossSectionDefinitionZW;

            if (csd == null) return;

            // Adjust the section widths to match the maximum flow width of the cross section
            var sectionsTotalWidth = csd.SectionsTotalWidth();
            var flowWidth = csd.FlowWidth();
            if (!sectionsTotalWidth.Equals(flowWidth))
            {
                var factor = flowWidth / sectionsTotalWidth;
                csd.Sections.ForEach(section =>
                {
                    section.MinY *= factor;
                    section.MaxY *= factor;
                });
            }

            csd.SummerDike = new SummerDike
            {
                CrestLevel = crestLevel,
                FloodSurface = floodSurface,
                TotalSurface = totalSurface,
                FloodPlainLevel = floodPlainLevel
            };
        }
        public static void AddCrossSectionRectangle(IBranch branch, double chainage, double width,
            double height, bool isClosed = true)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Rectangle;
            var crossSectionDefinitionRectangleShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeRectangle;
            if (crossSectionDefinitionRectangleShape == null) return;
            crossSectionDefinitionRectangleShape.Width = width;
            crossSectionDefinitionRectangleShape.Height = height;
            crossSectionDefinitionRectangleShape.Closed = isClosed;
        }

        public static void AddCrossSectionElliptical(IBranch branch, double chainage, double width, double height)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Elliptical;
            var crossSectionDefinitionEllipticalShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeElliptical;
            if (crossSectionDefinitionEllipticalShape == null) return;
            crossSectionDefinitionEllipticalShape.Width = width;
            crossSectionDefinitionEllipticalShape.Height = height;
        }

        public static void AddCrossSectionCircle(IBranch branch, double chainage, double diameter)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Circle;
            var crossSectionDefinitionCircleShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeCircle;
            if (crossSectionDefinitionCircleShape == null) return;
            crossSectionDefinitionCircleShape.Diameter = diameter;
        }

        public static void AddCrossSectionEgg(IBranch branch, double chainage, double width)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Egg;
            var crossSectionDefinitionEggShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeEgg;
            if (crossSectionDefinitionEggShape == null) return;
                crossSectionDefinitionEggShape.Width = width; //don't set height this will be done automaticly
        }

        public static void AddCrossSectionArch(IBranch branch, double chainage, double width, double height, double archHeight)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Arch;
            var crossSectionDefinitionArchShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeArch;
            if (crossSectionDefinitionArchShape == null) return;
            crossSectionDefinitionArchShape.Width = width; 
            crossSectionDefinitionArchShape.Height = height; 
            crossSectionDefinitionArchShape.ArcHeight = archHeight; 
        }

        public static void AddCrossSectionTrapezium(IBranch branch, double chainage, double slope, double maximumFlowWidth, double bottomWidth)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Trapezium;
            var crossSectionDefinitionTrapeziumShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeTrapezium;
            if (crossSectionDefinitionTrapeziumShape == null) return;
            crossSectionDefinitionTrapeziumShape.Slope = slope;
            crossSectionDefinitionTrapeziumShape.MaximumFlowWidth = maximumFlowWidth;
            crossSectionDefinitionTrapeziumShape.BottomWidthB = bottomWidth;
        }

        public static void AddCrossSectionCunette(IBranch branch, double chainage, double width)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Cunette;
            var crossSectionDefinitionCunetteShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeCunette;
            if (crossSectionDefinitionCunetteShape == null) return;
            crossSectionDefinitionCunetteShape.Width = width;
        }

        public static void AddCrossSectionSteelCunette(IBranch branch, double chainage,
            double height, double r, double r1, double r2, double r3, double a, double a1)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.SteelCunette;
            var crossSectionDefinitionSteelCunetteShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeSteelCunette;
            if (crossSectionDefinitionSteelCunetteShape == null) return;
            crossSectionDefinitionSteelCunetteShape.Height = height;
            crossSectionDefinitionSteelCunetteShape.RadiusR = r;
            crossSectionDefinitionSteelCunetteShape.RadiusR1 = r1;
            crossSectionDefinitionSteelCunetteShape.RadiusR2 = r2;
            crossSectionDefinitionSteelCunetteShape.RadiusR3 = r3;
            crossSectionDefinitionSteelCunetteShape.AngleA = a;
            crossSectionDefinitionSteelCunetteShape.AngleA1 = a1;
        }

        public static void WriteCrossSectionsToIni(IEnumerable<ICrossSection> crossSections)
        {
            var iniSection = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion, 
                    GeneralRegion.CrossSectionDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.CrossSectionDefinition)
            };

            var processCsDefinitions = new List<string>();
            foreach (var crossSection in crossSections)
            {
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory
                    .GetDefinitionGeneratorCrossSection(crossSection.Definition);
                if (definitionGeneratorCrossSectionDefinition != null)
                {
                    string csDefinitionId = crossSection.Definition.Name;
                    if (!processCsDefinitions.Contains(csDefinitionId))
                    {
                        var definitionRegion = definitionGeneratorCrossSectionDefinition
                            .CreateDefinitionRegion(crossSection.Definition, true, "");
                        iniSection.Add(definitionRegion);
                        processCsDefinitions.Add(csDefinitionId);
                    }
                }
            }
            
            if (File.Exists(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions)) File.Delete(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);
            new IniFileWriter().WriteIniFile(iniSection, FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);
        }
    }
}