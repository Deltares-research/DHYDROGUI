using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class CompositeBranchStructureConverterTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channels;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch("node1", "node2", "branch");
            channels = originalNetwork.Channels.ToList();
        }

        [Test]
        public void WhenACompositeBranchStructureConverterIsConstructedWithNoArguments_ThenNoExceptionIsThrown()
        {
            Assert.That(new CompositeBranchStructureConverter(), Is.Not.Null);
        }

        [Test]
        public void GivenSomeFactoryAndSomeCompositeBranchStructureConverter_WhenACompositeBranchStructureConverterIsConstructed_ThenNoExceptionIsThrown()
        {
            Func<string, StructureConverter> someFactory = a => null;
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> someCompositeBranchStructureConverter = (a, b,c) => null;
           
            Assert.That(new CompositeBranchStructureConverter(someFactory, someCompositeBranchStructureConverter), Is.Not.Null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "getCompositeBranchStructureFunc cannot be null.")]
        public void GivenSomeFactoryAndANullCompositeBranchStructureConverter_WhenACompositeBranchStructureConverterIsConstructed_ThenAnArgumentExceptionIsThrown()
        {
            Func<string, StructureConverter> someFactory = a => null;

            var converter = new CompositeBranchStructureConverter(someFactory, null);
            Assert.IsNotNull(converter);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "getTypeConverterFunc cannot be null.")]
        public void GivenANullFactoryAndSomeCompositeBranchStructureConverter_WhenACompositeBranchStructureConverterIsConstructed_ThenAnArgumentExceptionIsThrown()
        {
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> someCompositeBranchStructureConverter = (a, b, c) => null;

            var converter = new CompositeBranchStructureConverter(null,someCompositeBranchStructureConverter);
            Assert.IsNotNull(converter);
        }

        [Test]
        public void GivenTwoCategoriesOnTheSameCompositeStructure_WhenImporting_ThenACompositeStructureShouldBeCreatedWithTheTwoStructures()
        {
            //Given
            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();


            categories.Add(category);

            var category2 = CreatePerfectCategory();
            category2.SetProperty(StructureRegion.Id.Key, "Weir2");


            categories.Add(category2);

            //When
            var compositeBranchStructures = (new CompositeBranchStructureConverter()).Convert(categories, channels, errorMessages);

            //Then
            Assert.AreEqual(1, compositeBranchStructures.Count);
            Assert.AreEqual(2, compositeBranchStructures[0].Structures.Count);
            Assert.AreEqual("Weir1", compositeBranchStructures[0].Structures[0].Name);
            Assert.AreEqual("Weir2", compositeBranchStructures[0].Structures[1].Name);
        }
        
        [Test]
        public void GivenAnUnknownTypeForAStructure_WhenTheConverterFactoryIsCreatingAConverter_ThenAnErrorMessageShouldBeCreated()
        {
            //Given
            var mocks = new MockRepository();

            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();
            categories.Add(category);

            var someFactoryMock = mocks.DynamicMock<Func<string, StructureConverter>>();
            someFactoryMock.Expect(e => e.Invoke("weir"))
                .Return(null)
                .Repeat.AtLeastOnce();
            
            mocks.ReplayAll();
            
            // Used for the constructor, but will not be executed.
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> compositeBranchStructuresFunc = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded;

            //When
            var converter = new CompositeBranchStructureConverter(someFactoryMock, compositeBranchStructuresFunc );
            converter.Convert(categories, channels, errorMessages);

            //Then
            mocks.VerifyAll();

            Assert.AreEqual(1, errorMessages.Count);
            var expectedMessage = 
                string.Format(Resources.CompositeBranchStructureConverter_CreationOfStructuresAndCompositeBranchStructures_A__0__is_found_in_the_structure_file__line__1___and_this_type_is_not_supported_during_an_import_,
                "weir", 55);
            Assert.AreEqual(expectedMessage, errorMessages[0]);
        }

        [Test]
        public void GivenNullForStructure_WhenCreatingThisStructure_ThenAnErrorMessageShouldBeCreated()
        {
            //Given
            var mocks = new MockRepository();

            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();
            categories.Add(category);

            var convertMock = mocks.StrictMock<IStructureConverter>();
            convertMock.Expect(e => e.ConvertToStructure1D(category, channels.FirstOrDefault()))
                .Return(null)
                .Repeat.AtLeastOnce();

            var someFactoryMock = mocks.DynamicMock<Func<string, IStructureConverter>>();
            someFactoryMock.Expect(e => e.Invoke("weir"))
                .Return(convertMock)
                .Repeat.AtLeastOnce();

            mocks.ReplayAll();

            // Used for the constructor, but will not be executed.
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> compositeBranchStructuresFunc = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded;

            //When
            var converter = new CompositeBranchStructureConverter(someFactoryMock, compositeBranchStructuresFunc);
            converter.Convert(categories, channels, errorMessages);

            //Then

            mocks.VerifyAll();

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(
                "Failed to create a structure from the structures file (line 55)",
                errorMessages[0]);
        }

        [Test]
        public void GivenNullForCompositeBranchStructure_WhenCreatingThisCorrespondingCompositeBranchStructureForAStructure_ThenAnErrorMessageShouldBeCreated()
        {
            //Given
            var mocks = new MockRepository();

            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();
            
            var category = CreatePerfectCategory();
            categories.Add(category);
            
            var someStructure = mocks.DynamicMock<IStructure1D>();
            someStructure.Expect(s => s.Name).Return("Weir").Repeat.Any();
            
            var convertMock = mocks.DynamicMock<IStructureConverter>();
            convertMock.Expect(e => e.ConvertToStructure1D(category, channels.FirstOrDefault()))
                .Return(someStructure)
                .Repeat.AtLeastOnce();

            var someFactoryMock = mocks.DynamicMock<Func<string, IStructureConverter>>();
            someFactoryMock.Expect(e => e.Invoke("weir"))
                .Return(convertMock)
                .IgnoreArguments()
                .Repeat.AtLeastOnce();

            var someCompositeBranchStructureMock = mocks
                .DynamicMock<Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>,
                    ICompositeBranchStructure>>();
            someCompositeBranchStructureMock.Expect(e => e.Invoke(null, null, null))
                .IgnoreArguments()
                .Return(null)
                .Repeat.AtLeastOnce();

            mocks.ReplayAll();
            
            //When
            var converter = new CompositeBranchStructureConverter(someFactoryMock, someCompositeBranchStructureMock);
            converter.Convert(categories, channels, errorMessages);

            //Then

            mocks.VerifyAll();

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Failed to create structure Weir from the structures file (line 55)",
                errorMessages[0]);
        }

        private DelftIniCategory CreatePerfectCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Compound.Key, "1");
            category.AddProperty(StructureRegion.CompoundName.Key, "Bla");
            category.AddProperty(StructureRegion.DefinitionType.Key, "weir");
            
            category.AddProperty(StructureRegion.CrestLevel.Key, "1.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, "100");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.1");
            category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.2");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            category.LineNumber = 55;

            return category;
        }
    }
}