using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;



namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlOutputFileFunctionStoreTest
    {
        /// <summary>
        /// GIVEN a new FunctionStore
        ///   AND a function to provide some set of features
        /// WHEN SetFeaturesWith is called with this function
        /// THEN the features contain the set of features
        /// </summary>
        [Test]
        public void GivenANewFunctionStoreAndAFunctionToProvideSomeSetOfFeatures_WhenSetFeaturesWithIsCalledWithThisFunction_ThenTheFeaturesContainTheSetOfFeatures()
        {
            // Given
            var functionStore = new RealTimeControlOutputFileFunctionStore();

            var mockRepository = new MockRepository();

            var mockedFeature1 = mockRepository.DynamicMock<IFeature>();
            var mockedFeature2 = mockRepository.DynamicMock<IFeature>();
            var mockedFeature3 = mockRepository.DynamicMock<IFeature>();

            var simpleList = new List<IFeature>
            {
                mockedFeature1,
                mockedFeature2,
                mockedFeature3,
            };

            Func<IEnumerable<IFeature>> lazyFeatureFunc = () => simpleList;

            mockRepository.ReplayAll();

            // When
            functionStore.SetFeaturesWith(lazyFeatureFunc);

            mockRepository.VerifyAll();

            // Then
            Assert.That(functionStore.Features, Is.Not.Null);
            Assert.That(functionStore.Features.Count(), Is.EqualTo(3));

            Assert.That(functionStore.Features.Contains(mockedFeature1));
            Assert.That(functionStore.Features.Contains(mockedFeature2));
            Assert.That(functionStore.Features.Contains(mockedFeature3));
        }

        /// <summary>
        /// GIVEN a new FunctionStore
        ///   AND a function to provide some set of features
        /// WHEN LazySetFeatures is called with this function
        ///  AND SetFeaturesWith null is called
        /// THEN the features do not contain any values
        /// </summary>
        [Test]
        public void GivenANewFunctionStoreAndAFunctionToProvideSomeSetOfFeatures_WhenLazySetFeaturesIsCalledWithThisFunctionAndSetFeaturesWithNullIsCalled_ThenTheFeaturesDoNotContainAnyValues()
        {
            // Given
            var functionStore = new RealTimeControlOutputFileFunctionStore();

            var mockRepository = new MockRepository();

            var mockedFeature1 = mockRepository.DynamicMock<IFeature>();
            var mockedFeature2 = mockRepository.DynamicMock<IFeature>();
            var mockedFeature3 = mockRepository.DynamicMock<IFeature>();

            var simpleList = new List<IFeature>
            {
                mockedFeature1,
                mockedFeature2,
                mockedFeature3,
            };

            Func<IEnumerable<IFeature>> lazyFeatureFunc = () => simpleList;

            mockRepository.ReplayAll();

            // When
            functionStore.SetFeaturesWith(lazyFeatureFunc);
            functionStore.SetFeaturesWith();

            mockRepository.VerifyAll();

            // Then
            Assert.That(functionStore.Features, Is.Not.Null);
            Assert.That(functionStore.Features.Count(), Is.EqualTo(0));
            Assert.That(functionStore.Features.Any(), Is.False);
        }
    }
}
