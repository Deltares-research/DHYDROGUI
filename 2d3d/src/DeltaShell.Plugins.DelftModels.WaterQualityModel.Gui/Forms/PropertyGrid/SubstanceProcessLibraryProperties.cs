using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Substance process library")]
    public class SubstanceProcessLibraryProperties : ObjectProperties<SubstanceProcessLibrary>
    {
        [Category("General")]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [Category("General")]
        [DisplayName("Process count")]
        [PropertyOrder(2)]
        public int ProcessCount => data.Processes.Count;

        [Category("General")]
        [DisplayName("Parameter count")]
        [PropertyOrder(3)]
        public int ParameterCount => data.Parameters.Count;

        [Category("General")]
        [DisplayName("Substance count")]
        [PropertyOrder(4)]
        public int SubstanceCount => data.Substances.Count;

        [Category("General")]
        [DisplayName("Output parameter count")]
        [PropertyOrder(5)]
        public int OutputParameterCount => data.OutputParameters.Count;

        [PropertyOrder(1)]
        [Category("Process files")]
        [DisplayName("Process definition files")]
        [Description(
            "File path of the process definition files that are used (without extension: the corresponding [file path].def and [file path].dat files will be automatically retrieved during the calculation)")]
        public string ProcessDefinitionFilesPath => data.ProcessDefinitionFilesPath;

        [PropertyOrder(2)]
        [Category("Process files")]
        [DisplayName("Process dll")]
        [Description("File path of the process dll that is used (with extension)")]
        public string ProcessDllFilePath => data.ProcessDllFilePath;

        [PropertyOrder(3)]
        [Category("Process files")]
        [DisplayName("Imported substance file location")]
        [Description("The file location of the imported substance file")]
        public string ImportedSubFilePath => data.ImportedSubstanceFilePath;
    }
}