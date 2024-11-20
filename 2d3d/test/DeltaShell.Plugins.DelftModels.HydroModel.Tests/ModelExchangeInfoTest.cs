using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.Common;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class ModelExchangeInfoTest
    {
        [Test]
        [TestCaseSource(nameof(GetConstructorArgumentNullTestCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ICoupledModel sourceModel, ICoupledModel targetModel)
        {
            // Call
            void Call() => _ = new ModelExchangeInfo(sourceModel, targetModel);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        private static IEnumerable<TestCaseData> GetConstructorArgumentNullTestCases()
        {
            var model = Substitute.For<ICoupledModel>();
            yield return new TestCaseData(null, model);
            yield return new TestCaseData(model, null);
        }

        [Test]
        public void Constructor_SetsSourceAndTargetModelName()
        {
            // Setup
            var sourceModel = Substitute.For<ICoupledModel>();
            const string sourceModelName = "Pikachu";
            sourceModel.Name.Returns(sourceModelName);

            var targetModel = Substitute.For<ICoupledModel>();
            const string targetModelName = "Blastoise";
            targetModel.Name.Returns(targetModelName);

            // Call
            var modelExchangeInfo = new ModelExchangeInfo(sourceModel, targetModel);

            // Assert
            Assert.That(modelExchangeInfo.SourceModelName, Is.EqualTo(sourceModelName));
            Assert.That(modelExchangeInfo.TargetModelName, Is.EqualTo(targetModelName));
        }

        [Test]
        [TestCaseSource(nameof(GetAddExchangeArgumentNullCases))]
        public void AddExchange_ArgumentNull_ThrowsArgumentNullException(IDataItem sourceDataItem, IDataItem targetDataItem)
        {
            // Setup
            var sourceModel = Substitute.For<ICoupledModel>();
            var targetModel = Substitute.For<ICoupledModel>();

            var modelExchangeInfo = new ModelExchangeInfo(sourceModel, targetModel);

            // Call
            void Call() => modelExchangeInfo.AddExchange(sourceDataItem, targetDataItem);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        private static IEnumerable<TestCaseData> GetAddExchangeArgumentNullCases()
        {
            var dataItem = Substitute.For<IDataItem>();
            yield return new TestCaseData(null, dataItem);
            yield return new TestCaseData(dataItem, null);
        }

        [Test]
        [TestCaseSource(nameof(GetModelExchangeConstructorArgumentNullCases))]
        public void ModelExchangeConstructor_ArgumentNull_ThrowsArgumentNullException(string sourceName, string targetName)
        {
            // Call
            void Call() => _ = new ModelExchange(sourceName, targetName);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        private static IEnumerable<TestCaseData> GetModelExchangeConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, "Blastoise");
            yield return new TestCaseData("Pikachu", null);
        }

        [Test]
        public void ModelExchangeConstructor_SetsSourceNameAndTargetName()
        {
            // Setup
            const string sourceName = "Pikachu";
            const string targetName = "Blastoise";

            // Call
            var modelExchange = new ModelExchange(sourceName, targetName);

            // Assert
            Assert.That(modelExchange.SourceName, Is.EqualTo(sourceName));
            Assert.That(modelExchange.TargetName, Is.EqualTo(targetName));
        }
    }
}