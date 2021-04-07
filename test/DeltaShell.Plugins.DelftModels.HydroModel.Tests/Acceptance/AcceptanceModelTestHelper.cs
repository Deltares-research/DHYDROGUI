using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Dimr.Gui;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    /// <summary>
    /// Class containing helper methods that can be used in acceptance model tests.
    /// </summary>
    public static class AcceptanceModelTestHelper
    {
        /// <summary>
        /// Creates a running <see cref="DeltaShellGui"/> instance with all relevant plugins.
        /// </summary>
        /// <returns>The created <see cref="DeltaShellGui"/> instance.</returns>
        public static DeltaShellGui CreateRunningDeltaShellGui()
        {
            var gui = new DeltaShellGui();
            gui.Plugins.Add(new DimrGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new FlowFMGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new RainfallRunoffGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Plugins.Add(new ScriptingGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new SobekImportGuiPlugin());
            gui.Plugins.Add(new ToolboxGuiPlugin());

            var app = gui.Application;
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new ToolboxApplicationPlugin());

            gui.Run();
            
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
        /// Saves, loads and resaves the project that is part of <paramref name="application"/>.
        /// </summary>
        /// <param name="application">The application containing the project.</param>
        /// <param name="tempProjectPath1">The temporary project file path to be used for the first save action.</param>
        /// <param name="tempProjectPath2">The temporary project file path to be used for the second save action.</param>
        public static void SaveLoadAndResaveProject(IApplication application, string tempProjectPath1, string tempProjectPath2)
        {
            Console.WriteLine("Saving (first time): " + tempProjectPath1);
            application.SaveProjectAs(tempProjectPath1);

            Console.WriteLine("Closing model");
            application.CloseProject();

            Console.WriteLine("Opening");
            application.OpenProject(tempProjectPath1);

            Console.WriteLine("Saving (second time: " + tempProjectPath2);
            application.SaveProjectAs(tempProjectPath2);
        }

        /// <summary>
        /// Compares the files of two project directories.
        /// </summary>
        /// <remarks>
        /// For specific files, you can ignore lines starting with a specific string by providing a lookup of files mapping to a collection of strings.
        /// </remarks>
        /// <param name="firstSaveProjectDirectory">Path to directory containing first project files to compare.</param>
        /// <param name="secondSaveProjectDirectory">Path to directory containing second project files to compare.</param>
        /// <param name="mduFileName">Name of the mdu file that corresponds with the folder name where the FlowFM data is located.</param>
        /// <param name="tempDirectory">Path to temporary directory.</param>
        /// <param name="hasRrData">Whether or not Rainfall Runoff data should be compared.</param>
        /// <param name="flowFmLinesToIgnorePerFile">Lookup for which lines should be ignored per FlowFM file.</param>
        /// <param name="rainfallRunoffLinesToIgnorePerFile">Lookup for which lines should be ignored per Rainfall Runoff file.</param>
        public static void CompareProjectDirectories(string firstSaveProjectDirectory,
                                                     string secondSaveProjectDirectory,
                                                     string mduFileName,
                                                     string tempDirectory,
                                                     bool hasRrData,
                                                     IReadOnlyDictionary<string, IEnumerable<string>> flowFmLinesToIgnorePerFile,
                                                     IReadOnlyDictionary<string, IEnumerable<string>> rainfallRunoffLinesToIgnorePerFile)
        {
            Console.WriteLine("Comparing FlowFM saved data");
            string flowFmInitialSaveDirectory = Path.Combine(firstSaveProjectDirectory, mduFileName, "input");
            string flowFmSecondSaveDirectory = Path.Combine(secondSaveProjectDirectory, mduFileName, "input");
            CompareFlowFMFiles(flowFmInitialSaveDirectory, flowFmSecondSaveDirectory, tempDirectory, flowFmLinesToIgnorePerFile);

            if (hasRrData)
            {
                Console.WriteLine("Comparing Rainfall Runoff saved data");
                string rrInitialSaveDirectory = Path.Combine(firstSaveProjectDirectory, "Rainfall Runoff");
                string rrSecondSaveDirectory = Path.Combine(secondSaveProjectDirectory, "Rainfall Runoff");
                CompareRainfallRunoffFiles(rrInitialSaveDirectory, rrSecondSaveDirectory, rainfallRunoffLinesToIgnorePerFile);
            }
        }
        
        /// <summary>
        /// Compares the files of two project directories.
        /// </summary>
        /// <param name="firstSaveProjectDirectory">Path to directory containing first project files to compare.</param>
        /// <param name="secondSaveProjectDirectory">Path to directory containing second project files to compare.</param>
        /// <param name="mduFileName">Name of the mdu file that corresponds with the folder name where the FlowFM data is located.</param>
        /// <param name="tempDirectory">Path to temporary directory.</param>
        /// <param name="hasRrData">Whether or not Rainfall Runoff data should be compared.</param>
        public static void CompareProjectDirectories(string firstSaveProjectDirectory,
                                                     string secondSaveProjectDirectory,
                                                     string mduFileName,
                                                     string tempDirectory,
                                                     bool hasRrData)
        {
            var linesToIgnore = new Dictionary<string, IEnumerable<string>>(); // don't ignore anything
            CompareProjectDirectories(firstSaveProjectDirectory, 
                                      secondSaveProjectDirectory, 
                                      mduFileName, 
                                      tempDirectory, 
                                      hasRrData, 
                                      linesToIgnore, 
                                      linesToIgnore);
        }
        
        private static void CompareFlowFMFiles(string flowFmInitialSaveDirectory, 
                                               string flowFmSecondSaveDirectory, 
                                               string tempDirectory, 
                                               IReadOnlyDictionary<string, IEnumerable<string>> flowFmLinesToIgnorePerFile)
        {
            string[] flowFmInitialSaveFiles = Directory.GetFiles(flowFmInitialSaveDirectory);
            if (!flowFmInitialSaveFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {flowFmInitialSaveDirectory}.");
            }
            
            string[] flowFmSecondSaveFiles = Directory.GetFiles(flowFmSecondSaveDirectory);
            if (!flowFmSecondSaveFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {flowFmSecondSaveDirectory}.");
            }
            
            FlowFmFileComparer.Compare(flowFmInitialSaveFiles, flowFmSecondSaveFiles, tempDirectory, flowFmLinesToIgnorePerFile);
        }

        private static void CompareRainfallRunoffFiles(string rrInitialSaveDirectory,
                                                       string rrSecondSaveDirectory,
                                                       IReadOnlyDictionary<string, IEnumerable<string>> rainfallRunoffLinesToIgnorePerFile)
        {
            string[] rrInitialSaveFiles = Directory.GetFiles(rrInitialSaveDirectory);
            if (!rrInitialSaveFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {rrInitialSaveDirectory}.");
            }
            
            string[] rrSecondSaveFiles = Directory.GetFiles(rrSecondSaveDirectory);
            if (!rrSecondSaveFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {rrSecondSaveDirectory}.");
            }
            
            RainfallRunoffFileComparer.Compare(rrInitialSaveFiles, rrSecondSaveFiles, rainfallRunoffLinesToIgnorePerFile);
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
        /// Mapping of FlowFM filenames to a collection of strings indicating lines to be ignored if they start with this string.
        /// </summary>
        /// <param name="mduFileName">The name of the mdu file.</param>
        /// <returns>The mapping of filenames to a collection of lines to ignore.</returns>
        public static IReadOnlyDictionary<string, IEnumerable<string>> GetFlowFmLinesToIgnore(string mduFileName)
        {
            return new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    $"{mduFileName}", new[]
                    {
                        "HisInterval", "MapInterval", "DtUser"
                    }
                }
            };
        }
        
    }
}
