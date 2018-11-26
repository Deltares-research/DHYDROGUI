using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class CompositeBranchStructureConverter
        {

            public static IList<ICompositeBranchStructure> Convert(IList<DelftIniCategory> categories, IList<IChannel> channelsList,
                List<string> errorMessages)
            {
                

            IList<ICompositeBranchStructure> compositeBranchStructures = new List<ICompositeBranchStructure>();

            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header))
            {
                
                try
                {

                    IStructure1D structure = null;
                    var type = structureBranchCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);

                    switch (type)
                    {
                        case StructureRegion.StructureTypeName.Weir:
                            structure = WeirConverter.ConvertToWeir(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.UniversalWeir:
                            structure = UniversalWeirConverter.ConvertToUniversalWeir(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.RiverWeir:
                            structure = RiverWeirConverter.ConvertToRiverWeir(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.AdvancedWeir:
                            structure = AdvancedWeirConverter.ConvertToAdvancedWeir(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.Orifice:
                            structure = OrificeConverter.ConvertToOrifice(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.GeneralStructure:
                            structure = GeneralStructureConverter.ConvertToGeneralStructure(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.ExtraResistanceStructure:
                            structure = ExtraResistanceConverter.ConvertToExtraResistance(structureBranchCategory, channelsList);
                            break;
                        case StructureRegion.StructureTypeName.Pump:
                            throw new Exception("Pumps are not supported during an import and therefore it is not imported in the GUI");
                           break;
                        case StructureRegion.StructureTypeName.Culvert:
                            throw new Exception("Culverts are not supported during an import and therefore it is not imported in the GUI");
                            break;
                        case StructureRegion.StructureTypeName.Siphon:
                            throw new Exception("Siphons are not supported during an import and therefore it is not imported in the GUI");
                            break;
                        case StructureRegion.StructureTypeName.InvertedSiphon:
                            throw new Exception("Inverted siphons are not supported during an import and therefore it is not imported in the GUI");
                            break;
                        default:
                            throw new Exception("Unknown type for structures found in the structures file");
                            break;
                    }
                    BasicStructuresOperations.CreateCompositeBranchStructuresIfNeededAndAddStructure(structureBranchCategory, structure, compositeBranchStructures);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);   
                }
            }

                return compositeBranchStructures;
            }
        }
    }