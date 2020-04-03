using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Linq;

namespace DeltaShell.NGHS.TestUtils.AssertConstraints
{
    /// <summary>
    /// This constraint check whether a collection contains and only contains the expected item.
    /// </summary>
    /// <seealso cref="Constraint" />
    public class ContainsOnlyConstraint : Constraint
    {
        private readonly object expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsOnlyConstraint"/> class.
        /// </summary>
        /// <param name="expected">The only expected item.</param>
        public ContainsOnlyConstraint(object expected)
        {
            this.expected = expected;
        }

        /// <summary>
        /// Test whether the constraint is satisfied by checking whether
        /// the actual collection only contains the expected item.
        /// </summary>
        /// <param name="actualValue">The collection to be tested</param>
        /// <returns>
        /// True for success, false for failure
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="actualValue"/> is not an IEnumerable.
        /// </exception>
        public override bool Matches(object actualValue)
        {
            if (!(actualValue is IEnumerable collection))
            {
                throw new ArgumentException("The actual value must be an IEnumerable", nameof(actualValue));
            }

            actual = actualValue;

            object[] objects = collection.Cast<object>().ToArray();
            return objects.Length == 1 && ReferenceEquals(objects[0], expected);
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WriteMessageLine($"Collection containing only <{expected}>;");
        }
    }
}
