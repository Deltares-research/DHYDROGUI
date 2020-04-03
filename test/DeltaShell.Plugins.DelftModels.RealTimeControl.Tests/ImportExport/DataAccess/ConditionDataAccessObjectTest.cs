using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.DataAccess
{
    [TestFixture]
    public class ConditionDataAccessObjectTest
    {
        [Test]
        public void Constructor_IdNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new ConditionDataAccessObject(null, Substitute.For<ConditionBase>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("id"));
        }

        [Test]
        public void Constructor_ConditionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new ConditionDataAccessObject("", null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("condition"));
        }

        [TestCase("", "")]
        [TestCase("name", "name")]
        [TestCase("control_group/name", "control_group")]
        [TestCase("[tag]control_group", "control_group")]
        [TestCase("[tag]control_group/name", "control_group")]
        public void Constructor_InitializesInstanceCorrectly(string id, string expectedControlGroupName)
        {
            // Setup
            var condition = Substitute.For<ConditionBase>();

            // Call
            var dataAccessObject = new ConditionDataAccessObject(id, condition);

            // Assert
            Assert.That(dataAccessObject.Id, Is.EqualTo(id));
            Assert.That(dataAccessObject.ControlGroupName, Is.EqualTo(expectedControlGroupName));
            Assert.That(dataAccessObject.Object, Is.SameAs(condition));
            Assert.That(dataAccessObject.InputReferences, Is.Not.Null);
            Assert.That(dataAccessObject.TrueOutputReferences, Is.Not.Null);
            Assert.That(dataAccessObject.FalseOutputReferences, Is.Not.Null);
        }
    }
}
