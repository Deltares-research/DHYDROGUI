using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Extensions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Extensions
{
    [TestFixture]
    public class SnappedFeaturesRelatedExtensionsTest
    {
        private static IEnumerable<TestCaseData> GetHasSnappedOutputFeaturesData()
        {
            TestCaseData ToCase(bool writeSnappedFeatures, string path, bool expectedResult)
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                model.WriteSnappedFeatures.Returns(writeSnappedFeatures);
                model.OutputSnappedFeaturesPath.Returns(path);

                return new TestCaseData(model, expectedResult);
            }

            // Current directory should exist.
            const string validDirectoryPath = ".";
            const string invalidDirectoryPath = "C:/I/Really/Hope/This/Does/Not/Exist/Otherwise/What/Are/You/Doing";

            yield return new TestCaseData(null, false);
            yield return ToCase(false, validDirectoryPath, false);
            yield return ToCase(true, invalidDirectoryPath, false);
            yield return ToCase(false, invalidDirectoryPath, false);
            yield return ToCase(true, validDirectoryPath, true);
        }

        [Test]
        [TestCaseSource(nameof(GetHasSnappedOutputFeaturesData))]
        public void HasSnappedOutputFeatures_ExpectedResults(IWaterFlowFMModel model, bool expectedResult)
        {
            Assert.That(model.HasSnappedOutputFeatures(), Is.EqualTo(expectedResult));
        }
        
    }
}