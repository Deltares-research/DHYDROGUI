using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils.IO.InitialField;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
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
        public void StoreDataFileName_WithoutSpatialOperationNameOrQuantity_DataFileNotStored(string name, string quantity)
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData fieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();

            fieldData.SpatialOperationName = name;
            fieldData.SpatialOperationQuantity = quantity;

            Assert.That(context.DataFileNames, Is.Empty);
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
        public void RestoreDataFileName_WithoutSpatialOperationNameOrQuantity_DataFileNotRestored(string name, string quantity)
        {
            InitialFieldFileContext context = CreateContext();
            InitialFieldData originalFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            InitialFieldData actualFieldData = InitialFieldDataBuilder.Start().Build();

            originalFieldData.SpatialOperationName = "SomeOperation";
            originalFieldData.SpatialOperationQuantity = "Roughness";

            actualFieldData.SpatialOperationName = name;
            actualFieldData.SpatialOperationQuantity = quantity;

            context.StoreDataFileName(originalFieldData);
            context.RestoreDataFileName(actualFieldData);

            Assert.That(actualFieldData.DataFile, Is.Not.EqualTo(originalFieldData.DataFile));
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