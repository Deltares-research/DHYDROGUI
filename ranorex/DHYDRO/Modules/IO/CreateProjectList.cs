using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DHYDRO.Code;
using Ranorex;
using Ranorex.Core.Testing;
using WinForms = System.Windows.Forms;

namespace DHYDRO.DataSources.Open_Many_Projects
{
	[TestModule("ABDF2CFC-2868-4128-B830-5529A7B6D67B", ModuleType.UserCode, 1)]
    public class CreateProjectList : ITestModule
    {
	    public CreateProjectList()
        {
            // Do not delete - a parameterless constructor is required!
        }
        
    	string _InputDirectory = "";
        [TestVariable("8702f27e-ffa5-4a8b-bc1f-63bb8e4aad9e")]
        public string InputDirectory
        {
        	get { return _InputDirectory; }
        	set { _InputDirectory = value; }
        }
        
        string _ProjectListPath = "";
        [TestVariable("c96efdbb-5b66-428a-a778-fbc4d6e62a52")]
        public string ProjectListPath
        {
        	get { return _ProjectListPath; }
        	set { _ProjectListPath = value; }
        }
        
        
        string _ProjectExclusionList = "";
        [TestVariable("49463ec7-8c20-4cf2-9996-5106e937e279")]
        public string ProjectExclusionList
        {
        	get { return _ProjectExclusionList; }
        	set { _ProjectExclusionList = value; }
        }
        
        void ITestModule.Run()
        {
        	UpdatePathVariables();
        	CreateProjectListDirectory();
        	
        	var projectFiles = GetProjectFiles();

			if (!projectFiles.Any())
			{
				throw new RanorexException($"No project files found in directory '{InputDirectory}'.");
			}
			
			Report.Log(ReportLevel.Info, $"Project files found:{Environment.NewLine}{string.Join(Environment.NewLine, projectFiles)}");
			
			File.WriteAllLines(ProjectListPath, projectFiles);
        }
        
	    private void UpdatePathVariables()
	    {
		    InputDirectory = FileUtils.GetAbsolutePath(InputDirectory);
		    ProjectListPath = FileUtils.GetAbsolutePath(ProjectListPath);
	        
	        Report.Log(ReportLevel.Info, $"InputDirectory: {InputDirectory}");
	        Report.Log(ReportLevel.Info, $"ProjectListPath: {ProjectListPath}");
	    }
	    
        private void CreateProjectListDirectory()
        {
        	var projectListDir = Path.GetDirectoryName(ProjectListPath);
	        
	        Report.Log(ReportLevel.Info, $"Creating project list dir: {projectListDir}");
        	
			if (!Directory.Exists(projectListDir))
			{
				Directory.CreateDirectory(projectListDir);
			}
        }
        
        private IReadOnlyList<string> GetProjectFiles()
        {
			var exclusionList = ProjectExclusionList.Split(';');

			Report.Log(ReportLevel.Info, $"Retrieving project files from {InputDirectory}");
			
			var projects = Directory.GetFiles(InputDirectory, "*.dsproj", SearchOption.AllDirectories).ToArray();
			var projectsFiltered = projects.Where(p => !exclusionList.Any(x => ContainsIgnoreCase(p, x))).ToArray();
			
			return projectsFiltered;
        }
        
        private bool ContainsIgnoreCase(string source, string substring)
        {
	        return source.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
