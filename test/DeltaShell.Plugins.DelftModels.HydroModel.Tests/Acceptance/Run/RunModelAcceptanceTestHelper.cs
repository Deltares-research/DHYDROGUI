using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    public static class RunModelAcceptanceTestHelper
    {
        /// <summary>
        /// Compares actual FlowFM output to reference output.
        /// </summary>
        /// <param name="acceptanceModelName">The name of the acceptance model.</param>
        /// <param name="referenceOutputDirectory">The output directory of the reference data.</param>
        /// <param name="tempDirectory">A temporary work directory.</param>
        /// <param name="keepOutput">Whether or not to keep the actual output files.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        public static void CompareFlowFmOutput(string acceptanceModelName, string referenceOutputDirectory, 
                                        string tempDirectory, bool keepOutput)
        {
            if (acceptanceModelName == null)
            {
                throw new ArgumentNullException(nameof(acceptanceModelName));
            }
            
            if (referenceOutputDirectory == null)
            {
                throw new ArgumentNullException(nameof(acceptanceModelName));
            }
            
            if (tempDirectory == null)
            {
                throw new ArgumentNullException(nameof(acceptanceModelName));
            }
            
            string expectedOutputFolder = Path.Combine(referenceOutputDirectory, acceptanceModelName, "FlowFM");
            if (!Directory.Exists(expectedOutputFolder))
            {
                return;
            }
            string[] expectedOutputFiles = Directory.GetFiles(expectedOutputFolder);
            
            if (!expectedOutputFiles.Any())
            {
                return;
            }
            
            string actualOutputFolder = Path.Combine(tempDirectory, "SavedModel_data", "FlowFM", "DFM_OUTPUT_FlowFM");
            string[] actualOutputFiles = Directory.GetFiles(actualOutputFolder);
            
            OutputFileComparer.Compare(expectedOutputFiles, actualOutputFiles, tempDirectory);

            if (keepOutput)
            {
                KeepOutput(acceptanceModelName, "FlowFM", actualOutputFiles);
            }
        }
        
        private static void KeepOutput(string acceptanceModelName, string modelName, IEnumerable<string> actualOutputFiles)
        {
            string targetDirectory = Path.Combine(TestHelper.GetTestWorkingDirectory(), "AcceptanceModelOutput", acceptanceModelName, modelName);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
                
            foreach (string outputFile in actualOutputFiles)
            {
                File.Copy(outputFile, Path.Combine(targetDirectory, Path.GetFileName(outputFile)), true);
            }
        }
    }
}