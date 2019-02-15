using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class CompositeBranchStructureConverter
    {
        private readonly Func<string, IStructureConverter> getTypeConverterFunc;

        private readonly
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure>
            getCompositeBranchStructureFunc;

        public CompositeBranchStructureConverter() : this(StructureConverterFactory.GetStructureConverter,
            BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded)
        {
        }

        public CompositeBranchStructureConverter(Func<string, IStructureConverter> getTypeConverter,
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure>
                getCompositeBranchStructureFunc)
        {
            if (getTypeConverter != null) getTypeConverterFunc = getTypeConverter;
            else throw new ArgumentException("getTypeConverterFunc cannot be null.");

            if (getCompositeBranchStructureFunc != null)
                this.getCompositeBranchStructureFunc = getCompositeBranchStructureFunc;
            else throw new ArgumentException("getCompositeBranchStructureFunc cannot be null.");
        }

        public IList<ICompositeBranchStructure> Convert(IList<DelftIniCategory> categories,
            IList<IChannel> channels,
            IList<ICrossSectionDefinition> crossSectionDefinitions,
            GroundLayerDTO[] groundLayerDataTransferObject,
            List<string> warningMessages)
        {
            IList<ICompositeBranchStructure> compositeBranchStructures = new List<ICompositeBranchStructure>();

            // Do this in two steps and first the real composite branch structures (multiple structures at one location), 
            // so that the real composite branch structures names will not be adjusted.
            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header && System.Convert.ToInt32((category.GetPropertyValue(StructureRegion.Compound.Key))) != 0))
            {
                CreationOfStructuresAndCompositeBranchStructures(structureBranchCategory, channels, crossSectionDefinitions, groundLayerDataTransferObject, compositeBranchStructures, warningMessages);
            }
            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header && System.Convert.ToInt32((category.GetPropertyValue(StructureRegion.Compound.Key))) == 0))
            {
                CreationOfStructuresAndCompositeBranchStructures(structureBranchCategory, channels, crossSectionDefinitions, groundLayerDataTransferObject, compositeBranchStructures, warningMessages);
            }
            return compositeBranchStructures;
        }

        private void CreationOfStructuresAndCompositeBranchStructures(DelftIniCategory structureBranchCategory,
            IEnumerable<IBranch> channels, 
            IEnumerable<ICrossSectionDefinition> crossSectionDefinitions,
            GroundLayerDTO[] groundLayerDataTransferObject,
            IList<ICompositeBranchStructure> compositeBranchStructures,
            IList<string> warningMessages)   
        {
            try
            {
                var type = structureBranchCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);
                var branchName = structureBranchCategory.ReadProperty<string>(StructureRegion.BranchId.Key);
                var channel = channels.FirstOrDefault(c => c.Name == branchName);

                var converter = getTypeConverterFunc.Invoke(type);

                if (converter == null)
                {
                    throw new Exception(string.Format(
                        Resources.CompositeBranchStructureConverter_CreationOfStructuresAndCompositeBranchStructures_A__0__is_found_in_the_structure_file__line__1___and_this_type_is_not_supported_during_an_import_,
                        type, structureBranchCategory.LineNumber));
                }

                var structure = converter.ConvertToStructure1D(structureBranchCategory, channel, warningMessages);
                
                if (structure == null)
                {
                    throw new Exception(string.Format(
                        "Failed to create a structure from the structures file (line {0})",
                        structureBranchCategory.LineNumber));
                }

                if (structure is Culvert culvert)
                {
                    var crossSectionDefinitionId = structureBranchCategory.ReadProperty<string>(StructureRegion.CsDefId.Key);
                    var matchingCrossSectionDefinition = crossSectionDefinitions.FirstOrDefault(csd => csd.Name == crossSectionDefinitionId);
                    if (matchingCrossSectionDefinition != null)
                    {
                        SetCulvertCrossSectionDefinitionProperties(culvert, matchingCrossSectionDefinition);
                    }

                    var matchingGroundLayerData = groundLayerDataTransferObject.FirstOrDefault(g => g.CrossSectionDefinitionId == crossSectionDefinitionId);
                    SetGroundLayerProperties(matchingGroundLayerData, culvert);
                }

                if (structure is Bridge bridge)
                {
                    var crossSectionDefinitionId = structureBranchCategory.ReadProperty<string>(StructureRegion.CsDefId.Key, true);
                    if (crossSectionDefinitionId != null)
                    {
                        var matchingCrossSectionDefinition = crossSectionDefinitions.FirstOrDefault(csd => csd.Name == crossSectionDefinitionId);
                        if (matchingCrossSectionDefinition != null)
                        {
                            SetBridgeCrossSectionDefinitionProperties(matchingCrossSectionDefinition, bridge);
                        }

                        var matchingGroundLayerData = groundLayerDataTransferObject.FirstOrDefault(g => g.CrossSectionDefinitionId == crossSectionDefinitionId);
                        SetGroundLayerProperties(matchingGroundLayerData, bridge);
                    }
                }

                var compositeBranchStructure = getCompositeBranchStructureFunc.Invoke(structureBranchCategory,
                    structure, compositeBranchStructures);

                if (compositeBranchStructure == null)
                {
                    throw new Exception(string.Format(
                        "Failed to create structure {0} from the structures file (line {1})", structure.Name,
                        structureBranchCategory.LineNumber));
                }

                HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, structure);
            }
            catch (Exception e)
            {
                warningMessages.Add(e.Message);
            }
        }

        private static void SetGroundLayerProperties(GroundLayerDTO matchingGroundLayerData, IGroundLayer culvert)
        {
            if (matchingGroundLayerData != null)
            {
                culvert.GroundLayerEnabled = matchingGroundLayerData.GroundLayerUsed;
                if (matchingGroundLayerData.GroundLayerUsed)
                {
                    culvert.GroundLayerThickness = matchingGroundLayerData.GroundLayerThickness;
                }
                else
                {
                    culvert.GroundLayerRoughness = 0.0;
                    culvert.GroundLayerThickness = 0.0;
                }
            }
        }

        private static void SetBridgeCrossSectionDefinitionProperties(ICrossSectionDefinition crossSectionDefinition, Bridge bridge)
        {
            switch (crossSectionDefinition)
            {
                case CrossSectionDefinitionStandard crossSectionDefinitionStandard:
                    SetBridgeCrossSectionDefinitionStandardProperties(bridge, crossSectionDefinitionStandard);
                    break;
                case CrossSectionDefinitionZW crossSectionDefinitionZw:
                    bridge.BridgeType = BridgeType.Tabulated;
                    bridge.TabulatedCrossSectionDefinition = crossSectionDefinitionZw;
                    break;
            }
        }

        private static void SetBridgeCrossSectionDefinitionStandardProperties(IBridge bridge, CrossSectionDefinitionStandard crossSectionDefinitionStandard)
        {
            if (crossSectionDefinitionStandard.ShapeType == CrossSectionStandardShapeType.Rectangle)
            {
                var rectangleShape = (CrossSectionStandardShapeRectangle) crossSectionDefinitionStandard.Shape;
                bridge.BridgeType = BridgeType.Rectangle;
                bridge.Width = rectangleShape.Width;
                bridge.Height = rectangleShape.Height;
            }
            else
            {
                throw new Exception(string.Format(
                    Resources.CompositeBranchStructureConverter_SetBridgeCrossSectionDefinition_Bridge___0___references_cross_section_definition___1___with_shape_type___2____Only_shape_types___3___or_tabulated_are_supported_for_Bridges__so_Bridge___4___was_not_imported_,
                    bridge.Name, crossSectionDefinitionStandard.Name, crossSectionDefinitionStandard.ShapeType, CrossSectionStandardShapeType.Rectangle, bridge.Name));
            }
        }

        private static void SetCulvertCrossSectionDefinitionProperties(Culvert culvert, ICrossSectionDefinition crossSectionDefinition)
        {
            switch (crossSectionDefinition)
            {
                case CrossSectionDefinitionStandard crossSectionDefinitionStandard:
                    SetCulvertCrossSectionDefinitionStandardProperties(culvert, crossSectionDefinitionStandard);
                    break;
                case CrossSectionDefinitionZW crossSectionDefinitionZw:
                    culvert.GeometryType = CulvertGeometryType.Tabulated;
                    culvert.TabulatedCrossSectionDefinition = crossSectionDefinitionZw;
                    break;
            }
        }

        private static void SetCulvertCrossSectionDefinitionStandardProperties(ICulvert culvert, CrossSectionDefinitionStandard crossSectionDefinitionStandard)
        {
            switch (crossSectionDefinitionStandard.ShapeType)
            {
                case CrossSectionStandardShapeType.Round:
                    var roundShape = (CrossSectionStandardShapeRound) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.Round;
                    culvert.Diameter = roundShape.Diameter;
                    break;
                case CrossSectionStandardShapeType.Rectangle:
                    var rectangleShape = (CrossSectionStandardShapeRectangle) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.Rectangle;
                    culvert.Width = rectangleShape.Width;
                    culvert.Height = rectangleShape.Height;
                    break;
                case CrossSectionStandardShapeType.Arch:
                    var archShape = (CrossSectionStandardShapeArch) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.Arch;
                    culvert.Width = archShape.Width;
                    culvert.Height = archShape.Height;
                    culvert.ArcHeight = archShape.ArcHeight;
                    break;
                case CrossSectionStandardShapeType.Cunette:
                    var cunetteShape = (CrossSectionStandardShapeCunette) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.Cunette;
                    culvert.Width = cunetteShape.Width;
                    break;
                case CrossSectionStandardShapeType.Elliptical:
                    var ellipticalShape =
                        (CrossSectionStandardShapeElliptical) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.Ellipse;
                    culvert.Width = ellipticalShape.Width;
                    culvert.Height = ellipticalShape.Height;
                    break;
                case CrossSectionStandardShapeType.SteelCunette:
                    var steelCunetteShape =
                        (CrossSectionStandardShapeSteelCunette) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.SteelCunette;
                    culvert.Height = steelCunetteShape.Height;
                    culvert.Radius = steelCunetteShape.RadiusR;
                    culvert.Radius1 = steelCunetteShape.RadiusR1;
                    culvert.Radius2 = steelCunetteShape.RadiusR2;
                    culvert.Radius3 = steelCunetteShape.RadiusR3;
                    culvert.Angle = steelCunetteShape.AngleA;
                    culvert.Angle1 = steelCunetteShape.AngleA1;
                    break;
                case CrossSectionStandardShapeType.Egg:
                    var eggShape = (CrossSectionStandardShapeEgg) crossSectionDefinitionStandard.Shape;
                    culvert.GeometryType = CulvertGeometryType.Egg;
                    culvert.Width = eggShape.Width;
                    break;
                default:
                    throw new Exception(
                        string.Format(
                            Resources.CompositeBranchStructureConverter_SetCulvertCrossSectionDefinitionStandardProperties_Culvert___0___references_cross_section_definition___1___with_shape_type___2____which_is_not_supported_for_Culverts__So_Culvert___3___was_not_imported_,
                            culvert.Name, crossSectionDefinitionStandard.Name, crossSectionDefinitionStandard.ShapeType, culvert.Name));
            }
        }
    }
}