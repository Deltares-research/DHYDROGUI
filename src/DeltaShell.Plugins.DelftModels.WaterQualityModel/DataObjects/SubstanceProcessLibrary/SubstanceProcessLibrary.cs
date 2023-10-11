using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary
{
    /// <summary>
    /// Substance process library
    /// </summary>
    [Entity]
    public class SubstanceProcessLibrary : Unique<long>, INameable, ICloneable
    {
        public static readonly string DefaultSobekProcessDefinitionFilesPath =
            Path.Combine(DelwaqFileStructureHelper.GetDelwaqDataDefaultFolderPath(), "proc_def");

        public static readonly string DefaultDuflowProcessDefinitionFilesPath =
            Path.Combine(DelwaqFileStructureHelper.GetDelwaqDataDefaultFolderPath(), "proc_def_duflow");

        public static readonly string DefaultDuflowProcessDllFilePath =
            Path.Combine(DelwaqFileStructureHelper.GetDelwaqKernelPluginFolderPath(), "x64", "duflow.dll");

        /// <summary>
        /// Creates a substance process library without substances
        /// </summary>
        public SubstanceProcessLibrary() : this(null) {}

        /// <summary>
        /// Creates a substance process library with
        /// <param name="substances"/>
        /// </summary>
        /// <param name="substances"> The substances of the substance process library </param>
        public SubstanceProcessLibrary(IEventedList<WaterQualitySubstance> substances)
        {
            Substances = substances ?? new EventedList<WaterQualitySubstance>();
            Parameters = new EventedList<WaterQualityParameter>();
            Processes = new EventedList<WaterQualityProcess>();
            OutputParameters = new EventedList<WaterQualityOutputParameter>();

            ProcessDefinitionFilesPath = DefaultSobekProcessDefinitionFilesPath;
        }

        /// <summary>
        /// The substances of the substance process library
        /// </summary>
        public IEventedList<WaterQualitySubstance> Substances { get; private set; }

        /// <summary>
        /// The active substances of the substance process library
        /// </summary>
        public IEnumerable<WaterQualitySubstance> ActiveSubstances
        {
            get
            {
                return Substances.Where(s => s.Active);
            }
        }

        /// <summary>
        /// The inactive substances of the substance process library
        /// </summary>
        public IEnumerable<WaterQualitySubstance> InActiveSubstances
        {
            get
            {
                return Substances.Where(s => !s.Active);
            }
        }

        /// <summary>
        /// The parameters of the substance process library
        /// </summary>
        public IEventedList<WaterQualityParameter> Parameters { get; private set; }

        /// <summary>
        /// The processes of the substance process library
        /// </summary>
        public IEventedList<WaterQualityProcess> Processes { get; private set; }

        /// <summary>
        /// The output parameters of the substance process library
        /// </summary>
        public IEventedList<WaterQualityOutputParameter> OutputParameters { get; private set; }

        /// <summary>
        /// The path to the process dll that is used (according to the selected process type)
        /// </summary>
        public string ProcessDllFilePath { get; set; }

        /// <summary>
        /// The path to the process definition files that are used (according to the selected process type); without *.dat/*.def
        /// extension
        /// </summary>
        public string ProcessDefinitionFilesPath { get; set; }

        public string ImportedSubstanceFilePath { get; set; }

        /// <summary>
        /// The name of the substance process library
        /// </summary>
        public string Name { get; set; }

        public void Clear()
        {
            Substances.Clear();
            Parameters.Clear();
            Processes.Clear();
            OutputParameters.Clear();

            ProcessDefinitionFilesPath = DefaultSobekProcessDefinitionFilesPath;
        }

        public override string ToString()
        {
            var libraryString = "";

            libraryString +=
                Substances.Aggregate("Substances\n", (current, substance) => current + substance.Name + "\n");
            libraryString += Processes.Aggregate("\nProcesses\n", (current, process) => current + process.Name + "\n");
            libraryString +=
                Parameters.Aggregate("\nParameters\n", (current, process) => current + process.Name + "\n");
            libraryString +=
                OutputParameters.Aggregate("\nOutput parameters\n",
                                           (current, process) => current + process.Name + "\n");

            return libraryString;
        }

        public object Clone()
        {
            var clone = new SubstanceProcessLibrary {Name = Name};

            clone.Substances.AddRange(Substances.Select(s => (WaterQualitySubstance) s.Clone()));
            clone.Parameters.AddRange(Parameters.Select(p => (WaterQualityParameter) p.Clone()));
            clone.Processes.AddRange(Processes.Select(p => (WaterQualityProcess) p.Clone()));
            clone.OutputParameters.AddRange(OutputParameters.Select(op => (WaterQualityOutputParameter) op.Clone()));

            return clone;
        }
    }
}