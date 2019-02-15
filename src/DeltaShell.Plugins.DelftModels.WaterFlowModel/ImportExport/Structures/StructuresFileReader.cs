using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

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
            GroundLayerDTO[] groundLayerDataTransferObject)
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
                createAndAddErrorReport?.Invoke(
                    string.Format(Resources.StructuresFileReader_ReadStructures_While_reading_the_structures_from_file_at___0____an_error_occured, filePath), errorMessages);

            return compositeBranchStructures;

        }
    }
}