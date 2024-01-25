using System.Collections.Generic;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data
{
    /// <summary>
    /// Represents the data contained in an initial field file.
    /// This file is referenced by the MDU file through the `InitialFieldFile` property.
    /// </summary>
    public sealed class InitialFieldFileData
    {
        private readonly IList<InitialField> initialConditions;
        private readonly IList<InitialField> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldFileData"/> class.
        /// </summary>
        public InitialFieldFileData()
        {
            General = new InitialFieldFileInfo("2.00", "iniField");
            initialConditions = new List<InitialField>();
            parameters = new List<InitialField>();
        }

        /// <summary>
        /// General information of the file.
        /// </summary>
        public InitialFieldFileInfo General { get; }

        /// <summary>
        /// The initial condition fields in the file.
        /// </summary>
        public IEnumerable<InitialField> InitialConditions => initialConditions;

        /// <summary>
        /// The  parameter fields in the file.
        /// </summary>
        public IEnumerable<InitialField> Parameters => parameters;

        /// <summary>
        /// Add a new initial condition field.
        /// </summary>
        /// <param name="initialField"> The new initial condition field. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialField"/> is <c>null</c>.
        /// </exception>
        public void AddInitialCondition(InitialField initialField)
        {
            Ensure.NotNull(initialField, nameof(initialField));
            initialConditions.Add(initialField);
        }

        /// <summary>
        /// Add a new parameter field.
        /// </summary>
        /// <param name="initialField"> The new parameter field. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialField"/> is <c>null</c>.
        /// </exception>
        public void AddParameter(InitialField initialField)
        {
            Ensure.NotNull(initialField, nameof(initialField));
            parameters.Add(initialField);
        }
    }
}