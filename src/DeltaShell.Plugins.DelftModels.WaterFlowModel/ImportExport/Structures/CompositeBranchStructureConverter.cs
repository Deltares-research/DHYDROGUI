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
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
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
            GroundLayerDataTransferObject[] groundLayerDataTransferObject,
            List<string> errorMessages)
        {
            IList<ICompositeBranchStructure> compositeBranchStructures = new List<ICompositeBranchStructure>();

            // Do this in two steps and first the real composite branch structures (multiple structures at one location), 
            // so that the real composite branch structures names will not be adjusted.
            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header && System.Convert.ToInt32((category.GetPropertyValue(StructureRegion.Compound.Key))) != 0))
            {
                CreationOfStructuresAndCompositeBranchStructures(structureBranchCategory, channels, crossSectionDefinitions, groundLayerDataTransferObject, compositeBranchStructures, errorMessages);
            }
            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header && System.Convert.ToInt32((category.GetPropertyValue(StructureRegion.Compound.Key))) == 0))
            {
                CreationOfStructuresAndCompositeBranchStructures(structureBranchCategory, channels, crossSectionDefinitions, groundLayerDataTransferObject, compositeBranchStructures, errorMessages);
            }
            return compositeBranchStructures;
        }

        private void CreationOfStructuresAndCompositeBranchStructures(DelftIniCategory structureBranchCategory,
            IEnumerable<IBranch> channels, 
            IEnumerable<ICrossSectionDefinition> crossSectionDefinitions,
            GroundLayerDataTransferObject[] groundLayerDataTransferObject,
            IList<ICompositeBranchStructure> compositeBranchStructures,
            ICollection<string> errorMessages)   
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

                var structure = converter.ConvertToStructure1D(structureBranchCategory, channel);
                if (structure is Culvert culvert)
                {
                    var crossSectionDefinitionId = structureBranchCategory.ReadProperty<string>(StructureRegion.CsDefId.Key);
                    var matchingCrossSectionDefinition = crossSectionDefinitions.FirstOrDefault(csd => csd.Name == crossSectionDefinitionId);
                    if (matchingCrossSectionDefinition != null)
                    {
                        SetCulvertCrossSectionDefinition(matchingCrossSectionDefinition, culvert);
                    }

                    var matchingGroundLayerData = groundLayerDataTransferObject.FirstOrDefault(g => g.CrossSectionDefinitionId == crossSectionDefinitionId);
                    if (matchingGroundLayerData != null)
                    {
                        culvert.GroundLayerEnabled = matchingGroundLayerData.GroundLayerUsed;
                        culvert.GroundLayerThickness = matchingGroundLayerData.GroundLayerThickness;
                    }
                }

                if (structure == null)
                {
                    throw new Exception(string.Format(
                        "Failed to create a structure from the structures file (line {0})",
                        structureBranchCategory.LineNumber));
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
                errorMessages.Add(e.Message);
            }
        }

        private static void SetCulvertCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, Culvert culvert)
        {
            switch (crossSectionDefinition)
            {
                case CrossSectionDefinitionStandard crossSectionDefinitionStandard:
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
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case CrossSectionDefinitionZW crossSectionDefinitionZw:
                    culvert.GeometryType = CulvertGeometryType.Tabulated;
                    culvert.TabulatedCrossSectionDefinition = crossSectionDefinitionZw;
                    break;
            }
        }
    }
}