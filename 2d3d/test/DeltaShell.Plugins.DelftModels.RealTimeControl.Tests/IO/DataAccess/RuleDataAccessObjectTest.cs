using System;
using System.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class RuleDataAccessObjectTest
    {
        [Test]
        public void Constructor_IdNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RuleDataAccessObject(null, Substitute.For<RuleBase>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("id"));
        }

        [Test]
        public void Constructor_RuleNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RuleDataAccessObject("", null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("rule"));
        }

        [TestCase("", "")]
        [TestCase("name", "name")]
        [TestCase("control_group/name", "control_group")]
        [TestCase("[tag]control_group", "control_group")]
        [TestCase("[tag]control_group/name", "control_group")]
        public void Constructor_InitializesInstanceCorrectly(string id, string expectedControlGroupName)
        {
            // Setup
            var rule = Substitute.For<RuleBase>();

            // Call
            var dataAccessObject = new RuleDataAccessObject(id, rule);

            // Assert
            Assert.That(dataAccessObject.Id, Is.EqualTo(id));
            Assert.That(dataAccessObject.ControlGroupName, Is.EqualTo(expectedControlGroupName));
            Assert.That(dataAccessObject.Object, Is.SameAs(rule));

            AssertIsEmpty(dataAccessObject.InputReferences);
            AssertIsEmpty(dataAccessObject.SignalReferences);
            AssertIsEmpty(dataAccessObject.OutputReferences);
        }

        private static void AssertIsEmpty(IEnumerable source)
        {
            Assert.That(source, Is.Not.Null);
            Assert.That(source, Is.Empty);
        }
    }
}