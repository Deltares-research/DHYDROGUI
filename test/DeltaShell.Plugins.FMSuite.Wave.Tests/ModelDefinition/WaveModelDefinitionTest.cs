using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.ModelDefinition
{
    [TestFixture]
    public class WaveModelDefinitionTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadModelDefinitionFromMdw()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var modelDefinition = new MdwFile().Load(mdwPath);
            
            Assert.AreEqual(7, modelDefinition.ModelSchema.GuiPropertyGroups.Count);
            Assert.AreEqual(6, modelDefinition.ModelSchema.ModelDefinitionCategory.Count);

            Assert.AreEqual("nautical",
                            modelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, "DirConvention")
                                           .GetValueAsString());
        }
    }
}
