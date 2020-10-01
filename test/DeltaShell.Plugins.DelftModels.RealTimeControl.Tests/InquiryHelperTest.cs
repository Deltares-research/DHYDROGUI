using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using Netron.GraphLib;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    internal class InquiryHelperTest
    {
        [Test]
        public void ControlGroupEditor_InquiryHelperThrowsNullExceptionWithNullArguments()
        {
            // Setup
            var InquiryHelper = new InquiryHelper();

            // Call
            TestDelegate call = () => InquiryHelper.InquireContinuation(null);

            // Assert
            Assert.That(call, Throws.InstanceOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("query"));
        }

        [Test]
        public void ControlGroupEditor_InquiryHelperWithParameterIsInstanceOfIInquiryHelper()
        {
            // Setup
            var inquiryHelper = Substitute.For<IInquiryHelper>();
            var instance = new InquiryHelper();

            // Call
            inquiryHelper.InquireContinuation("test");

            // Assert
            Assert.That(instance, Is.InstanceOf<IInquiryHelper>());
        }

        [Test]
        public void Link_ShapeTagIsOutputAndInquireContinuationTrue_LinksDataItem()
        {
            // Arrange
            var groupEditor = new ControlGroupEditor();

            var controlGroup = new ControlGroup();
            groupEditor.Data = controlGroup;
            var model = Substitute.For<IRealTimeControlModel>();
            groupEditor.Model = model;

            var target = Substitute.For<IDataItem>();
            model.GetDataItemByValue(default(object)).ReturnsForAnyArgs(target);
            model.WhenForAnyArgs(x => x.BeginEdit(null)).Do(x => { return; });

            var shape = Substitute.For<Shape>();
            shape.Tag = new Output();

            var dataItem = Substitute.For<IDataItem>();
            dataItem.Role = DataItemRole.Input;

            var inquiryHelper = Substitute.For<IInquiryHelper>();
            inquiryHelper.InquireContinuation(Resources.RealTimeControlModelNodePresenter_OutputLocationWarningMessage).Returns(true);

            // Act
            groupEditor.Link(shape, dataItem, inquiryHelper);

            // Assert

            dataItem.ReceivedWithAnyArgs(1).LinkTo(target);
        }

        [Test]
        public void Link_ShapeTagIsOutputAndInquireContinuationTrue_DoesNotLinksDataItem()
        {
            // Arrange
            // Arrange
            var groupEditor = new ControlGroupEditor();

            var controlGroup = new ControlGroup();
            groupEditor.Data = controlGroup;
            var model = Substitute.For<IRealTimeControlModel>();
            groupEditor.Model = model;

            var target = Substitute.For<IDataItem>();

            var shape = Substitute.For<Shape>();
            shape.Tag = new Output();

            var dataItem = Substitute.For<IDataItem>();
            dataItem.Role = DataItemRole.Input;
            var inquiryHelper = Substitute.For<IInquiryHelper>();
            inquiryHelper.InquireContinuation(Resources.RealTimeControlModelNodePresenter_OutputLocationWarningMessage).Returns(false);

            // Act
            groupEditor.Link(shape, dataItem, inquiryHelper);

            // Assert

            dataItem.ReceivedWithAnyArgs(0).LinkTo(target);
        }
    }
}