using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class WaveDirectoryStructureMigrationHelperTest
    {
        [Test]
        public void Migrate_WaveModelNull_ThrowsArgumentNullException()
        {
            void Call() => WaveDirectoryStructureMigrationHelper.Migrate(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveModel"));
        }
    }
}