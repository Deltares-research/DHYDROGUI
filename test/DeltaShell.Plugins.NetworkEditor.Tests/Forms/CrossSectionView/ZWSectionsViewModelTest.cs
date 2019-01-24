using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class ZWSectionsViewModelTest
    {
        [Test]
        public void ViewModelWidthAssumesASymmetricalProfile()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));
            Assert.AreEqual(10.0, crossSectionZwSectionsViewModel.MainWidth);
            Assert.AreEqual(4.0, crossSectionZwSectionsViewModel.FloodPlain1Width);
            Assert.AreEqual(16.0, crossSectionZwSectionsViewModel.FloodPlain2Width);
        }

        [Test]
        public void ViewModelUpdatesSectionsOnMain()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));
            
            //decrease in main is added to fp2
            crossSectionZwSectionsViewModel.MainWidth = 2;

            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0.0, sections[0].MinY);
            Assert.AreEqual(1.0, sections[0].MaxY);
            Assert.AreEqual(1.0, sections[1].MinY);
            Assert.AreEqual(3.0, sections[1].MaxY);
            Assert.AreEqual(3.0, sections[2].MinY);
            Assert.AreEqual(15.0, sections[2].MaxY);
        }

        [Test]
        public void ViewModelUpdatesSectionsOnFp1()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));

            //decrease in fp1 is added to fp2
            crossSectionZwSectionsViewModel.FloodPlain1Width = 3;

            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0.0, sections[0].MinY);
            Assert.AreEqual(5.0, sections[0].MaxY);
            Assert.AreEqual(5.0, sections[1].MinY);
            Assert.AreEqual(6.5, sections[1].MaxY);
            Assert.AreEqual(6.5, sections[2].MinY);
            Assert.AreEqual(15.0, sections[2].MaxY);
        }

        [Test]
        public void ViewModelUpdatesSectionsOnMainTooBig()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));

            // Increase of main width should put fp2 width to 0 and decrease fp1 width
            crossSectionZwSectionsViewModel.MainWidth = 28;

            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0.0, sections[0].MinY);
            Assert.AreEqual(14.0, sections[0].MaxY);
            Assert.AreEqual(14.0, sections[1].MinY);
            Assert.AreEqual(15.0, sections[1].MaxY);
            Assert.AreEqual(15.0, sections[2].MinY);
            Assert.AreEqual(15.0, sections[2].MaxY);
        }

        [Test]
        public void GivenCrossSectionDefinitionWithZeroWidthFloodPlain1AndFloodPlain2Section_WhenSettingMainSectionWidthToDifferentValue_ThenFloodPlain1AndMainAreEqualToTotalWidth()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 30.0), new Tuple<bool, double>(true, 0.0), new Tuple<bool, double>(true, 0.0));

            // Increase of main width should put fp2 width to 0 and decrease fp1 width
            crossSectionZwSectionsViewModel.MainWidth = 20.0;

            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0.0, sections[0].MinY);
            Assert.AreEqual(10.0, sections[0].MaxY);
            Assert.AreEqual(10.0, sections[1].MinY);
            Assert.AreEqual(15.0, sections[1].MaxY);
            Assert.AreEqual(15.0, sections[2].MinY);
            Assert.AreEqual(15.0, sections[2].MaxY);
        }

        [Test]
        public void GivenCrossSectionDefinitionWithZeroWidthFloodPlain1Section_WhenSettingMainSectionWidthToDifferentValue_ThenFloodPlain1AndMainAreEqualToTotalWidth()
        {
            // Given
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 0.0), new Tuple<bool, double>(true, 16.0));

            var table = crossSectionZwSectionsViewModel.CrossSectionDefinitionZw.ZWDataTable;
            table.Clear();
            table.AddCrossSectionZWRow(10d, 30d, 10d);
            table.AddCrossSectionZWRow(0d, 10d, 5d);
            
            // When
            crossSectionZwSectionsViewModel.MainWidth = 2.0;

            // Then
            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0, sections[0].MinY);
            Assert.AreEqual(1.0, sections[0].MaxY);
            Assert.AreEqual(1.0, sections[1].MinY);
            Assert.AreEqual(10.0, sections[1].MaxY);
            Assert.AreEqual(10.0, sections[2].MinY);
            Assert.AreEqual(10.0, sections[2].MaxY);
        }

        [Test]
        public void GivenCrossSectionDefinitionWithZeroWidthFloodPlain1Section_WhenSettingFloodPlain1SectionWidthSectionToDifferentValue_ThenFloodPlain1AndMainAreEqualToTotalWidth()
        {
            // Given
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));

            // When
            crossSectionZwSectionsViewModel.FloodPlain1Width = 50.0;

            // Then
            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0, sections[0].MinY);
            Assert.AreEqual(5.0, sections[0].MaxY);
            Assert.AreEqual(5.0, sections[1].MinY);
            Assert.AreEqual(15.0, sections[1].MaxY);
            Assert.AreEqual(15.0, sections[2].MinY);
            Assert.AreEqual(15.0, sections[2].MaxY);
        }

        [TestCase(10.0)]
        [TestCase(50.0)]
        public void GivenCrossSectionDefinitionWithMainSection_WhenSettingMainSectionToDifferentValueThanTotalWidth_ThenMainWidthIsCorrectedToTotalWidth(double setValue)
        {
            // Given
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 30.0), new Tuple<bool, double>(false, 4.0), new Tuple<bool, double>(false, 16.0));

            // When
            crossSectionZwSectionsViewModel.MainWidth = setValue;

            // Then
            Assert.AreEqual(0.0, crossSectionZwSectionsViewModel.CrossSectionDefinitionZw.Sections[0].MinY);
            Assert.AreEqual(15.0, crossSectionZwSectionsViewModel.CrossSectionDefinitionZw.Sections[0].MaxY);
        }

        [Test]
        public void GivenCrossSectionDefinitionWithMainAndFloodPlain1Sections_WhenSettingMainSectionToDifferentValueThanTotalWidth_ThenFloodPlain1WidthIsCorrectedToTotalWidth()
        {
            // Given
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 30.0), new Tuple<bool, double>(true, 0.0), new Tuple<bool, double>(false, 0.0));

            // When
            crossSectionZwSectionsViewModel.MainWidth = 20.0;

            // Then
            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0, sections[0].MinY);
            Assert.AreEqual(10.0, sections[0].MaxY);
            Assert.AreEqual(10.0, sections[1].MinY);
            Assert.AreEqual(15.0, sections[1].MaxY);
        }

        [Test]
        public void GivenCrossSectionDefinitionWithMainAndFloodPlain1Sections_WhenSettingMainSectionToLargerValueThanTotalWidth_ThenFloodPlain1WidthIsZeroAndMainWidthIsEqualToTotalWidth()
        {
            // Given
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 20.0), new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(false, 0.0));

            // When
            // Total width is 30.0, so set main width to higher value
            crossSectionZwSectionsViewModel.MainWidth = 40.0;

            // Then
            var sections = crossSectionZwSectionsViewModel.CrossSectionSections;
            Assert.AreEqual(0, sections[0].MinY);
            Assert.AreEqual(15.0, sections[0].MaxY);
            Assert.AreEqual(15.0, sections[1].MinY);
            Assert.AreEqual(15.0, sections[1].MaxY);
        }

        [Test]
        public void EnabledIsBasedOnExistenceOfSectionsWithNames()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));
            Assert.IsTrue(crossSectionZwSectionsViewModel.MainEnabled);

            //remove the fp1 section
            crossSectionZwSectionsViewModel.CrossSectionSectionTypes.RemoveAt(1);

            crossSectionZwSectionsViewModel.UpdateViewModelFromCrossSection();
            //this should disabled main in the VM
            Assert.IsFalse(crossSectionZwSectionsViewModel.MainEnabled);
        }

        [Test]
        public void MissingSectionsAreShownWithZeroWidth()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));

            //remove main
            var main = crossSectionZwSectionsViewModel.CrossSectionDefinitionZw.Sections.First(s => s.SectionType.Name == "Main");
            crossSectionZwSectionsViewModel.CrossSectionDefinitionZw.Sections.Remove(main);

            crossSectionZwSectionsViewModel.UpdateViewModelFromCrossSection();
            //this should not disable main but merely set it 0
            Assert.IsTrue(crossSectionZwSectionsViewModel.MainEnabled);
            Assert.AreEqual(0, crossSectionZwSectionsViewModel.MainWidth);
        }

        [Test]
        public void RemovingSectionTypeCanResultInDisablingOfTextBox()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));
            Assert.IsTrue(crossSectionZwSectionsViewModel.MainEnabled);

            //remove fp1 section type from network
            crossSectionZwSectionsViewModel.CrossSectionSectionTypes.RemoveAt(1);

            Assert.IsFalse(crossSectionZwSectionsViewModel.MainEnabled);
        }

        [Test]
        public void RemovingSectionTypeFiresPropertyChangedOfMainEnabled()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));
            IList<string> propertyNames = new List<string>();
            ((INotifyPropertyChange) crossSectionZwSectionsViewModel).PropertyChanged += (s, e) =>
                                                                                             {
                                                                                                 Assert.AreEqual(s,
                                                                                                                 crossSectionZwSectionsViewModel);
                                                                                                 propertyNames.Add(e.PropertyName);
                                                                                             };

            //action! remove main section type from network
            crossSectionZwSectionsViewModel.CrossSectionSectionTypes.RemoveAllWhere(s => s.Name == "Main");

            Assert.IsTrue(propertyNames.Contains("MainEnabled"));
        }

        [Test]
        public void RenamingSectionTypeFiresPropertyChangedOfMainEnabled()
        {
            var crossSectionZwSectionsViewModel = SetupCrossSectionDefinitionZw(new Tuple<bool, double>(true, 10.0), new Tuple<bool, double>(true, 4.0), new Tuple<bool, double>(true, 16.0));

            IList<string> propertyNames = new List<string>();
            ((INotifyPropertyChange)crossSectionZwSectionsViewModel).PropertyChanged += (s, e) =>
            {
                Assert.AreEqual(s, crossSectionZwSectionsViewModel);
                propertyNames.Add(e.PropertyName);
            };

            //action! remove main section type from network
            var main = crossSectionZwSectionsViewModel.CrossSectionSectionTypes.First(s => s.Name == "Main");
            main.Name = "notMain";
            
            Assert.IsTrue(propertyNames.Contains("MainEnabled"));
        }

        private static TestZwSectionsViewModel SetupCrossSectionDefinitionZw(Tuple<bool, double> mainExistsWidthPair, Tuple<bool, double> fp1ExistsWidthPair, Tuple<bool, double> fp2ExistsWidthPair)
        {
            var crossSectionDefinition = new CrossSectionDefinitionZW();

            // Define table with total flow width of 30
            var table = crossSectionDefinition.ZWDataTable;
            table.AddCrossSectionZWRow(10d, 30d, 0d);
            table.AddCrossSectionZWRow(0d, 10d, 0d);

            var csSectionTypes = new EventedList<CrossSectionSectionType>();
            
            if (mainExistsWidthPair.First)
            {
                var mainSectionType = new CrossSectionSectionType { Name = "Main" };
                csSectionTypes.Add(mainSectionType);
                crossSectionDefinition.AddSection(mainSectionType, mainExistsWidthPair.Second);
            }
            if (fp1ExistsWidthPair.First)
            {
                var fp1SectionType = new CrossSectionSectionType { Name = "FloodPlain1" };
                csSectionTypes.Add(fp1SectionType);
                crossSectionDefinition.AddSection(fp1SectionType, fp1ExistsWidthPair.Second);
            }
            if (fp2ExistsWidthPair.First)
            {
                var fp2SectionType = new CrossSectionSectionType { Name = "FloodPlain2" };
                csSectionTypes.Add(fp2SectionType);
                crossSectionDefinition.AddSection(fp2SectionType, fp2ExistsWidthPair.Second);
            }

            return new TestZwSectionsViewModel(crossSectionDefinition, csSectionTypes);
        }

        private class TestZwSectionsViewModel : ZWSectionsViewModel
        {
            public TestZwSectionsViewModel(ICrossSectionDefinition crossSectionDefinition, IEventedList<CrossSectionSectionType> crossSectionSectionTypes) : base(crossSectionDefinition, crossSectionSectionTypes)
            {
            }

            public CrossSectionDefinitionZW CrossSectionDefinitionZw => crossSectionDefinition as CrossSectionDefinitionZW;

            public IList<CrossSectionSection> CrossSectionSections => crossSectionDefinition.Sections;
            public IList<CrossSectionSectionType> CrossSectionSectionTypes => crossSectionSectionTypes;
        }
    }
}

