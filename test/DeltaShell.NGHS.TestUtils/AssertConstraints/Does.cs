using NUnit.Framework.Constraints;

namespace DeltaShell.NGHS.TestUtils.AssertConstraints
{
    /// <summary>
    /// Helper class with properties and methods that supply
    /// a number of constraints used in Asserts.
    /// </summary>
    public static class Does
    {
        /// <summary>
        /// Gets the exist constraint
        /// </summary>
        public static ExistConstraint Exist => new ExistConstraint();
    }
}