using System;
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
        /// Compares the files of two project folders.
        /// </summary>
        /// <param name="firstSaveProjectPath">Path to directory containig first project files to compare.</param>
        /// <param name="secondSaveProjectPath">Path to directory containing second project files to compare.</param>
        /// <param name="tempDirectory">Path to temporary directory.</param>
        /// <param name="hasRrData">Whether or not Rainfall Runoff data should be compared.</param>
        public static void CompareInitialSaveToSecondSave(string firstSaveProjectPath, 
                                                          string secondSaveProjectPath, 
                                                          string tempDirectory, 
                                                          bool hasRrData)
        {
            Console.WriteLine("Comparing FlowFM saved data");
            CompareFlowFMFiles(firstSaveProjectPath, secondSaveProjectPath, tempDirectory);
            
            if (hasRrData)
            {
                Console.WriteLine("Comparing Rainfall Runoff saved data");
                CompareRainfallRunoffFiles(firstSaveProjectPath, secondSaveProjectPath);
            }
        }
        
        private static void CompareFlowFMFiles(string firstSaveProjectPath, string secondSaveProjectPath, string tempDirectory)
        {
            string flowFmInitialSaveDirectory = Path.Combine(firstSaveProjectPath + "_data", "FlowFM", "input");
            string[] flowFmInitialSaveFiles = Directory.GetFiles(flowFmInitialSaveDirectory);
            if (!flowFmInitialSaveFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {flowFmInitialSaveFiles}.");
            }
            
            string flowFmSecondSaveDirectory = Path.Combine(secondSaveProjectPath + "_data", "FlowFM", "input");
            string[] flowFmSecondSaveFiles = Directory.GetFiles(flowFmSecondSaveDirectory);
            if (!flowFmSecondSaveFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {flowFmSecondSaveDirectory}.");
            }
            
            FlowFmFileComparer.Compare(flowFmInitialSaveFiles, flowFmSecondSaveFiles, tempDirectory);
        }

        private static void CompareRainfallRunoffFiles(string firstSaveProjectPath, string secondSaveProjectPath)
        {
            string rrInitialSaveDirectory = Path.Combine(firstSaveProjectPath + "_data", "Rainfall Runoff");
            string[] rrInitialSaveFiles = Directory.GetFiles(rrInitialSaveDirectory);
            if (!rrInitialSaveFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {rrInitialSaveFiles}.");
            }
            
            string rrSecondSaveDirectory = Path.Combine(secondSaveProjectPath + "_data", "Rainfall Runoff");
            string[] rrSecondSaveFiles = Directory.GetFiles(rrSecondSaveDirectory);
            if (!rrSecondSaveFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {rrSecondSaveDirectory}.");
            }
            
            RainfallRunoffFileComparer.Compare(rrInitialSaveFiles, rrSecondSaveFiles);
        }
    }
}
