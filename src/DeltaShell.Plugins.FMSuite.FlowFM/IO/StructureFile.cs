using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

using WriteTimeSeriesAction = System.Action<string, System.DateTime, object>;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// <see cref="StructureFile"/> provides the logic to generate the <see cref="DelftIniCategory"/>
    /// objects corresponding with the structures in a set of <see cref="IHydroRegion"/> through
    /// <see cref="GenerateStructureCategoriesFromFmModel"/>.
    /// </summary>
    public static class StructureFile
    {
        private static readonly Dictionary<Type, WriteTimeSeriesAction> writeTimeSeriesActions =
            new Dictionary<Type, WriteTimeSeriesAction>();

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
        /// Generate the <see cref="DelftIniCategory"/> objects corresponding with the structures in the provided <paramref name="regions"/>.
        /// </summary>
        /// <param name="regions">The regions from which the structures should be obtained.</param>
        /// <param name="referenceTime">The reference time of the model.</param>
        /// <returns>
        /// A collection of <see cref="DelftIniCategory"/> corresponding with the structures in the provided <paramref name="regions"/>.
        /// </returns>
        /// <remarks>
        /// Note that only the first <see cref="HydroArea"/> and first <see cref="IHydroNetwork"/> in the <paramref name="regions"/>
        /// are generated.
        /// </remarks>
        public static IEnumerable<DelftIniCategory> GenerateStructureCategoriesFromFmModel(IEnumerable<IHydroRegion> regions, DateTime referenceTime)
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
        public static void WriteStructureTimFiles(IEnumerable<IHydroRegion> regions,
                                                  string mduFilePath,
                                                  DateTime referenceTime)
        {
            IHydroRegion[] regionsArray = regions.ToArray();
            WriteStructure1DTimeSeries(regionsArray, mduFilePath, referenceTime);
            WriteStructure2DTimeSeries(regionsArray, mduFilePath, referenceTime);
        }

        private static IEnumerable<DelftIniCategory> Generate1DStructureDescriptions(IEnumerable<IHydroRegion> regions)
        {
            IHydroNetwork network = regions.OfType<IHydroNetwork>().FirstOrDefault();
            return network != null ? ExtractFunctionStructuresOfNetworkGenerator(network)
                                   : Enumerable.Empty<DelftIniCategory>();
        }

        private static IEnumerable<DelftIniCategory> Generate2DStructureDescriptions(IEnumerable<IHydroRegion> regions, DateTime referenceTime)
        {
            HydroArea area = regions.OfType<HydroArea>().FirstOrDefault();
            return area != null ? ExtractFunctionStructuresOfAreaGenerator(area, referenceTime)
                                : Enumerable.Empty<DelftIniCategory>();
        }

        private static void WriteStructure1DTimeSeries(IEnumerable<IHydroRegion> regions,
                                                       string mduFilePath,
                                                       DateTime referenceTime)
        {
            IHydroNetwork network = regions.OfType<IHydroNetwork>().FirstOrDefault();
            if (network is null) return;

            IEnumerable<ValueTuple<IStructure1D, TimeSeries>> ToProperties(IHasSteerableProperties structure) =>
                structure.RetrieveSteerableProperties()
                         .Where(p => p.CurrentDriver == SteerablePropertyDriver.TimeSeries)
                         .Select(p => ((IStructure1D)structure, p.TimeSeries));

            IEnumerable<ValueTuple<IStructure1D, TimeSeries>> timeSeries =
                    network.GetStructures().OfType<IHasSteerableProperties>()
                                           .SelectMany(ToProperties);

            foreach ((IStructure1D structure, TimeSeries ts) in timeSeries)
            {
                string timFilePath = GenerateTimeSeriesFileName(mduFilePath, structure, ts);
                new TimFile().Write(timFilePath, ts, referenceTime);
            }
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

        private static IEnumerable<DelftIniCategory> ExtractFunctionStructuresOfNetworkGenerator(IHydroNetwork network)
        {
            List<ICompositeBranchStructure> compositeStructures = network.GetCompositeStructures().ToList();

            foreach (IStructure1D structure in network.GetStructures(compositeStructures))
            {
                DelftIniCategory category = ExtractStructureCategory(structure);
                if (category != null)
                    yield return category;
            }

            foreach (ICompositeBranchStructure compositeStructure in compositeStructures.Where(ShouldCreateRegion))
            {
                yield return new DefinitionGeneratorCompound().CreateStructureRegion(compositeStructure);
            }
        }

        private static bool ShouldCreateRegion(ICompositeBranchStructure branchStructure) =>
            branchStructure.Structures.Count > 0 ||
            branchStructure.Branch is SewerConnection;

        private static DelftIniCategory ExtractStructureCategory(IStructure1D structure)
        {
            StructureType structureType = structure.GetStructureType();
            IDefinitionGeneratorStructure definitionGeneratorStructure =
                DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structureType);
            if (definitionGeneratorStructure == null)
                return null;

            DelftIniCategory structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure);
            if (structure is IFrictionData structureFrictionData)
            {
                if (structure is IBridge)
                {
                    //key is friction
                    AddFrictionData(structureCategory,
                                    structureFrictionData.FrictionDataType,
                                    structureFrictionData.Friction);
                }
                else
                {
                    //key is bed friction
                    AddBedFrictionData(structureCategory,
                                       structureFrictionData.FrictionDataType,
                                       structureFrictionData.Friction);
                }
            }

            return structureCategory;
        }

        private static void AddBedFrictionData(IDelftIniCategory category, Friction frictionType, double friction)
        {
            category.AddProperty(StructureRegion.BedFrictionType.Key, frictionType.ToString().ToLower(), StructureRegion.BedFrictionType.Description);
            category.AddProperty(StructureRegion.BedFriction.Key, friction, StructureRegion.BedFriction.Description, StructureRegion.BedFriction.Format);
        }
        private static void AddFrictionData(IDelftIniCategory category, Friction frictionType, double friction)
        {
            category.AddProperty(StructureRegion.FrictionType.Key, frictionType.ToString().ToLower(), StructureRegion.FrictionType.Description);
            category.AddProperty(StructureRegion.Friction.Key, friction, StructureRegion.Friction.Description, StructureRegion.Friction.Format);
        }

        private static IEnumerable<DelftIniCategory> ExtractFunctionStructuresOfAreaGenerator(HydroArea area, DateTime referenceDateTime) =>
            GetStructures2D(area).Select(structure => CreateStructure2DDelftIniCategory(structure, referenceDateTime));

        private static IEnumerable<IStructure2D> GetStructures2D(HydroArea area) =>
            area.AllHydroObjects.Cast<IStructure2D>();

        private static DelftIniCategory CreateStructure2DDelftIniCategory(IStructure2D structure, DateTime referenceDateTime)
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
                string timFilePath = GenerateTimeSeriesFileName(mduFilePath, pump, pump.CapacityTimeSeries);
                new TimFile().Write(timFilePath, pump.CapacityTimeSeries, referenceDateTime);
            }
        }

        private static void WriteWeirTimeSeriesFile(string mduFilePath, DateTime referenceDateTime, IWeir weir)
        {
            if (weir != null && weir.HasCrestLevelTimeSeries())
            {
                string timFilePath = GenerateTimeSeriesFileName(mduFilePath, weir, weir.CrestLevelTimeSeries);
                new TimFile().Write(timFilePath, weir.CrestLevelTimeSeries, referenceDateTime);
            }
        }

        private static void WriteGateTimeSeriesFiles(string mduFilePath, DateTime referenceDateTime, IGate gate)
        {
            if (gate.UseSillLevelTimeSeries)
            {
                string timFilePath = GenerateTimeSeriesFileName(mduFilePath, gate, gate.SillLevelTimeSeries);
                new TimFile().Write(timFilePath, gate.SillLevelTimeSeries, referenceDateTime);
            }
            if (gate.UseLowerEdgeLevelTimeSeries)
            {
                string timFilePath = GenerateTimeSeriesFileName(mduFilePath, gate, gate.LowerEdgeLevelTimeSeries);
                new TimFile().Write(timFilePath, gate.LowerEdgeLevelTimeSeries, referenceDateTime);
            }
            if (gate.UseOpeningWidthTimeSeries)
            {
                string timFilePath = GenerateTimeSeriesFileName(mduFilePath, gate, gate.OpeningWidthTimeSeries);
                new TimFile().Write(timFilePath, gate.OpeningWidthTimeSeries, referenceDateTime);
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
            new TimFile().Write(timFilePath, timeSeries, leveeBreachSettings.StartTimeBreachGrowth, commentLines);
        }

        private static bool HasCapacityTimeSeries(this IPump pump) =>
            pump.CanBeTimedependent
            && pump.UseCapacityTimeSeries
            && pump.CapacityTimeSeries != null;

        private static bool HasCrestLevelTimeSeries(this IWeir weir) =>
            weir.IsUsingTimeSeriesForCrestLevel()
            && weir.CrestLevelTimeSeries != null;

        private static string GenerateTimeSeriesFileName(string mduFilePath, IStructure structure, ITimeSeries timeSeries) =>
            NGHSFileBase.GetOtherFilePathInSameDirectory(
                mduFilePath,
                StructureTimFileNameGenerator.Generate(structure, timeSeries));
    }
}
