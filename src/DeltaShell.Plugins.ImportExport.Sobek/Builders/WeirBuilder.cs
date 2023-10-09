using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders
{

    /// <summary>
    /// Constructs Weir entities from Sobek helper files
    /// </summary>
    public class WeirBuilder:BranchStructureBuilderBase<Weir>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WeirBuilder));
        
        public Dictionary<string, SobekCrossSectionDefinition> SobekCrossSectionDefinitions { get; private set; }

        public WeirBuilder(Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions)
        {
            SobekCrossSectionDefinitions = sobekCrossSectionDefinitions;
        }

        private IWeirFormula CreateWeirFormula(IWeir weir, SobekStructureDefinition structure)
        {
            var type = (SobekStructureType) structure.Type;
            
            switch (type)
            {
                case SobekStructureType.riverWeir:
                    return GetRiverWeirFormula((SobekRiverWeir) structure.Definition);
                case SobekStructureType.riverAdvancedWeir:
                    return GetPierWeirFormula((SobekRiverAdvancedWeir) structure.Definition);
                case SobekStructureType.generalStructure:
                    return GetGeneralStructureWeirFormula((SobekGeneralStructure) structure.Definition);
                case SobekStructureType.weir:
                    return GetSimpleWeirFormula((SobekWeir) structure.Definition);
                case SobekStructureType.orifice:
                    return GetGatedWeirFormula((SobekOrifice) structure.Definition);
                case SobekStructureType.universalWeir:
                    return GetUniversalWeirFormula(weir, (SobekUniversalWeir) structure.Definition, SobekCrossSectionDefinitions);
            }
            throw new ArgumentOutOfRangeException();
        }

        private IWeirFormula GetGeneralStructureWeirFormula(SobekGeneralStructure definition)
        {
            double extraResistance = definition.ExtraResistance != null ? (double)definition.ExtraResistance : 0.0;
            double gateOpening = definition.GateHeight - definition.BedLevelStructureCentre;

            if (definition.ImportFromRE)
            {
                gateOpening = definition.GateHeight;
            }

            var generalStructureWeirFormula = new GeneralStructureWeirFormula
            {
                GateOpening = gateOpening,
                ExtraResistance = extraResistance,
                LowerEdgeLevel = definition.GateHeight,

                //coefficients
                NegativeContractionCoefficient = definition.NegativeContractionCoefficient,
                PositiveContractionCoefficient = definition.PositiveContractionCoefficient,
                NegativeDrownedGateFlow = definition.NegativeDrownedGateFlow,
                PositiveDrownedGateFlow = definition.PositiveDrownedGateFlow,
                NegativeDrownedWeirFlow = definition.NegativeDrownedWeirFlow,
                PositiveDrownedWeirFlow = definition.PositiveDrownedWeirFlow,
                NegativeFreeGateFlow = definition.NegativeFreeGateFlow,
                PositiveFreeGateFlow = definition.PositiveFreeGateFlow,
                NegativeFreeWeirFlow = definition.NegativeFreeWeirFlow,
                PositiveFreeWeirFlow = definition.PositiveFreeWeirFlow,

                //bed levels..
                BedLevelLeftSideStructure = definition.BedLevelLeftSideStructure,
                BedLevelLeftSideOfStructure = definition.BedLevelLeftSideOfStructure,
                BedLevelStructureCentre = definition.BedLevelStructureCentre,
                BedLevelRightSideOfStructure = definition.BedLevelRightSideOfStructure,
                BedLevelRightSideStructure = definition.BedLevelRightSideStructure,

                //width
                WidthStructureLeftSide = definition.WidthStructureLeftSide,
                WidthLeftSideOfStructure = definition.WidthLeftSideOfStructure,
                WidthStructureCentre = definition.WidthStructureCentre,
                WidthRightSideOfStructure = definition.WidthRightSideOfStructure,
                WidthStructureRightSide = definition.WidthStructureRightSide
            };
            return generalStructureWeirFormula;
        }

        private static IWeirFormula GetUniversalWeirFormula(IWeir weir, 
                                                            SobekUniversalWeir sobekUniversalWeir, 
                                                            IReadOnlyDictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions)
        {
            var freeFormWeirFormula = new FreeFormWeirFormula { DischargeCoefficient = sobekUniversalWeir.DischargeCoefficient };
            
            if (sobekCrossSectionDefinitions.TryGetValue(sobekUniversalWeir.CrossSectionId, out SobekCrossSectionDefinition definition))
            {
                IList<Coordinate> yzValues = definition.YZ;
                freeFormWeirFormula.SetShape(yzValues.Select(yz => yz.X).ToArray(),
                                             yzValues.Select(yz => yz.Y + sobekUniversalWeir.CrestLevelShift).ToArray());
                weir.OffsetY = 0;
            }
            return freeFormWeirFormula;
        }

        private static IWeirFormula GetGatedWeirFormula(SobekOrifice sobekOrifice)
        {
            var formula = new GatedWeirFormula(true)
            {
                GateOpening = sobekOrifice.GateHeight - sobekOrifice.CrestLevel,
                LowerEdgeLevel = sobekOrifice.GateHeight,
                ContractionCoefficient = sobekOrifice.ContractionCoefficient * sobekOrifice.LateralContractionCoefficient,
                UseMaxFlowNeg = sobekOrifice.UseMaximumFlowNeg,
                MaxFlowNeg = sobekOrifice.MaximumFlowNeg,
                UseMaxFlowPos = sobekOrifice.UseMaximumFlowPos,
                MaxFlowPos = sobekOrifice.MaximumFlowPos
            };

            return formula;
        }

        private static void SetDataTableRowsToFunction(DataTable dt, IFunction function)
        {
            function.Clear(); //clear first

            foreach (DataRow dataRow in dt.Rows)
            {
                var x = Convert.ToDouble(dataRow[0]);
                var y = Convert.ToDouble(dataRow[1]);
                function[x] = y;
            }
        }

        private static IWeirFormula GetPierWeirFormula(SobekRiverAdvancedWeir weir)
        {
            return new PierWeirFormula
            {
                AbutmentContractionNeg = weir.NegativeAbutmentContractionCoefficient,
                AbutmentContractionPos = weir.PositiveAbutmentContractionCoefficient,
                DesignHeadNeg = weir.NegativeWeirDesignHead,
                DesignHeadPos = weir.PositiveWeirDesignHead,
                NumberOfPiers = weir.NumberOfPiers,
                PierContractionNeg = weir.NegativePierContractionCoefficient,
                PierContractionPos = weir.PositivePierContractionCoefficient,
                UpstreamFaceNeg = weir.NegativeUpstreamHeight,
                UpstreamFacePos = weir.PositiveUpstreamFaceHeight
            };
        }

        private static IWeirFormula GetSimpleWeirFormula(SobekWeir sobekWeir)
        {
            return new SimpleWeirFormula 
            {
                CorrectionCoefficient = sobekWeir.DischargeCoefficient * sobekWeir.LateralContractionCoefficient
            };            
        }

        private static IWeirFormula GetRiverWeirFormula(SobekRiverWeir sobekRiverWeir)
        {
            var formula = new RiverWeirFormula 
            { 
                CorrectionCoefficientNeg = sobekRiverWeir.CorrectionCoefficientNeg,
                CorrectionCoefficientPos = sobekRiverWeir.CorrectionCoefficientPos
            };
            
            SetDataTableRowsToFunction(sobekRiverWeir.NegativeReductionTable, formula.SubmergeReductionNeg);
            SetDataTableRowsToFunction(sobekRiverWeir.PositiveReductionTable, formula.SubmergeReductionPos);
            
            formula.SubmergeLimitNeg = sobekRiverWeir.SubmergeLimitNeg;
            formula.SubmergeLimitPos = sobekRiverWeir.SubmergeLimitPos;
            
            return formula;
        }
       
        public SobekStructureType SobekStructureDefinitionType => SobekStructureType.weir;

        public override IEnumerable<Weir> GetBranchStructures(SobekStructureDefinition sobekStructureDefinition)
        {
            var weirSobekTypes = new List<int> { 0, 1, 2, 6, 7, 11 };

            if (weirSobekTypes.TrueForAll(type => type != sobekStructureDefinition.Type))
            {
                yield break;
            }
            
            var weir = new Weir(sobekStructureDefinition.Id);
            weir.WeirFormula = CreateWeirFormula(weir, sobekStructureDefinition);
            
            SetGeneralProperties(sobekStructureDefinition, weir);
            
            if (weir.IsGated)
            {
                var orifice = new Orifice(sobekStructureDefinition.Id);
                orifice.CopyFrom(weir);
                yield return orifice;
            }
            else
            {
                switch (weir.WeirFormula)
                {
                    case PierWeirFormula pierWeirFormula:
                        log.WarnFormat("Weirs with pier formula are not yet supported in the kernel, skipping this weir with id : {0}", (string.IsNullOrEmpty(weir.Name) ? "<No id is set>" : weir.Name));
                        yield break; //not yet implemented in the kernel
                    case RiverWeirFormula riverWeirFormula:
                        log.WarnFormat("Weirs with river formula are not yet supported in the kernel, skipping this weir with id : {0}", (string.IsNullOrEmpty(weir.Name) ? "<No id is set>" : weir.Name));
                        yield break; //not yet implemented in the kernel
                }
                yield return weir;
            }
        }
        
        private void SetGeneralProperties(SobekStructureDefinition structure, Weir weir)
        {
            if (structure.Definition is SobekWeir sobekWeir)
            {
                weir.CrestLevel = sobekWeir.CrestLevel;
                weir.CrestWidth = sobekWeir.CrestWidth;
                weir.FlowDirection = GetFlowDirection(sobekWeir.FlowDirection);
            }

            if (structure.Definition is SobekRiverAdvancedWeir pierWeir)
            {
                weir.CrestLevel = pierWeir.CrestLevel;
                weir.CrestWidth = pierWeir.SillWidth;
            }

            if (structure.Definition is SobekUniversalWeir sobekUniversalWeir)
            {
                weir.CrestLevel = (weir.WeirFormula as FreeFormWeirFormula)?.Z.DefaultIfEmpty().Min() ?? sobekUniversalWeir.CrestLevelShift;
            }

            if (structure.Definition is SobekRiverWeir riverWeir)
            {
                weir.CrestLevel = riverWeir.CrestLevel;
                weir.CrestWidth = riverWeir.CrestWidth;
                switch (riverWeir.CrestShape)
                {
                    case 0:
                        weir.CrestShape = CrestShape.Broad;
                        break;
                    case 1:
                        weir.CrestShape = CrestShape.Triangular;
                        break;
                    case 2:
                        weir.CrestShape = CrestShape.Round;
                        break;
                    case 3:
                        weir.CrestShape = CrestShape.Sharp;
                        break;
                }
            }

            if (structure.Definition is SobekOrifice orifice)
            {
                weir.CrestLevel = orifice.CrestLevel;
                weir.CrestWidth = orifice.CrestWidth;
                weir.FlowDirection = GetFlowDirection(orifice.FlowDirection);
            }
        }
    }
}