using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

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
            if (getTypeConverter != null) this.getTypeConverterFunc = getTypeConverter;
            else throw new ArgumentException("getTypeConverterFunc cannot be null.");

            if (getCompositeBranchStructureFunc != null)
                this.getCompositeBranchStructureFunc = getCompositeBranchStructureFunc;
            else throw new ArgumentException("getCompositeBranchStructureFunc cannot be null.");
        }

        public IList<ICompositeBranchStructure> Convert(IList<DelftIniCategory> categories,
            IList<IChannel> channelsList,
            List<string> errorMessages)
        {
            IList<ICompositeBranchStructure> compositeBranchStructures = new List<ICompositeBranchStructure>();

            // Do this in two steps and first the real composite branch structures (multiple structures at one location), 
            // so that the real composite branch structures names will not be adjusted.
            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header && System.Convert.ToInt32((category.GetPropertyValue(StructureRegion.Compound.Key))) != 0))
            {
                CreationOfStructuresAndCompositeBranchStructures(structureBranchCategory, channelsList, compositeBranchStructures, errorMessages);
            }
            foreach (var structureBranchCategory in categories.Where(
                category => category.Name == StructureRegion.Header && System.Convert.ToInt32((category.GetPropertyValue(StructureRegion.Compound.Key))) == 0))
            {
                CreationOfStructuresAndCompositeBranchStructures(structureBranchCategory, channelsList, compositeBranchStructures, errorMessages);
            }
            return compositeBranchStructures;
        }

        private void CreationOfStructuresAndCompositeBranchStructures(DelftIniCategory structureBranchCategory,
            IEnumerable<IChannel> channels, IList<ICompositeBranchStructure> compositeBranchStructures,
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
                        "A {0} is found in the structure file (line {1}) and this type is not supported during an import.Therefore it is not imported in the GUI",
                        type, structureBranchCategory.LineNumber));
                }

                var structure = converter.ConvertToStructure1D(structureBranchCategory, channel);

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
    }
}