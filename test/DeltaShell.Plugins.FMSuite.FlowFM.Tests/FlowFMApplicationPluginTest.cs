using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Utils.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMApplicationPluginTest
    {
        [Test]
        public void ProjectTemplates_ContainsTemplateThatOpensDefaultViewForWaterFlowFMModel()
        {
            // Setup
            var applicationPlugin = new FlowFMApplicationPlugin();
            var project = new Project();
            var settings = new ModelSettings();

            // Call
            IEnumerable<ProjectTemplate> projectTemplates = applicationPlugin.ProjectTemplates();

            // Assert
            ProjectTemplate fmProjectTemplate = projectTemplates.FirstOrDefault(
                template => template.Id.EqualsCaseInsensitive("FMModel"));
            
            Assert.That(fmProjectTemplate, Is.Not.Null);
            Assert.That(fmProjectTemplate.ExecuteTemplateOpenView, Is.Not.Null);

            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, settings);
            Assert.That(returnValue, Is.TypeOf<WaterFlowFMModel>());
        }
    }
}