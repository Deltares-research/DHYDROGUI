using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class CrossSectionDefinitionFileWritersTestHelper
    {
        public static void AddMultipleCrossSections(this IList<IBranch> branches)
        {
            if (branches.Count < 4) return;
            AddCrossSectionYz(branches[0], 1, 20.0);
            AddCrossSectionXyz(branches[1], 2, 25.0,
                new[] { 585.0, 610.0, 635.0, 660.0, 685.0, 710.0 },
                new[] { 950.0, 910.0, 870.0, 830.0, 790.0, 750.0 },
                new[] { 10.0, 6.5, 2.5, 2.5, 6.5, 10.0 });


            AddCrossSectionZw(branches[2], 3, 30.0, -2.0,
                100.0, 200.0, 0.5);
            AddCulvertWithCrossSection(CulvertGeometryType.Rectangle, branches[0], 35.0,
                100.0, 80.0);
            AddCulvertWithCrossSection(CulvertGeometryType.Ellipse, branches[0], 40.0,
                100.0, 80.0);
            AddCulvertWithCrossSection(CulvertGeometryType.Round, branches[0], 45.0,
                200.0);
            AddCulvertWithCrossSection(CulvertGeometryType.Egg, branches[0], 50.0,
                150.0);
            AddCulvertWithCrossSection(CulvertGeometryType.Arch, branches[0], 55.0,
                100.0, 200.0, 150.0);
            AddCulvertWithCrossSection(CulvertGeometryType.Cunette, branches[0], 60.0,
                100.0, 200.0);
            AddCulvertWithCrossSection(CulvertGeometryType.SteelCunette, branches[0], 65.0,
                100.0, 50.0, 100.0, 50.0, 100.0, 45.0, 135.0);

            AddBridgeWithTabulatedCrossSection(branches[0], 70.0, -2.0,
                100.0, 200.0, 0.5);

            AddCulvertWithCrossSection(CulvertGeometryType.Tabulated, branches[0], 75.0,
                100.0, 200.0);

            AddCrossSectionTrapezium(branches[3], 14, 30.0, 100.0, 200.0, 150.0);

        }

        public static void AddEvenMoreMultipleCrossSections(this IList<IBranch> branches)
        {
            if (branches.Count < 9) return;
            AddCrossSectionRectangle(branches[4], 1, 45, 100, 50);
            AddCrossSectionElliptical(branches[5], 1, 45, 100, 80);
            AddCrossSectionCunette(branches[6], 3, 23, 100);
            AddCrossSectionSteelCunette(branches[7], 10, 30.0, 100.0, 50.0, 100.0, 50.0, 100.0, 45.0, 135.0);
            AddCrossSectionArch(branches[8], 42, 53, 100, 200, 150);
            //AddCrossSectionCircle(branches[9], 1, 22, 200); // Cannot test circle on a branch because the validator of the cross sections of the WaterFlowModel1D doesn't accept this!
            //AddCrossSectionEgg(branches[10], 3, 23, 100);// Cannot test circle on a branch because the validator of the cross sections of the WaterFlowModel1D doesn't accept this!
            
        }

        public static void AddCrossSectionYz(IBranch branch, int csId, double chainage)
        {
            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.YZ, csId, chainage);
        }

        public static void AddCrossSectionXyz(IBranch branch, int csId, double chainage, double[] xCoors, double[] yCoors, double[] zCoors)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.GeometryBased, csId, chainage);

            var csd = crossSection.Definition as CrossSectionDefinitionXYZ;
            if (csd != null)
            {
                var coordinates = new Coordinate[xCoors.Length];
                for (var i = 0; i < coordinates.Length; i++)
                    coordinates[i] = new Coordinate(xCoors[i], yCoors[i], zCoors[i]);

                csd.Geometry = new LineString(coordinates);  
            }
        }

        public static void AddCrossSectionZw(IBranch branch, int csId, double chainage, 
            double crestLevel, double floodSurface, double totalSurface, double floodPlainLevel)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.ZW, csId, chainage);

            var csd = crossSection.Definition as CrossSectionDefinitionZW;
            if (csd != null)
            {
                csd.SummerDike = new SummerDike
                {
                    CrestLevel = crestLevel,
                    FloodSurface = floodSurface,
                    TotalSurface = totalSurface,
                    FloodPlainLevel = floodPlainLevel
                };
            }
        }
        public static void AddCrossSectionRectangle(IBranch branch, int csId, double chainage, double width, double height)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Rectangle;
            var crossSectionDefinitionRectangleShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeRectangle;
            if (crossSectionDefinitionRectangleShape == null) return;
            crossSectionDefinitionRectangleShape.Width = width;
            crossSectionDefinitionRectangleShape.Height = height;
        }

        public static void AddCrossSectionElliptical(IBranch branch, int csId, double chainage, double width, double height)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Elliptical;
            var crossSectionDefinitionEllipticalShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeElliptical;
            if (crossSectionDefinitionEllipticalShape == null) return;
            crossSectionDefinitionEllipticalShape.Width = width;
            crossSectionDefinitionEllipticalShape.Height = height;
        }

        public static void AddCrossSectionCircle(IBranch branch, int csId, double chainage, double diameter)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Round;
            var crossSectionDefinitionCircleShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeRound;
            if (crossSectionDefinitionCircleShape == null) return;
            crossSectionDefinitionCircleShape.Diameter = diameter;
        }

        public static void AddCrossSectionEgg(IBranch branch, int csId, double chainage, double width)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Egg;
            var crossSectionDefinitionEggShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeEgg;
            if (crossSectionDefinitionEggShape == null) return;
                crossSectionDefinitionEggShape.Width = width; //don't set height this will be done automaticly
        }
        
        public static void AddCulvertWithCrossSection(CulvertGeometryType geometryType, IBranch branch, double chainage, double width, double height = 0.0, double archHeight = 0.0)
        {
            if (geometryType == CulvertGeometryType.SteelCunette) return;
            var culvert = new Culvert
            {
                Branch = branch,
                Width = width,
                Height = height,
                Diameter = width,
                ArcHeight = archHeight,
                Chainage = chainage,
                //GeometryType = geometryType,
                GroundLayerEnabled =  true,
                GroundLayerThickness = 1.2157
            };
            if (geometryType == CulvertGeometryType.Tabulated)
            {
                culvert.TabulatedCrossSectionDefinition = (CrossSectionDefinitionZW)(CreateSimpleTabulatedProfileCrossSection(branch, chainage).Definition);
            }
            culvert.Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion)branch.Network, culvert);
            culvert.GeometryType = geometryType; 
            branch.BranchFeatures.Add(culvert);
            
        }

        public static void AddCulvertWithCrossSection(CulvertGeometryType geometryType, IBranch branch, double chainage, double height, double r, double r1, double r2, double r3, double a, double a1)
        {
            if (geometryType != CulvertGeometryType.SteelCunette) return;
            var culvert = new Culvert
            {
                Branch = branch,
                Height = height,
                Radius = r,
                Radius1 = r1,
                Radius2 = r2,
                Radius3 = r3,
                Angle = a,
                Angle1 = a1,

                Chainage = chainage,
                GeometryType = geometryType,
                GroundLayerEnabled = true,
                GroundLayerThickness = 1.2157
            };
            culvert.GeometryType = geometryType;
            culvert.Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion)branch.Network, culvert);
            branch.BranchFeatures.Add(culvert);
        }

        public static void AddCrossSectionArch(IBranch branch, int csId, double chainage, double width, double height, double archHeight)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Arch;
            var crossSectionDefinitionArchShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeArch;
            if (crossSectionDefinitionArchShape == null) return;
            crossSectionDefinitionArchShape.Width = width; 
            crossSectionDefinitionArchShape.Height = height; 
            crossSectionDefinitionArchShape.ArcHeight = archHeight; 
        }

        public static void AddCrossSectionTrapezium(IBranch branch, int csId, double chainage, double slope, double maximumFlowWidth, double bottomWidth)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Trapezium;
            var crossSectionDefinitionTrapeziumShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeTrapezium;
            if (crossSectionDefinitionTrapeziumShape == null) return;
            crossSectionDefinitionTrapeziumShape.Slope = slope;
            crossSectionDefinitionTrapeziumShape.MaximumFlowWidth = maximumFlowWidth;
            crossSectionDefinitionTrapeziumShape.BottomWidthB = bottomWidth;
        }

        public static void AddCrossSectionCunette(IBranch branch, int csId, double chainage, double width)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
            var crossSectionDefinitionStandard = crossSection.Definition as CrossSectionDefinitionStandard;
            if (crossSectionDefinitionStandard == null) return;
            crossSectionDefinitionStandard.ShapeType = CrossSectionStandardShapeType.Cunette;
            var crossSectionDefinitionCunetteShape = crossSectionDefinitionStandard.Shape as CrossSectionStandardShapeCunette;
            if (crossSectionDefinitionCunetteShape == null) return;
            crossSectionDefinitionCunetteShape.Width = width;
        }

        public static void AddCrossSectionSteelCunette(IBranch branch, int csId, double chainage,
            double height, double r, double r1, double r2, double r3, double a, double a1)
        {
            var crossSection = FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.Standard, csId, chainage);
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

        public static void AddBridgeWithTabulatedCrossSection(IBranch branch,  double chainage, 
            double crestLevel, double floodSurface, double totalSurface, double floodPlainLevel)
        {
            var crossSection = CreateSimpleTabulatedProfileCrossSection(branch, chainage, crestLevel, floodSurface, totalSurface, floodPlainLevel);

            var bridge = new Bridge
            {
                Branch = branch,
                TabulatedCrossSectionDefinition = crossSection.Definition as CrossSectionDefinitionZW,
                Chainage = chainage,
                GroundLayerEnabled = true,
                GroundLayerThickness = 1.2157
            };
            bridge.Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion)branch.Network, bridge);
            branch.BranchFeatures.Add(bridge);
        }

        private static ICrossSection CreateSimpleTabulatedProfileCrossSection(IBranch branch, 
            double chainage, double crestLevel = 0.0, double floodSurface = 0.0, double totalSurface = 0.0, double floodPlainLevel = 0.0)
        {
            var crossSection = CrossSection.CreateDefault(CrossSectionType.ZW, branch, chainage);
            crossSection.Name = HydroNetworkHelper.GetUniqueFeatureName(branch.Network as HydroNetwork, crossSection);
            crossSection.Definition.Name = crossSection.Name;
            crossSection.Definition.Sections.Add(new CrossSectionSection
            {
                SectionType = new CrossSectionSectionType {Name = CrossSectionDefinitionZW.MainSectionName},
                MinY = 0.0,
                MaxY = 25.0
            });
            crossSection.Definition.Sections.Add(new CrossSectionSection
            {
                SectionType = new CrossSectionSectionType {Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName},
                MinY = 25.0,
                MaxY = 75.0
            });
            crossSection.Definition.Sections.Add(new CrossSectionSection
            {
                SectionType = new CrossSectionSectionType {Name = CrossSectionDefinitionZW.Floodplain2SectionTypeName},
                MinY = 75.0,
                MaxY = 100.0
            });

            var csd = crossSection.Definition as CrossSectionDefinitionZW;
            if (csd != null)
            {
                csd.SummerDike = new SummerDike
                {
                    CrestLevel = crestLevel,
                    FloodSurface = floodSurface,
                    TotalSurface = totalSurface,
                    FloodPlainLevel = floodPlainLevel
                };
            }
            return crossSection;
        }

        public static void WriteCrossSectionsToIni(IEnumerable<ICrossSection> crossSections)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion, 
                    GeneralRegion.CrossSectionDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.CrossSectionDefinition)
            };

            var processCsDefinitions = new List<string>();
            foreach (var crossSection in crossSections)
            {
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory
                    .GetDefinitionGeneratorCrossSection(crossSection.Definition, crossSection.CrossSectionType);
                if (definitionGeneratorCrossSectionDefinition != null)
                {
                    string csDefinitionId = crossSection.Definition.Name;
                    if (!processCsDefinitions.Contains(csDefinitionId))
                    {
                        var definitionRegion = definitionGeneratorCrossSectionDefinition
                            .CreateDefinitionRegion(crossSection.Definition);
                        categories.Add(definitionRegion);
                        processCsDefinitions.Add(csDefinitionId);
                    }
                }
            }
            
            if (File.Exists(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions)) File.Delete(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);
            new IniFileWriter().WriteIniFile(categories, FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);
        }
    }
}