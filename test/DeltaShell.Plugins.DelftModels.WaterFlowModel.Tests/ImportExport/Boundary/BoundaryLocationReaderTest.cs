using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    public class BoundaryLocationReaderTest
    {
        /// <summary>
        /// WHEN a BoundaryLocationReader is constructed with no arguments
        /// THEN no exception is thrown
        /// </summary>
        [Test]
        public void WhenABoundaryLocationReaderIsConstructedWithNoArguments_ThenNoExceptionIsThrown()
        {
            Assert.That(new BoundaryLocationReader(), Is.Not.Null);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        /// WHEN a BoundaryLocationReader is constructed with this ErrorReportFunction
        /// THEN no exception is thrown
        /// </summary>
        [Test]
        public void GivenSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithThisErrorReportFunction_ThenNoExceptionIsThrown()
        {
            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };

            Assert.That(new BoundaryLocationReader(someErrorReportFunction), Is.Not.Null);
        }

        /// <summary>
        /// GIVEN some Reader
        ///   AND some Converter
        ///   AND some ErrorReportFunction
        /// WHEN a BoundaryLocationReader is constructed with this ErrorReportFunction
        /// THEN no exception is thrown
        /// </summary>
        [Test]
        public void GivenSomeReaderAndSomeConverterAndSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithThisErrorReportFunction_ThenNoExceptionIsThrown()
        {
            Func<string, IList<DelftIniCategory>> someParser = a => null;
            Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>> someConverter = (a, b) => null;
            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            Assert.That(new BoundaryLocationReader(someParser, someConverter, someErrorReportFunction), Is.Not.Null);
        }

        /// <summary>
        /// GIVEN some Converter function
        ///   AND a null reader
        ///   AND some ErrorReportFunction
        /// WHEN A BoundaryLocationReader is constructed with these arguments
        /// THEN An argument exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Parser cannot be null.")]
        public void GivenSomeConverterFunctionAndANullReaderAndSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithTheseArguments_ThenAnArgumentExceptionIsThrown()
        {
            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>> someConverter = (a, b) => null;

            var thisWillGenerateAnException = new BoundaryLocationReader(null, someConverter, someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN some Reader
        ///   AND a null converter function
        ///   AND some ErrorReportFunction
        /// WHEN A BoundaryLocationReader is constructed with these arguments
        /// THEN An argument exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Converter cannot be null.")]
        public void GivenSomeReaderAndANullConverterFunctionAndSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithTheseArguments_ThenAnArgumentExceptionIsThrown()
        {
            Func<string, IList<DelftIniCategory>> someParser = a => null;
            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };

            var thisWillGenerateAnException = new BoundaryLocationReader(someParser, null, someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN some Reader
        ///   AND some Converter function
        ///   AND some ErrorReportFunction
        /// WHEN A BoundaryLocationReader is constructed with these arguments
        ///  AND A file is read with this BoundaryLocationReader
        /// THEN The correct BoundaryLocations are returned
        ///  AND No exceptions are thrown
        /// </summary>
        [Test]
        public void GivenSomeReaderAndSomeConverterFunctionAndSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithTheseArgumentsAndAFileIsReadWithThisBoundaryLocationReader_ThenTheCorrectBoundaryLocationsAreReturnedAndNoExceptionsAreThrown()
        {
            // Given
            const string filePath = "somePath.File";
            var errorHandlingHasBeenCalled = false;

            var mocks = new MockRepository();

            var someIniCategories = new List<DelftIniCategory>
            {
                new DelftIniCategory("aCategory1"),
                new DelftIniCategory("aCategory2"),
                new DelftIniCategory("aCategory3")
            };

            var someParser = mocks.DynamicMock<Func<string, IList<DelftIniCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(someIniCategories)
                .Repeat.AtLeastOnce();

            var someOutputValues = new List<BoundaryLocation>();
            var outputVal1 = new BoundaryLocation("aName1", BoundaryType.Level, 0.0);
            var outputVal2 = new BoundaryLocation("aName2", BoundaryType.Level, 0.0);
            someOutputValues.Add(outputVal1);
            someOutputValues.Add(outputVal2);

            var someConverter = mocks.DynamicMock<Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>>>();
            someConverter.Expect(e => e.Invoke(null, null))
                .Return(someOutputValues)
                .IgnoreArguments()
                .Repeat.AtLeastOnce();

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { errorHandlingHasBeenCalled = true; };

            mocks.ReplayAll();
            var reader = new BoundaryLocationReader(someParser, someConverter, someErrorReportFunction);

            // When
            var outputSet = reader.Read(filePath);

            mocks.VerifyAll();
            // Then
            Assert.That(errorHandlingHasBeenCalled, Is.False);

            Assert.That(outputSet.Count, Is.EqualTo(2));
            Assert.That(outputSet.Contains(outputVal1));
            Assert.That(outputSet.Contains(outputVal2));
        }

        /// <summary>
        /// GIVEN some Reader which fails on reading
        ///   AND some Converter function
        ///   AND some ErrorReportFunction
        /// WHEN A BoundaryLocationReader is constructed with these arguments
        ///  AND A file is read with this BoundaryLocationReader
        /// THEN a null reference will be returned
        ///  AND an error will be logged
        /// </summary>
        [Test]
        public void GivenSomeReaderWhichFailsOnReadingAndSomeConverterFunctionAndSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithTheseArgumentsAndAFileIsReadWithThisBoundaryLocationReader_ThenANullReferenceWillBeReturnedAndAnErrorWillBeLogged()
        {
            // Given
            const string filePath = "somePath.File";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            var someIniCategories = new List<DelftIniCategory>
            {
                new DelftIniCategory("aCategory1"),
                new DelftIniCategory("aCategory2"),
                new DelftIniCategory("aCategory3")
            };

            const string errorMsg = "Some exception occurred during reading.";
            var someParser = mocks.DynamicMock<Func<string, IList<DelftIniCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(someIniCategories)
                .Throw(new Exception(errorMsg))
                .Repeat.AtLeastOnce();

            var someOutputValues = new List<BoundaryLocation>();
            var outputVal1 = new BoundaryLocation("aName1", BoundaryType.Level, 0.0);
            var outputVal2 = new BoundaryLocation("aName2", BoundaryType.Level, 0.0);
            someOutputValues.Add(outputVal1);
            someOutputValues.Add(outputVal2);

            var someConverter = mocks.DynamicMock<Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>>>();
            someConverter.Expect(e => e.Invoke(null, null))
                .Return(someOutputValues)
                .IgnoreArguments()
                .Repeat.Any();

            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            mocks.ReplayAll();
            var reader = new BoundaryLocationReader(someParser, someConverter, someErrorReportFunction);

            // When
            var outputSet = reader.Read(filePath);

            mocks.VerifyAll();
            // Then
            Assert.That(outputSet, Is.Null);

            Assert.That(errorHandlingHasBeenCalled, Is.True);
            Assert.That(errorHeader, Is.EqualTo("While reading the boundary locations from file, the following errors occured:"));
            Assert.That(loggedErrors.Count, Is.EqualTo(1));
            Assert.That(loggedErrors.Contains(errorMsg));
        }

        /// <summary>
        /// GIVEN some Reader
        ///   AND some Converter function which will report an error and give back an empty set
        ///   AND some ErrorReportFunction
        /// WHEN A BoundaryLocationReader is constructed with these arguments
        ///  AND A file is read with this BoundaryLocationReader
        /// THEN an empty set will be returned
        ///  AND an error will be logged
        /// </summary>
        [Test]
        public void GivenSomeReaderAndSomeConverterFunctionWhichWillReportAnErrorAndGiveBackAnEmptySetAndSomeErrorReportFunction_WhenABoundaryLocationReaderIsConstructedWithTheseArgumentsAndAFileIsReadWithThisBoundaryLocationReader_ThenAnEmptySetWillBeReturnedAndAnErrorWillBeLogged()
        {
            // Given
            const string filePath = "somePath.File";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            var someIniCategories = new List<DelftIniCategory>
            {
                new DelftIniCategory("aCategory1"),
                new DelftIniCategory("aCategory2"),
                new DelftIniCategory("aCategory3")
            };

            var someParser = mocks.DynamicMock<Func<string, IList<DelftIniCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(someIniCategories)
                .Repeat.AtLeastOnce();

            var someOutputValues = new List<BoundaryLocation>();
            var outputVal1 = new BoundaryLocation("aName1", BoundaryType.Level, 0.0);
            var outputVal2 = new BoundaryLocation("aName2", BoundaryType.Level, 0.0);
            someOutputValues.Add(outputVal1);
            someOutputValues.Add(outputVal2);

            const string errorMsg = "Some error during converting.";
            var someConverter = mocks.DynamicMock<Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>>>();
            someConverter.Expect(e => e.Invoke(null, null))
                .Return(someOutputValues)
                .IgnoreArguments()
                .WhenCalled(invocation => { (invocation.Arguments[1] as IList<string>)?.Add(errorMsg); })
                .Repeat.AtLeastOnce();

            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            mocks.ReplayAll();
            var reader = new BoundaryLocationReader(someParser, someConverter, someErrorReportFunction);

            // When
            var outputSet = reader.Read(filePath);

            mocks.VerifyAll();
            // Then
            Assert.That(outputSet.Count, Is.EqualTo(2));
            Assert.That(outputSet.Contains(outputVal1));
            Assert.That(outputSet.Contains(outputVal2));

            Assert.That(errorHandlingHasBeenCalled, Is.True);
            Assert.That(errorHeader, Is.EqualTo("While reading the boundary locations from file, the following errors occured:"));
            Assert.That(loggedErrors.Count, Is.EqualTo(1));
            Assert.That(loggedErrors.Contains(errorMsg));
        }
    }
}
