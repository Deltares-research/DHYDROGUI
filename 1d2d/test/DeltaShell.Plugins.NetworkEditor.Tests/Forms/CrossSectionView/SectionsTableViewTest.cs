using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class SectionsTableViewTest
    {
        public EventedList<CrossSectionSectionType> TempSectionTypesList { get; set; }
        
        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var view = new SectionsTableView
                           {
                               Data =
                                   new SectionsBindingList(new List<CrossSectionSection>
                                                                            {
                                                                                new CrossSectionSection
                                                                                    {
                                                                                        MinY = 0,
                                                                                        MaxY = 50,
                                                                                        SectionType =
                                                                                            new CrossSectionSectionType()
                                                                                                {Name = "section 1"}
                                                                                    },
                                                                                    new CrossSectionSection
                                                                                    {
                                                                                        MinY = 50,
                                                                                        MaxY = 500,
                                                                                        SectionType =
                                                                                            new CrossSectionSectionType()
                                                                                                {Name = "section 2"}
                                                                                    }
                                                                            })
                           };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void CheckPropertyChangedAfterRenamingSectionType()
        {
            var sectionType = new CrossSectionSectionType {Name = "aap"};
            var sectionList = new EventedList<CrossSectionSectionType>(new List<CrossSectionSectionType> { sectionType }); 
            
            var view = new SectionsTableView
            {
                SectionTypeList = sectionList
            };
            
            int callCount = 0;
            ((INotifyPropertyChanged)(view.SectionTypeList)).PropertyChanged += (s, e) =>
            {
                callCount++;
            };

            sectionType.Name = "noot";
            Assert.AreEqual(1, callCount);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void ShowWithVariousSectionTypes()
        {
            TempSectionTypesList = FillSectionTypesList();
            
            var view = new SectionsTableView
                           {
                               SectionTypeList = TempSectionTypesList,
                               Data =
                                   new SectionsBindingList(new List<CrossSectionSection>
                                                                            {
                                                                                new CrossSectionSection
                                                                                    {
                                                                                        MinY = 0,
                                                                                        MaxY = 100,
                                                                                        SectionType =
                                                                                            new CrossSectionSectionType()
                                                                                                {Name = "section 2"}
                                                                                    }
                                                                            })};
            
            WindowsFormsTestHelper.ShowModal(view);
        }
        /*
        [Test]
        public void ViewMakesRoughnessSectionsValid()
        {
            var bindingList =
                new BindingList<CrossSectionSection>(new List<CrossSectionSection>
                                                                  {
                                                                      new CrossSectionSection
                                                                          {MinY = 0, MaxY = 100}
                                                                  });
            var view = new SectionsTableView
            {
                Data = bindingList
            };

            var roughnessSection = new CrossSectionSection();
            bindingList.Add(roughnessSection);

            roughnessSection.MinY = 80;
            roughnessSection.MaxY = 120;

            Assert.AreEqual(0, bindingList[0].MinY);
            Assert.AreEqual(80, bindingList[0].MaxY);
            Assert.AreEqual(80, bindingList[1].MinY);
            Assert.AreEqual(100, bindingList[1].MaxY);
        }
        */
        private EventedList<CrossSectionSectionType> FillSectionTypesList()
        {
            return new EventedList<CrossSectionSectionType>()
                                       {
                                           new CrossSectionSectionType(){Name = "section 1"},
                                           new CrossSectionSectionType(){Name = "section 2"},
                                           new CrossSectionSectionType(){Name = "section 3"},
                                           new CrossSectionSectionType(){Name = "section 4"}
                                       };
        }
    }
}
