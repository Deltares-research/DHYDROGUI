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
        private ProjectTemplate fmProjectTemplate;
        private FlowFMApplicationPlugin applicationPlugin;
        private Project project;
        private ModelSettings modelSettings;
        
        [SetUp]
        public void ConfigurePluginProjectTemplate()
        {
            // Setup
            applicationPlugin = new FlowFMApplicationPlugin();
            project = new Project();
            modelSettings = new ModelSettings();

            // Call
            IEnumerable<ProjectTemplate> projectTemplates = applicationPlugin.ProjectTemplates();

            // Assert
            fmProjectTemplate = projectTemplates.FirstOrDefault(
                template => template.Id.EqualsCaseInsensitive("FMModel"));
            
            Assert.That(fmProjectTemplate, Is.Not.Null);
            Assert.That(fmProjectTemplate.ExecuteTemplateOpenView, Is.Not.Null);
        }
        
        [Test]
        public void ProjectTemplates_ContainsTemplateThatOpensDefaultViewForWaterFlowFMModel()
        {
            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            Assert.That(returnValue, Is.TypeOf<WaterFlowFMModel>());
        }

        [Test]
        public void ModelSettings_UseModelNameForProject_DefaultIsTrue()
        {
            var settings = new ModelSettings();
            Assert.IsTrue(settings.UseModelNameForProject);
        }
        
        [Test]
        public void ProjectTemplate_IfUseModelNameForProjectIsTrue_ProjectNameIsSetToModelName()
        {
            project.Name = "";
            
            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            var model = returnValue as WaterFlowFMModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(modelSettings.ModelName,model.Name);
            Assert.AreEqual(model.Name, project.Name);
        }
        
        [Test]
        public void ProjectTemplate_IfUseModelNameForProjectIsFalse_ProjectNameIsNotChanged()
        {
            // Change the name of this project to show that the name is not changed rather than being set to default
            string projectName = "MyProject";
            project.Name = projectName;

            modelSettings.UseModelNameForProject = false;
            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            var model = returnValue as WaterFlowFMModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(modelSettings.ModelName,model.Name);
            Assert.AreEqual(projectName, project.Name);
        }
    }
}