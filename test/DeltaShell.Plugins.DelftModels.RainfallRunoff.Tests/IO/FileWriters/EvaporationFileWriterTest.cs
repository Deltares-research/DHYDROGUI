using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.FileWriters
{
    [TestFixture]
    public class EvaporationFileWriterTest
    {
        [Test]
        [TestCaseSource(nameof(ArgNullCases))]
        public void Write_ArgNull_ThrowsArgumentNullException(IEvaporationFile evaporationFile, TextWriter textWriter, string expParamName)
        {
            // Setup
            var writer = new EvaporationFileWriter();

            // Call
            void Call() => writer.Write(evaporationFile, textWriter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo(expParamName));
        }

        [Test]
        public void Write_WritesToStream()
        {
            // Setup
            var writer = new EvaporationFileWriter();
            var evaporationFile = Substitute.For<IEvaporationFile>();
            evaporationFile.Header.Returns(new[]
            {
                "header_line_1",
                "header_line_2",
                "header_line_3"
            });
            evaporationFile.Evaporation.Returns(new Dictionary<EvaporationDate, double[]>
            {
                {
                    new EvaporationDate(2000, 9, 11), new[]
                    {
                        1.2323,
                        2.3434,
                        3.4545
                    }
                },
                {
                    new EvaporationDate(2001, 10, 10), new[]
                    {
                        4.5656,
                        5.6767,
                        6.7878
                    }
                },
                {
                    new EvaporationDate(2002, 11, 9), new[]
                    {
                        7.8989,
                        8.9090,
                        9.0101
                    }
                }
            });
            var textWriter = Substitute.For<TextWriter>();

            // Call
            writer.Write(evaporationFile, textWriter);

            // Assert
            Received.InOrder(() =>
            {
                textWriter.WriteLine("*header_line_1");
                textWriter.WriteLine("*header_line_2");
                textWriter.WriteLine("*header_line_3");
                textWriter.WriteLine("2000 09 11 1.2323 2.3434 3.4545");
                textWriter.WriteLine("2001 10 10 4.5656 5.6767 6.7878");
                textWriter.WriteLine("2002 11 09 7.8989 8.909 9.0101");
            });
        }

        private static IEnumerable<TestCaseData> ArgNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<TextWriter>(), "evaporationFile").SetName("evaporationFile null");
            yield return new TestCaseData(Substitute.For<IEvaporationFile>(), null, "textWriter").SetName("textWriter null");
        }
    }
}