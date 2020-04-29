using System;
using System.IO;
using NUnit.Framework.Constraints;

namespace DeltaShell.NGHS.TestUtils.AssertConstraints
{
    /// <summary>
    /// This constraint checks whether a file or directory exists.
    /// </summary>
    /// <seealso cref="Constraint"/>
    public class ExistConstraint : Constraint
    {
        private string path;

        /// <summary>
        /// Test whether the constraint is satisfied by checking whether the file
        /// or directory at the specified <paramref name="actualValue"/> exists.
        /// </summary>
        /// <param name="actualValue">The path to check. </param>
        /// <returns>
        /// True for success, false for failure
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="actualValue"/> is not a string.
        /// </exception>
        public override bool Matches(object actualValue)
        {
            if (!(actualValue is string stringValue))
            {
                throw new ArgumentException("The value must be a string", nameof(actualValue));
            }

            path = stringValue;
            actual = File.Exists(path) || Directory.Exists(path);

            return (bool) actual;
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WriteMessageLine($"File or directory existing at <{path}>.");
        }
    }
}