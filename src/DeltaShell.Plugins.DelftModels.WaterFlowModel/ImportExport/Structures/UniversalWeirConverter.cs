using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class UniversalWeirConverter
    {
        public static IWeir ConvertToUniversalWeir(DelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weir = new Weir();
            weir.WeirFormula = new FreeFormWeirFormula();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.FlowDirection =
                (FlowDirection) structureBranchCategory.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            var Yvalues = structureBranchCategory.ReadProperty<double[]>(StructureRegion.YValues.Key);
            var Zvalues = structureBranchCategory.ReadProperty<double[]>(StructureRegion.ZValues.Key);

            if (Yvalues.Length < Zvalues.Length)
            {
                throw new Exception("There are more values for the Z coordinate for universal weir");
            }
            else if (Yvalues.Length > Zvalues.Length)
            {
                throw new Exception("There are more values for the Y coordinate for universal weir");
            }
            
            ((FreeFormWeirFormula) (weir.WeirFormula)).SetShape(Yvalues, Zvalues);
            var counterCheck = structureBranchCategory.ReadProperty<int>(StructureRegion.LevelsCount.Key);

            if (counterCheck != ((FreeFormWeirFormula) (weir.WeirFormula)).Y.Count())
            {
                throw new Exception("There are more YZ coordinates given than mentioned in the levelsCount parameter");
            }

            ((FreeFormWeirFormula) (weir.WeirFormula)).DischargeCoefficient =
                structureBranchCategory.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);

            return weir;
        }
    }
}
    
