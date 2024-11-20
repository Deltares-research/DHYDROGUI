using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SubFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new SubFileImporter();

            // Assert
            Assert.IsInstanceOf<IFileImporter>(importer);
            Assert.AreEqual("Substance Process Library", importer.Name);
            Assert.IsEmpty(importer.Description);
            Assert.IsNull(importer.Image);
            Assert.IsFalse(importer.CanImportOnRootLevel);
            Assert.AreEqual("Sub Files (*.sub)|*.sub", importer.FileFilter);
            Assert.IsFalse(importer.OpenViewAfterImport);
            Assert.IsNull(importer.TargetDataDirectory);

            CollectionAssert.AreEqual(new[]
            {
                typeof(SubstanceProcessLibrary)
            }, importer.SupportedItemTypes);
        }

        [Test]
        public void CanImportOn_Always_ReturnsTrue()
        {
            // Setup
            var importer = new SubFileImporter();

            // Call
            bool result = importer.CanImportOn(null);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ImporterShouldThrowOnSubstanceProcessLibraryIsNull()
        {
            string combine = TestHelper.GetTestFilePath("ValidWaqModels\\Eutrof_simple_sobek.sub");

            Assert.That(() => new SubFileImporter().Import(null, combine),
                Throws.InvalidOperationException.With.Message.EqualTo("Substance process library is not set"));
        }

        [Test]
        public void ImporterLogsErrorWhenPathIsNull()
        {
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => new SubFileImporter().Import(new SubstanceProcessLibrary(), null),
                Resources.SubFileImporter_Path_not_set);
        }

        [Test]
        public void ImporterLogsErrorOnNonExistingFile()
        {
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => new SubFileImporter().Import(new SubstanceProcessLibrary(), "test.sub"),
                Resources.SubFileImporter_File_not_found);
        }

        [Test]
        public void Import()
        {
            var library = new SubstanceProcessLibrary();

            Assert.IsNull(library.Name);
            Assert.AreEqual(0, library.Substances.Count);
            Assert.AreEqual(0, library.Parameters.Count);
            Assert.AreEqual(0, library.Processes.Count);
            Assert.AreEqual(0, library.OutputParameters.Count);

            // Perform import on empty substance process library
            string testFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\Eutrof_simple_sobek.sub");
            new SubFileImporter().Import(library, testFilePath);

            Assert.AreEqual("Eutrof_simple_sobek", library.Name);
            Assert.AreEqual(12, library.Substances.Count);
            Assert.AreEqual(58, library.Parameters.Count);
            Assert.AreEqual(37, library.Processes.Count);
            Assert.AreEqual(15, library.OutputParameters.Count);              // 11x imported output parameter, 4x default output parameter 
            Assert.AreEqual(testFilePath, library.ImportedSubstanceFilePath); //To check if the property ImportedSubstanceFilePath & the imported file path are equal.

            WaterQualitySubstance firstSubstanceVariable = library.Substances[0];
            WaterQualityParameter firstParameter = library.Parameters[0];
            WaterQualityProcess firstProcess = library.Processes[0];
            WaterQualityOutputParameter firstOutputParameter = library.OutputParameters[0];
            WaterQualityOutputParameter secondOutputParameter = library.OutputParameters[1];

            Assert.AreEqual("AAP", firstSubstanceVariable.Name);
            Assert.AreEqual(true, firstSubstanceVariable.Active);
            Assert.AreEqual("adsorbed ortho phosphate", firstSubstanceVariable.Description);
            Assert.AreEqual("gP/m3", firstSubstanceVariable.ConcentrationUnit);
            Assert.AreEqual("-", firstSubstanceVariable.WasteLoadUnit);

            Assert.AreEqual("SWAdsP", firstParameter.Name);
            Assert.AreEqual("switch PO4 adsorption <0=Kd|1=Langmuir|2=pHdep>", firstParameter.Description);
            Assert.AreEqual("-", firstParameter.Unit);
            Assert.AreEqual(0, firstParameter.DefaultValue);

            Assert.AreEqual("AdsPO4AAP", firstProcess.Name);
            Assert.AreEqual("Ad(De)Sorption ortho phosphorus to inorg. matter", firstProcess.Description);

            Assert.AreEqual("AlgN", firstOutputParameter.Name);
            Assert.AreEqual("total nitrogen in algae", firstOutputParameter.Description);
            Assert.AreEqual(true, firstOutputParameter.ShowInHis);
            Assert.AreEqual(true, firstOutputParameter.ShowInMap);

            // Perform import on non empty substance process library: some objects must be still present after the import
            new SubFileImporter().Import(library, Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "Eutrof_simple_custom1.sub"));

            Assert.AreEqual("Eutrof_simple_custom1", library.Name);
            Assert.AreEqual(3, library.Parameters.Count);
            Assert.AreEqual(3, library.Processes.Count);
            Assert.AreEqual(3, library.Substances.Count);
            Assert.AreEqual(7, library.OutputParameters.Count); // 3x imported output parameter, 4x default output parameter

            Assert.AreSame(firstSubstanceVariable, library.Substances[0]);
            Assert.AreSame(firstParameter, library.Parameters[0]);
            Assert.AreSame(firstProcess, library.Processes[0]);
            Assert.AreSame(firstOutputParameter, library.OutputParameters[0]);
            Assert.AreSame(secondOutputParameter, library.OutputParameters[1]);

            firstSubstanceVariable = library.Substances[0];
            firstParameter = library.Parameters[0];
            firstProcess = library.Processes[0];
            firstOutputParameter = library.OutputParameters[0];

            // Perform import on non empty substance process library again: altough only the units or descriptions are different, none of the previous items should persist
            new SubFileImporter().Import(library, Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "Eutrof_simple_custom2.sub"));

            Assert.AreEqual("Eutrof_simple_custom2", library.Name);
            Assert.AreEqual(1, library.Parameters.Count);
            Assert.AreEqual(1, library.Processes.Count);
            Assert.AreEqual(1, library.Substances.Count);
            Assert.AreEqual(5, library.OutputParameters.Count); // 1x imported output parameter, 4x default output parameter

            Assert.AreNotSame(firstSubstanceVariable, library.Substances[0]);
            Assert.AreNotSame(firstParameter, library.Parameters[0]);
            Assert.AreNotSame(firstProcess, library.Processes[0]);
            Assert.AreNotSame(firstOutputParameter, library.OutputParameters[0]);
        }

        [Test]
        public void ImportEntitiesWithExtendedCharactersInDescription()
        {
            var library = new SubstanceProcessLibrary();

            // Perform import on empty substance process library
            new SubFileImporter().Import(library, Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "sub_files", "SubFileImporter_test.sub"));

            Assert.AreEqual("SubFileImporter_test", library.Name);
            Assert.AreEqual(1, library.Parameters.Count);
            Assert.AreEqual(1, library.Substances.Count);
            Assert.AreEqual(1, library.Processes.Count);
            Assert.AreEqual(5, library.OutputParameters.Count); // 1x imported output parameter, 4x default output parameter

            WaterQualitySubstance firstSubstanceVariable = library.Substances[0];
            WaterQualityParameter firstParameter = library.Parameters[0];
            WaterQualityProcess firstProcess = library.Processes[0];
            WaterQualityOutputParameter firstOutputParameter = library.OutputParameters[0];

            Assert.AreEqual("Continuity", firstSubstanceVariable.Name);
            Assert.AreEqual(true, firstSubstanceVariable.Active);
            Assert.AreEqual("Continuity!", firstSubstanceVariable.Description);
            Assert.AreEqual("g/m3", firstSubstanceVariable.ConcentrationUnit);
            Assert.AreEqual("-", firstSubstanceVariable.WasteLoadUnit);

            Assert.AreEqual("NH4", firstParameter.Name);
            Assert.AreEqual("Ammonium (dummy!)", firstParameter.Description);
            Assert.AreEqual("gN/m3", firstParameter.Unit);
            Assert.AreEqual(0, firstParameter.DefaultValue);

            Assert.AreEqual("BLOOM_P", firstProcess.Name);
            Assert.AreEqual("BLOOM II algae module!", firstProcess.Description);

            Assert.AreEqual("Cl", firstOutputParameter.Name);
            Assert.AreEqual("Chloride!", firstOutputParameter.Description);
            Assert.AreEqual(true, firstOutputParameter.ShowInHis);
            Assert.AreEqual(true, firstOutputParameter.ShowInMap);
        }

        [Test]
        public void AllowPercentageSignForUnits()
        {
            var library = new SubstanceProcessLibrary();

            new SubFileImporter().Import(library, Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "SubstateWithPercentageSign.sub"));

            Assert.AreEqual(3, library.Substances.Count);
            Assert.AreEqual(8, library.Parameters.Count);
            Assert.AreEqual(2, library.Processes.Count);
            Assert.AreEqual(21, library.OutputParameters.Count); // 17x imported output parameter, 4x default output parameter 

            WaterQualitySubstance substanceVariableTest1 = library.Substances.First(s => s.Name == "Test1");
            WaterQualitySubstance substanceVariableTest2 = library.Substances.First(s => s.Name == "Test2");
            WaterQualityParameter parameterZon = library.Parameters.First(p => p.Name == "Zon");

            Assert.AreEqual("%", substanceVariableTest1.ConcentrationUnit);
            Assert.AreEqual("-", substanceVariableTest1.WasteLoadUnit);

            Assert.AreEqual("-", substanceVariableTest2.ConcentrationUnit);
            Assert.AreEqual("%", substanceVariableTest2.WasteLoadUnit);

            Assert.AreEqual("%", parameterZon.Unit);
        }

        [Test]
        public void CheckWhenImportingASubFileAndSetImportedSubstanceFilePathIsTrueTheCorrectLogMessageIsShown()
        {
            var library = new SubstanceProcessLibrary();
            var subFileImporter = new SubFileImporter();

            string testFilePath = TestHelper.GetTestFilePath(@"IO\SubstateWithPercentageSign.sub");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);

            string expectedMessage = string.Format(Resources.SubFileImporter_Import_Sub_file_successfully_imported_from___0_, testFilePath);

            Action action = () => { subFileImporter.ImportItem(testFilePath, library); };
            TestHelper.AssertAtLeastOneLogMessagesContains(action, expectedMessage);
        }

        [Test]
        public void CheckWhenImportingASubFileAndSetImportedSubstanceIsImportedFileFlagEqualsTrue()
        {
            var library = new SubstanceProcessLibrary();
            var subFileImporter = new SubFileImporter();

            string testFilePath = TestHelper.GetTestFilePath(@"IO\SubstateWithPercentageSign.sub");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);

            subFileImporter.ImportItem(testFilePath, library);

            Assert.IsTrue(subFileImporter.IsSubFileSuccessfullyImported);
        }

        [Test]
        public void Import_WhenSubstancesAndParametersAreDefinedOnSingleLine_ExpectedSubstancesAndParametersImported()
        {
            // Setup
            const string fileName = "AlternativeSubstanceAndParameterFormat";
            string testFilePath = TestHelper.GetTestFilePath(Path.Combine("ValidWaqModels", $"{fileName}.sub"));

            var library = new SubstanceProcessLibrary();
            var importer = new SubFileImporter();

            // Call
            importer.Import(library, testFilePath);

            // Assert
            IEventedList<WaterQualitySubstance> waterQualitySubstances = library.Substances;
            CollectionAssert.AreEqual(new[]
            {
                "EColi",
                "OXY"
            }, waterQualitySubstances.Select(s => s.Name));

            CollectionAssert.AreEqual(new[]
            {
                true,
                true
            }, waterQualitySubstances.Select(s => s.Active));

            CollectionAssert.AreEqual(new[]
            {
                "MPN/m3",
                "gO2/m3"
            }, waterQualitySubstances.Select(s => s.ConcentrationUnit));

            CollectionAssert.AreEqual(new[]
            {
                "-",
                "-"
            }, waterQualitySubstances.Select(s => s.WasteLoadUnit));

            IEventedList<WaterQualityParameter> parameters = library.Parameters;
            CollectionAssert.AreEqual(new[]
            {
                "SWresusalg",
                "SWAdsP"
            }, parameters.Select(p => p.Name));

            CollectionAssert.AreEqual(new[]
            {
                "Respunsion Diat <0=Diat|1=DetC>",
                "switch formulation <0=Kd|1=Langmuir|2=GEM>"
            }, parameters.Select(p => p.Description));

            CollectionAssert.AreEqual(new[]
            {
                "-",
                "-"
            }, parameters.Select(p => p.Unit));

            CollectionAssert.AreEqual(new[]
            {
                1,
                0
            }, parameters.Select(p => p.DefaultValue));
        }

        # region Setup / Teardown

        private CultureInfo originalCulture;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            originalCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalCulture;
        }

        # endregion
    }
}