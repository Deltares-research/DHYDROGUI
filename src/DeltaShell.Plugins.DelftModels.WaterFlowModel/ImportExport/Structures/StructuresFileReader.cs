using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class StructuresFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public StructuresFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IList<ICompositeBranchStructure> ReadStructures(
            string filePath, 
            IList<IChannel> channels, 
            IList<ICrossSectionDefinition> crossSectionDefinitions, 
            GroundLayerDataTransferObject[] groundLayerDataTransferObject)
        {
            var errorMessages = new List<string>();
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }
            
            var compositeBranchStructures = new CompositeBranchStructureConverter().Convert(categories, channels, crossSectionDefinitions, groundLayerDataTransferObject, errorMessages);
            
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the structures from file '{filePath}', an error occured", errorMessages);

            return compositeBranchStructures;

        }
    }
}