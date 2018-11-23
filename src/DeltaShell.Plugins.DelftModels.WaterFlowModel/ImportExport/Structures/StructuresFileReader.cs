using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using static DeltaShell.NGHS.IO.FileWriters.Structure.StructureRegion;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class StructuresFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public StructuresFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IList<ICompositeBranchStructure> ReadStructures(string filePath, IList<IChannel> channelsList)
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
            
           var compositeBranchStructures = CompositeBranchStructureConverter.Convert(categories, channelsList, errorMessages);
            
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the structures from file, an error occured", errorMessages);

            return compositeBranchStructures;

        }
    }
}