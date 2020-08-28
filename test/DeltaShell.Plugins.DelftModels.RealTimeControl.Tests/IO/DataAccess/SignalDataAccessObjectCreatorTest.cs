using System;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class SignalDataAccessObjectCreatorTest
    {
        [Test]
        public void Create_SignalElementNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => SignalDataAccessObjectCreator.Create(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("signalElement"));
        }
    }
}