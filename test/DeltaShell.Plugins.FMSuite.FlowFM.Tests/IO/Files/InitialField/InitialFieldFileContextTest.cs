using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    public class InitialFieldFileContextTest
    {
        [Test]
        public void Constructor_FieldsInitialized()
        {
            InitialFieldFileContext context = CreateContext();

            Assert.That(context.DataFileNames, Is.Empty);
        }

        [Test]
        public void StoreDataFileName_InitialFieldDataIsNull_ThrowsArgumentNullException()
        {
            InitialFieldFileContext context = CreateContext();

            Assert.That(() => context.StoreDataFileName(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RestoreDataFileName_InitialFieldDataIsNull_ThrowsArgumentNullException()
        {
            InitialFieldFileContext context = CreateContext();

            Assert.That(() => context.RestoreDataFileName(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null, "Roughness")]
        [TestCase("", "Roughness")]
        [TestCase("MyOperation", null)]
        [TestCase("MyOperation", "")]
        public void StoreDataFileName_WithoutSpatialOperationNameOrQuantity_ThrowsInvalidOperationException(string name, string quantity)
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData fieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();

            fieldData.SpatialOperationName = name;
            fieldData.SpatialOperationQuantity = quantity;

            Assert.That(() => context.StoreDataFileName(fieldData), Throws.InvalidOperationException);
        }

        [Test]
        public void StoreDataFileName_WithSpatialOperationNameAndQuantity_DataFileStored()
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData fieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();

            fieldData.SpatialOperationName = "SomeOperation";
            fieldData.SpatialOperationQuantity = "Roughness";

            context.StoreDataFileName(fieldData);

            Assert.That(context.DataFileNames, Has.Exactly(1).EqualTo(fieldData.DataFile));
        }

        [Test]
        [TestCase(null, "Roughness")]
        [TestCase("", "Roughness")]
        [TestCase("MyOperation", null)]
        [TestCase("MyOperation", "")]
        public void RestoreDataFileName_WithoutSpatialOperationNameOrQuantity_ThrowsInvalidOperationException(string name, string quantity)
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData fieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();

            fieldData.SpatialOperationName = name;
            fieldData.SpatialOperationQuantity = quantity;

            Assert.That(() => context.RestoreDataFileName(fieldData), Throws.InvalidOperationException);
        }

        [Test]
        public void RestoreDataFileName_WithMatchingSpatialOperationNameAndQuantity_DataFileRestored()
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData originalFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            InitialFieldData actualFieldData = InitialFieldDataBuilder.Start().Build();

            string expectedDataFile = originalFieldData.DataFile;

            originalFieldData.SpatialOperationName = "SomeOperation";
            originalFieldData.SpatialOperationQuantity = "Roughness";

            actualFieldData.SpatialOperationName = "SomeOperation";
            actualFieldData.SpatialOperationQuantity = "Roughness";

            context.StoreDataFileName(originalFieldData);
            context.RestoreDataFileName(actualFieldData);

            Assert.That(actualFieldData.DataFile, Is.EqualTo(expectedDataFile));
        }

        [Test]
        public void ClearDataFileNames_WithStoredDataFiles_DataFilesCleared()
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData fieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();

            fieldData.SpatialOperationName = "SomeOperation";
            fieldData.SpatialOperationQuantity = "Roughness";

            context.StoreDataFileName(fieldData);
            context.ClearDataFileNames();

            Assert.That(context.DataFileNames, Is.Empty);
        }

        private static InitialFieldFileContext CreateContext()
        {
            return new InitialFieldFileContext();
        }
    }
}