using DelftTools.Controls;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.GraphicsProviders;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.GraphicsProvider
{
    [TestFixture]
    public class FMGuiGraphicsProviderTest
    {
        [TestCase(FlowFMApplicationPlugin.FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID, true)]
        [TestCase(FlowFMApplicationPlugin.FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID, true)]
        [TestCase("Unknown", false)]
        [TestCase(null, false)]

        public void GivenFmGuiGraphicsProvider_WhenCanProvideDrawingGroupFor_CheckExpectedResult(string fmProjectTemplateId, bool expectedResult)
        {
            //Arrange
            var fmGuiProvider = new FmGuiGraphicsProvider();
            var template = Substitute.For<ProjectTemplate>();
            template.Id = fmProjectTemplateId;

            // Act & Assert
            Assert.That(fmGuiProvider.CanProvideDrawingGroupFor(template), Is.EqualTo(expectedResult));
        }
        
        [TestCase(FlowFMApplicationPlugin.FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID, true)]
        [TestCase(FlowFMApplicationPlugin.FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID, true)]
        [TestCase("Unknown", false)]
        [TestCase(null, false)]

        public void GivenFmGuiGraphicsProvider_When_CheckExpectedResult(string fmProjectTemplateId, bool expectedResult)
        {
            //Arrange
            var fmGuiProvider = new FmGuiGraphicsProvider();
            var template = Substitute.For<ProjectTemplate>();
            template.Id = fmProjectTemplateId;

            // Act & Assert
            if (expectedResult)
            {
                Assert.That(fmGuiProvider.CreateDrawingGroupFor(template), Is.Not.Null);
            }
            else
            {
                Assert.That(fmGuiProvider.CreateDrawingGroupFor(template), Is.Null);
            }
        }

    }
}