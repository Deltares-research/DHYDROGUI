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
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();

            // Call
            var nameProvider = new UniqueBoundaryNameProvider(boundaryContainer);

            // Assert
            Assert.That(nameProvider, Is.InstanceOf<IUniqueBoundaryNameProvider>());
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new UniqueBoundaryNameProvider(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"), 
                        "Expected a different ParamName:");
        }

        [Test]
        [TestCaseSource(nameof(GetUniqueBoundaryNameTestData))]
        public void GivenAnUniqueBoundaryNameProviderWithABoundaryContainer_WhenGetUniqueNameIsCalled_ThenTheExpectedResultIsReturned(IBoundaryContainer boundaryContainer, string expectedResult)
        {
            // Given
            var provider = new UniqueBoundaryNameProvider(boundaryContainer);

            // When
            string result = provider.GetUniqueName();

            // Then
            Assert.That(result, Is.EqualTo(expectedResult), "Expected a different result.");
        }

        private static IEnumerable<TestCaseData> GetUniqueBoundaryNameTestData()
        {

            yield return new TestCaseData(EmptyContainer, UniqueBoundaryNameProvider.DefaultBoundaryName);
            yield return new TestCaseData(NoDefaultContainer,            "Boundary(1)");
            yield return new TestCaseData(GetContainerWithNElements(5),  "Boundary(5)");
            yield return new TestCaseData(GetContainerWithNElements(12), "Boundary(12)");
            yield return new TestCaseData(GetContainerWithNElements(53), "Boundary(53)");
        }

        private static IBoundaryContainer EmptyContainer
        {
            get
            {
                var containerEmpty = Substitute.For<IBoundaryContainer>();
                containerEmpty.Boundaries.Returns(new EventedList<IWaveBoundary>());

                return containerEmpty;
            }
        }

        private static IBoundaryContainer NoDefaultContainer
        {
            get
            { 
                var containerNoDefault = Substitute.For<IBoundaryContainer>(); 
                var boundariesNoDefault = new EventedList<IWaveBoundary> 
                {
                    GetBoundaryMockWithName("a1"), 
                    GetBoundaryMockWithName("b2"), 
                    GetBoundaryMockWithName("c3"),
                };

                containerNoDefault.Boundaries.Returns(boundariesNoDefault);
                return containerNoDefault;
            }
        }

        private static IBoundaryContainer GetContainerWithNElements(int nElements)
        {
            var container= Substitute.For<IBoundaryContainer>();
            var boundaries = new EventedList<IWaveBoundary>
            {
                GetBoundaryMockWithName(UniqueBoundaryNameProvider.DefaultBoundaryName),
            };

            const string template = UniqueBoundaryNameProvider.DefaultBoundaryName + "({0})";

            for (var i = 1; i < nElements; i++)
            {
                boundaries.Add(GetBoundaryMockWithName(string.Format(template, i)));
            }

            container.Boundaries.Returns(boundaries);
            return container;
        }

        private static IWaveBoundary GetBoundaryMockWithName(string name)
        {
            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name.Returns(name);

            return boundary;
        }
    }
}