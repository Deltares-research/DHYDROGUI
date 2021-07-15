using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    public class CulvertDefinitionReader : IStructureDefinitionReader
    {
        public IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key);
            var definition = crossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.InvariantCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;
            
            var culvert = new Culvert
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                GeometryType = GetGeometryType(standardCrossSectionDefinition?.ShapeType),
                TabulatedCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW 
                    ? definition as CrossSectionDefinitionZW
                    : standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition() ?? CrossSectionDefinitionZW.CreateDefault(),
                FlowDirection = (FlowDirection) category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key).GetEnumValueFromDisplayName(typeof(FlowDirection)),
                InletLevel = category.ReadProperty<double>(StructureRegion.LeftLevel.Key),
                OutletLevel = category.ReadProperty<double>(StructureRegion.RightLevel.Key),
                Length = category.ReadProperty<double>(StructureRegion.Length.Key),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key),
                IsGated = category.ReadProperty<string>(StructureRegion.ValveOnOff.Key) != "0",
                BendLossCoefficient = category.ReadProperty<double>(StructureRegion.BendLossCoef.Key, true),
                SiphonOnLevel = category.ReadProperty<double>(StructureRegion.TurnOnLevel.Key, true),
                SiphonOffLevel = category.ReadProperty<double>(StructureRegion.TurnOffLevel.Key, true),
                FrictionDataType = (Friction) Enum.Parse(typeof(Friction), category.ReadProperty<string>(StructureRegion.BedFrictionType.Key), true),
                Friction = category.ReadProperty<double>(StructureRegion.BedFriction.Key)
            };
            culvert.GateInitialOpening = category.ReadProperty<double>(StructureRegion.IniValveOpen.Key, !culvert.IsGated);

            SetCulvertDimensionsBasedOnProfile(culvert, definition);
            var numLossCoeff = category.ReadProperty<int>(StructureRegion.LossCoeffCount.Key, true);
            if (numLossCoeff > 0)
            {
                var relOpening = category.ReadProperty<string>(StructureRegion.RelativeOpening.Key).ToDoubleArray();
                var lossCoeff = category.ReadProperty<string>(StructureRegion.LossCoefficient.Key).ToDoubleArray();

                culvert.GateOpeningLossCoefficientFunction =
                    culvert.GateOpeningLossCoefficientFunction.CreateFunctionFromArrays(relOpening, lossCoeff);
            }

            culvert.CulvertType = GetCulvertType(category);
            
            return culvert;
        }

        private static CulvertType GetCulvertType(IDelftIniCategory category)
        {
            if (category.GetProperty(StructureRegion.SubType.Key)?.Value == "invertedSiphon")
            {
                return CulvertType.InvertedSiphon;
            }

            return category.GetProperty(StructureRegion.TurnOnLevel.Key) != null 
                       ? CulvertType.Siphon 
                       : CulvertType.Culvert;
        }

        private void SetCulvertDimensionsBasedOnProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            switch (culvert.GeometryType)
            {
                case CulvertGeometryType.Round:
                {
                    var stdDef = definition as CrossSectionDefinitionStandard;
                    if (stdDef != null)
                    {
                        var round = stdDef.Shape as CrossSectionStandardShapeCircle;
                        if (round != null)
                            culvert.Diameter = round.Diameter;
                    }

                    break;
                }
                case CulvertGeometryType.Rectangle:
                {
                    var stdDef = definition as CrossSectionDefinitionStandard;
                    if (stdDef != null)
                    {
                        var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
                        if (heightbase != null)
                        {
                            culvert.Width = heightbase.Width;
                            culvert.Height = heightbase.Height;
                            culvert.Closed = (heightbase as ICrossSectionStandardShapeOpenClosed)?.Closed ?? false;
                        }
                    }
                }
                    break;
                case CulvertGeometryType.Egg:
                case CulvertGeometryType.InvertedEgg:
                case CulvertGeometryType.Cunette:
                case CulvertGeometryType.Ellipse:
                {
                    var stdDef = definition as CrossSectionDefinitionStandard;
                    if (stdDef != null)
                    {
                        var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
                        if (heightbase != null)
                        {
                            culvert.Width = heightbase.Width;
                            culvert.Height = heightbase.Height;
                        }
                    }
                }
                    break;
                case CulvertGeometryType.Arch:
                case CulvertGeometryType.UShape:
                {
                    var stdDef = definition as CrossSectionDefinitionStandard;
                    if (stdDef != null)
                    {
                        var arch = stdDef.Shape as CrossSectionStandardShapeArch;
                        if (arch != null)
                        {
                            culvert.Width = arch.Width;
                            culvert.Height = arch.Height;
                            culvert.ArcHeight = arch.ArcHeight;
                        }
                    }
                }
                    break;
                case CulvertGeometryType.SteelCunette:
                {
                    var stdDef = definition as CrossSectionDefinitionStandard;
                    if (stdDef != null)
                    {
                        var steelcunette = stdDef.Shape as CrossSectionStandardShapeSteelCunette;
                        if (steelcunette != null)
                        {
                            culvert.Angle = steelcunette.AngleA;
                            culvert.Angle1 = steelcunette.AngleA1;
                            culvert.Height = steelcunette.Height;
                            culvert.Radius = steelcunette.RadiusR;
                            culvert.Radius1 = steelcunette.RadiusR1;
                            culvert.Radius2 = steelcunette.RadiusR2;
                            culvert.Radius3 = steelcunette.RadiusR3;
                        }
                    }
                }
                    break;
                case CulvertGeometryType.Tabulated:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CulvertGeometryType GetGeometryType(CrossSectionStandardShapeType? standardCrossSectionDefinition)
        {
            switch (standardCrossSectionDefinition)
            {
                case CrossSectionStandardShapeType.Rectangle: 
                    return CulvertGeometryType.Rectangle;

                case CrossSectionStandardShapeType.Arch: 
                    return CulvertGeometryType.Arch;
                
                case CrossSectionStandardShapeType.Cunette:
                    return CulvertGeometryType.Cunette;

                case CrossSectionStandardShapeType.Elliptical:
                    return CulvertGeometryType.Ellipse;

                case CrossSectionStandardShapeType.SteelCunette:
                    return CulvertGeometryType.SteelCunette;

                case CrossSectionStandardShapeType.Egg:
                    return CulvertGeometryType.Egg;

                case CrossSectionStandardShapeType.Circle:
                    return CulvertGeometryType.Round;

                case CrossSectionStandardShapeType.InvertedEgg:
                    return CulvertGeometryType.InvertedEgg;

                case CrossSectionStandardShapeType.UShape:
                    return CulvertGeometryType.UShape;
                case null:
                    return CulvertGeometryType.Tabulated;
                default:
                    throw new ArgumentOutOfRangeException(nameof(standardCrossSectionDefinition), standardCrossSectionDefinition, null);
            }
        }
    }
}