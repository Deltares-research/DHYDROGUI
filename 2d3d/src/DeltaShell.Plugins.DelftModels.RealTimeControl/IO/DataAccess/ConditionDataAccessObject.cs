using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Data access object for importing a <see cref="ConditionBase"/> from the tools config xml file.
    /// </summary>
    /// <seealso cref="IRtcDataAccessObject{ConditionBase}"/>
    public class ConditionDataAccessObject : IRtcDataAccessObject<ConditionBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionDataAccessObject"/> class.
        /// </summary>
        /// <param name="id"> The identifier that was read from the file. </param>
        /// <param name="condition"> The created condition. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="id"/> or <paramref name="condition"/> is <c>null</c>.
        /// </exception>
        public ConditionDataAccessObject(string id, ConditionBase condition)
        {
            Ensure.NotNull(id, nameof(id));
            Ensure.NotNull(condition, nameof(condition));

            Id = id;
            ControlGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(Id);
            Object = condition;
        }

        /// <summary>
        /// Gets the references to the condition inputs.
        /// </summary>
        /// <value>
        /// The input references.
        /// </value>
        public IList<string> InputReferences { get; } = new List<string>();

        /// <summary>
        /// Gets the references to the condition true outputs.
        /// </summary>
        /// <value>
        /// The true output references.
        /// </value>
        public IList<string> TrueOutputReferences { get; } = new List<string>();

        /// <summary>
        /// Gets the references to the condition false outputs.
        /// </summary>
        /// <value>
        /// The false output references.
        /// </value>
        public IList<string> FalseOutputReferences { get; } = new List<string>();

        public string Id { get; }

        public string ControlGroupName { get; }

        /// <summary>
        /// Gets the <see cref="ConditionBase"/> that was created from the tools config file.
        /// </summary>
        /// <value>
        /// The created condition.
        /// </value>
        public ConditionBase Object { get; }
    }
}