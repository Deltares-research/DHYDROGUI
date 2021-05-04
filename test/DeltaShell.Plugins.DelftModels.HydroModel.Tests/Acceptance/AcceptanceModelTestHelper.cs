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
        
        /// <summary>
        /// Given a collection of filepaths, returns only input files.
        /// </summary>
        /// <param name="filepaths">The filepaths to filter.</param>
        /// <returns>An collection of input files.</returns>
        public static IEnumerable<string> FilterInputFiles(IEnumerable<string> filepaths)
        {
            return filepaths.Where(fp =>
            {
                string fileExtension = Path.GetExtension(fp);

                return !string.Equals(fileExtension, ncExtension, StringComparison.InvariantCultureIgnoreCase)
                       && !string.Equals(fileExtension, hisExtension, StringComparison.InvariantCultureIgnoreCase)
                       && !string.Equals(fileExtension, mapExtension, StringComparison.InvariantCultureIgnoreCase)
                       && !string.Equals(fileExtension, hiaExtension, StringComparison.InvariantCultureIgnoreCase);
            });
        }
        
        /// <summary>
        /// Given a collection of filepaths, returns only output files.
        /// </summary>
        /// <param name="filepaths">The filepaths to filter.</param>
        /// <returns>A collection of output files.</returns>
        public static IEnumerable<string> FilterOutputFiles(IEnumerable<string> filepaths)
        {
            return filepaths.Where(fp =>
            {
                string fileExtension = Path.GetExtension(fp);
                return string.Equals(fileExtension, ncExtension, StringComparison.InvariantCultureIgnoreCase)
                       || string.Equals(fileExtension, hisExtension, StringComparison.InvariantCultureIgnoreCase)
                       || string.Equals(fileExtension, mapExtension, StringComparison.InvariantCultureIgnoreCase)
                       || string.Equals(fileExtension, hiaExtension, StringComparison.InvariantCultureIgnoreCase);
            });
        }
        
        /// <summary>
        /// Given a filepath, returns true if the file is a .nc file.
        /// </summary>
        /// <param name="filePath">The filepath to check.</param>
        /// <returns><c>True</c> if the file is a netcdf file.</returns>
        public static bool IsNetcdfFile(string filePath)
        {
            return filePath.EndsWith(ncExtension, StringComparison.InvariantCultureIgnoreCase);
        }
        
        /// <summary>
        /// Given a filepath, returns true if the file is a valid Rainfall Runoff output file.
        /// </summary>
        /// <param name="filePath">The filepath to check.</param>
        /// <returns><c>True</c> if the file is a valid Rainfall Runoff file.</returns>
        public static bool IsSupportedRainfallRunoffOutputFile(string filePath)
        {
            return filePath.EndsWith(ncExtension, StringComparison.InvariantCultureIgnoreCase)
                   || filePath.EndsWith(hisExtension, StringComparison.InvariantCultureIgnoreCase)
                   || filePath.EndsWith(mapExtension, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
