using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.DelftIniOperations.PostBehaviours
{
    [TestFixture]
    public class DeleteSourcePostOperationBehaviourTest : DelftIniPostOperationBehaviourTestFixture
    {
        protected override DelftIniPostOperationBehaviour ConstructPostBehaviour() =>
            new DeleteSourcePostOperationBehaviour();

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var behaviour = new DeleteSourcePostOperationBehaviour();

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IDelftIniPostOperationBehaviour>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_RemovesSourceFilePath()
        {
            // Setup
            var behaviour = new DeleteSourcePostOperationBehaviour();

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = tempDir.CreateFile("someFile");

                // Call
                behaviour.Invoke(Stream.Null, 
                                 filePath, 
                                 new List<DelftIniCategory>(), 
                                 null);

                // Assert
                var fileInfo = new FileInfo(filePath);
                Assert.That(fileInfo.Exists, Is.False);
            }
        }
    }
}