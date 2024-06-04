using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Represents the data contained in an initial field file.
    /// This file is referenced by the MDU file through the <c>IniFieldFile</c> property.
    /// </summary>
    public sealed class InitialFieldFileData
    {
        private readonly IList<InitialFieldData> initialConditions;
        private readonly IList<InitialFieldData> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldFileData"/> class.
        /// </summary>
        public InitialFieldFileData()
        {
            General = new InitialFieldFileInfo("2.00", "iniField");
            initialConditions = new List<InitialFieldData>();
            parameters = new List<InitialFieldData>();
        }

        /// <summary>
        /// General information of the file.
        /// </summary>
        public InitialFieldFileInfo General { get; }

        /// <summary>
        /// The initial condition fields in the file.
        /// </summary>
        public IEnumerable<InitialFieldData> InitialConditions => initialConditions;

        /// <summary>
        /// The parameter fields in the file.
        /// </summary>
        public IEnumerable<InitialFieldData> Parameters => parameters;

        /// <summary>
        /// The initial conditions and the parameter fields.
        /// </summary>
        public IEnumerable<InitialFieldData> AllFields => initialConditions.Concat(parameters);

        /// <summary>
        /// Add a new initial condition field.
        /// </summary>
        /// <param name="initialFieldData"> The new initial condition field. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialFieldData"/> is <c>null</c>.
        /// </exception>
        public void AddInitialCondition(InitialFieldData initialFieldData)
        {
            Ensure.NotNull(initialFieldData, nameof(initialFieldData));
            initialConditions.Add(initialFieldData);
        }

        /// <summary>
        /// Add a new parameter field.
        /// </summary>
        /// <param name="initialFieldData"> The new parameter field. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialFieldData"/> is <c>null</c>.
        /// </exception>
        public void AddParameter(InitialFieldData initialFieldData)
        {
            Ensure.NotNull(initialFieldData, nameof(initialFieldData));
            parameters.Add(initialFieldData);
        }
    }
}