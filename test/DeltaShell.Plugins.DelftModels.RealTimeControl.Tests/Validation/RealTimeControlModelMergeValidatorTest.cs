using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
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

            var validationReport = RealTimeControlModelMergeValidator.ValidateControlGroups(destModel, srcModel);

            Assert.That(validationReport.WarningCount, Is.EqualTo(1));
		} 

        [Test]
		public void Given2RTCModelsWithControlgroupsWithDifferentNamesWhenValidateControlGroupsThenValidationReportIsEmpty()
		{
			var destModel = new RealTimeControlModel();
			var srcModel = new RealTimeControlModel();

            destModel.ControlGroups.Add(new ControlGroup());
            srcModel.ControlGroups.Add(new ControlGroup(){Name = "sourceName"});

            var validationReport = RealTimeControlModelMergeValidator.ValidateControlGroups(destModel, srcModel);

            Assert.That(validationReport.IsEmpty, Is.True);
		} 
    }

    [TestFixture]
    public class RealTimeControlModelMergeTest
    {
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

            ((IModelMerge)subModel1).Expect(m => m.CanMerge(subModel2)).Return(subModelsAreCompatible);
            
            mocks.ReplayAll();
            
            var destModel = new RealTimeControlModel();
            var srcModel = new RealTimeControlModel();

            SetPrivatePropertyValue(destModel, "InternalControlledModelsList", new EventedList<IModel> {subModel1});
            SetPrivatePropertyValue(srcModel, "InternalControlledModelsList", new EventedList<IModel> { subModel2 });

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