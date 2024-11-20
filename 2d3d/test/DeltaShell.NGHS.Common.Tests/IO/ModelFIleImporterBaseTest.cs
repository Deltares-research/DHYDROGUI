using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.IO;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO
{
    [TestFixture]
    public class ModelFileImporterBaseTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new TestModelFileImporterBase();

            // Assert
            Assert.That(importer, Is.InstanceOf<IFileImporter>());
        }

        [Test]
        public void ImportItem_WithoutTargetArgument_CallsOnImportItemWithCorrectArguments()
        {
            // Setup
            const string path = "file path";
            var returnArgument = new object();
            var importer = new TestModelFileImporterBase
            {
                ImportedItem = returnArgument
            };

            // Call
            object importedItem = importer.ImportItem(path);

            // Assert
            Assert.That(importer.IsOnImportItemCalled, Is.True);
            Assert.That(importedItem, Is.SameAs(returnArgument));

            Tuple<string, object> inputArguments = importer.OnImportItemArguments;
            Assert.That(inputArguments.Item1, Is.EqualTo(path));
            Assert.That(inputArguments.Item2, Is.Null);
        }

        [Test]
        public void ImportItem_WithTargetArgument_CallsOnImportItemWithCorrectArguments()
        {
            // Setup
            const string path = "file path";
            var target = new object();
            var returnArgument = new object();
            var importer = new TestModelFileImporterBase
            {
                ImportedItem = returnArgument 
            };

            // Call
            object importedItem = importer.ImportItem(path, target);

            // Assert
            Assert.That(importer.IsOnImportItemCalled, Is.True);
            Assert.That(importedItem, Is.SameAs(returnArgument));

            Tuple<string, object> inputArguments = importer.OnImportItemArguments;
            Assert.That(inputArguments.Item1, Is.EqualTo(path));
            Assert.That(inputArguments.Item2, Is.SameAs(target));
        }

        [Test]
        public void ImportItem_ImporterReturnsNull_LogsExpectedFailureMessage()
        {
            // Setup
            var importer = new TestModelFileImporterBase();

            // Call
            void Call() => importer.ImportItem(null);

            // Assert
            TestHelper.AssertLogMessagesAreGenerated(Call, new[]
            {
                "Start importing model data.",
                "Importing model data failed."
            }, 2);
        }
        
        [Test]
        public void ImportItem_ImporterReturnsResult_LogsExpectedFailureMessage()
        {
            // Setup
            var importer = new TestModelFileImporterBase {ImportedItem = new object()};

            // Call
            void Call() => importer.ImportItem(null);

            // Assert
            TestHelper.AssertLogMessagesAreGenerated(Call, new[]
            {
                "Start importing model data.",
                "Importing model data successful."
            }, 2);
        }

        [Test]
        public void ImportItem_OnImportItemThrowsException_LogsErrorAndThrowsException()
        {
            // Setup
            var exception = new Exception("Something went wrong when calling OnImportItem");
            var importer = new TestModelFileImporterBase
            {
                ThrowOnImportItemException = exception
            };

            var isExceptionThrown = false;
            Exception thrownException = null;

            // Call
            Action call = () =>
            {
                try
                {
                    importer.ImportItem(null);
                }
                catch (Exception e)
                {
                    isExceptionThrown = true;
                    thrownException = e;
                }
            };

            // Assert
            TestHelper.AssertLogMessagesAreGenerated(call, new[]
            {
                "Start importing model data.",
                "Importing model data failed."
            }, 2);

            Assert.That(isExceptionThrown, Is.True);
            Assert.That(thrownException, Is.SameAs(exception));
        }

        private class TestModelFileImporterBase : ModelFileImporterBase
        {
            public override string Name { get; }
            public override string Category { get; }
            public override string Description { get; }
            public override Bitmap Image { get; }
            public override IEnumerable<Type> SupportedItemTypes { get; }
            public override bool CanImportOnRootLevel { get; }
            public override string FileFilter { get; }
            public override string TargetDataDirectory { get; set; }
            public override bool ShouldCancel { get; set; }
            public override ImportProgressChangedDelegate ProgressChanged { get; set; }
            public override bool OpenViewAfterImport { get; }
            public object ImportedItem { private get; set; }
            public Tuple<string, object> OnImportItemArguments { get; private set; }
            public bool IsOnImportItemCalled { get; private set; }

            public Exception ThrowOnImportItemException { private get; set; }

            public override bool CanImportOn(object targetObject)
            {
                throw new NotImplementedException();
            }

            protected override object OnImportItem(string path, object target = null)
            {
                if (ThrowOnImportItemException != null)
                {
                    throw ThrowOnImportItemException;
                }

                OnImportItemArguments = new Tuple<string, object>(path, target);
                IsOnImportItemCalled = true;
                return ImportedItem;
            }
        }
    }
}