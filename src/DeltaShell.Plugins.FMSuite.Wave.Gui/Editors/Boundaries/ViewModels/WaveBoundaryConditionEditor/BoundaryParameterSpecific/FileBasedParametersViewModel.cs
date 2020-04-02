using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="FileBasedParametersViewModel"/> defines the view model for the FileBasedParametersView.
    /// </summary>
    public class FileBasedParametersViewModel
    {
        /// <summary>
        /// Creates a new <see cref="FileBasedParametersViewModel"/>.
        /// </summary>
        /// <param name="parameters">The observed file based parameters.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public FileBasedParametersViewModel(FileBasedParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ObservedParameters = parameters;
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public FileBasedParameters ObservedParameters { get; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public string FilePath
        {
            get => ObservedParameters.FilePath;
            set => ObservedParameters.FilePath = value;
        }
    }
}