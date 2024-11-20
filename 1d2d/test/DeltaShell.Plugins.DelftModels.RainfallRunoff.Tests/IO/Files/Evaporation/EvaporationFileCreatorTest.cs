using System;
using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Evaporation
{
    [TestFixture]
    public class EvaporationFileCreatorTest
    {
        [Test]
        public void CreateFor_IOEvaporationMeteoDataSourceUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var creator = new EvaporationFileCreator();

            // Call
            void Call() => creator.CreateFor((IOEvaporationMeteoDataSource)99);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<InvalidEnumArgumentException>()
                                    .With.Message.EqualTo("ioEvaporationMeteoDataSource"));
        }

        [Test]
        [TestCase(IOEvaporationMeteoDataSource.UserDefined, typeof(UserDefinedEvaporationFile))]
        [TestCase(IOEvaporationMeteoDataSource.LongTermAverage, typeof(LongTermAverageEvaporationFile))]
        [TestCase(IOEvaporationMeteoDataSource.GuidelineSewerSystems, typeof(GuidelineSewerSystemsEvaporationFile))]
        public void CreateFor_ReturnsCorrectResult(IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource, Type expEvaporationFileType)
        {
            // Setup
            var creator = new EvaporationFileCreator();

            // Call
            IEvaporationFile result = creator.CreateFor(ioEvaporationMeteoDataSource);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf(expEvaporationFileType));
        }
    }
}