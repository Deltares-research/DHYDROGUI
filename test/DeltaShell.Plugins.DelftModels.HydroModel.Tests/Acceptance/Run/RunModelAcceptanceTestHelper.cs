using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

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
        
        /// <summary>
        /// Sets some required model settings for the <see cref="HydroModel"/> in order to run.
        /// </summary>
        /// <param name="hydroModel">The <see cref="HydroModel"/> to set the settings for.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="hydroModel"/> is <c>null</c>.</exception>
        public static void SetHydroModelSettings(HydroModel hydroModel)
        {
            if (hydroModel == null)
            {
                throw new ArgumentNullException(nameof(hydroModel));
            }
            
            hydroModel.StartTime = new DateTime(2020, 01, 01, 0, 0, 0);
            hydroModel.StopTime = new DateTime(2020, 01, 01, 1, 0, 0);
            hydroModel.TimeStep = new TimeSpan(1, 0, 0);
        }

        /// <summary>
        /// Sets some required model settings for the <see cref="WaterFlowFMModel"/> in order to run.
        /// </summary>
        /// <param name="fmModel">The <see cref="WaterFlowFMModel"/> to set the settings for.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="fmModel"/> is <c>null</c>.</exception>
        public static void SetFlowFMModelSettings(WaterFlowFMModel fmModel)
        {
            if (fmModel == null)
            {
                throw new ArgumentNullException(nameof(fmModel));
            }
            
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.RefDate, "20200101000000");
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, "3600");
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.MapOutputDeltaT, "3600");
        }

        /// <summary>
        /// Sets some required model settings for the <see cref="RainfallRunoffModel"/> in order to run.
        /// </summary>
        /// <param name="rrModel">The <see cref="RainfallRunoffModel"/> to set the settings for.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="rrModel"/> is <c>null</c>.</exception>
        public static void SetRrModelSettings(RainfallRunoffModel rrModel)
        {
            if (rrModel == null)
            {
                throw new ArgumentNullException(nameof(rrModel));
            }
            
            rrModel.Precipitation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Precipitation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 0, 0, 0)));
            rrModel.Precipitation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Precipitation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 1, 0, 0)));
            rrModel.Evaporation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Evaporation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 0, 0, 0)));
            rrModel.Evaporation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Evaporation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 1, 0, 0)));
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