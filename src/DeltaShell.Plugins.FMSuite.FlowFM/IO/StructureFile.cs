using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class StructureFile
    {
        private static readonly Dictionary<Type, Action<string, DateTime, object>> WriteTimeSeriesActions = new Dictionary<Type, Action<string, DateTime, object>>();

        private static readonly Dictionary<string, Action<string, string, DateTime, IStructure2D>> StructureWriteActions = new Dictionary<string, Action<string, string, DateTime, IStructure2D>>
        {
            {StructureRegion.Capacity.Key, WriteTimeSeriesFile},
            {StructureRegion.CrestLevel.Key, WriteTimeSeriesFile },
            {StructureRegion.GateLowerEdgeLevel.Key, WriteTimeSeriesFile },
            {StructureRegion.GateOpeningWidth.Key, WriteTimeSeriesFile },
            {StructureRegion.TimeFileName.Key, WriteTimeSeriesFile }
        };

        static StructureFile()
        {
            RegisterWriteTimeSeriesAction<IPump>(WritePumpTimeSeriesFile);
            RegisterWriteTimeSeriesAction<IWeir>(WriteWeirTimeSeriesFile);
            RegisterWriteTimeSeriesAction<IGate>(WriteGateTimeSeriesFiles);
            RegisterWriteTimeSeriesAction<LeveeBreach>(WriteLeveeBreachTimeSeriesFile);
        }

        private static void RegisterWriteTimeSeriesAction<T>(Action<string, DateTime, T> action)
        {
            WriteTimeSeriesActions.Add(typeof(T), (s, time, structure2D) => action(s,time, (T)structure2D));
        }

        public static IEnumerable<DelftIniCategory> Generate2DStructureCategoriesFromFmModel(IEnumerable<IHydroRegion> regions, DateTime referenceTime, string mduPath)
        {
            var hydroRegions = regions as IHydroRegion[] ?? regions.ToArray();
            var network = hydroRegions.OfType<IHydroNetwork>().FirstOrDefault();
            if (network == null) return Enumerable.Empty<DelftIniCategory>();

            var categories1D = NetworkEditor.IO.StructureFile.ExtractFunctionStructuresOfNetworkGenerator(network);
            var area = hydroRegions.OfType<HydroArea>().FirstOrDefault();
            if (area == null) return categories1D; 
            var categories2D = ExtractFunctionStructuresOfAreaGenerator(area, referenceTime).ToList();

            Action<string, string, DateTime, IStructure2D> myAction;
            foreach (var category2D in categories2D)
            {
                var structureName = category2D.GetPropertyValue(StructureRegion.Id.Key);
                if (string.IsNullOrEmpty(structureName)) continue;

                foreach (var propertyName in category2D.Properties.Select(property => property.Name))
                {
                    if (StructureWriteActions.TryGetValue(propertyName, out myAction))
                    {
                        var fileNameProperty = category2D.Properties.FirstOrDefault(p => p.Name == propertyName);
                        var structure2D = area.AllHydroObjects.Cast<IStructure2D>().FirstOrDefault(o => o.Name == structureName);
                        if (fileNameProperty != null) myAction(fileNameProperty.Value, mduPath, referenceTime, structure2D);
                    }
                }
            }

            return categories1D.Concat(categories2D);
        }

        private static IEnumerable<DelftIniCategory> ExtractFunctionStructuresOfAreaGenerator(HydroArea area, DateTime referenceDateTime)
        {
            foreach (var structure2D in area.AllHydroObjects.Cast<IStructure2D>())
            {
                var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structure2D.Structure2DType, referenceDateTime);
                var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure2D);
                yield return structureCategory;
            }
        }

        private static void WriteTimeSeriesFile(string fileName, string mduFilePath, DateTime referenceTime, IStructure2D structure2D)
        {
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFilePath, fileName);
            WriteTimeSeriesActions.Keys.Where(k => k.IsInstanceOfType(structure2D)).ForEach(key => WriteTimeSeriesActions[key](timFilePath, referenceTime, structure2D));
        }

        private static void WritePumpTimeSeriesFile(string timFilePath, DateTime referenceDateTime, IPump pump)
        {
            if (pump != null && pump.HasCapacityTimeSeries())
            {
                new TimFile().Write(timFilePath, pump.CapacityTimeSeries, referenceDateTime);
            }
        }

        private static void WriteWeirTimeSeriesFile(string timFilePath, DateTime referenceDateTime, IWeir weir)
        {
            if (weir != null && weir.HasCrestLevelTimeSeries())
            {
                new TimFile().Write(timFilePath, weir.CrestLevelTimeSeries, referenceDateTime);
            }
        }

        private static void WriteGateTimeSeriesFiles(string timFilePath, DateTime referenceDateTime, IGate gate)
        {
            if (gate.UseSillLevelTimeSeries && timFilePath.Contains($"{gate.Name}_{StructureRegion.GateCrestLevel.Key}.tim"))
            {
                new TimFile().Write(timFilePath, gate.SillLevelTimeSeries, referenceDateTime);
            }
            if (gate.UseLowerEdgeLevelTimeSeries && timFilePath.Contains($"{gate.Name}_{StructureRegion.GateLowerEdgeLevel.Key}.tim"))
            {
                new TimFile().Write(timFilePath, gate.LowerEdgeLevelTimeSeries, referenceDateTime);
            }
            if (gate.UseOpeningWidthTimeSeries && timFilePath.Contains($"{gate.Name}_{StructureRegion.GateOpeningWidth.Key}.tim"))
            {
                new TimFile().Write(timFilePath, gate.OpeningWidthTimeSeries, referenceDateTime);
            }
        }

        private static void WriteLeveeBreachTimeSeriesFile(string timFilePath, DateTime referenceDateTime, IStructure2D structure2D)
        {
            var leveeBreach = structure2D as LeveeBreach;
            var leveeBreachSettings = leveeBreach?.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
            if (leveeBreach == null || leveeBreachSettings == null) return;

            var timeSeries = leveeBreachSettings.CreateTimeSeriesFromTable();
            var commentLines = new List<string> { "Time entries are defined in minutes, relative to the breach growth start" };
            new TimFile().Write(timFilePath, timeSeries, leveeBreachSettings.StartTimeBreachGrowth, commentLines);
        }

        private static bool HasCapacityTimeSeries(this IPump pump)
        {
            return pump.CanBeTimedependent
                   && pump.UseCapacityTimeSeries
                   && pump.CapacityTimeSeries != null;
        }

        private static bool HasCrestLevelTimeSeries(this IWeir weir)
        {
            return weir.CanBeTimedependent
                   && weir.UseCrestLevelTimeSeries
                   && weir.CrestLevelTimeSeries != null;
        }
    }
}
