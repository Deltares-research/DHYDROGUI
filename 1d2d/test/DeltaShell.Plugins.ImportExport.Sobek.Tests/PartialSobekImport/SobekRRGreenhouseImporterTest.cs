using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekRRGreenhouseImporterTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var importer = new SobekRRGreenhouseImporter();

            // Assert
            Assert.That(importer.DisplayName, Is.EqualTo("Rainfall Runoff greenhouse data"));
            Assert.That(importer.Category, Is.EqualTo(SobekImporterCategories.RainfallRunoff));
            Assert.That(importer.PathSobek, Is.Null);
            Assert.That(importer.TargetObject, Is.Null);
            Assert.That(importer.PartialSobekImporter, Is.Null);
            Assert.That(importer.IsActive, Is.True);
            Assert.That(importer.IsVisible, Is.True);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.BeforeImport, Is.Null);
            Assert.That(importer.AfterImport, Is.Null);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Import_ImportsGreenHouseData()
        {
            var importer = new SobekRRGreenhouseImporter();

            GreenhouseData greenHouse1 = CreateGreenHouseData("Greenhouse_id_1");
            GreenhouseData greenHouse2 = CreateGreenHouseData("Greenhouse_id_2");
            GreenhouseData greenHouse3 = CreateGreenHouseData("Greenhouse_id_3");

            string mainFileContent =
                string.Join(Environment.NewLine,
                            "GRHS id 'Greenhouse_id_1' na 10 ar 1.0 1.1 1.2 1.3 1.4 1.5 1.6 1.7 1.8 1.9 sl 1.1 as 1.2 si 'Silo_id_1' sd 'Storage_id_1' ms 'Station_name_1' aaf 1.3 is 1.4 grhs",
                            "GRHS id 'Greenhouse_id_2' na 10 ar 2.0 2.1 2.2 2.3 2.4 2.5 2.6 2.7 2.8 2.9 sl 2.1 as 2.2 si 'Silo_id_2' sd 'Storage_id_2' ms 'Station_name_2' aaf 2.3 is 2.4 grhs",
                            "GRHS id 'Greenhouse_id_3' na 10 ar 3.0 3.1 3.2 3.3 3.4 3.5 3.6 3.7 3.8 3.9 sl 3.1 as 3.2 si 'Silo_id_3' sd 'Storage_id_3' ms 'Station_name_3' aaf 3.3 is 3.4 grhs");

            string siloFileContent =
                string.Join(Environment.NewLine,
                            "SILO id 'Silo_id_1' nm 'Silo_name_1' sc 1.5 pc 1.6 silo",
                            "SILO id 'Silo_id_2' nm 'Silo_name_2' sc 2.5 pc 2.6 silo",
                            "SILO id 'Silo_id_3' nm 'Silo_name_3' sc 3.5 pc 3.6 silo");

            string storageFileContent =
                string.Join(Environment.NewLine,
                            "STDF id 'Storage_id_1' nm 'Storage_name_1' mk 1.7 ik 1.8 stdf",
                            "STDF id 'Storage_id_2' nm 'Storage_name_2' mk 2.7 ik 2.8 stdf",
                            "STDF id 'Storage_id_3' nm 'Storage_name_3' mk 3.7 ik 3.8 stdf");

            using (var model = new RainfallRunoffModel())
            using (var temp = new TemporaryDirectory())
            {
                model.ModelData.Add(greenHouse1);
                model.ModelData.Add(greenHouse2);
                model.ModelData.Add(greenHouse3);

                temp.CreateFile("GREENHSE.3B", mainFileContent);
                temp.CreateFile("GREENHSE.SIL", siloFileContent);
                temp.CreateFile("GREENHSE.RF", storageFileContent);

                importer.PathSobek = Path.Combine(temp.Path, "NETWORP.TP");
                importer.TargetObject = model;

                // Call
                importer.Import();

                // Assert
                AssertGreenHouse(greenHouse1, area: 14.5, sl: 1.1, @as: 1.2, aaf: 1.3, useSilo: true, sc: 1.5, pc: 1.6, mk: 1.7, ik: 1.8, ms: "Station_name_1");
                AssertGreenHouse(greenHouse2, area: 24.5, sl: 2.1, @as: 2.2, aaf: 2.3, useSilo: true, sc: 2.5, pc: 2.6, mk: 2.7, ik: 2.8, ms: "Station_name_2");
                AssertGreenHouse(greenHouse3, area: 34.5, sl: 3.1, @as: 3.2, aaf: 3.3, useSilo: true, sc: 3.5, pc: 3.6, mk: 3.7, ik: 3.8, ms: "Station_name_3");
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Import_GreenhouseIdNotFound_LogsWarningAndDoesNotImportGreenHouseData()
        {
            var importer = new SobekRRGreenhouseImporter();

            GreenhouseData greenHouse = CreateGreenHouseData("Greenhouse_id");

            const string mainFileContent = "GRHS id 'Other_greenhouse_id' na 10 ar 1.0 1.1 1.2 1.3 1.4 1.5 1.6 1.7 1.8 1.9 sl 1.1 as 1.2 si 'Silo_id' sd 'Storage_id' ms 'Station_name' aaf 1.3 is 1.4 grhs";
            const string siloFileContent = "SILO id 'Silo_id' nm 'Silo_name' sc 1.5 pc 1.6 silo";
            const string storageFileContent = "STDF id 'Storage_id' nm 'Storage_name' mk 1.7 ik 1.8 stdf";

            using (var model = new RainfallRunoffModel())
            using (var temp = new TemporaryDirectory())
            {
                model.ModelData.Add(greenHouse);

                temp.CreateFile("GREENHSE.3B", mainFileContent);
                temp.CreateFile("GREENHSE.SIL", siloFileContent);
                temp.CreateFile("GREENHSE.RF", storageFileContent);

                importer.PathSobek = Path.Combine(temp.Path, "NETWORP.TP");
                importer.TargetObject = model;

                // Call
                void Call() => importer.Import();

                // Assert
                IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
                Assert.That(errors.Single(), Is.EqualTo("Rainfall runoff area with id Other_greenhouse_id has not been found. Item has been skipped..."));
                AssertGreenHouse(greenHouse, area: 0.0, sl: 1.5, @as: 0.0, aaf: 1.0, useSilo: false, sc: 200, pc: 0.02, mk: 0.0, ik: 0.0, ms: string.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Import_SiloIdNotFound_LogsErrorAndImportsGreenHouseData()
        {
            var importer = new SobekRRGreenhouseImporter();

            GreenhouseData greenHouse = CreateGreenHouseData("Greenhouse_id");

            const string mainFileContent = "GRHS id 'Greenhouse_id' na 10 ar 1.0 1.1 1.2 1.3 1.4 1.5 1.6 1.7 1.8 1.9 sl 1.1 as 1.2 si 'Other_silo_id' sd 'Storage_id' ms 'Station_name' aaf 1.3 is 1.4 grhs";
            const string siloFileContent = "SILO id 'Silo_id' nm 'Silo_name' sc 1.5 pc 1.6 silo";
            const string storageFileContent = "STDF id 'Storage_id' nm 'Storage_name' mk 1.7 ik 1.8 stdf";

            using (var model = new RainfallRunoffModel())
            using (var temp = new TemporaryDirectory())
            {
                model.ModelData.Add(greenHouse);

                temp.CreateFile("GREENHSE.3B", mainFileContent);
                string siloFilePath = temp.CreateFile("GREENHSE.SIL", siloFileContent);
                temp.CreateFile("GREENHSE.RF", storageFileContent);

                importer.PathSobek = Path.Combine(temp.Path, "NETWORP.TP");
                importer.TargetObject = model;

                // Call
                void Call() => importer.Import();

                // Assert
                IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
                Assert.That(errors.Single(), Is.EqualTo($"Silo definition with id 'Other_silo_id' has not been found: {siloFilePath}"));
                AssertGreenHouse(greenHouse, area: 14.5, sl: 1.1, @as: 1.2, aaf: 1.3, useSilo: true, sc: 200, pc: 0.02, mk: 1.7, ik: 1.8, ms: "Station_name");
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Import_SiloIdNotFound_NoStorageArea_LogsNoErrorAndImportsGreenHouseData()
        {
            var importer = new SobekRRGreenhouseImporter();

            GreenhouseData greenHouse = CreateGreenHouseData("Greenhouse_id");

            const string mainFileContent = "GRHS id 'Greenhouse_id' na 10 ar 1.0 1.1 1.2 1.3 1.4 1.5 1.6 1.7 1.8 1.9 sl 1.1 as 0 si 'Other_silo_id' sd 'Storage_id' ms 'Station_name' aaf 1.3 is 1.4 grhs";
            const string siloFileContent = "SILO id 'Silo_id' nm 'Silo_name' sc 1.5 pc 1.6 silo";
            const string storageFileContent = "STDF id 'Storage_id' nm 'Storage_name' mk 1.7 ik 1.8 stdf";

            using (var model = new RainfallRunoffModel())
            using (var temp = new TemporaryDirectory())
            {
                model.ModelData.Add(greenHouse);

                temp.CreateFile("GREENHSE.3B", mainFileContent);
                string siloFilePath = temp.CreateFile("GREENHSE.SIL", siloFileContent);
                temp.CreateFile("GREENHSE.RF", storageFileContent);

                importer.PathSobek = Path.Combine(temp.Path, "NETWORP.TP");
                importer.TargetObject = model;

                // Call
                void Call() => importer.Import();

                // Assert
                IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
                Assert.That(errors, Is.Empty);
                AssertGreenHouse(greenHouse, area: 14.5, sl: 1.1, @as: 0, aaf: 1.3, useSilo: false, sc: 200, pc: 0.02, mk: 1.7, ik: 1.8, ms: "Station_name");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Import_StorageIdNotFound_LogsErrorAndImportsGreenHouseData()
        {
            var importer = new SobekRRGreenhouseImporter();

            GreenhouseData greenHouse = CreateGreenHouseData("Greenhouse_id");

            const string mainFileContent = "GRHS id 'Greenhouse_id' na 10 ar 1.0 1.1 1.2 1.3 1.4 1.5 1.6 1.7 1.8 1.9 sl 1.1 as 1.2 si 'Silo_id' sd 'Other_storage_id' ms 'Station_name' aaf 1.3 is 1.4 grhs";
            const string siloFileContent = "SILO id 'Silo_id' nm 'Silo_name' sc 1.5 pc 1.6 silo";
            const string storageFileContent = "STDF id 'Storage_id' nm 'Storage_name' mk 1.7 ik 1.8 stdf";

            using (var model = new RainfallRunoffModel())
            using (var temp = new TemporaryDirectory())
            {
                model.ModelData.Add(greenHouse);

                temp.CreateFile("GREENHSE.3B", mainFileContent);
                temp.CreateFile("GREENHSE.SIL", siloFileContent);
                string storageFilePath = temp.CreateFile("GREENHSE.RF", storageFileContent);

                importer.PathSobek = Path.Combine(temp.Path, "NETWORP.TP");
                importer.TargetObject = model;

                // Call
                void Call() => importer.Import();

                // Assert
                IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
                Assert.That(errors.Single(), Is.EqualTo($"Storage definition with id 'Other_storage_id' has not been found: {storageFilePath}"));
                AssertGreenHouse(greenHouse, area: 14.5, sl: 1.1, @as: 1.2, aaf: 1.3, useSilo: true, sc: 1.5, pc: 1.6, mk: 0, ik: 0, ms: "Station_name");
            }
        }

        private static void AssertGreenHouse(GreenhouseData greenHouse, double area, double sl, double @as, double aaf, bool useSilo, double sc, double pc, double mk, double ik, string ms)
        {
            Assert.That(greenHouse.CalculationArea, Is.EqualTo(area), "Incorrect calculation area (Σar)");
            Assert.That(greenHouse.SurfaceLevel, Is.EqualTo(sl), "Incorrect surface level (sl)");
            Assert.That(greenHouse.SubSoilStorageArea, Is.EqualTo(@as), "Incorrect subsoil storage area (as)");
            Assert.That(greenHouse.AreaAdjustmentFactor, Is.EqualTo(aaf), "Incorrect area adjustment factor (aaf)");
            Assert.That(greenHouse.UseSubsoilStorage, Is.EqualTo(useSilo), "Incorrect use subsoil storage (as > 0)");
            Assert.That(greenHouse.SiloCapacity, Is.EqualTo(sc), "Incorrect silo capacity (sc)");
            Assert.That(greenHouse.PumpCapacity, Is.EqualTo(pc), "Incorrect pump capacity (pc)");
            Assert.That(greenHouse.MaximumRoofStorage, Is.EqualTo(mk), "Incorrect maximum roof storage (mk)");
            Assert.That(greenHouse.InitialRoofStorage, Is.EqualTo(ik), "Incorrect initial roof storage (ik)");
            Assert.That(greenHouse.MeteoStationName, Is.EqualTo(ms), "Incorrect meteo station name (ms)");
        }

        private static GreenhouseData CreateGreenHouseData(string name)
        {
            return new GreenhouseData(new Catchment()) { Name = name };
        }
    }
}