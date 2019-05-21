using System;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekWindReaderTest
    {
        [Test]
        public void ParseGlobalWindConstant()
        {
            string source =
                @"GLMT MTEO nm '(null)' ss 0 id 0 ci -1 lc 9.9999e+009 wu 1" + Environment.NewLine +
                @"wv tv 0 11 9.9999e+009 wd td 0 241.11 9.9999e+009 su 0 sh ts" + Environment.NewLine +
                @"0 9.9999e+009 9.9999e+009 tu 0 tp tw 0 9.9999e+009 9.9999e+009 au 0 at ta 0" + Environment.NewLine +
                @"9.9999e+009 9.9999e+009 mteo glmt";

            var windjes = new SobekWindReader().Parse(source);
            Assert.AreEqual(1, windjes.Count());
            SobekWind sobekWind = windjes.FirstOrDefault();
            Assert.IsTrue(sobekWind.IsGlobal);
            Assert.AreEqual("0", sobekWind.Id);
            Assert.AreEqual("(null)", sobekWind.Name);
            Assert.AreEqual("-1", sobekWind.BranchId);
            Assert.IsTrue(sobekWind.Used);

            Assert.IsTrue(sobekWind.IsConstantDirection);
            Assert.AreEqual(241.11, sobekWind.ConstantDirection, 1.0e-6);
            Assert.IsTrue(sobekWind.IsConstantVelocity);
            Assert.AreEqual(11, sobekWind.ConstantVelocity, 1.0e-6);
        }


        [Test]
        public void ParseGlobalWindTimeseries()
        {
            string source = @"GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1" + Environment.NewLine +
                @"wv tv 1 98 9.9999e+009 'Wind Velocity' PDIN 0 0 '' pdin" + Environment.NewLine +
                @"CLTT 'Time' 'Velocity' cltt CLID '(null)' '(null)' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1951/01/01;00:00:00' 1 <" + Environment.NewLine +
                @"'1951/01/02;15:00:00' 4 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"wd td 1 100 9.9999e+009 'Wind Direction' PDIN 0 0 '' pdin" + Environment.NewLine +
                @"CLTT 'Time' 'Direction' cltt CLID '(null)' '(null)' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1951/01/01;00:00:00' 12 <" + Environment.NewLine +
                @"'1951/01/02;15:00:00' 144 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"su 0 sh ts 0 9.9999e+009 9.9999e+009 tu 0 tp tw 0" + Environment.NewLine +
                @"9.9999e+009 9.9999e+009 au 0 at ta 0 9.9999e+009 9.9999e+009 mteo" + Environment.NewLine +
                @"glmt";

            var windjes = new SobekWindReader().Parse(source);
            Assert.AreEqual(1, windjes.Count());
            SobekWind sobekWind = windjes.FirstOrDefault();
            Assert.IsTrue(sobekWind.IsGlobal);
            Assert.AreEqual("0", sobekWind.Id);
            Assert.AreEqual("(null)", sobekWind.Name);
            Assert.AreEqual("-1", sobekWind.BranchId);
            Assert.IsTrue(sobekWind.Used);

            Assert.IsFalse(sobekWind.IsConstantDirection);
            Assert.IsFalse(sobekWind.IsConstantVelocity);

            Assert.AreEqual(2, sobekWind.Wind.Arguments[0].Values.Count);
            Assert.AreEqual(new DateTime(1951, 1, 1, 0, 0, 0), (DateTime)sobekWind.Wind.Arguments[0].Values[0]);
            Assert.AreEqual(new DateTime(1951, 1, 2, 15, 0, 0), (DateTime)sobekWind.Wind.Arguments[0].Values[1]);

            Assert.AreEqual(1, (double)sobekWind.Wind.Components[0].Values[0], 1.0e-6);
            Assert.AreEqual(4, (double)sobekWind.Wind.Components[0].Values[1], 1.0e-6);

            Assert.AreEqual(12, (double)sobekWind.Wind.Components[1].Values[0], 1.0e-6);
            Assert.AreEqual(144, (double)sobekWind.Wind.Components[1].Values[1], 1.0e-6);
        }

        [Test]
        public void MergeWindTimeseriesInterpolate()
        {
            string source = @"GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1" + Environment.NewLine +
                @"wv tv 1 98 9.9999e+009 'Wind Velocity' PDIN 0 0 '' pdin" + Environment.NewLine +
                @"CLTT 'Time' 'Velocity' cltt CLID '(null)' '(null)' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1951/01/01;00:00:00' 1 <" + Environment.NewLine +
                @"'1951/01/02;00:00:00' 4 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"wd td 1 100 9.9999e+009 'Wind Direction' PDIN 0 0 '' pdin" + Environment.NewLine +
                @"CLTT 'Time' 'Direction' cltt CLID '(null)' '(null)' clid" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1951/01/01;00:00:00' 12 <" + Environment.NewLine +
                @"'1951/01/04;00:00:00' 144 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"su 0 sh ts 0 9.9999e+009 9.9999e+009 tu 0 tp tw 0" + Environment.NewLine +
                @"9.9999e+009 9.9999e+009 au 0 at ta 0 9.9999e+009 9.9999e+009 mteo" + Environment.NewLine +
                @"glmt";

            var windjes = new SobekWindReader().Parse(source);
            Assert.AreEqual(1, windjes.Count());
            SobekWind sobekWind = windjes.FirstOrDefault();
            Assert.IsTrue(sobekWind.IsGlobal);
            Assert.AreEqual(3, sobekWind.Wind.Arguments[0].Values.Count);

            Assert.AreEqual(new DateTime(1951, 1, 1, 0, 0, 0), (DateTime)sobekWind.Wind.Arguments[0].Values[0]);
            Assert.AreEqual(new DateTime(1951, 1, 2, 0, 0, 0), (DateTime)sobekWind.Wind.Arguments[0].Values[1]);
            Assert.AreEqual(new DateTime(1951, 1, 4, 0, 0, 0), (DateTime)sobekWind.Wind.Arguments[0].Values[2]);

            Assert.AreEqual(1, (double)sobekWind.Wind.Components[0].Values[0], 1.0e-6);
            Assert.AreEqual(4, (double)sobekWind.Wind.Components[0].Values[1], 1.0e-6);
            Assert.AreEqual(4, (double)sobekWind.Wind.Components[0].Values[2], 1.0e-6);

            Assert.AreEqual(12, (double)sobekWind.Wind.Components[1].Values[0], 1.0e-6);
            Assert.AreEqual(12 + 1*(144 - 12)/3, (double)sobekWind.Wind.Components[1].Values[1], 1.0e-6);
            Assert.AreEqual(144, (double)sobekWind.Wind.Components[1].Values[2], 1.0e-6);
        }

    }
}
