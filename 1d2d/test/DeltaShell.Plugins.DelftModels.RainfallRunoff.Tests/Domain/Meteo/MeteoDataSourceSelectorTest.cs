using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataSourceSelectorTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(IManifestRetriever manifestRetriever,
                                                                    MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator,
                                                                    string expParamName)
        {
            // Call
            void Call() => new MeteoDataSourceSelector(manifestRetriever, meteoTimeSeriesInstanceCreator);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        [TestCase(MeteoDataSource.LongTermAverage, "EVAPOR.GEM")]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, "EVAPOR.PLV")]
        public void GetMeteoTimeSeries_ForNonUserDefined_ModelEvaporationFileDoesNotExist_GetsEvaporationDataFromManifest(MeteoDataSource meteoDataSource, string expFileName)
        {
            //Arrange
            var manifestRetriever = Substitute.For<IManifestRetriever>();
            var stream = Substitute.For<Stream>();
            stream.CanRead.Returns(true);
            manifestRetriever.GetFixedStream(expFileName).Returns(stream);
            var meteoTimeSeriesInstanceCreator = new MeteoTimeSeriesInstanceCreator();
            var sourceSelector = new MeteoDataSourceSelector(manifestRetriever, meteoTimeSeriesInstanceCreator);

            //Act
            IFunction timeSeries = sourceSelector.GetMeteoTimeSeries(meteoDataSource, new DirectoryInfo("does_not_exist"));

            //Assert
            manifestRetriever.Received(1).GetFixedStream(expFileName);
            Assert.That(timeSeries, Is.Not.Null);
        }

        [Test]
        [TestCase(MeteoDataSource.LongTermAverage, "EVAPOR.GEM")]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, "EVAPOR.PLV")]
        public void GetMeteoTimeSeries_ForNonUserDefined_ModelEvaporationFileDoesExist_GetsEvaporationDataFromModel(MeteoDataSource meteoDataSource, string expFileName)
        {
            //Arrange
            var manifestRetriever = Substitute.For<IManifestRetriever>();
            var meteoTimeSeriesInstanceCreator = new MeteoTimeSeriesInstanceCreator();
            var sourceSelector = new MeteoDataSourceSelector(manifestRetriever, meteoTimeSeriesInstanceCreator);

            using (var temp = new TemporaryDirectory())
            {
                string content = string.Join(Environment.NewLine,
                                             "0000 01 02 1.23",
                                             "0000 03 04 2.34",
                                             "0000 05 06 3.45");
                temp.CreateFile(expFileName, content);

                //Act
                IFunction timeSeries = sourceSelector.GetMeteoTimeSeries(meteoDataSource, new DirectoryInfo(temp.Path));

                //Assert
                manifestRetriever.Received(0).GetFixedStream(expFileName);
                Assert.That(timeSeries, Is.Not.Null);
                Assert.That(timeSeries.GetValues(), Has.Count.EqualTo(3));
                Assert.That(timeSeries[new DateTime(1904, 01, 02)], Is.EqualTo(1.23));
                Assert.That(timeSeries[new DateTime(1904, 03, 04)], Is.EqualTo(2.34));
                Assert.That(timeSeries[new DateTime(1904, 05, 06)], Is.EqualTo(3.45));
            }
        }

        [Test]
        public void GetMeteoTimeSeries_ForUserDefined_GetsEmptyEvaporationData()
        {
            //Arrange
            var manifestRetriever = Substitute.For<IManifestRetriever>();
            var meteoTimeSeriesInstanceCreator = new MeteoTimeSeriesInstanceCreator();
            var sourceSelector = new MeteoDataSourceSelector(manifestRetriever, meteoTimeSeriesInstanceCreator);

            //Act
            IFunction timeSeries = sourceSelector.GetMeteoTimeSeries(MeteoDataSource.UserDefined, new DirectoryInfo("does_not_exist"));

            //Assert
            Assert.That(timeSeries, Is.Not.Null);
            Assert.That(timeSeries.Name, Is.EqualTo("Global"));
            Assert.That(timeSeries.GetValues(), Is.Empty);
        }

        [Test]
        public void GetMeteoTimeSeries_MeteoDataSourceUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var manifestRetriever = Substitute.For<IManifestRetriever>();
            var meteoTimeSeriesInstanceCreator = new MeteoTimeSeriesInstanceCreator();
            var sourceSelector = new MeteoDataSourceSelector(manifestRetriever, meteoTimeSeriesInstanceCreator);

            // Call
            void Call() => sourceSelector.GetMeteoTimeSeries((MeteoDataSource)99, new DirectoryInfo("does_not_Exist"));

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<InvalidEnumArgumentException>()
                                    .With.Message.EqualTo("meteoDataSource"));
        }
        
        [Test]
        public void GetMeteoTimeSeries_ModelDirectoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var manifestRetriever = Substitute.For<IManifestRetriever>();
            var meteoTimeSeriesInstanceCreator = new MeteoTimeSeriesInstanceCreator();
            var sourceSelector = new MeteoDataSourceSelector(manifestRetriever, meteoTimeSeriesInstanceCreator);

            // Call
            void Call() => sourceSelector.GetMeteoTimeSeries(MeteoDataSource.UserDefined, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo("modelDirectory"));
        }

        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null,
                                          new MeteoTimeSeriesInstanceCreator(),
                                          "manifestRetriever");
            yield return new TestCaseData(Substitute.For<IManifestRetriever>(),
                                          null,
                                          "meteoTimeSeriesInstanceCreator");
        }
    }
}