using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Validation
{
    [TestFixture]
    public class RealTimeControlModelMergeValidatorTest
    {
        [Test]
        public void Given2RTCModelsWithControlgroupsWithTheSameNameWhenValidateControlGroupsThenValidationReportWithWarning()
        {
            var destModel = new RealTimeControlModel();
            var srcModel = new RealTimeControlModel();

            destModel.ControlGroups.Add(new ControlGroup());
            srcModel.ControlGroups.Add(new ControlGroup());

            ValidationReport validationReport = RealTimeControlModelMergeValidator.ValidateControlGroups(destModel, srcModel);

            Assert.That(validationReport.WarningCount, Is.EqualTo(1));
        }

        [Test]
        public void Given2RTCModelsWithControlgroupsWithDifferentNamesWhenValidateControlGroupsThenValidationReportIsEmpty()
        {
            var destModel = new RealTimeControlModel();
            var srcModel = new RealTimeControlModel();

            destModel.ControlGroups.Add(new ControlGroup());
            srcModel.ControlGroups.Add(new ControlGroup() {Name = "sourceName"});

            ValidationReport validationReport = RealTimeControlModelMergeValidator.ValidateControlGroups(destModel, srcModel);

            Assert.That(validationReport.IsEmpty, Is.True);
        }
    }

    [TestFixture]
    public class RealTimeControlModelMergeTest
    {
        [Test]
        public void TestMerge2ModelsWithTheSameControlGroupNames_ResultingControlGroupNamesAreUnique()
        {
            var destModel = new RealTimeControlModel();
            var srcModel = new RealTimeControlModel();

            destModel.ControlGroups.Add(new ControlGroup {Name = "ControlGroup1"});
            srcModel.ControlGroups.Add(new ControlGroup {Name = "ControlGroup1"});
            srcModel.ControlGroups.Add(new ControlGroup {Name = "ControlGroup1"});

            TestHelper.AssertAtLeastOneLogMessagesContains(() => destModel.Merge(srcModel, null),
                                                           string.Format("There already exists a ControlGroup named ControlGroup1 in Model {0}, ControlGroup ControlGroup1 will be renamed", destModel.Name));

            Assert.AreEqual(3, destModel.ControlGroups.Count);
            Assert.AreEqual(destModel.ControlGroups[0].Name, "ControlGroup1");
            Assert.AreNotEqual(destModel.ControlGroups[1].Name, "ControlGroup1");
            Assert.AreNotEqual(destModel.ControlGroups[2].Name, "ControlGroup1");

            Assert.IsTrue(destModel.ControlGroups.Select(cg => cg.Name).HasUniqueValues());
        }

        [Test]
        public void Given2RTCModelsWithControlGroupWhenMergeThenInDestinationModel2ControlGroups()
        {
            var destModel = new RealTimeControlModel();
            var srcModel = new RealTimeControlModel();

            destModel.ControlGroups.Add(new ControlGroup());
            srcModel.ControlGroups.Add(new ControlGroup());
            destModel.Merge(srcModel, null);
            Assert.That(destModel.ControlGroups.Count, Is.EqualTo(2));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CanMergeTestsForDependModel(bool subModelsAreCompatible)
        {
            var mocks = new MockRepository();

            var subModel1 = mocks.DynamicMultiMock<IModel>(typeof(IModelMerge));
            var subModel2 = mocks.DynamicMultiMock<IModel>(typeof(IModelMerge));

            ((IModelMerge) subModel1).Expect(m => m.CanMerge(subModel2)).Return(subModelsAreCompatible);

            mocks.ReplayAll();

            var destModel = new RealTimeControlModel();
            var srcModel = new RealTimeControlModel();

            SetPrivatePropertyValue(destModel, "InternalControlledModelsList", new EventedList<IModel> {subModel1});
            SetPrivatePropertyValue(srcModel, "InternalControlledModelsList", new EventedList<IModel> {subModel2});

            Assert.That(destModel.CanMerge(srcModel), Is.EqualTo(subModelsAreCompatible));

            mocks.VerifyAll();
        }

        private void SetPrivatePropertyValue(object instance, string propertyName, object value)
        {
            instance.GetType()
                    .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(instance, value, null);
        }
    }
}