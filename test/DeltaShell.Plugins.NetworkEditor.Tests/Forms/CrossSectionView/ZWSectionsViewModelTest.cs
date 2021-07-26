using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class ZWSectionsViewModelTest
    {
        private ZWSectionsViewModel crossSectionZWSectionsViewModel;
        private CrossSectionDefinitionZW crossSectionDefinition;
        private IEventedList<CrossSectionSectionType> crossSectionSectionTypes;

        [SetUp]
        public void SetUp()
        {
            //hook up a crossection to a network via a branch.. because the network is used te get the section types
            crossSectionDefinition = new CrossSectionDefinitionZW();
            
            var main = new CrossSectionSectionType { Name = "Main" };
            var fp1 = new CrossSectionSectionType { Name = "FloodPlain1" };
            var fp2 = new CrossSectionSectionType { Name = "FloodPlain2" };
            crossSectionSectionTypes = new EventedList<CrossSectionSectionType>();
            crossSectionSectionTypes.Clear();//remove the 'default' one
            crossSectionSectionTypes.Add(main);
            crossSectionSectionTypes.Add(fp1);
            crossSectionSectionTypes.Add(fp2);

            var table = crossSectionDefinition.ZWDataTable;
            table.AddCrossSectionZWRow(10d, 30d, 0d);
            table.AddCrossSectionZWRow(0d, 10d, 0d);

            crossSectionDefinition.AddSection(main, 10);
            crossSectionDefinition.AddSection(fp1, 4);
            crossSectionDefinition.AddSection(fp2, 16);

            crossSectionZWSectionsViewModel = new ZWSectionsViewModel(crossSectionDefinition, crossSectionSectionTypes);
                
        }

        private void AlterTestModelToIncludeStorage()
        {
            var table = crossSectionDefinition.ZWDataTable;
            table.Clear();
            table.AddCrossSectionZWRow(10d, 30d, 20d);
            table.AddCrossSectionZWRow(0d, 10d, 10d);
            crossSectionZWSectionsViewModel.UpdateViewModelFromCrossSection(true);
        }

        [Test]
        public void ViewModelWidthAssumesASymetricalProfile()
        {
            Assert.AreEqual(10,crossSectionZWSectionsViewModel.MainWidth);
            Assert.AreEqual(4, crossSectionZWSectionsViewModel.FloodPlain1Width);
            Assert.AreEqual(16, crossSectionZWSectionsViewModel.FloodPlain2Width);
        }

        [Test]
        public void ViewModelUpdatesSectionsOnMain()
        {
            //decrease in main is added to fp2
            crossSectionZWSectionsViewModel.MainWidth = 2.2;

            Assert.AreEqual(0, crossSectionDefinition.Sections[0].MinY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[0].MaxY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[1].MinY);
            Assert.AreEqual(3.1, crossSectionDefinition.Sections[1].MaxY);
            Assert.AreEqual(3.1, crossSectionDefinition.Sections[2].MinY);
            Assert.AreEqual(15, crossSectionDefinition.Sections[2].MaxY);
        }

        [Test]
        public void ViewModelUpdatesSectionsOnFp1()
        {   //decrease in fp1 is added to fp2
            crossSectionZWSectionsViewModel.FloodPlain1Width = 3;

            Assert.AreEqual(0, crossSectionDefinition.Sections[0].MinY);
            Assert.AreEqual(5, crossSectionDefinition.Sections[0].MaxY);
            Assert.AreEqual(5, crossSectionDefinition.Sections[1].MinY);
            Assert.AreEqual(6.5, crossSectionDefinition.Sections[1].MaxY);
            Assert.AreEqual(6.5, crossSectionDefinition.Sections[2].MinY);
            Assert.AreEqual(15.0, crossSectionDefinition.Sections[2].MaxY);
        }

        [Test]
        public void ViewModelUpdatesSectionsOnMainTooBig()
        {
            crossSectionZWSectionsViewModel.MainWidth = 28;
            Assert.AreEqual(0, crossSectionDefinition.Sections[0].MinY);
            Assert.AreEqual(14, crossSectionDefinition.Sections[0].MaxY);
            Assert.AreEqual(14, crossSectionDefinition.Sections[1].MinY);
            Assert.AreEqual(15, crossSectionDefinition.Sections[1].MaxY);
        }

        [Test]
        public void ViewModelUpdateSectionsUsingStorage() // TODO: this test should be re-worked
        {
            AlterTestModelToIncludeStorage();
            
            //decrease in main add to fp2
            crossSectionZWSectionsViewModel.MainWidth = 2.2;
            Assert.AreEqual(0, crossSectionDefinition.Sections[0].MinY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[0].MaxY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[1].MinY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[1].MaxY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[2].MinY);
            Assert.AreEqual(5, crossSectionDefinition.Sections[2].MaxY);

            // increase in fp1 should remove from fp2
            crossSectionZWSectionsViewModel.FloodPlain1Width += 2d;
            Assert.AreEqual(0, crossSectionDefinition.Sections[0].MinY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[0].MaxY);
            Assert.AreEqual(1.1, crossSectionDefinition.Sections[1].MinY);
            Assert.AreEqual(2.1, crossSectionDefinition.Sections[1].MaxY);
            Assert.AreEqual(2.1, crossSectionDefinition.Sections[2].MinY);
            Assert.AreEqual(5, crossSectionDefinition.Sections[2].MaxY);

            // big increase in main should remove from fp1 and fp2
            crossSectionZWSectionsViewModel.MainWidth += 7d;
            Assert.AreEqual(0, crossSectionDefinition.Sections[0].MinY);
            Assert.AreEqual(4.6, crossSectionDefinition.Sections[0].MaxY);
            Assert.AreEqual(4.6, crossSectionDefinition.Sections[1].MinY);
            Assert.AreEqual(5.0, crossSectionDefinition.Sections[1].MaxY);
            Assert.AreEqual(5.0, crossSectionDefinition.Sections[2].MinY);
            Assert.AreEqual(5, crossSectionDefinition.Sections[2].MaxY);
        }

        [Test]
        public void EnabledIsBasedOnExistenceOfSectionsWithNames()
        {
            Assert.IsTrue(crossSectionZWSectionsViewModel.MainEnabled);

            //remove the fp1 section
            crossSectionSectionTypes.RemoveAt(1);
            
            crossSectionZWSectionsViewModel.UpdateViewModelFromCrossSection();
            //this should disabled main in the VM
            Assert.IsFalse(crossSectionZWSectionsViewModel.MainEnabled);
        }

        [Test]
        public void MissingSectionsAreShownWithZeroWidth()
        {
            //remove main
            var main = crossSectionDefinition.Sections.First(s => s.SectionType.Name == "Main");
            crossSectionDefinition.Sections.Remove(main);

            crossSectionZWSectionsViewModel.UpdateViewModelFromCrossSection();
            //this should not disable main but merely set it 0
            Assert.IsTrue(crossSectionZWSectionsViewModel.MainEnabled);
            Assert.AreEqual(0,crossSectionZWSectionsViewModel.MainWidth);
        }
        
        [Test]
        public void RemovingSectionTypeCanResultInDisablingOfTextBox()
        {
            Assert.IsTrue(crossSectionZWSectionsViewModel.MainEnabled);

            //remove fp1 section type from network
            crossSectionSectionTypes.RemoveAt(1);

            Assert.IsFalse(crossSectionZWSectionsViewModel.MainEnabled);
        }

        [Test]
        public void RemovingSectionTypeFiresPropertyChangedOfMainEnabled()
        {
            
            IList<string> propertyNames = new List<string>();
            ((INotifyPropertyChange) crossSectionZWSectionsViewModel).PropertyChanged += (s, e) =>
                                                                                             {
                                                                                                 Assert.AreEqual(s,
                                                                                                                 crossSectionZWSectionsViewModel);
                                                                                                 propertyNames.Add(e.PropertyName);
                                                                                             };

            //action! remove main section type from network
            var main = crossSectionSectionTypes.First(s => s.Name == "Main");
            crossSectionSectionTypes.Remove(main);

            Assert.IsTrue(propertyNames.Contains("MainEnabled"));
        }

        [Test]
        public void RenamingSectionTypeFiresPropertyChangedOfMainEnabled()
        {

            IList<string> propertyNames = new List<string>();
            ((INotifyPropertyChange)crossSectionZWSectionsViewModel).PropertyChanged += (s, e) =>
            {
                Assert.AreEqual(s,
                                crossSectionZWSectionsViewModel);
                propertyNames.Add(e.PropertyName);
            };

            //action! remove main section type from network
            var main = crossSectionSectionTypes.First(s => s.Name == "Main");
            main.Name = "notMain";
            
            Assert.IsTrue(propertyNames.Contains("MainEnabled"));
        }
    }
}

