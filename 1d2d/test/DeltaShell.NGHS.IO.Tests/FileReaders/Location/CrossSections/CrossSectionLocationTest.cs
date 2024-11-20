using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders.Location.CrossSections;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Location.CrossSections
{
    [TestFixture]
    public class CrossSectionLocationTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var location = new CrossSectionLocation("some_id",
                                                    "some_long_name",
                                                    "some_branch_id",
                                                    1.23,
                                                    2.34,
                                                    "some_definition_id");

            // Assert
            Assert.That(location.Id, Is.EqualTo("some_id"));
            Assert.That(location.LongName, Is.EqualTo("some_long_name"));
            Assert.That(location.BranchId, Is.EqualTo("some_branch_id"));
            Assert.That(location.Chainage, Is.EqualTo(1.23));
            Assert.That(location.Shift, Is.EqualTo(2.34));
            Assert.That(location.DefinitionId, Is.EqualTo("some_definition_id"));
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullOrEmptyCases))]
        public void Constructor_ArgumentNullOrEmpty_ThrowsArgumentNullException(string id, string branchId, string definitionId, string expParamName)
        {
            // Call
            void Call()
            {
                new CrossSectionLocation(id, "some_long_name", branchId, 1.23, 2.34, definitionId);
            }

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Constructor_ChainageNegative_ThrowsArgumentOutOfRangeException()
        {
            // Call
            void Call() => new CrossSectionLocation("some_id", 
                                                    "some_long_name", 
                                                    "some_branch_id", 
                                                    -1.23, 
                                                    2.34, 
                                                    "some_definition_id");

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("chainage"));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullOrEmptyCases()
        {
            yield return new TestCaseData(null, "some_branch_id", "some_definition_id", "id");
            yield return new TestCaseData(string.Empty, "some_branch_id", "some_definition_id", "id");
            yield return new TestCaseData("some_id", null, "some_definition_id", "branchId");
            yield return new TestCaseData("some_id", string.Empty, "some_definition_id", "branchId");
            yield return new TestCaseData("some_id", "some_branch_id", null, "definitionId");
            yield return new TestCaseData("some_id", "some_branch_id", string.Empty, "definitionId");
        }
    }
}