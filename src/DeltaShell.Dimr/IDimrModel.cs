using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Validation;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// <see cref="IDimrModel"/> defines the interface of a DIMR
    /// model which can be run with the <see cref="DimrRunner"/>.
    /// </summary>
    /// <seealso cref="IModel" />
    public interface IDimrModel : ITimeDependentModel
    {
        /// <summary>
        /// Gets the name of the library.
        /// </summary>
        string LibraryName { get; }

        /// <summary>
        /// Gets the input file.
        /// </summary>
        string InputFile { get; }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        string DirectoryName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDimrModel"/>
        /// defines the time step, or follows another model.
        /// </summary>
        bool IsMasterTimeStep { get; }

        /// <summary>
        /// Gets the short name of this <see cref="IDimrModel"/>.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets the type of the exporter.
        /// </summary>
        Type ExporterType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDimrModel"/> can run parallel.
        /// </summary>
        bool CanRunParallel { get; }

        /// <summary>
        /// Gets the mpi communicator string.
        /// </summary>
        string MpiCommunicatorString { get; }

        /// <summary>
        /// Gets the kernel directory location.
        /// </summary>
        string KernelDirectoryLocation { get; }

        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        new DateTime CurrentTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the <see cref="IDimrModel"/>.
        /// </summary>
        new ActivityStatus Status { get; set; }

        bool RunsInIntegratedModel { get; set; }

        /// <summary>
        /// Gets the dimr export directory path.
        /// </summary>
        string DimrExportDirectoryPath { get; }

        /// <summary>
        /// Gets the dimr model relative working directory.
        /// </summary>
        string DimrModelRelativeWorkingDirectory { get; }

        /// <summary>
        /// Gets the dimr model relative output directory.
        /// </summary>
        string DimrModelRelativeOutputDirectory { get; }

        /// <summary>
        /// Gets the item string.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        string GetItemString(IDataItem dataItem);

        /// <summary>
        /// Gets the data items that match the given item string.
        /// </summary>
        /// <param name="itemString">The item string.</param>
        /// <returns>A collection of matching data items.</returns>
        IEnumerable<IDataItem> GetDataItemsByItemString(string itemString);

        /// <summary>
        /// Gets the exporter path.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        string GetExporterPath(string directoryName);

        /// <summary>
        /// Disconnects the output.
        /// </summary>
        void DisconnectOutput();

        /// <summary>
        /// Connects the output.
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        void ConnectOutput(string outputPath);

        /// <summary>
        /// Validates this <see cref="IDimrModel"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="ValidationReport"/> of the current state of the
        /// <see cref="IDimrModel"/>.
        /// </returns>
        ValidationReport Validate();

        Array GetVar(string category, string itemName = null, string parameter = null);

        void SetVar(Array values, string category, string itemName = null, string parameter = null);

        /// <summary>
        /// Actions, which should be done in the IDimrModel after a successful integrated model run.
        /// </summary>
        /// <param name="hydroModelWorkingDirectoryPath">
        /// Working directory path of the integrated model.
        /// </param>
        void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath);

        /// <summary>
        /// Gets the file exceptions cleaning working directory.
        /// </summary>
        /// <value>
        /// The file exceptions cleaning working directory.
        /// </value>
        ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory { get; }
    }
}