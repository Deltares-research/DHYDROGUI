using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Validation;

namespace DeltaShell.Dimr
{
    public interface IDimrModel : IModel
    {
        string LibraryName { get; }

        string InputFile { get; }

        string DirectoryName { get; }

        bool IsMasterTimeStep { get; }

        string ShortName { get; }

        Type ExporterType { get; }

        bool CanRunParallel { get; }

        string MpiCommunicatorString { get; }

        string KernelDirectoryLocation { get; }

        DateTime CurrentTime { get; set; }
        DateTime StartTime { get; }

        new ActivityStatus Status { get; set; }

        // TODO: temporary, need to remove when models can no longer run without Dimr
        bool RunsInIntegratedModel { get; set; }

        string DimrExportDirectoryPath { get; }

        string DimrModelRelativeWorkingDirectory { get; }

        string DimrModelRelativeOutputDirectory { get; }

        string GetItemString(IDataItem dataItem);

        /// <summary>
        /// Gets the data items that match the given item string.
        /// </summary>
        /// <param name="itemString">The item string.</param>
        /// <returns>A collection of matching data items.</returns>
        IEnumerable<IDataItem> GetDataItemsByItemString(string itemString);

        string GetExporterPath(string directoryName);

        void DisconnectOutput();

        void ConnectOutput(string outputPath);

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
    }
}