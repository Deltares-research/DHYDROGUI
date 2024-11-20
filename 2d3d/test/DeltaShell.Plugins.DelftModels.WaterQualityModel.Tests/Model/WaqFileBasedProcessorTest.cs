using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqFileBasedProcessorTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAWaqFileBasedPreProcessorAndSettingsWithTheGridFilePath_WhenCallWriteIncludeFilesAndBinaryFiles_ThenGridIncludeFileIsWritten()
        {
            // Given
            WaqInitializationSettings settings = CreateBasicWaqInitializationSettings();

            using (var temp = new TemporaryDirectory())
            {
                string tempPath = temp.Path;
                string expectedGridFilePath = Path.Combine(tempPath, "B3_ugrid.inc");
                settings.GridFile = expectedGridFilePath;

                // When
                new WaqFileBasedPreProcessor().WriteIncludeFilesAndBinaryFiles(settings, tempPath);

                // Then
                Assert.That(File.Exists(expectedGridFilePath));
            }
        }

        private static WaqInitializationSettings CreateBasicWaqInitializationSettings()
        {
            return new WaqInitializationSettings
            {
                SubstanceProcessLibrary = new SubstanceProcessLibrary(),
                OutputLocations = new Dictionary<string, IList<int>>(),
                Dispersion = new List<IFunction> {new Function()},
                BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                BoundaryAliases = new Dictionary<string, IList<string>>(),
                BoundaryDataManager = new DataTableManager(),
                LoadAndIds = new ConcurrentDictionary<WaterQualityLoad, int>(),
                LoadsAliases = new Dictionary<string, IList<string>>(),
                LoadsDataManager = new DataTableManager(),
                ProcessCoefficients = new List<IFunction>()
            };
        }
    }
}