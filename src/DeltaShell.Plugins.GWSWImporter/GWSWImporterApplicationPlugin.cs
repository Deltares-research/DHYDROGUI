using System.Collections.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    [Extension(typeof(IPlugin))]
    public class GWSWImporterApplicationPlugin : ApplicationPlugin
    {
        internal const string GWSWImportTemplateId = "GWSWImportTemplate";

        public override string Name
        {
            get { return "GWSWImporterApplicationPlugin"; }
        }

        public override string DisplayName
        {
            get { return "GWSW Importer Application Plugin"; }
        }
        public override string Description { get; }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public override IEnumerable<ProjectTemplate> ProjectTemplates()
        {
            yield return new ProjectTemplate
            {
                Id = GWSWImportTemplateId,
                Name = "GWSW import",
                Category = ProductCategories.ImportTemplateCategory,
                Description = "Generate a model from GWSW files",
                ExecuteTemplate = (p, s) =>
                {
                    if (!(s is GwswFileImporter importer))
                    {
                        return;
                    }

                    var model = (IModel) importer.ImportItem(null, p);
                    p.RootFolder.Items.Add(model);
                }
            };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new GwswFileImporter(new DefinitionsProvider()){ActivityRunner = Application.ActivityRunner};
        }
    }
}