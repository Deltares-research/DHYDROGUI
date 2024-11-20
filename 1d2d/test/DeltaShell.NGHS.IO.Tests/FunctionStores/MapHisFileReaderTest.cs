using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FunctionStores;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FunctionStores
{
    [TestFixture]
    public class MapHisFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMapFileReaderReadMetaData()
        {
            var mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var mapFileMetaData = MapHisFileReader.ReadMetaData(mapFilePath);

            Assert.AreEqual(25, mapFileMetaData.NumberOfTimeSteps);
            Assert.AreEqual(2, mapFileMetaData.NumberOfLocations);
            Assert.AreEqual(6, mapFileMetaData.NumberOfParameters);
            Assert.AreEqual(288, mapFileMetaData.DataBlockOffsetInBytes);

            Assert.AreEqual("Salinity", mapFileMetaData.Parameters[0]);
            Assert.AreEqual("Temperature", mapFileMetaData.Parameters[1]);
            Assert.AreEqual("OXY", mapFileMetaData.Parameters[2]);
            Assert.AreEqual("AAP", mapFileMetaData.Parameters[3]);
            Assert.AreEqual("SOD", mapFileMetaData.Parameters[4]);
            Assert.AreEqual("IM1S1", mapFileMetaData.Parameters[5]);

            Assert.AreEqual(mapFileMetaData.NumberOfTimeSteps, mapFileMetaData.Times.Count);
            Assert.IsTrue(mapFileMetaData.Times[0].CompareTo(new DateTime(2010, 1, 1, 0, 0, 0)) == 0);
            Assert.IsTrue(mapFileMetaData.Times[1].CompareTo(new DateTime(2010, 1, 1, 1, 0, 0)) == 0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMapFileReaderReadTimeStep()
        {
            var mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var mapFileMetaData = MapHisFileReader.ReadMetaData(mapFilePath);
            var valuesSalinity = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "Salinity");

            Assert.AreEqual(2, valuesSalinity.Count);
            Assert.AreEqual(19.767536163330078, valuesSalinity[0]);
            Assert.AreEqual(27.733558654785156, valuesSalinity[1]);

            var valuesOxy = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "OXY");

            Assert.AreEqual(2, valuesOxy.Count);
            Assert.AreEqual(1.97675359249115, valuesOxy[0]);
            Assert.AreEqual(2.7733557224273682, valuesOxy[1]);

            // Check the substance values for the last time step for two random substances
            valuesSalinity = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "Salinity");

            Assert.AreEqual(2, valuesSalinity.Count);
            Assert.AreEqual(5.6551790237426758, valuesSalinity[0]);
            Assert.AreEqual(14.770989418029785, valuesSalinity[1]);

            valuesOxy = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "OXY");

            Assert.AreEqual(2, valuesOxy.Count);
            Assert.AreEqual(0.56551802158355713, valuesOxy[0]);
            Assert.AreEqual(1.4770994186401367, valuesOxy[1]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMapFileReaderReadTimeStepForOneSegment()
        {
            var mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var mapFileMetaData = MapHisFileReader.ReadMetaData(mapFilePath);
            var valuesSalinity = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "Salinity", 1);

            Assert.AreEqual(1, valuesSalinity.Count);
            Assert.AreEqual(27.733558654785156, valuesSalinity[0]);

            var valuesOxy = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "OXY", 1);

            Assert.AreEqual(1, valuesOxy.Count);
            Assert.AreEqual(2.7733557224273682, valuesOxy[0]);

            // Check the substance values for the last time step for two random substances
            valuesSalinity = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "Salinity", 0);

            Assert.AreEqual(1, valuesSalinity.Count);
            Assert.AreEqual(5.6551790237426758, valuesSalinity[0]);

            valuesOxy = MapHisFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "OXY", 0);

            Assert.AreEqual(1, valuesOxy.Count);
            Assert.AreEqual(0.56551802158355713, valuesOxy[0]);
        }

        [Test]
        public void ReadHisFileMetaData()
        {
            var path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "flowhis.his");
            var metaData = MapHisFileReader.ReadMetaData(path);

            Assert.AreEqual(3, metaData.NumberOfParameters);
            Assert.AreEqual(6, metaData.NumberOfLocations);
            Assert.AreEqual(145, metaData.NumberOfTimeSteps);
        }

        [Test]
        public void ReadHisFile()
        {
            #region refValues

            var refValues = new[]{  0.93588440,
                                     0.93588840,
                                     0.93590270,
                                     0.93597170,
                                     0.93625090,
                                     0.93700560,
                                     0.93843570,
                                     0.94044270,
                                     0.94273480,
                                     0.94524370,
                                     0.94821810,
                                     0.95191430,
                                     0.95643230,
                                     0.96168300,
                                     0.96739750,
                                     0.97347340,
                                     0.98013820,
                                     0.98756110,
                                     0.99573940,
                                     1.00461300,
                                     1.01399500,
                                     1.02379600,
                                     1.03411800,
                                     1.04492400,
                                     1.05613500,
                                     1.06780700,
                                     1.07990900,
                                     1.09231400,
                                     1.10480900,
                                     1.11707100,
                                     1.12901900,
                                     1.14098900,
                                     1.15322200,
                                     1.16540500,
                                     1.17684700,
                                     1.18698800,
                                     1.19587800,
                                     1.20427900,
                                     1.21269200,
                                     1.22026100,
                                     1.22534400,
                                     1.22680700,
                                     1.22504900,
                                     1.22207100,
                                     1.21921700,
                                     1.21569100,
                                     1.21066600,
                                     1.20438000,
                                     1.19694300,
                                     1.18870500,
                                     1.18038100,
                                     1.17184700,
                                     1.16270000,
                                     1.15307700,
                                     1.14304300,
                                     1.13270600,
                                     1.12239700,
                                     1.11213400,
                                     1.10182100,
                                     1.09158300,
                                     1.08146700,
                                     1.07144300,
                                     1.06156200,
                                     1.05187700,
                                     1.04245900,
                                     1.03343700,
                                     1.02487100,
                                     1.01666700,
                                     1.00869700,
                                     1.00097600,
                                     0.99366500,
                                     0.98693620,
                                     0.98086120,
                                     0.97533530,
                                     0.97013700,
                                     0.96525320,
                                     0.96102430,
                                     0.95791110,
                                     0.95619650,
                                     0.95570750,
                                     0.95589230,
                                     0.95651610,
                                     0.95789060,
                                     0.96024930,
                                     0.96357260,
                                     0.96778520,
                                     0.97263160,
                                     0.97794650,
                                     0.98395250,
                                     0.99081740,
                                     0.99850630,
                                     1.00696200,
                                     1.01598900,
                                     1.02548200,
                                     1.03554400,
                                     1.04612800,
                                     1.05714900,
                                     1.06866100,
                                     1.08062500,
                                     1.09291500,
                                     1.10531400,
                                     1.11749200,
                                     1.12937300,
                                     1.14128500,
                                     1.15346900,
                                     1.16561100,
                                     1.17701800,
                                     1.18713200,
                                     1.19599900,
                                     1.20438000,
                                     1.21277600,
                                     1.22033200,
                                     1.22540300,
                                     1.22685800,
                                     1.22509200,
                                     1.22210800,
                                     1.21924800,
                                     1.21571800,
                                     1.21068900,
                                     1.20440000,
                                     1.19696000,
                                     1.18871900,
                                     1.18039300,
                                     1.17185700,
                                     1.16271000,
                                     1.15308500,
                                     1.14305000,
                                     1.13271200,
                                     1.12240200,
                                     1.11213900,
                                     1.10182500,
                                     1.09158600,
                                     1.08147000,
                                     1.07144600,
                                     1.06156400,
                                     1.05187900,
                                     1.04246100,
                                     1.03343900,
                                     1.02487200,
                                     1.01666800,
                                     1.00869800,
                                     1.00097700,
                                     0.99366570,
                                     0.98693660,
                                     0.98086170 };


            #endregion

            var path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "flowhis.his");
            var mapHisFileMetaData = MapHisFileReader.ReadMetaData(path);

            var hisValues = MapHisFileReader.GetTimeSeriesData(path, mapHisFileMetaData, "Water level", mapHisFileMetaData.Locations.IndexOf("branch1_9000.00"));
            Assert.AreEqual(refValues.Length, hisValues.Count);

            for (int i = 0; i < refValues.Length; i++)
            {
                Assert.AreEqual(refValues[i], hisValues[i], 0.00001);
            }
        }

        [Test]
        public void ReadOneTimeStep()
        {
            //1/1/1995	0:50:00
            var refValues = new[]
                                {
                                    0.93700560,
                                    0.59889330,
                                    0.39594050,
                                    2.57769800,
                                    -0.41666680,
                                    0.10005170
                                };

            var path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "flowhis.his");
            var mapHisFileMetaData = MapHisFileReader.ReadMetaData(path);

            var lstTimeStepValues = MapHisFileReader.GetTimeStepData(path, mapHisFileMetaData, 5, "Water level");
            
            Assert.AreEqual(refValues.Length, lstTimeStepValues.Count);

            for (int i = 0; i < refValues.Length; i++)
            {
                Assert.AreEqual(refValues[i], lstTimeStepValues[i], 0.00001);
            }

        }

        [Test]
        public void ReadOneLocation()
        {
            // Parameter     1: Water level         
            //Feature     5: branch2_10000.00    : Column     5

            #region refValues

            var refValues = new[]{  -0.5000000  ,
                                    -0.4833335  ,
                                    -0.4666668  ,
                                    -0.4500001  ,
                                    -0.4333335  ,
                                    -0.4166668  ,
                                    -0.4000001  ,
                                    -0.3700002  ,
                                    -0.3400002  ,
                                    -0.3100000  ,
                                    -0.2800000  ,
                                    -0.2500000  ,
                                    -0.2200000  ,
                                    -0.1833334  ,
                                    -0.1466667  ,
                                    -0.1100000  ,
                                    -0.0733334  ,
                                    -0.0366667  ,
                                    0.0000000   ,
                                    0.0366667   ,
                                    0.0733333   ,
                                    0.1100000   ,
                                    0.1466666   ,
                                    0.1833333   ,
                                    0.2200000   ,
                                    0.2500000   ,
                                    0.2800000   ,
                                    0.3100000   ,
                                    0.3400000   ,
                                    0.3700000   ,
                                    0.4000000   ,
                                    0.4166667   ,
                                    0.4333332   ,
                                    0.4499999   ,
                                    0.4666665   ,
                                    0.4833332   ,
                                    0.4999999   ,
                                    0.4833335   ,
                                    0.4666668   ,
                                    0.4500001   ,
                                    0.4333335   ,
                                    0.4166668   ,
                                    0.4000001   ,
                                    0.3700002   ,
                                    0.3400000   ,
                                    0.3100000   ,
                                    0.2800000   ,
                                    0.2500000   ,
                                    0.2200000   ,
                                    0.1833334   ,
                                    0.1466667   ,
                                    0.1100000   ,
                                    0.0733334   ,
                                    0.0366667   ,
                                    0.0000000   ,
                                    -0.0366667  ,
                                    -0.0733333  ,
                                    -0.1100000  ,
                                    -0.1466666  ,
                                    -0.1833333  ,
                                    -0.2200000  ,
                                    -0.2500000  ,
                                    -0.2800000  ,
                                    -0.3100000  ,
                                    -0.3400000  ,
                                    -0.3700000  ,
                                    -0.4000000  ,
                                    -0.4166667  ,
                                    -0.4333332  ,
                                    -0.4499999  ,
                                    -0.4666665  ,
                                    -0.4833332  ,
                                    -0.4999999  ,
                                    -0.4833335  ,
                                    -0.4666668  ,
                                    -0.4500001  ,
                                    -0.4333335  ,
                                    -0.4166668  ,
                                    -0.4000001  ,
                                    -0.3700002  ,
                                    -0.3400002  ,
                                    -0.3100002  ,
                                    -0.2800000  ,
                                    -0.2500000  ,
                                    -0.2200000  ,
                                    -0.1833334  ,
                                    -0.1466667  ,
                                    -0.1100000  ,
                                    -0.0733334  ,
                                    -0.0366667  ,
                                    0.0000000   ,
                                    0.0366667   ,
                                    0.0733333   ,
                                    0.1100000   ,
                                    0.1466666   ,
                                    0.1833333   ,
                                    0.2200000   ,
                                    0.2500000   ,
                                    0.2800000   ,
                                    0.3100000   ,
                                    0.3400000   ,
                                    0.3700000   ,
                                    0.4000000   ,
                                    0.4166667   ,
                                    0.4333332   ,
                                    0.4499999   ,
                                    0.4666665   ,
                                    0.4833332   ,
                                    0.4999999   ,
                                    0.4833335   ,
                                    0.4666668   ,
                                    0.4500001   ,
                                    0.4333335   ,
                                    0.4166668   ,
                                    0.4000001   ,
                                    0.3700002   ,
                                    0.3400000   ,
                                    0.3100000   ,
                                    0.2800000   ,
                                    0.2500000   ,
                                    0.2200000   ,
                                    0.1833334   ,
                                    0.1466667   ,
                                    0.1100000   ,
                                    0.0733334   ,
                                    0.0366667   ,
                                    0.0000000   ,
                                    -0.0366667  ,
                                    -0.0733333  ,
                                    -0.1100000  ,
                                    -0.1466666  ,
                                    -0.1833333  ,
                                    -0.2200000  ,
                                    -0.2500000  ,
                                    -0.2800000  ,
                                    -0.3100000  ,
                                    -0.3400000  ,
                                    -0.3700000  ,
                                    -0.4000000  ,
                                    -0.4166667  ,
                                    -0.4333332  ,
                                    -0.4499999  ,
                                    -0.4666665  ,
                                    -0.4833332  ,
                                    -0.4999999
                                    };

            #endregion

            var path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "flowhis.his");
            var mapHisFileMetaData = MapHisFileReader.ReadMetaData(path);

            var lstLocationValues = MapHisFileReader.GetTimeSeriesData(path,mapHisFileMetaData, "Water level", 4);
            
            Assert.AreEqual(refValues.Length, lstLocationValues.Count);

            for (int i = 0; i < refValues.Length; i++)
            {
                Assert.AreEqual(refValues[i], lstLocationValues[i], 0.00001);
            }
        }

        [Test]
        public void ReadOneLocation2ndCheck()
        {
            //  Parameter     2: Total Area          
            //  Feature     6: branch3_5000.00     : Column    12

            #region refValues

            var refValues = new[]{  92.821880   ,
                                    92.821730   ,
                                    92.821720   ,
                                    92.821740   ,
                                    92.821900   ,
                                    92.822500   ,
                                    92.824080   ,
                                    92.827160   ,
                                    92.831770   ,
                                    92.837570   ,
                                    92.848800   ,
                                    92.861960   ,
                                    92.878120   ,
                                    92.897930   ,
                                    92.921200   ,
                                    92.947040   ,
                                    92.974630   ,
                                    93.004290   ,
                                    93.036980   ,
                                    93.073020   ,
                                    93.112050   ,
                                    93.153550   ,
                                    93.196970   ,
                                    93.242250   ,
                                    93.289570   ,
                                    93.338710   ,
                                    93.389460   ,
                                    93.441930   ,
                                    93.495740   ,
                                    93.549960   ,
                                    93.603570   ,
                                    93.655880   ,
                                    93.707420   ,
                                    93.759330   ,
                                    93.811230   ,
                                    93.860990   ,
                                    93.906230   ,
                                    93.946130   ,
                                    93.982430   ,
                                    94.017400   ,
                                    94.049760   ,
                                    94.074130   ,
                                    94.085140   ,
                                    94.082070   ,
                                    94.069970   ,
                                    94.055180   ,
                                    94.038830   ,
                                    94.017690   ,
                                    93.990580   ,
                                    93.958750   ,
                                    93.923160   ,
                                    93.885570   ,
                                    93.847400   ,
                                    93.807570   ,
                                    93.765270   ,
                                    93.721240   ,
                                    93.675930   ,
                                    93.629970   ,
                                    93.584280   ,
                                    93.538730   ,
                                    93.493190   ,
                                    93.448180   ,
                                    93.403750   ,
                                    93.359810   ,
                                    93.316700   ,
                                    93.274730   ,
                                    93.234310   ,
                                    93.195840   ,
                                    93.159240   ,
                                    93.124020   ,
                                    93.089960   ,
                                    93.057430   ,
                                    93.027210   ,
                                    92.999870   ,
                                    92.975220   ,
                                    92.952480   ,
                                    92.931270   ,
                                    92.912510   ,
                                    92.898170   ,
                                    92.889870   ,
                                    92.887520   ,
                                    92.889310   ,
                                    92.893490   ,
                                    92.900270   ,
                                    92.911090   ,
                                    92.926320   ,
                                    92.945430   ,
                                    92.967700   ,
                                    92.992320   ,
                                    93.019360   ,
                                    93.049790   ,
                                    93.083930   ,
                                    93.121300   ,
                                    93.161380   ,
                                    93.203600   ,
                                    93.247840   ,
                                    93.294300   ,
                                    93.342680   ,
                                    93.392790   ,
                                    93.444740   ,
                                    93.498090   ,
                                    93.551930   ,
                                    93.605220   ,
                                    93.657260   ,
                                    93.708580   ,
                                    93.760300   ,
                                    93.812030   ,
                                    93.861660   ,
                                    93.906790   ,
                                    93.946590   ,
                                    93.982830   ,
                                    94.017720   ,
                                    94.050030   ,
                                    94.074370   ,
                                    94.085340   ,
                                    94.082240   ,
                                    94.070110   ,
                                    94.055310   ,
                                    94.038940   ,
                                    94.017780   ,
                                    93.990660   ,
                                    93.958820   ,
                                    93.923210   ,
                                    93.885620   ,
                                    93.847440   ,
                                    93.807610   ,
                                    93.765300   ,
                                    93.721260   ,
                                    93.675950   ,
                                    93.630000   ,
                                    93.584300   ,
                                    93.538740   ,
                                    93.493200   ,
                                    93.448200   ,
                                    93.403760   ,
                                    93.359820   ,
                                    93.316700   ,
                                    93.274740   ,
                                    93.234310   ,
                                    93.195850   ,
                                    93.159240   ,
                                    93.124020   ,
                                    93.089960   ,
                                    93.057440   ,
                                    93.027210
                                    };


            #endregion

            var path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "flowhis.his");
            var mapHisFileMetaData = MapHisFileReader.ReadMetaData(path);

            var lstLocationValues = MapHisFileReader.GetTimeSeriesData(path, mapHisFileMetaData, "Total Area", 5);

            Assert.AreEqual(refValues.Length, lstLocationValues.Count);

            for (int i = 0; i < refValues.Length; i++)
            {
                Assert.AreEqual(refValues[i], lstLocationValues[i], 0.00001);
            }
        }
    }
}
