using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Boundary;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Parser for lateral sources in the external forcing file.
    /// </summary>
    public class BndExtForceLateralSourceParser
    {
        private readonly ILogHandler logHandler;
        private readonly IBoundaryFileReader boundaryFileReader;
        private readonly string directory;
        private readonly bool useSalt;
        private readonly bool useTemperature;

        private readonly IDictionary<string, IDictionary<string, ILateralSourceBcSection>> bcDataByFile = new Dictionary<string, IDictionary<string, ILateralSourceBcSection>>();
        private readonly IDictionary<string, IPipe> pipesBySource = new Dictionary<string, IPipe>();
        private readonly IDictionary<string, IPipe> pipesByTarget = new Dictionary<string, IPipe>();
        private readonly IDictionary<string, IBranch> branchesBySource = new Dictionary<string, IBranch>();
        private readonly IDictionary<string, IBranch> branchesByTarget = new Dictionary<string, IBranch>();
        private readonly IDictionary<string, IBranch> branchesByName = new Dictionary<string, IBranch>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceLateralSourceParser"/> class.
        /// </summary>
        /// <param name="sourceFilePath"> The source file path of the data to parse, the *_bnd.ext file. </param>
        /// <param name="network"> The hydro network. </param>
        /// <param name="useSalt"> Whether or not the model uses salt. </param>
        /// <param name="useTemperature"> Whether or not the model uses temperature. </param>
        /// <param name="boundaryFileReader"> The boundary file reader. </param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="network"/> or <paramref name="boundaryFileReader"/> is <c>null</c>..
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="sourceFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public BndExtForceLateralSourceParser(string sourceFilePath, INetwork network, bool useSalt, bool useTemperature, IBoundaryFileReader boundaryFileReader, ILogHandler logHandler = null)
        {
            Ensure.NotNullOrEmpty(sourceFilePath, nameof(sourceFilePath));
            Ensure.NotNull(network, nameof(network));
            Ensure.NotNull(boundaryFileReader, nameof(boundaryFileReader));

            this.logHandler = logHandler;
            this.boundaryFileReader = boundaryFileReader;
            directory = Path.GetDirectoryName(sourceFilePath);
            this.useSalt = useSalt;
            this.useTemperature = useTemperature;

            IBranch[] branches = network.Branches.ToArray();
            foreach (IPipe pipe in branches.OfType<IPipe>())
            {
                if (!String.IsNullOrEmpty(pipe.SourceCompartmentName))
                {
                    pipesBySource[pipe.SourceCompartmentName] = pipe;
                }
                if (!String.IsNullOrEmpty(pipe.TargetCompartmentName))
                {
                    pipesByTarget[pipe.TargetCompartmentName] = pipe;
                }
            }

            foreach (IBranch branch in branches)
            {
                branchesBySource[branch.Source.Name] = branch;
                branchesByTarget[branch.Target.Name] = branch;
                branchesByName[branch.Name] = branch;
            }
        }

        /// <summary>
        /// Parses the <paramref name="section"/> from the external forcings file to a <see cref="Model1DLateralSourceData"/>.
        /// </summary>
        /// <param name="section"> The lateral source category. </param>
        /// <returns> The parsed <see cref="Model1DLateralSourceData"/>. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="section"/> is <c>null</c>.
        /// </exception>
        public Model1DLateralSourceData Parse(ILateralSourceExtSection section)
        {
            Ensure.NotNull(section, nameof(section));

            var lateralSource = new LateralSource
            {
                Name = section.Id,
                LongName = section.Name
            };
            var lateralSourceData = new Model1DLateralSourceData
            {
                Feature = lateralSource,
                UseSalt = useSalt,
                UseTemperature = useTemperature
            };

            SetLocation(section, lateralSourceData);
            SetDischarge(section, lateralSourceData);

            return lateralSourceData;
        }

        private void SetDischarge(ILateralSourceExtSection extSection, Model1DLateralSourceData lateralSourceData)
        {
            if (!double.IsNaN(extSection.Discharge))
            {
                lateralSourceData.Flow = extSection.Discharge;
                lateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
                return;
            }

            if (extSection.DischargeFile == null)
            {
                lateralSourceData.DataType = Model1DLateralDataType.FlowRealTime;
                return;
            }

            SetDischargeFromBcFile(extSection, lateralSourceData);
        }

        private void SetDischargeFromBcFile(ILateralSourceExtSection extSection, Model1DLateralSourceData lateralSourceData)
        {
            string bcFilePath = GetFullPath(extSection.DischargeFile);

            if (bcDataByFile.TryGetValue(bcFilePath, out IDictionary<string, ILateralSourceBcSection> dataFromFile))
            {
                SetDischargeData(extSection, lateralSourceData, dataFromFile);
            }
            else
            {
                if (!File.Exists(bcFilePath))
                {
                    logHandler?.ReportError($"File does not exist: {bcFilePath}");
                    return;
                }

                bcDataByFile[bcFilePath] = boundaryFileReader.ReadLateralSourcesFromBcFile(bcFilePath, logHandler).ToDictionary(s => s.Name);
                SetDischargeData(extSection, lateralSourceData, bcDataByFile[bcFilePath]);
            }
        }

        private void SetDischargeData(ILateralSourceExtSection section, Model1DLateralSourceData lateralSourceData, IDictionary<string, ILateralSourceBcSection> dataFromFile)
        {
            if (!dataFromFile.TryGetValue(section.Id, out ILateralSourceBcSection bcCategory))
            {
                logHandler?.ReportError($"Cannot find lateral source '{section.Id}' in file {section.DischargeFile}");
                return;
            }

            lateralSourceData.DataType = bcCategory.DataType;
            lateralSourceData.Flow = bcCategory.Discharge;
            if (bcCategory.DischargeFunction != null)
            {
                lateralSourceData.Data = bcCategory.DischargeFunction;
            }
        }

        private void SetLocation(ILateralSourceExtSection section, Model1DLateralSourceData lateralSourceData)
        {
            if (!string.IsNullOrEmpty(section.NodeName))
            {
                if (pipesBySource.TryGetValue(section.NodeName, out IPipe pipeForSource))
                {
                    SetLocationData(lateralSourceData, pipeForSource, 0d, pipeForSource.SourceCompartment);
                }
                else if (pipesByTarget.TryGetValue(section.NodeName, out IPipe pipeForTarget))
                {
                    SetLocationData(lateralSourceData, pipeForTarget, pipeForTarget.Length, pipeForTarget.TargetCompartment);
                }
                else if (branchesBySource.TryGetValue(section.NodeName, out IBranch branchForSource))
                {
                    SetLocationData(lateralSourceData, branchForSource, 0d);
                }
                else if (branchesByTarget.TryGetValue(section.NodeName, out IBranch branchForTarget))
                {
                    SetLocationData(lateralSourceData, branchForTarget, branchForTarget.Length);
                }
                else
                {
                    logHandler?.ReportError($"Cannot find node '{section.NodeName}' for lateral source '{section.Id}'");
                }
            }
            else
            {
                if (section.BranchName != null && branchesByName.TryGetValue(section.BranchName, out IBranch branchForName))
                {
                    SetLocationData(lateralSourceData, branchForName, section.Chainage);
                }
                else
                {
                    logHandler?.ReportError($"Cannot find branch '{section.BranchName}' for lateral source '{section.Id}'");
                }
            }
        }

        private static void SetLocationData(Model1DLateralSourceData lateralSourceData, IBranch branch, double chainage, ICompartment compartment = null)
        {
            lateralSourceData.Feature.Branch = branch;
            lateralSourceData.Feature.Chainage = chainage;
            lateralSourceData.Compartment = compartment;
            lateralSourceData.Feature.Geometry = HydroNetworkHelper.GetStructureGeometry(branch, chainage);

            branch.BranchFeatures.Add(lateralSourceData.Feature);
        }

        private string GetFullPath(string relativePath) => Path.Combine(directory, relativePath);
    }
}