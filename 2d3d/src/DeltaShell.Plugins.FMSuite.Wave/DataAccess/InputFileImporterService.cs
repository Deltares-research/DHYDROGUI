using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// <see cref="InputFileImporterService"/> implements an abstraction over handling
    /// files with respect to the input folder.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.Wave.DataAccess.IInputFileImporterService" />
    public sealed class InputFileImporterService : IInputFileImporterService
    {
        private readonly IWaveModel observedWaveModel;

        /// <summary>
        /// Creates a new <see cref="InputFileImporterService"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public InputFileImporterService(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));
            observedWaveModel = waveModel;
        }

        public bool HasFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            // Note we currently assume that we will only be copying directly into the
            // model's input folder, therefor we only search the top directory and not
            // nested directories.
            var searchDirInfo = new DirectoryInfo(GetInputFolderPath());
            return searchDirInfo.EnumerateFiles(fileName, SearchOption.TopDirectoryOnly).Any();
        }

        public void CopyFile(string sourceFilePath, string fileName = null)
        {
            Ensure.NotNull(sourceFilePath, nameof(sourceFilePath));

            // Note we copy directly into the model's input folder, and do not currently
            // support copying in nested folders.
            string goalPath = Path.Combine(GetInputFolderPath(),
                                           fileName ?? Path.GetFileName(sourceFilePath));
            File.Copy(sourceFilePath, goalPath, true);
        }

        public bool IsInInputFolder(string path)
        {
            IList<string> pathParts = GetParts(path);
            IList<string> modelInputFolderParts = GetParts(GetInputFolderPath());

            return pathParts.Count >= modelInputFolderParts.Count &&
                   HasEqualDirectories(modelInputFolderParts, pathParts);
        }

        private static bool HasEqualDirectories(IEnumerable<string> inputFolderParts,
                                                IEnumerable<string> pathFolderParts) =>
            inputFolderParts.Zip(pathFolderParts, (sa, sb) => new ValueTuple<string, string>(sa, sb))
                            .All(t => t.Item1.Equals(t.Item2, StringComparison.OrdinalIgnoreCase));

        private static IList<string> GetParts(string path)
        {
            string absolutePath = Path.GetFullPath(path);
            string root = Path.GetPathRoot(absolutePath);

            var parts = new List<string>();

            while (!Equals(absolutePath, root))
            {
                parts.Add(Path.GetFileName(absolutePath));
                absolutePath = Path.GetDirectoryName(absolutePath);
            }
            parts.Add(root);

            parts.Reverse();
            return parts;
        }

        public string GetAbsolutePath(string relativePath) =>
            Path.GetFullPath(Path.Combine(GetInputFolderPath(), relativePath));

        public string GetRelativePath(string absolutePath) =>
            absolutePath.Substring(GetInputFolderPath().Length + 1);

        private string GetInputFolderPath() =>
            Path.Combine(Path.GetDirectoryName(observedWaveModel.Path),
                         observedWaveModel.Name,
                         DirectoryNameConstants.InputDirectoryName);
    }
}