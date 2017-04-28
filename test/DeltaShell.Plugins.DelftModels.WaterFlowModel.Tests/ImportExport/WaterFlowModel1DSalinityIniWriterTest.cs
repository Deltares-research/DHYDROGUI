using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DSalinityIniWriterTest
    {
        private readonly string destinationFile = TestHelper.GetTestFilePath(@"FileWriters/salinity/salinity.ini");

        [SetUp]
        public void Setup()
        {
            FileUtils.DeleteIfExists(destinationFile);
        }

        [Test]
        [ExpectedException]
        public void TestSalinityIniWriterThrowsExceptionWithConstantDispersionType()
        {
            var sourceFile = TestHelper.GetTestFilePath(@"FileWriters/salinity/ThatcherHarleman/salinity.ini");
            WaterFlowModel1DSalinityIniWriter.WriteFile(sourceFile, "", DispersionFormulationType.Constant);
        }

        [TestCase("ThatcherHarleman/salinity.ini", DispersionFormulationType.ThatcherHarleman, "ThatcherHarleman/salinity.ini")]
        [TestCase("ThatcherHarleman/salinity.ini", DispersionFormulationType.KuijperVanRijnPrismatic, "KuijperVanRijn/prismatisch/salinity.ini")]
        [TestCase("ThatcherHarleman/salinity.ini", DispersionFormulationType.KuijperVanRijnConvergent, "KuijperVanRijn/convergent/salinity.ini")]
        [TestCase("ThatcherHarleman/salinity.ini", DispersionFormulationType.Savenije, "Savenije/salinity.ini")]
        [TestCase("ThatcherHarleman/salinity.ini", DispersionFormulationType.Gisen, "Gisen/salinity.ini")]
        [TestCase("ThatcherHarleman/salinity.ini", DispersionFormulationType.Zhang, "Zhang/salinity.ini")]

        [TestCase("KuijperVanRijn/prismatisch/salinity.ini", DispersionFormulationType.ThatcherHarleman, "ThatcherHarleman/salinity.ini")]
        [TestCase("KuijperVanRijn/prismatisch/salinity.ini", DispersionFormulationType.KuijperVanRijnPrismatic, "KuijperVanRijn/prismatisch/salinity.ini")]
        [TestCase("KuijperVanRijn/prismatisch/salinity.ini", DispersionFormulationType.KuijperVanRijnConvergent, "KuijperVanRijn/convergent/salinity.ini")]
        [TestCase("KuijperVanRijn/prismatisch/salinity.ini", DispersionFormulationType.Savenije, "Savenije/salinity.ini")]
        [TestCase("KuijperVanRijn/prismatisch/salinity.ini", DispersionFormulationType.Gisen, "Gisen/salinity.ini")]
        [TestCase("KuijperVanRijn/prismatisch/salinity.ini", DispersionFormulationType.Zhang, "Zhang/salinity.ini")]

        [TestCase("KuijperVanRijn/convergent/salinity.ini", DispersionFormulationType.ThatcherHarleman, "ThatcherHarleman/salinity.ini")]
        [TestCase("KuijperVanRijn/convergent/salinity.ini", DispersionFormulationType.KuijperVanRijnPrismatic, "KuijperVanRijn/prismatisch/salinity.ini")]
        [TestCase("KuijperVanRijn/convergent/salinity.ini", DispersionFormulationType.KuijperVanRijnConvergent, "KuijperVanRijn/convergent/salinity.ini")]
        [TestCase("KuijperVanRijn/convergent/salinity.ini", DispersionFormulationType.Savenije, "Savenije/salinity.ini")]
        [TestCase("KuijperVanRijn/convergent/salinity.ini", DispersionFormulationType.Gisen, "Gisen/salinity.ini")]
        [TestCase("KuijperVanRijn/convergent/salinity.ini", DispersionFormulationType.Zhang, "Zhang/salinity.ini")]

        [TestCase("Savenije/salinity.ini", DispersionFormulationType.ThatcherHarleman, "ThatcherHarleman/salinity.ini")]
        [TestCase("Savenije/salinity.ini", DispersionFormulationType.KuijperVanRijnPrismatic, "KuijperVanRijn/prismatisch/salinity.ini")]
        [TestCase("Savenije/salinity.ini", DispersionFormulationType.KuijperVanRijnConvergent, "KuijperVanRijn/convergent/salinity.ini")]
        [TestCase("Savenije/salinity.ini", DispersionFormulationType.Savenije, "Savenije/salinity.ini")]
        [TestCase("Savenije/salinity.ini", DispersionFormulationType.Gisen, "Gisen/salinity.ini")]
        [TestCase("Savenije/salinity.ini", DispersionFormulationType.Zhang, "Zhang/salinity.ini")]

        [TestCase("Gisen/salinity.ini", DispersionFormulationType.ThatcherHarleman, "ThatcherHarleman/salinity.ini")]
        [TestCase("Gisen/salinity.ini", DispersionFormulationType.KuijperVanRijnPrismatic, "KuijperVanRijn/prismatisch/salinity.ini")]
        [TestCase("Gisen/salinity.ini", DispersionFormulationType.KuijperVanRijnConvergent, "KuijperVanRijn/convergent/salinity.ini")]
        [TestCase("Gisen/salinity.ini", DispersionFormulationType.Savenije, "Savenije/salinity.ini")]
        [TestCase("Gisen/salinity.ini", DispersionFormulationType.Gisen, "Gisen/salinity.ini")]
        [TestCase("Gisen/salinity.ini", DispersionFormulationType.Zhang, "Zhang/salinity.ini")]

        [TestCase("Zhang/salinity.ini", DispersionFormulationType.ThatcherHarleman, "ThatcherHarleman/salinity.ini")]
        [TestCase("Zhang/salinity.ini", DispersionFormulationType.KuijperVanRijnPrismatic, "KuijperVanRijn/prismatisch/salinity.ini")]
        [TestCase("Zhang/salinity.ini", DispersionFormulationType.KuijperVanRijnConvergent, "KuijperVanRijn/convergent/salinity.ini")]
        [TestCase("Zhang/salinity.ini", DispersionFormulationType.Savenije, "Savenije/salinity.ini")]
        [TestCase("Zhang/salinity.ini", DispersionFormulationType.Gisen, "Gisen/salinity.ini")]
        [TestCase("Zhang/salinity.ini", DispersionFormulationType.Zhang, "Zhang/salinity.ini")]

        public void TestSalinityIniFileIsSuccessfullyConverted(string sourceFile, DispersionFormulationType type, string expectedFile)
        {
            // write converted file from source
            var sourceFilePath = TestHelper.GetTestFilePath(Path.Combine(@"FileWriters/salinity", sourceFile));
            WaterFlowModel1DSalinityIniWriter.WriteFile(sourceFilePath, destinationFile, type);
            
            // retrieve generated properties from destination file
            var generatedCategories = new DelftIniReader().ReadDelftIniFile(destinationFile);
            var generatedProperties = generatedCategories.SelectMany(c => c.Properties).ToList();

            // retrieve expected properties from test file
            var expectedFilePath = TestHelper.GetTestFilePath(Path.Combine(@"FileWriters/salinity", expectedFile));
            var expectedCategories = new DelftIniReader().ReadDelftIniFile(expectedFilePath);
            var expectedProperties = expectedCategories.SelectMany(c => c.Properties).ToList();

            // compare all properties to expected (except those that shouldn't have changed in this instance)
            Assert.AreEqual(expectedProperties.Count, generatedProperties.Count);
            var unchangedProperties = GetUnchangedPropertiesByDispersionFormulationType(type);

            for (var i = 0; i < expectedProperties.Count; i++)
            {
                var expectedProperty = expectedProperties[i];
                if (unchangedProperties.Contains(expectedProperty.Name)) continue;
                var generatedProperty = generatedProperties[i];

                Assert.AreEqual(expectedProperty.Name, generatedProperty.Name);
                Assert.AreEqual(expectedProperty.Value, generatedProperty.Value);
            }

            if (!unchangedProperties.Any()) return;

            // compare all properties (that didnt not change in this instance) to source
            var sourceCategories = new DelftIniReader().ReadDelftIniFile(sourceFilePath);
            var sourceProperties = sourceCategories.SelectMany(c => c.Properties).ToList();

            foreach (var unchangedProperty in unchangedProperties)
            {
                var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == unchangedProperty);
                Assert.NotNull(sourceProperty);

                var generatedProperty = generatedProperties.FirstOrDefault(p => p.Name == unchangedProperty);
                Assert.NotNull(generatedProperty);

                Assert.AreEqual(sourceProperty.Value, generatedProperty.Value);
            }

        }

        private static IList<string> GetUnchangedPropertiesByDispersionFormulationType(DispersionFormulationType type)
        {
            // When converting to Savenije, Gisen, or Zhang, there are some propertie(s) that should not change
            var unchangedProperties = new List<string>();
            switch (type)
            {
                case DispersionFormulationType.Savenije:
                case DispersionFormulationType.Gisen:
                case DispersionFormulationType.Zhang:
                    unchangedProperties.Add("c10");
                    break;
            }

            if(type == DispersionFormulationType.Zhang) unchangedProperties.Add("c5");

            return unchangedProperties;
        }
    }
}
