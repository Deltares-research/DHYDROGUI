using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    /// <summary>
    /// Class containing helper methods that can be used in acceptance model tests.
    /// </summary>
    public static class AcceptanceModelTestHelper
    {
        private static string ncExtension = ".nc";
        private static string hisExtension = ".his";
        private static string mapExtension = ".map";
        private static string hiaExtension = ".hia";
        private static string outExtension = ".out";
        private static string logExtension = ".log";
        private static string rtnExtension = ".rtn";
        private static string txtExtension = ".txt";
        
        
        
        /// <summary>
        /// Creates a running <see cref="DeltaShellGui"/> instance with all relevant plugins.
        /// </summary>
        /// <returns>The created <see cref="DeltaShellGui"/> instance.</returns>
        public static IGui CreateRunningDeltaShellGui()
        {
            var gui = new DHYDROGuiBuilder().WithDimr()
                                            .WithFlowFM()
                                            .WithRainfallRunoff()
                                            .WithRealTimeControl()
                                            .WithHydroModel()
                                            .WithSobekImport()
                                            .Build();

            var app = gui.Application;

            gui.Run();

            app.ProjectService.CreateProject();
            
            return gui;
        }

        /// <summary>
        /// Creates a <see cref="HydroModel"/> for RHU and adds this model to the provided folder.
        /// </summary>
        /// <param name="folder">The folder to add the <see cref="HydroModel"/> to.</param>
        /// <returns>The created <see cref="HydroModel"/>.</returns>
        public static HydroModel AddRhuHydroModel(Folder folder)
        {
            var hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);

            folder.Add(hydroModel);

            return hydroModel;
        }

        /// <summary>
        /// Saves, closes, loads and resaves the project within the <paramref name="projectService"/>.
        /// </summary>
        /// <param name="projectService"> The project service.</param>
        /// <param name="tempProjectPath1">The temporary project file path to be used for the first save action.</param>
        /// <param name="tempProjectPath2">The temporary project file path to be used for the second save action.</param>
        public static void SaveLoadAndResaveProject(IProjectService projectService, string tempProjectPath1, string tempProjectPath2)
        {
            Console.WriteLine("Saving (first time): " + tempProjectPath1);
            projectService.SaveProjectAs(tempProjectPath1);

            Console.WriteLine("Closing model");
            projectService.CloseProject();

            Console.WriteLine("Opening");
            projectService.OpenProject(tempProjectPath1);

            Console.WriteLine("Saving (second time: " + tempProjectPath2);
            projectService.SaveProjectAs(tempProjectPath2);
        }
        
        /// <summary>
        /// Mapping of Rainfall Runoff filenames to a collection of strings indicating lines to be ignored if they start with this string.
        /// </summary>
        public static IReadOnlyDictionary<string, IEnumerable<string>> RainfallRunoffLinesToIgnore { get; } = new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                "default.evp", new []
                {
                    string.Empty // Ignore entire file as this can contain today's date, which keeps changing.
                } 
            },
            {
                "delft_3b.ini", new []
                {
                    "StartTime", "EndTime" // Based on today's datetime, which keeps changing.
                }
            },
            {
                "default.bui", new []
                {
                    string.Empty // Ignore entire file as this is only set when running a model
                }
            }
        };
        
        /// <summary>
        /// Given a collection of filepaths, returns only input files.
        /// </summary>
        /// <param name="filepaths">The filepaths to filter.</param>
        /// <returns>An collection of input files.</returns>
        public static IEnumerable<string> FilterInputFiles(IEnumerable<string> filepaths)
        {
            return filepaths.Where(fp => !IsRainfallRunoffOutputFile(fp));
        }

        private static bool IsRainfallRunoffOutputFile(string fp)
        {
            string fileName = Path.GetFileName(fp);
            if (fileName == "RR-ready" || fileName == "RSRR_OUT")
            {
                return true;
            }

            string fileExtension = Path.GetExtension(fp);
            return string.Equals(fileExtension, ncExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, hisExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, mapExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, hiaExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, txtExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, logExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, rtnExtension, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(fileExtension, outExtension, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Enables all Rainfall Runoff Output by setting the aggregation option for each engine parameter to 'current'.
        /// </summary>
        /// <param name="rrModel">The rainfall runoff model to enable the output for.</param>
        public static void EnableAllRainfallRunoffOutputSettings(RainfallRunoffModel rrModel)
        {
            IEventedList<EngineParameter> engineParameters = rrModel.OutputSettings.EngineParameters;
            engineParameters.ForEach(ep => ep.IsEnabled = true);
        }
    }
}
