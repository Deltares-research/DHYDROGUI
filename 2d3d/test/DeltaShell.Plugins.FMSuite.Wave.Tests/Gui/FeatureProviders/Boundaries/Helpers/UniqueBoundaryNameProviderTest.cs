using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Helpers
{
    [TestFixture]
    public class UniqueBoundaryNameProviderTest
    {
        private static IBoundaryProvider EmptyProvider
        {
            get
            {
                var emptyProvider = Substitute.For<IBoundaryProvider>();
                emptyProvider.Boundaries.Returns(new EventedList<IWaveBoundary>());

                return emptyProvider;
            }
        }

        private static IBoundaryProvider NoDefaultProvider
        {
            get
            {
                var providerDefault = Substitute.For<IBoundaryProvider>();
                var boundariesNoDefault = new EventedList<IWaveBoundary>
                {
                    GetBoundaryMockWithName("a1"),
                    GetBoundaryMockWithName("b2"),
                    GetBoundaryMockWithName("c3")
                };

                providerDefault.Boundaries.Returns(boundariesNoDefault);
                return providerDefault;
            }
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();

            // Call
            var nameProvider = new UniqueBoundaryNameProvider(boundaryProvider);

            // Assert
            Assert.That(nameProvider, Is.InstanceOf<IUniqueBoundaryNameProvider>());
        }

        [Test]
        public void Constructor_BoundaryProviderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new UniqueBoundaryNameProvider(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryProvider"),
                        "Expected a different ParamName:");
        }

        [Test]
        [TestCaseSource(nameof(GetUniqueBoundaryNameTestData))]
        public void GetUniqueName_ExpectedResults(IBoundaryProvider boundaryProvider, string expectedResult)
        {
            // Given
            var provider = new UniqueBoundaryNameProvider(boundaryProvider);

            // When
            string result = provider.GetUniqueName();

            // Then
            Assert.That(result, Is.EqualTo(expectedResult), "Expected a different result.");
        }

        private static IEnumerable<TestCaseData> GetUniqueBoundaryNameTestData()
        {
            yield return new TestCaseData(EmptyProvider, UniqueBoundaryNameProvider.DefaultBoundaryName);
            yield return new TestCaseData(NoDefaultProvider, "Boundary(1)");
            yield return new TestCaseData(GetProviderWithNElements(5), "Boundary(5)");
            yield return new TestCaseData(GetProviderWithNElements(12), "Boundary(12)");
            yield return new TestCaseData(GetProviderWithNElements(53), "Boundary(53)");
        }

        private static IBoundaryProvider GetProviderWithNElements(int nElements)
        {
            var provider = Substitute.For<IBoundaryProvider>();
            var boundaries = new EventedList<IWaveBoundary> {GetBoundaryMockWithName(UniqueBoundaryNameProvider.DefaultBoundaryName)};

            const string template = UniqueBoundaryNameProvider.DefaultBoundaryName + "({0})";

            for (var i = 1; i < nElements; i++)
            {
                boundaries.Add(GetBoundaryMockWithName(string.Format(template, i)));
            }

            provider.Boundaries.Returns(boundaries);
            return provider;
        }

        private static IWaveBoundary GetBoundaryMockWithName(string name)
        {
            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name.Returns(name);

            return boundary;
        }
    }
}