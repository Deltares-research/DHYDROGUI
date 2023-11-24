using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for bridges.
    /// </summary>
    public class CulvertDefinitionParser : CrossSectionDependentStructureParserBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CulvertDefinitionParser));

        private const string invertedSiphonTypeName = "invertedSiphon";
        private readonly ITimeSeriesFileReader fileReader;
        private readonly string structuresFilePath;
        private readonly DateTime referenceDateTime;

        /// <summary>
        /// Initializes a new instance of <see cref="CulvertDefinitionParser"/>.
        /// </summary>
        /// <param name="fileReader">The file reader</param>
        /// <param name="structureType">The structure type.</param>
        /// <param name="iniSection">The <see cref="IniSection"/> to parse a structure from.</param>
        /// <param name="crossSectionDefinitions">A collection of cross-section definitions.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilePath">The path to the structures file.</param>
        /// <param name="referenceDateTime">The reference time date.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public CulvertDefinitionParser(ITimeSeriesFileReader fileReader,
                                       StructureType structureType,
                                       IniSection iniSection,
                                       ICollection<ICrossSectionDefinition> crossSectionDefinitions,
                                       IBranch branch,
                                       string structuresFilePath,
                                       DateTime referenceDateTime)
            : base(structureType, iniSection, crossSectionDefinitions, branch, Path.GetFileName(structuresFilePath))
        {
            Ensure.NotNull(fileReader, nameof(fileReader));

            this.fileReader = fileReader;
            this.structuresFilePath = structuresFilePath;
            this.referenceDateTime = referenceDateTime;
        }

        protected override IStructure1D Parse()
        {
            var crossSectionDefinitionId = IniSection.ReadProperty<string>(StructureRegion.CsDefId.Key);
            var definition = CrossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.InvariantCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;

            var culvert = new Culvert
            {
                Name = IniSection.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = IniSection.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(IniSection.ReadProperty<double>(StructureRegion.Chainage.Key)),
                GeometryType = GetGeometryType(standardCrossSectionDefinition?.ShapeType),
                TabulatedCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW
                    ? definition as CrossSectionDefinitionZW
                    : standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition() ?? CrossSectionDefinitionZW.CreateDefault(),
                FlowDirection = EnumUtils.GetEnumValueByDescription<FlowDirection>(IniSection.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key)),
                InletLevel = IniSection.ReadProperty<double>(StructureRegion.LeftLevel.Key),
                OutletLevel = IniSection.ReadProperty<double>(StructureRegion.RightLevel.Key),
                Length = IniSection.ReadProperty<double>(StructureRegion.Length.Key),
                InletLossCoefficient = IniSection.ReadProperty<double>(StructureRegion.InletLossCoeff.Key),
                OutletLossCoefficient = IniSection.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key),
                IsGated = IniSection.ReadProperty<string>(StructureRegion.ValveOnOff.Key) != "0",
                BendLossCoefficient = IniSection.ReadProperty<double>(StructureRegion.BendLossCoef.Key, true),
                FrictionDataType = (Friction)Enum.Parse(typeof(Friction), IniSection.ReadProperty<string>(StructureRegion.BedFrictionType.Key), true),
                Friction = IniSection.ReadProperty<double>(StructureRegion.BedFriction.Key)
            };
            SetGateInitialOpening(culvert);

            SetCulvertDimensionsBasedOnProfile(culvert, definition);
            var numLossCoeff = IniSection.ReadProperty<int>(StructureRegion.LossCoeffCount.Key, true);
            if (numLossCoeff > 0)
            {
                var relOpening = IniSection.ReadProperty<string>(StructureRegion.RelativeOpening.Key).ToDoubleArray();
                var lossCoeff = IniSection.ReadProperty<string>(StructureRegion.LossCoefficient.Key).ToDoubleArray();

                culvert.GateOpeningLossCoefficientFunction =
                    culvert.GateOpeningLossCoefficientFunction.CreateFunctionFromArrays(relOpening, lossCoeff);
            }

            culvert.CulvertType = string.Equals(IniSection.GetPropertyValue(StructureRegion.SubType.Key), invertedSiphonTypeName, StringComparison.InvariantCultureIgnoreCase)
                                      ? CulvertType.InvertedSiphon
                                      : CulvertType.Culvert;

            return culvert;
        }

        private void SetGateInitialOpening(ICulvert culvert)
        {
            var gateInitialOpeningValue = IniSection.ReadProperty<string>(StructureRegion.IniValveOpen.Key, !culvert.IsGated, defaultValue: null);

            if (gateInitialOpeningValue == null)
            {
                return;
            }

            if (fileReader.IsTimeSeriesProperty(gateInitialOpeningValue))
            {
                ReadGateInitialOpeningTimeSeries(culvert, gateInitialOpeningValue);
            }
            else
            { 
                culvert.GateInitialOpening = 
                    IniSection.ReadProperty<double>(StructureRegion.IniValveOpen.Key, !culvert.IsGated);
            }
        }

        private void ReadGateInitialOpeningTimeSeries(ICulvert culvert, string relativeGateInitialOpeningPath)
        {
            string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, relativeGateInitialOpeningPath);
            culvert.UseGateInitialOpeningTimeSeries = true;

            try
            {
                fileReader.Read(relativeGateInitialOpeningPath, filePath, new StructureTimeSeries(culvert, culvert.GateInitialOpeningTimeSeries), referenceDateTime);
            }
            catch (FileReadingException e)
            {
                log.WarnFormat("Could not read the time series at {0} using default Valve Opening Height instead: {1}", filePath, e.Message);
                culvert.UseGateInitialOpeningTimeSeries = false;
            }
        }

        private static CulvertGeometryType GetGeometryType(CrossSectionStandardShapeType? standardCrossSectionDefinition)
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
        
        private static void SetCulvertDimensionsBasedOnProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            switch (culvert.GeometryType)
            {
                case CulvertGeometryType.Round:
                    SetCulvertDimensionsBasedOnRoundProfile(culvert, definition);
                    break;
                case CulvertGeometryType.Rectangle:
                    SetCulvertDimensionsBasedOnRectangleProfile(culvert, definition);
                    break;
                case CulvertGeometryType.Egg:
                case CulvertGeometryType.InvertedEgg:
                case CulvertGeometryType.Cunette:
                case CulvertGeometryType.Ellipse:
                    SetCulvertDimensionsBasedOnEggProfile(culvert, definition);
                    break;
                case CulvertGeometryType.Arch:
                case CulvertGeometryType.UShape:
                    SetCulvertDimensionsBasedOnArchProfile(culvert, definition);
                    break;
                case CulvertGeometryType.SteelCunette:
                    SetCulvertDimensionsBasedOnSteelCunetteProfile(culvert, definition);
                    break;
                case CulvertGeometryType.Tabulated:
                    break;
                default:
                    throw new InvalidOperationException(string.Format(Resources.CulvertDefinitionParser_Unsupported_culvert_geometry_type,
                                                                      culvert.GeometryType));
            }
        }

        private static void SetCulvertDimensionsBasedOnSteelCunetteProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            if (stdDef == null)
            {
                return;
            }

            var steelcunette = stdDef.Shape as CrossSectionStandardShapeSteelCunette;
            if (steelcunette == null)
            {
                return;
            }

            culvert.Angle = steelcunette.AngleA;
            culvert.Angle1 = steelcunette.AngleA1;
            culvert.Height = steelcunette.Height;
            culvert.Radius = steelcunette.RadiusR;
            culvert.Radius1 = steelcunette.RadiusR1;
            culvert.Radius2 = steelcunette.RadiusR2;
            culvert.Radius3 = steelcunette.RadiusR3;
        }

        private static void SetCulvertDimensionsBasedOnArchProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            if (stdDef == null)
            {
                return;
            }

            var arch = stdDef.Shape as CrossSectionStandardShapeArch;
            if (arch == null)
            {
                return;
            }

            culvert.Width = arch.Width;
            culvert.Height = arch.Height;
            culvert.ArcHeight = arch.ArcHeight;
        }

        private static void SetCulvertDimensionsBasedOnEggProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            if (stdDef == null)
            {
                return;
            }

            var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
            if (heightbase == null)
            {
                return;
            }

            culvert.Width = heightbase.Width;
            culvert.Height = heightbase.Height;
        }

        private static void SetCulvertDimensionsBasedOnRectangleProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            if (stdDef == null)
            {
                return;
            }

            var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
            if (heightbase == null)
            {
                return;
            }

            culvert.Width = heightbase.Width;
            culvert.Height = heightbase.Height;
            culvert.Closed = (heightbase as ICrossSectionStandardShapeOpenClosed)?.Closed ?? false;
        }

        private static void SetCulvertDimensionsBasedOnRoundProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            if (stdDef == null)
            {
                return;
            }

            var round = stdDef.Shape as CrossSectionStandardShapeCircle;
            if (round != null)
            {
                culvert.Diameter = round.Diameter;
            }
        }
    }
}