using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
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
                    var type = structureBranchCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);

                    var converter =  StructureConverterFactory.GetSpecificConverter(type);

                    if (converter == null)
                    {
                        throw new Exception(string.Format("A {0} is found in the structure file and this type is not supported during an import.Therefore it is not imported in the GUI", type));
                    }

                    var structure = converter.ConvertToStructure1D(structureBranchCategory, channelsList);

                    if (structure == null)
                    {
                        throw new Exception("Failed to create a structure from the structures file");
                    }
                    
                    var compositeBranchStructure = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded(structureBranchCategory, structure, compositeBranchStructures);

                    if (compositeBranchStructure == null)
                    {
                        throw new Exception(string.Format("Failed to create structure {0} from the structures file", structure.Name));
                    }

                    HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, structure);
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