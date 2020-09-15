using System;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class SignalDataAccessObjectTest
    {
        [Test]
        public void Constructor_IdNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SignalDataAccessObject(null, Substitute.For<SignalBase>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("id"));
        }

        [Test]
        public void Constructor_SignalNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SignalDataAccessObject("", null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("signal"));
        }

        [TestCase("", "")]
        [TestCase("name", "name")]
        [TestCase("control_group/name", "control_group")]
        [TestCase("[tag]control_group", "control_group")]
        [TestCase("[tag]control_group/name", "control_group")]
        public void Constructor_InitializesInstanceCorrectly(string id, string expectedControlGroupName)
        {
            // Setup
            var signal = Substitute.For<SignalBase>();

            // Call
            var dataAccessObject = new SignalDataAccessObject(id, signal);

            // Assert
            Assert.That(dataAccessObject.Id, Is.EqualTo(id));
            Assert.That(dataAccessObject.ControlGroupName, Is.EqualTo(expectedControlGroupName));
            Assert.That(dataAccessObject.Object, Is.SameAs(signal));
            Assert.That(dataAccessObject.InputReferences, Is.Not.Null);
        }
    }
}