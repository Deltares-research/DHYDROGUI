using System;
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

        string GetItemString(IDataItem value);

        /// <summary>
        /// Gets the data item by item string.
        /// </summary>
        /// <param name="itemString">The item string.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IDataItem GetDataItemByItemString(string itemString);

        Type ExporterType { get; }

        string GetExporterPath(string directoryName);

        bool CanRunParallel { get; }

        string MpiCommunicatorString { get; }

        string KernelDirectoryLocation { get; }

        void DisconnectOutput();

        void ConnectOutput(string outputPath);

        DateTime CurrentTime { get; set; }
        DateTime StartTime { get; }

        new ActivityStatus Status { get; set; }

        // TODO: temporary, need to remove when models can no longer run without Dimr
        bool RunsInIntegratedModel { get; set; }

        string DimrExportDirectoryPath { get; set; }

        string DimrModelRelativeWorkingDirectory { get; }

        string DimrModelRelativeOutputDirectory { get; }

        ValidationReport Validate();

        Array GetVar(string category, string itemName = null, string parameter = null);

        void SetVar(Array values, string category, string itemName = null, string parameter = null);

        /// <summary>
        /// Prepares the model for running it as part of an integrated model.
        /// </summary>
        void PrepareForIntegratedModelRun();
    }
}