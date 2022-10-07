using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.FileWriters
{
    [TestFixture]
    public class FnmFileWriterTest
    {
        private const int amountOfHeaderLinesInFnmFile = 12;
        private const int amountOfFileLinesInFnmFile = 123;
        private const int totalAmountOfLinesInFnmFile = amountOfHeaderLinesInFnmFile + amountOfFileLinesInFnmFile;

        [Test]
        [TestCaseSource(nameof(ArgNullCases))]
        public void Write_ArgNull_ThrowsArgumentNullException(FnmFile fnmFile, TextWriter textWriter, string expParamName)
        {
            // Setup
            var writer = new FnmFileWriter();

            // Call
            void Call() => writer.Write(fnmFile, textWriter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo(expParamName));
        }

        [Test]
        public void Write_WritesToStream()
        {
            // Setup
            var writer = new FnmFileWriter();
            var fnmFile = new FnmFile();
            var textWriter = Substitute.For<TextWriter>();

            // Call
            writer.Write(fnmFile, textWriter);

            // Assert
            textWriter.Received(totalAmountOfLinesInFnmFile).WriteLine(Arg.Any<string>());
        }

        private static IEnumerable<TestCaseData> ArgNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<TextWriter>(), "fnmFile").SetName("fnmFile null");
            yield return new TestCaseData(new FnmFile(), null, "textWriter").SetName("textWriter null");
        }
    }
}