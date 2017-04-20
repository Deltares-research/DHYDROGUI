using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class ZWSectionsViewTest
    {
        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void ShowWithInsufficientSections()
        {
            var crossSection = new CrossSectionDefinitionZW {};
            var crossSectionSectionTypes = new EventedList<CrossSectionSectionType>();
            var crossSectionZWSectionsViewModel = new ZWSectionsViewModel(crossSection, crossSectionSectionTypes);
            var view = new ZWSectionsView
            {
                Data = crossSectionZWSectionsViewModel
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void ShowWithZWSections()
        {
            var main = new CrossSectionSectionType {Name = "Main"};
            var fp1 = new CrossSectionSectionType {Name = "FloodPlain1"};
            var fp2 = new CrossSectionSectionType {Name = "FloodPlain2"};

            var crossSectionSectionTypes = new EventedList<CrossSectionSectionType>();
            crossSectionSectionTypes.Clear();//remove the 'default' one
            crossSectionSectionTypes.Add(main);
            crossSectionSectionTypes.Add(fp1);
            crossSectionSectionTypes.Add(fp2);

            var crossSectionSections = new EventedList<CrossSectionSection>
                                           {
                                               new CrossSectionSection
                                                   {SectionType = main, MinY = 0, MaxY = 5},
                                               new CrossSectionSection
                                                   {SectionType = fp1, MinY = 5, MaxY = 7},
                                               new CrossSectionSection
                                                   {SectionType = fp2, MinY = 7, MaxY = 15}
                                           };

            var crossSection = new CrossSectionDefinitionZW();
            crossSection.Sections.Add(crossSectionSections[0]);
            crossSection.Sections.Add(crossSectionSections[1]);
            crossSection.Sections.Add(crossSectionSections[2]);

            var crossSectionZWSectionsViewModel = new ZWSectionsViewModel(crossSection, crossSectionSectionTypes);
            var view = new ZWSectionsView
                           {
                               Data = crossSectionZWSectionsViewModel
                           };
            WindowsFormsTestHelper.ShowModal(view);

        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void SetDataToNullWithVisibleView()
        {
            var crossSection = new CrossSectionDefinitionZW();

            var crossSectionZWSectionsViewModel = new ZWSectionsViewModel(crossSection, new EventedList<CrossSectionSectionType>());
            var view = new ZWSectionsView
            {
                Data = crossSectionZWSectionsViewModel
            };
            WindowsFormsTestHelper.ShowModal(view, (f) =>
                                                       {
                                                           /*Application.DoEvents();
                                                           Thread.Sleep(2100);
                                                           Application.DoEvents();*/
                                                           view.Data = null;
                                                       });
        }

    }
}
