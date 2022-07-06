using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders
{
    public class CulvertBuilder : BranchStructureBuilderBase<Culvert>
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(CulvertBuilder));

        private readonly Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions;
        private IList<SobekValveData> sobekValveData;

        public CulvertBuilder(Dictionary<string, SobekCrossSectionDefinition> definitions, IList<SobekValveData> sobekValveData)
        {
            sobekCrossSectionDefinitions = definitions;
            this.sobekValveData = sobekValveData;
        }


        public override IEnumerable<Culvert> GetBranchStructures(SobekStructureDefinition structure)
        {
            if (structure == null || (!(structure.Definition is SobekCulvert)))
                yield break;
            var sobekCulvert = structure.Definition as SobekCulvert;
            var culvert= new Culvert((string.IsNullOrEmpty(structure.Name) ? "Culvert" : structure.Name))
                             {
                                 InletLossCoefficient = sobekCulvert.InletLossCoefficient,
                                 OutletLossCoefficient = sobekCulvert.OutletLossCoefficient,
                                 InletLevel = sobekCulvert.BedLevelLeft,
                                 OutletLevel = sobekCulvert.BedLevelRight,
                                 BendLossCoefficient = sobekCulvert.BendLossCoefficient,
                                 Length = sobekCulvert.Length,
                                 FlowDirection = GetFlowDirection(sobekCulvert.Direction),
                                 IsGated = (sobekCulvert.UseTableOffLossCoefficient == 1)
                             };
            
            //a table is defined so a valve/gate is defined..
            if (culvert.IsGated)
            {
                UpdateGateOpeningLossCoefficientFunction(culvert.GateOpeningLossCoefficientFunction,sobekCulvert.TableOfLossCoefficientId);
            }

            if (sobekCulvert.CulvertType == DeltaShell.Sobek.Readers.SobekDataObjects.CulvertType.Siphon)
            {
                Log.WarnFormat("Siphon culverts are not yet supported in the kernel, skipping this culvert with id : {0}", (string.IsNullOrEmpty(structure.Name) ? "<No id is set>" : structure.Name));
                yield break; //not yet implemented in the kernel
            }
            
            culvert.CulvertType = (DelftTools.Hydro.CulvertType)sobekCulvert.CulvertType;

            culvert.GateInitialOpening = sobekCulvert.ValveInitialOpeningLevel;
            
            if (sobekCulvert.CrossSectionId != null && sobekCrossSectionDefinitions.ContainsKey(sobekCulvert.CrossSectionId))
            {
                SobekCrossSectionDefinition sobekCrossSectionDefinition = sobekCrossSectionDefinitions[sobekCulvert.CrossSectionId];
                
                culvert.GroundLayerEnabled = sobekCrossSectionDefinition.UseGroundLayer;
                culvert.GroundLayerThickness = sobekCrossSectionDefinition.GroundLayerDepth;

                switch (sobekCrossSectionDefinition.Type)
                {
                    case SobekCrossSectionDefinitionType.Tabulated: // 0
                        {
                            var hfswData =
                                sobekCrossSectionDefinition.TabulatedProfile.Select(
                                    t => new HeightFlowStorageWidth(t.Height, t.TotalWidth, t.FlowWidth));

                            culvert.TabulatedCrossSectionDefinition.SetWithHfswData(hfswData);

                            culvert.GeometryType = CulvertGeometryType.Tabulated;
                            //special case.
                            if ((sobekCrossSectionDefinition.Name != null) && sobekCrossSectionDefinition.Name.StartsWith("r_"))
                            {
                                //set the type of the bridgegeometry and set width and height, bed level in the bridge
                                culvert.GeometryType = CulvertGeometryType.Rectangle;
                                culvert.Width = sobekCrossSectionDefinition.TabulatedProfile[1].TotalWidth;
                                culvert.Height = sobekCrossSectionDefinition.TabulatedProfile[1].Height -
                                                sobekCrossSectionDefinition.TabulatedProfile[0].Height;
                            }
                        }
                    break;
                    case SobekCrossSectionDefinitionType.ClosedCircle: // 4
                        culvert.GeometryType = CulvertGeometryType.Round;
                        culvert.Diameter = sobekCrossSectionDefinition.Radius*2;
                    break;
                    case SobekCrossSectionDefinitionType.EggShapedWidth: // 6
                        culvert.GeometryType = CulvertGeometryType.Egg;
                        culvert.Width = sobekCrossSectionDefinition.Width;
                        culvert.Height = sobekCrossSectionDefinition.Width * 1.5;
                        break;
                    default:
                    //case SobekCrossSectionDefinitionType.Trapezoidal: // 1
                    //case SobekCrossSectionDefinitionType.OpenCircle: // 2
                    //case SobekCrossSectionDefinitionType.Sedredge: // 3
                    //case SobekCrossSectionDefinitionType.Yztable: // 10
                    //case SobekCrossSectionDefinitionType.AsymmetricalTrapeziodal: // 11
                    Log.WarnFormat(string.Format("Culvert profiles of type {0} are not supported.", sobekCrossSectionDefinition.Type));
                    break;
                }
            }

            yield return culvert;
        }

        private void UpdateGateOpeningLossCoefficientFunction(IFunction gateLossFunction,string tableID)
        {
            var valveData = sobekValveData.FirstOrDefault(vd => vd.Id == tableID);
            if (valveData != null)
            {
                FunctionHelper.AddDataTableRowsToFunction(valveData.DataTable, gateLossFunction);    
            }
        }
    }
}