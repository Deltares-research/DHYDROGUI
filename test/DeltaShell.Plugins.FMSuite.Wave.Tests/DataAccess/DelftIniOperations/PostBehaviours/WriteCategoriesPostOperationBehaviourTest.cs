using System.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.DelftIniOperations.PostBehaviours
{
    [TestFixture]
    public class WriteCategoriesPostOperationBehaviourTest : DelftIniPostOperationBehaviourTestFixture
    {
        protected override DelftIniPostOperationBehaviour ConstructPostBehaviour() =>
            new WriteCategoriesPostOperationBehaviour(Substitute.For<IDelftIniWriter>(), "");

        [Test]
        public void Constructor_ExpectedResults()
        {
            var writer = Substitute.For<IDelftIniWriter>();
            const string goalDirectory = "someGoalDirectory";

            // Call
            var behaviour = new WriteCategoriesPostOperationBehaviour(writer, goalDirectory);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IDelftIniPostOperationBehaviour>());
        }

        [Test]
        public void Constructor_IniWriterNull_ThrowsArgumentNullException()
        {
            const string goalDirectory = "someGoalDirectory";

            // Call | Assert
            void Call() => new WriteCategoriesPostOperationBehaviour(null, goalDirectory);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("iniWriter"));
        }

        [Test]
        public void Constructor_GoalDirectoryNull_ThrowsArgumentNullException()
        {
            const string goalDirectory = "someGoalDirectory";

            // Call | Assert
            void Call() => new WriteCategoriesPostOperationBehaviour(null, goalDirectory);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("iniWriter"));
        }

        [Test]
        public void Invoke_WritesCategoriesWithIniWriter()
        {
            // Setup
            var writer = Substitute.For<IDelftIniWriter>();
            const string goalDirectory = "some/Goal/Directory";
            const string sourceFilePath = "anotherPath/to/a/directory/with/a/file.ini";

            string fileName = Path.GetFileName(sourceFilePath);

            var behaviour = new WriteCategoriesPostOperationBehaviour(writer, goalDirectory);
            var iniData = new IniData();

            // Call
            behaviour.Invoke(Stream.Null, sourceFilePath, iniData, null);

            // Assert
            writer.Received(1).WriteDelftIniFile(iniData, Path.Combine(goalDirectory, fileName), true);
        }
    }
}