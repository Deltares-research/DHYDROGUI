using System.Collections.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.Common.Tests
{
    [TestFixture]
    public class ApplicationHelperTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void FindParentProjectItem_WhenOwnerIsNull_ThenNullShouldBeReturned()
        {
            var rootFolder = MockRepository.GenerateStub<Folder>();
            Assert.IsNull(ApplicationHelper.FindParentProjectItemInsideProject(rootFolder, null));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void FindParentProjectItem_WhenRootFolderIsNull_ThenNullShouldBeReturned()
        {
            var owner = MockRepository.GenerateStub<Folder>();
            Assert.IsNull(ApplicationHelper.FindParentProjectItemInsideProject(null, owner));
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void FindParentProjectItem_WhenAProjectFolderIsSelected_ThenThisRootFolderShouldBeReturned()
        {
            var rootFolder = MockRepository.GenerateStub<Folder>();
            Folder owner = rootFolder;

            Assert.AreSame(rootFolder, ApplicationHelper.FindParentProjectItemInsideProject(rootFolder, owner));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void FindParentProjectItem_WhenACompositeModelIsSelected_ThenThisCompositeModelShouldBeReturned()
        {
            var rootFolder = MockRepository.GenerateStub<Folder>();

            var integratedModel = MockRepository.GenerateStub<ICompositeActivity>();
            ICompositeActivity owner = integratedModel;

            Assert.AreSame(integratedModel, ApplicationHelper.FindParentProjectItemInsideProject(rootFolder, owner));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void FindParentProjectItem_WhenAModelsFolderIsSelected_ThenTheCorrespondingCompositeModelShouldBeReturned()
        {
            var rootFolder = MockRepository.GenerateStub<Folder>();

            ICompositeActivity compositeActivity = MockRepository.GenerateMock<ICompositeActivity, IModel>();
            var listActivities = new EventedList<IActivity>();
            compositeActivity.Expect(ca => ca.Activities).Return(listActivities);

            var listModels = new List<IModel>
            {
                (IModel) compositeActivity
            };
            rootFolder.Models = listModels;
            rootFolder.Folders = new List<Folder>();

            var owner = new TreeFolder(compositeActivity, null, "models", FolderImageType.Input);

            Assert.AreSame(compositeActivity, ApplicationHelper.FindParentProjectItemInsideProject(rootFolder, owner));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void FindParentProjectItem_WhenAModelIsSelected_ThenTheCorrespondingCompositeModelShouldBeReturned()
        {
            var rootFolder = MockRepository.GenerateStub<Folder>();
            
            ICompositeActivity compositeActivity = MockRepository.GenerateMock<ICompositeActivity, IModel>();

            var activity = MockRepository.GenerateMock<IActivity, IModel>();
            var listActivities = new EventedList<IActivity>
            {
                activity
            };

            var listModels = new List<IModel>
            {
                (IModel) compositeActivity
            };

            compositeActivity.Expect(ca => ca.Activities).Return(listActivities);

            rootFolder.Models = listModels;
            rootFolder.Folders = new List<Folder>();

            IActivity owner = activity;

            Assert.AreSame(compositeActivity, ApplicationHelper.FindParentProjectItemInsideProject(rootFolder, owner));
        }
    }
}