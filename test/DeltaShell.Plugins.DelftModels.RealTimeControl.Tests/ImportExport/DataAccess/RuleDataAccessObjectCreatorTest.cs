using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using NUnit.Framework;
using System;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.DataAccess
{
    [TestFixture]
    public class RuleDataAccessObjectCreatorTest
    {
        [Test]
        public void Create_RuleElementNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => RuleDataAccessObjectCreator.Create(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("ruleElement"));
        }
    }
}
