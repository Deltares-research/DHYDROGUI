using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Data access object for importing a <see cref="RuleBase"/> from the tools config xml file.
    /// </summary>
    /// <seealso cref="IRtcDataAccessObject{RuleBase}"/>
    public class RuleDataAccessObject : IRtcDataAccessObject<RuleBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleDataAccessObject"/> class.
        /// </summary>
        /// <param name="id"> The identifier that was read from the file. </param>
        /// <param name="rule"> The created rule. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="id"/> or <paramref name="rule"/> is <c> null </c>.
        /// </exception>
        public RuleDataAccessObject(string id, RuleBase rule)
        {
            Ensure.NotNull(id, nameof(id));
            Ensure.NotNull(rule, nameof(rule));

            Id = id;
            ControlGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(Id);
            Object = rule;
        }

        /// <summary>
        /// Gets the references to the rule inputs.
        /// </summary>
        /// <value>
        /// The input references.
        /// </value>
        public IList<string> InputReferences { get; } = new List<string>();

        /// <summary>
        /// Gets the references to the rule outputs.
        /// </summary>
        /// <value>
        /// The output references.
        /// </value>
        public IList<string> OutputReferences { get; } = new List<string>();

        /// <summary>
        /// Gets the references to the signals.
        /// </summary>
        /// <value>
        /// The signal references.
        /// </value>
        public IList<string> SignalReferences { get; } = new List<string>();

        public string Id { get; }

        public string ControlGroupName { get; }

        /// <summary>
        /// Gets the <see cref="RuleBase"/> that was created from the tools config file.
        /// </summary>
        /// <value>
        /// The created rule.
        /// </value>
        public RuleBase Object { get; }
    }
}