using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekRetentionReaderTest
    {
        [Test]
        public void Sobek212()
        {
            const string source = @"NODE id '6' ty 1 ws 10000 ss 0 wl 22 node";

            SobekRetentionsReader sobekRetentionsReader = new SobekRetentionsReader {Sobek2Import = true};
            var retention = sobekRetentionsReader.GetRetention(source);
            Assert.AreEqual("6", retention.Name);
            Assert.AreEqual(22, retention.BedLevel, 1.0e-6);
            Assert.AreEqual(10000, retention.StorageArea, 1.0e-6);
        }

        [Test]
        public void SobekRE()
        {
            const string source = @"FLBR id '201' sc 0 dc lt 5 9.9999e+009 9.9999e+009 s2 '103' ar 551056 bl 44.82 ih 44.82 u1 1 ca 0 0 0 0 cj '-1' '-1' '-1' '-1' cb 1 1 1 0 ck '01' '119' '5596' '-1' lt 0 sd '102' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";

            SobekRetentionsReader sobekRetentionsReader = new SobekRetentionsReader {Sobek2Import = false};
            var retention = sobekRetentionsReader.GetRetention(source);
            Assert.AreEqual("201", retention.Name);
            Assert.AreEqual(44.82, retention.BedLevel, 1.0e-6);
            Assert.AreEqual(551056, retention.StorageArea, 1.0e-6);

            Assert.AreEqual("102", sobekRetentionsReader.RetentionStructures[retention]);
            Assert.AreEqual("103", sobekRetentionsReader.SecondRetentionStructures[retention]);
            Assert.IsFalse(retention.UseTable);
        }

        [Test]
        public void SobekRERetentionStreetLevelAndStreetStorageArea()
        {
            const string source = @"FLBR id '201' sc 0 dc lt 5 9.9999e+009 9.9999e+009 s2 '103' ar 551056 bl 44.82 ih 44.82 u1 1 ca 0 0 0 0 cj '-1' '-1' '-1' '-1' cb 1 1 1 0 ck '01' '119' '5596' '-1' lt 0 sd '102' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";
            SobekRetentionsReader sobekRetentionsReader = new SobekRetentionsReader {Sobek2Import = false};
            var retention = sobekRetentionsReader.GetRetention(source);
            Assert.AreEqual("201", retention.Name);
            //sobek RE StreetLevel should always be set to 999999
            Assert.AreEqual(999999.0, retention.StreetLevel, 1.0e-6);
            //sobek RE StreetStorageArea should always be set to StorageArea
            Assert.AreEqual(retention.StorageArea, retention.StreetStorageArea, 1.0e-6);
        }

        [Test]
        public void SobekTimeDependend()
        {
            string source = @"NODE id '6' ty 1 ct sw PDIN 1 0 '' pdin TBLE" + Environment.NewLine + 
                            @"3 5 <" + Environment.NewLine +
                            @"4 6 <" + Environment.NewLine +
                            @"5 6.9 <" + Environment.NewLine +
                            @"8 8 <" + Environment.NewLine +
                            @"9 10.01 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"ss 0 ml 999999 node";

            SobekRetentionsReader sobekRetentionsReader = new SobekRetentionsReader { Sobek2Import = true};
            var retention = sobekRetentionsReader.GetRetention(source);
            Assert.AreEqual("6", retention.Name);
            Assert.AreEqual(5.0, (double)retention.Data[3.0], 1.0e-6);
            Assert.AreEqual(6.0, (double)retention.Data[4.0], 1.0e-6);
            Assert.AreEqual(6.9, (double)retention.Data[5.0], 1.0e-6);
            Assert.IsTrue(retention.UseTable);
        }

        [Test]
        public void Sobek212WithMultipleStringId()
        {
            const string source = @"NODE id 'Aap noot mies' ty 1 ws 10000 ss 0 wl 0 ml 999999 node";

            SobekRetentionsReader sobekRetentionsReader = new SobekRetentionsReader {Sobek2Import = true};
            var retention = sobekRetentionsReader.GetRetention(source);
            Assert.AreEqual("Aap noot mies", retention.Name);
        }

        [Test]
        public void Sobek212_TypeConnectionNode_ShouldNotGiveARetention()
        {
            const string source = @"NODE id 'ND-4' ty 0 ni 1 r1 'rch-1' r2 'rch-2' node";

            SobekRetentionsReader sobekRetentionsReader = new SobekRetentionsReader { Sobek2Import = true };
            var retention = sobekRetentionsReader.GetRetention(source);
            Assert.IsNull(retention);
        }

        [Test]
        public void GetRetention_ForReservoirCompartment_ValueMissing_SetsTheFloodableAreaWith500_AndLogsWarning()
        {
            // Setup
            var reader = new SobekRetentionsReader { Sobek2Import = true };
            const string line = "NODE id '6' ty 1 ws 10000 ss wl 22 node";

            Retention retention = null;

            // Call
            void Call() => retention = reader.GetRetention(line);

            // Assert
            IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
            const string expWarning = "Could not read SS of retention definition with id 6 (\"NODE id '6' ty 1 ws 10000 ss wl 22 node\"). Using default value 500.";
            Assert.That(warnings, Does.Contain(expWarning));
            Assert.That(retention.StreetStorageArea, Is.EqualTo(500));
        }

        [Test]
        public void GetRetention_ForReservoirCompartment_ValueProvided_SetsTheFloodableAreaWithProvidedValue()
        {
            // Setup
            var reader = new SobekRetentionsReader { Sobek2Import = true };
            const string line = "NODE id '6' ty 1 ws 10000 ss 1.23 wl 22 node";

            // Call
            Retention retention = reader.GetRetention(line);

            // Assert
            Assert.That(retention.StreetStorageArea, Is.EqualTo(1.23));
        }

        [Test]
        [TestCase("NODE id '6' ty 2 ws 10000 ss wl 22 node")]
        [TestCase("NODE id '6' ty 3 ws 10000 ss wl 22 node")]
        public void GetRetention_ForNonReservoirCompartment_ValueMissing_SetsTheFloodableAreaWith500_AndLogsWarning(string line)
        {
            // Setup
            var reader = new SobekRetentionsReader { Sobek2Import = true };

            Retention retention = null;

            // Call
            void Call() => retention = reader.GetRetention(line);

            // Assert
            IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
            var expWarning = $"Could not read SS of retention definition with id 6 (\"{line}\"). Using default value 0.";
            Assert.That(warnings, Does.Contain(expWarning));
            Assert.That(retention.StreetStorageArea, Is.EqualTo(0));
        }

        [Test]
        [TestCase("NODE id '6' ty 2 ws 10000 ss 1.23 wl 22 node")]
        [TestCase("NODE id '6' ty 3 ws 10000 ss 1.23 wl 22 node")]
        public void GetRetention_ForNonReservoirCompartment_ValueProvided_SetsTheFloodableAreaWithProvidedValue(string line)
        {
            // Setup
            var reader = new SobekRetentionsReader { Sobek2Import = true };

            // Call
            Retention retention = reader.GetRetention(line);

            // Assert
            Assert.That(retention.StreetStorageArea, Is.EqualTo(1.23));
        }
    }
}
