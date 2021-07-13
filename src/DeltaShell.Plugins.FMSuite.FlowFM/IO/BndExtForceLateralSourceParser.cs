using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders;
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

        private readonly IDictionary<string, IDictionary<string, ILateralSourceBcCategory>> bcDataByFile = new Dictionary<string, IDictionary<string, ILateralSourceBcCategory>>();
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
        /// Parses the <paramref name="category"/> from the external forcings file to a <see cref="Model1DLateralSourceData"/>.
        /// </summary>
        /// <param name="category"> The lateral source category. </param>
        /// <returns> The parsed <see cref="Model1DLateralSourceData"/>. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="category"/> is <c>null</c>.
        /// </exception>
        public Model1DLateralSourceData Parse(ILateralSourceExtCategory category)
        {
            Ensure.NotNull(category, nameof(category));

            var lateralSource = new LateralSource
            {
                Name = category.Id,
                LongName = category.Name
            };
            var lateralSourceData = new Model1DLateralSourceData
            {
                Feature = lateralSource,
                UseSalt = useSalt,
                UseTemperature = useTemperature
            };

            SetLocation(category, lateralSourceData);
            SetDischarge(category, lateralSourceData);

            return lateralSourceData;
        }

        private void SetDischarge(ILateralSourceExtCategory extCategory, Model1DLateralSourceData lateralSourceData)
        {
            if (!double.IsNaN(extCategory.Discharge))
            {
                lateralSourceData.Flow = extCategory.Discharge;
                lateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
                return;
            }

            if (extCategory.DischargeFile == null)
            {
                lateralSourceData.DataType = Model1DLateralDataType.FlowRealTime;
                return;
            }

            SetDischargeFromBcFile(extCategory, lateralSourceData);
        }

        private void SetDischargeFromBcFile(ILateralSourceExtCategory extCategory, Model1DLateralSourceData lateralSourceData)
        {
            string bcFilePath = GetFullPath(extCategory.DischargeFile);

            if (bcDataByFile.TryGetValue(bcFilePath, out IDictionary<string, ILateralSourceBcCategory> dataFromFile))
            {
                SetDischargeData(extCategory, lateralSourceData, dataFromFile);
            }
            else
            {
                if (!File.Exists(bcFilePath))
                {
                    logHandler?.ReportError($"File does not exist: {bcFilePath}");
                    return;
                }

                bcDataByFile[bcFilePath] = boundaryFileReader.ReadLateralSourcesFromBcFile(bcFilePath, logHandler).ToDictionary(s => s.Name);
                SetDischargeData(extCategory, lateralSourceData, bcDataByFile[bcFilePath]);
            }
        }

        private void SetDischargeData(ILateralSourceExtCategory category, Model1DLateralSourceData lateralSourceData, IDictionary<string, ILateralSourceBcCategory> dataFromFile)
        {
            if (!dataFromFile.TryGetValue(category.Id, out ILateralSourceBcCategory bcCategory))
            {
                logHandler?.ReportError($"Cannot find lateral source '{category.Id}' in file {category.DischargeFile}");
                return;
            }

            lateralSourceData.DataType = bcCategory.DataType;
            lateralSourceData.Flow = bcCategory.Discharge;
            if (bcCategory.DischargeFunction != null)
            {
                lateralSourceData.Data = bcCategory.DischargeFunction;
            }
        }

        private void SetLocation(ILateralSourceExtCategory category, Model1DLateralSourceData lateralSourceData)
        {
            if (!string.IsNullOrEmpty(category.NodeName))
            {
                if (pipesBySource.TryGetValue(category.NodeName, out IPipe pipeForSource))
                {
                    SetLocationData(lateralSourceData, pipeForSource, 0d, pipeForSource.SourceCompartment);
                }
                else if (pipesByTarget.TryGetValue(category.NodeName, out IPipe pipeForTarget))
                {
                    SetLocationData(lateralSourceData, pipeForTarget, pipeForTarget.Length, pipeForTarget.TargetCompartment);
                }
                else if (branchesBySource.TryGetValue(category.NodeName, out IBranch branchForSource))
                {
                    SetLocationData(lateralSourceData, branchForSource, 0d);
                }
                else if (branchesByTarget.TryGetValue(category.NodeName, out IBranch branchForTarget))
                {
                    SetLocationData(lateralSourceData, branchForTarget, branchForTarget.Length);
                }
                else
                {
                    logHandler?.ReportError($"Cannot find node '{category.NodeName}' for lateral source '{category.Id}'");
                }
            }
            else
            {
                if (branchesByName.TryGetValue(category.BranchName, out IBranch branchForName))
                {
                    SetLocationData(lateralSourceData, branchForName, category.Chainage);
                }
                else
                {
                    logHandler?.ReportError($"Cannot find branch '{category.BranchName}' for lateral source '{category.Id}'");
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