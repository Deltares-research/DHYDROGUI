using System;
using System.Collections.Generic;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Represents the context of DIMR model import and export operations, including the DIMR file path and model directories.
    /// </summary>
    public sealed class HydroModelFileContext
    {
        private readonly IFileHierarchyResolver fileHierarchyResolver;
        private readonly IDictionary<IDimrModel, string> modelPaths;
        private string baseDirectory;
        private string dimrFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="HydroModelFileContext"/> class.
        /// </summary>
        /// <param name="fileHierarchyResolver"> The resolver used for resolving the DIMR model file structure. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileHierarchyResolver"/> is <c>null</c>.
        /// </exception>
        public HydroModelFileContext(IFileHierarchyResolver fileHierarchyResolver)
        {
            Ensure.NotNull(fileHierarchyResolver, nameof(fileHierarchyResolver));

            this.fileHierarchyResolver = fileHierarchyResolver;
            modelPaths = new Dictionary<IDimrModel, string>();
        }

        /// <summary>
        /// Whether this instance has been used during a DIMR model import.
        /// </summary>
        public bool IsInitialized => DimrFilePath != null;

        /// <summary>
        /// The full file path to the imported DIMR configuration file.
        /// </summary>
        public string DimrFilePath
        {
            get => dimrFilePath;
            set
            {
                Ensure.NotNullOrWhiteSpace(value, nameof(value));
                dimrFilePath = value;
            }
        }

        /// <summary>
        /// Adds a DIMR model along with its relative model directory path.
        /// The model directory is relative to the DIMR file path.
        /// </summary>
        /// <param name="model"> The DIMR model. </param>
        /// <param name="modelDirectory"> The relative model directory path. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="modelDirectory"/> is <c>null</c> or white space.
        /// </exception>
        public void AddRelativeModelDirectory(IDimrModel model, string modelDirectory)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNullOrWhiteSpace(modelDirectory, nameof(modelDirectory));

            modelPaths[model] = modelDirectory;
            baseDirectory = GetBaseDirectory();
        }

        /// <summary>
        /// Remove a DIMR model along with its relative model directory path.
        /// If the model does not exist in this context, the method returns.
        /// </summary>
        /// <param name="model"> The DIMR model to remove. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Removing a model does not affect the original base directory of the imported integrated model  
        /// </remarks>
        public void RemoveModel(IDimrModel model)
        {
            Ensure.NotNull(model, nameof(model));
            modelPaths.Remove(model);
        }

        /// <summary>
        /// Get the relative model directory for the DIMR model.
        /// </summary>
        /// <param name="dimrModel"> The DIMR model. </param>
        /// <returns>
        /// The relative DIMR model directory.
        /// This will either be the original relative path from the imported model,
        /// or a default model directory name when the model was made from scratch.
        /// </returns>
        public string GetRelativeModelDirectory(IDimrModel dimrModel)
        {
            Ensure.NotNull(dimrModel, nameof(dimrModel));

            return modelPaths.TryGetValue(dimrModel, out string path) ? path : dimrModel.DirectoryName;
        }

        /// <summary>
        /// Get the DIMR file path relative to the base directory.
        /// </summary>
        /// <returns>
        /// The relative DIMR file path.
        /// </returns>
        public string GetRelativeDimrFilePath()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(DimrFilePath)} should be set before calling method {nameof(GetRelativeDimrFilePath)}");
            }

            return FileUtils.GetRelativePath(baseDirectory, DimrFilePath);
        }

        private string GetBaseDirectory()
        {
            return fileHierarchyResolver.GetBaseDirectoryFromFileReferences(DimrFilePath, modelPaths.Values).FullName;
        }
    }
}