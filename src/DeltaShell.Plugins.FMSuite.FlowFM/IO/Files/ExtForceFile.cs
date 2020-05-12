using System.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class ExtForceFile : NGHSFileBase
    {
        private readonly ExtForceFileReader extForceFileReader;
        private readonly ExtForceFileWriter extForceFileWriter;

        private IDictionary<ExtForceFileItem, object> existingForceFileItems;
        private HashSet<ExtForceFileItem> supportedExtForceFileItems;
        private IDictionary<IFeatureData, ExtForceFileItem> polylineForceFileItems;

        private List<List<string>> headingCommentBlocks;

        public ExtForceFile()
        {
            extForceFileReader = new ExtForceFileReader();
            extForceFileWriter = new ExtForceFileWriter();

            existingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            supportedExtForceFileItems = new HashSet<ExtForceFileItem>();
            polylineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>();

            headingCommentBlocks = new List<List<string>>();
        }

        public bool WriteToDisk { get; set; }

        public IEnumerable<IBoundaryCondition> ExistingBoundaryConditions => extForceFileWriter.ExistingBoundaryConditions;

        /// <summary>
        /// Get the data files that are references in the extForceFile.
        /// </summary>
        /// <param name="modelDefinition"> </param>
        /// <returns> A list of tuples of name and file path. </returns>
        public IEnumerable<string[]> GetFeatureDataFiles(WaterFlowFMModelDefinition modelDefinition)
        {
            return extForceFileWriter.GetFeatureDataFiles(modelDefinition);
        }

        protected override bool ExcludeEqualsIdentifier => false;

        protected override void CreateCommonBlock()
        {
            extForceFileReader.CreateCommonBlock();
        }

        protected override bool WriteCommentBlock(string line, bool doWriteLine)
        {
            extForceFileWriter.WriteCommentBlock(line);
            return true;
        }

        public void Read(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                         string extSubFilesReferenceFilePath)
        {
            extForceFileReader.Read(extForceFilePath, modelDefinition, extSubFilesReferenceFilePath);

            existingForceFileItems = extForceFileReader.ExistingForceFileItems;
            supportedExtForceFileItems = extForceFileReader.SupportedExtForceFileItems;
            polylineForceFileItems = extForceFileReader.PolylineForceFileItems;
            headingCommentBlocks = extForceFileReader.HeadingCommentBlocks;
            commentBlocks.Clear();

            foreach (KeyValuePair<string, List<string>> commentBlock in extForceFileReader.CommentBlocks)
            {
                commentBlocks.Add(commentBlock.Key, commentBlock.Value);
            }
        }

        /// <summary>
        /// Writes the model definition external forcings to file.
        /// </summary>
        /// <param name="extForceFilePath"> File path </param>
        /// <param name="modelDefinition"> External forcings data </param>
        /// <param name="writeBoundaryConditions"> Whether we are writing boundary conditions. </param>
        public void Write(string extForceFilePath, WaterFlowFMModelDefinition modelDefinition,
                          bool writeBoundaryConditions = true, bool switchTo = true)
        {
            extForceFileWriter.Write(extForceFilePath, modelDefinition, writeBoundaryConditions, switchTo,
                                     existingForceFileItems, supportedExtForceFileItems, polylineForceFileItems, 
                                     headingCommentBlocks, commentBlocks);
        }

        /// <summary>
        /// Writes data files references by the external forcings file.
        /// </summary>
        /// <param name="path"> File path. </param>
        /// <param name="modelDefinition"> Contains data to be written. </param>
        /// <param name="switchTo"> Flag denoting whether to switch to the file path directory (save) </param>
        /// <param name="writeBoundaryConditions"> Flag denoting whether to write boundary conditions </param>
        /// <returns> Resulting force file items </returns>
        public IEnumerable<ExtForceFileItem> WriteExtForceFileSubFiles(string path,
                                                                       WaterFlowFMModelDefinition modelDefinition,
                                                                       bool switchTo, bool writeBoundaryConditions)
        {
            return extForceFileWriter.WriteExtForceFileSubFiles(path, modelDefinition, switchTo, writeBoundaryConditions);
        }
    }
}