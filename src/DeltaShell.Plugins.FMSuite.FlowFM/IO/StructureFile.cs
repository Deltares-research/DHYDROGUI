using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.FileWriters.TimeSeriesWriters;
using DeltaShell.NGHS.IO.Helpers;
using WriteTimeSeriesAction = System.Action<string, System.DateTime, object>;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// <see cref="StructureFile"/> provides the logic to generate the <see cref="IniSection"/>
    /// objects corresponding with the structures in a set of <see cref="IHydroRegion"/> through
    /// <see cref="GenerateStructureIniSectionsFromFmModel"/>.
    /// </summary>
    public static class StructureFile
    {
        private static readonly Dictionary<Type, WriteTimeSeriesAction> writeTimeSeriesActions =
            new Dictionary<Type, WriteTimeSeriesAction>();

        private static readonly IStructureFileNameGenerator structureBcFileNameGenerator = new StructureBcFileNameGenerator();
        private static readonly ITimeSeriesFileWriter bcTimeSeriesFileWriter = new BcTimeSeriesWriter(new BcWriter(new FileSystem()), new StructureBoundaryGenerator());
        private static readonly TimFile timTimeSeriesFileWriter = new TimFile();
        static StructureFile()
        {
            RegisterWriteTimeSeriesAction<IPump>(WritePumpTimeSeriesFile);
            RegisterWriteTimeSeriesAction<IWeir>(WriteWeirTimeSeriesFile);
            RegisterWriteTimeSeriesAction<IGate>(WriteGateTimeSeriesFiles);
            RegisterWriteTimeSeriesAction<LeveeBreach>(WriteLeveeBreachTimeSeriesFile);
        }

        private static void RegisterWriteTimeSeriesAction<T>(Action<string, DateTime, T> action)
        {
            writeTimeSeriesActions.Add(typeof(T), (s, time, structure2D) => action(s, time, (T)structure2D));
        }

        /// <summary>
        /// Generate the <see cref="IniSection"/> objects corresponding with the structures in the provided <paramref name="regions"/>.
        /// </summary>
        /// <param name="regions">The regions from which the structures should be obtained.</param>
        /// <param name="referenceTime">The reference time of the model.</param>
        /// <returns>
        /// A collection of <see cref="IniSection"/> corresponding with the structures in the provided <paramref name="regions"/>.
        /// </returns>
        /// <remarks>
        /// Note that only the first <see cref="HydroArea"/> and first <see cref="IHydroNetwork"/> in the <paramref name="regions"/>
        /// are generated.
        /// </remarks>
        public static IEnumerable<IniSection> GenerateStructureIniSectionsFromFmModel(IEnumerable<IHydroRegion> regions, DateTime referenceTime)
        {
            IHydroRegion[] regionsArray = regions.ToArray();
            return Enumerable.Concat(Generate1DStructureDescriptions(regionsArray),
                                     Generate2DStructureDescriptions(regionsArray, referenceTime));
        }

        /// <summary>
        /// Write the .tim files corresponding with the <see cref="TimeSeries"/> in the structures of the provided
        /// <paramref name="regions"/>.
        /// </summary>
        /// <param name="regions">The regions which should be inspected for time series.</param>
        /// <param name="mduFilePath">The path to the .mdu file.</param>
        /// <param name="referenceTime">The reference time.</param>
        public static void WriteStructureFiles(IEnumerable<IHydroRegion> regions,
                                               string mduFilePath,
                                               DateTime referenceTime)
        {
            IHydroRegion[] regionsArray = regions.ToArray();
            WriteStructure1DTimeSeries(regionsArray, mduFilePath, referenceTime);
            WriteStructure2DTimeSeries(regionsArray, mduFilePath, referenceTime);
        }

        private static IEnumerable<IniSection> Generate1DStructureDescriptions(IEnumerable<IHydroRegion> regions)
        {
            IHydroNetwork network = regions.OfType<IHydroNetwork>().FirstOrDefault();
            return network != null ? ExtractFunctionStructuresOfNetworkGenerator(network)
                                   : Enumerable.Empty<IniSection>();
        }

        private static IEnumerable<IniSection> Generate2DStructureDescriptions(IEnumerable<IHydroRegion> regions, DateTime referenceTime)
        {
            HydroArea area = regions.OfType<HydroArea>().FirstOrDefault();
            return area != null ? ExtractFunctionStructuresOfAreaGenerator(area, referenceTime)
                                : Enumerable.Empty<IniSection>();
        }

        private static void WriteStructure1DTimeSeries(IEnumerable<IHydroRegion> regions,
                                                       string mduFilePath,
                                                       DateTime referenceTime)
        {
            IHydroNetwork network = regions.OfType<IHydroNetwork>().FirstOrDefault();
            if (network is null) return;

            IEnumerable<IStructureTimeSeries> ToProperties(IHasSteerableProperties structure) =>
                structure.RetrieveSteerableProperties()
                         .Where(p => p.CurrentDriver == SteerablePropertyDriver.TimeSeries)
                         .Select(p => new StructureTimeSeries((IStructure1D)structure, p.TimeSeries));

            IEnumerable<IStructureTimeSeries> timeSeries =
                    network.GetStructures().OfType<IHasSteerableProperties>()
                                           .SelectMany(ToProperties);

            string filePath = GenerateTimeSeriesBcFileName(mduFilePath);
            bcTimeSeriesFileWriter.Write(filePath, timeSeries, referenceTime);
        }

        private static void WriteStructure2DTimeSeries(IEnumerable<IHydroRegion> regions,
                                                       string mduFilePath,
                                                       DateTime referenceTime)
        {
            HydroArea area = regions.OfType<HydroArea>().FirstOrDefault();
            if (area is null) return;

            foreach (IStructure2D structure in GetStructures2D(area))
            {
                WriteTimeSeriesFile(mduFilePath, referenceTime, structure);
            }
        }

        private static IEnumerable<IStructure1D> GetStructures(this IHydroNetwork network, IEnumerable<ICompositeBranchStructure> compositeStructures = null)
        {
            if (compositeStructures is null)
            {
                compositeStructures = network.GetCompositeStructures();
            }

            IEnumerable<IStructure1D> simpleStructures = network.GetSimpleStructures();
            return simpleStructures.Concat(compositeStructures.SelectMany(cs => cs.Structures)).Distinct();
        }

        private static IEnumerable<ICompositeBranchStructure> GetCompositeStructures(this IHydroNetwork network) =>
            network.Structures.Where(s => s.GetStructureType() == StructureType.CompositeBranchStructure)
                              .Cast<ICompositeBranchStructure>();

        private static IEnumerable<IStructure1D> GetSimpleStructures(this IHydroNetwork network) =>
            network.Structures.Where(s => s.GetStructureType() != StructureType.CompositeBranchStructure);

        private static IEnumerable<IniSection> ExtractFunctionStructuresOfNetworkGenerator(IHydroNetwork network)
        {
            List<ICompositeBranchStructure> compositeStructures = network.GetCompositeStructures().ToList();

            foreach (IStructure1D structure in network.GetStructures(compositeStructures))
            {
                IniSection iniSection = ExtractStructureIniSection(structure);
                if (iniSection != null)
                    yield return iniSection;
            }

            foreach (ICompositeBranchStructure compositeStructure in compositeStructures.Where(ShouldCreateRegion))
            {
                yield return new DefinitionGeneratorCompound().CreateStructureRegion(compositeStructure);
            }
        }

        private static bool ShouldCreateRegion(ICompositeBranchStructure branchStructure) =>
            branchStructure.Structures.Count > 0 ||
            branchStructure.Branch is SewerConnection;

        private static IniSection ExtractStructureIniSection(IStructure1D structure)
        {
            StructureType structureType = structure.GetStructureType();
            IDefinitionGeneratorStructure definitionGeneratorStructure =
                DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structureType);
            if (definitionGeneratorStructure == null)
                return null;

            IniSection structureIniSection = definitionGeneratorStructure.CreateStructureRegion(structure);
            if (structure is IFrictionData structureFrictionData)
            {
                if (structure is IBridge)
                {
                    //key is friction
                    AddFrictionData(structureIniSection,
                                    structureFrictionData.FrictionDataType,
                                    structureFrictionData.Friction);
                }
                else
                {
                    //key is bed friction
                    AddBedFrictionData(structureIniSection,
                                       structureFrictionData.FrictionDataType,
                                       structureFrictionData.Friction);
                }
            }

            return structureIniSection;
        }

        private static void AddBedFrictionData(IniSection iniSection, Friction frictionType, double friction)
        {
            iniSection.AddPropertyWithOptionalComment(StructureRegion.BedFrictionType.Key, frictionType.ToString().ToLower(), StructureRegion.BedFrictionType.Description);
            iniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.BedFriction.Key, friction, StructureRegion.BedFriction.Description, StructureRegion.BedFriction.Format);
        }
        private static void AddFrictionData(IniSection iniSection, Friction frictionType, double friction)
        {
            iniSection.AddPropertyWithOptionalComment(StructureRegion.FrictionType.Key, frictionType.ToString().ToLower(), StructureRegion.FrictionType.Description);
            iniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Friction.Key, friction, StructureRegion.Friction.Description, StructureRegion.Friction.Format);
        }

        private static IEnumerable<IniSection> ExtractFunctionStructuresOfAreaGenerator(HydroArea area, DateTime referenceDateTime) =>
            GetStructures2D(area).Select(structure => CreateStructure2DIniSection(structure, referenceDateTime));

        private static IEnumerable<IStructure2D> GetStructures2D(HydroArea area) =>
            area.AllHydroObjects.Cast<IStructure2D>();

        private static IniSection CreateStructure2DIniSection(IStructure2D structure, DateTime referenceDateTime)
        {
            DefinitionGeneratorStructure2D definitionGeneratorStructure =
                DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structure.Structure2DType, referenceDateTime);
            return definitionGeneratorStructure.CreateStructureRegion(structure);
        }

        private static void WriteTimeSeriesFile(string mduFilePath, DateTime referenceTime, IStructure2D structure2D) =>
            writeTimeSeriesActions.Keys
                                  .Where(k => k.IsInstanceOfType(structure2D))
                                  .Select(key => writeTimeSeriesActions[key])
                                  .ForEach(writeTimeSeries => writeTimeSeries(mduFilePath, referenceTime, structure2D));

        private static void WritePumpTimeSeriesFile(string mduFilePath, DateTime referenceDateTime, IPump pump)
        {
            if (pump != null && pump.HasCapacityTimeSeries())
            {
                string timFilePath = GenerateTimeSeriesTimFileName(mduFilePath, pump, pump.CapacityTimeSeries);
                timTimeSeriesFileWriter.Write(timFilePath, pump.CapacityTimeSeries, referenceDateTime);
            }
        }

        private static void WriteWeirTimeSeriesFile(string mduFilePath, DateTime referenceDateTime, IWeir weir)
        {
            if (weir != null && weir.HasCrestLevelTimeSeries())
            {
                string timFilePath = GenerateTimeSeriesTimFileName(mduFilePath, weir, weir.CrestLevelTimeSeries);
                timTimeSeriesFileWriter.Write(timFilePath, weir.CrestLevelTimeSeries, referenceDateTime);
            }
        }

        private static void WriteGateTimeSeriesFiles(string mduFilePath, DateTime referenceDateTime, IGate gate)
        {
            if (gate.UseSillLevelTimeSeries)
            {
                string timFilePath = GenerateTimeSeriesTimFileName(mduFilePath, gate, gate.SillLevelTimeSeries);
                timTimeSeriesFileWriter.Write(timFilePath,  gate.SillLevelTimeSeries, referenceDateTime);
            }
            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                string timFilePath = GenerateTimeSeriesTimFileName(mduFilePath, gate, gate.LowerEdgeLevelTimeSeries);
                timTimeSeriesFileWriter.Write(timFilePath,  gate.LowerEdgeLevelTimeSeries, referenceDateTime);
            }
            if (gate.UseOpeningWidthTimeSeries)
            {
                string timFilePath = GenerateTimeSeriesTimFileName(mduFilePath, gate, gate.OpeningWidthTimeSeries);
                timTimeSeriesFileWriter.Write(timFilePath,  gate.OpeningWidthTimeSeries, referenceDateTime);
            }
        }

        private static void WriteLeveeBreachTimeSeriesFile(string mduFilePath, DateTime referenceDateTime, IStructure2D structure2D)
        {
            if (!(structure2D is LeveeBreach leveeBreach) ||
                !(leveeBreach.GetActiveLeveeBreachSettings() is UserDefinedBreachSettings leveeBreachSettings))
                return;

            TimeSeries timeSeries = leveeBreachSettings.CreateTimeSeriesFromTable();
            string[] commentLines = { "Time entries are defined in minutes, relative to the breach growth start" };

            string timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFilePath,
                                                                              $"{leveeBreach.Name}.tim");

            timTimeSeriesFileWriter.Write(timFilePath,  timeSeries, leveeBreachSettings.StartTimeBreachGrowth, commentLines);
        }

        private static bool HasCapacityTimeSeries(this IPump pump) =>
            pump.CanBeTimedependent
            && pump.UseCapacityTimeSeries
            && pump.CapacityTimeSeries != null;

        private static bool HasCrestLevelTimeSeries(this IWeir weir) =>
            weir.IsUsingTimeSeriesForCrestLevel()
            && weir.CrestLevelTimeSeries != null;

        private static string GenerateTimeSeriesBcFileName(string mduFilePath) =>
            NGHSFileBase.GetOtherFilePathInSameDirectory(
                mduFilePath,
                structureBcFileNameGenerator.Generate());

        private static string GenerateTimeSeriesTimFileName(string mduFilePath, IStructure structure, ITimeSeries timeSeries) =>
            NGHSFileBase.GetOtherFilePathInSameDirectory(
                mduFilePath,
                StructureTimFileNameGenerator.Generate(structure, timeSeries));
    }
}
