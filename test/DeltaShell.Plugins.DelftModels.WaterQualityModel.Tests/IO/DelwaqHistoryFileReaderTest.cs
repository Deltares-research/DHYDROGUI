using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DelwaqHistoryFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestHisFileReaderRead()
        {
            string hisFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.his");
            DelwaqHisFileData[] delwaqBinaryFileVariableDataList = DelwaqHistoryFileReader.Read(hisFilePath);

            Assert.AreEqual(4, delwaqBinaryFileVariableDataList.Length);
            Assert.AreEqual("O1", delwaqBinaryFileVariableDataList[0].ObservationVariable);
            Assert.AreEqual("O2", delwaqBinaryFileVariableDataList[1].ObservationVariable);
            Assert.AreEqual("O3", delwaqBinaryFileVariableDataList[2].ObservationVariable);
            Assert.AreEqual("ALL SEGMENTS", delwaqBinaryFileVariableDataList[3].ObservationVariable);

            string[] substances = delwaqBinaryFileVariableDataList[0].OutputVariables;
            Assert.AreEqual(5, substances.Count());
            Assert.AreEqual("cTR1", substances[0]);
            Assert.AreEqual("cTR2", substances[1]);
            Assert.AreEqual("cTR3", substances[2]);
            Assert.AreEqual("cTR4", substances[3]);
            Assert.AreEqual("Continuity", substances[4]);

            // Check the timestep data for two random substances
            IEnumerable<DateTime> timeStepsO1 = delwaqBinaryFileVariableDataList[0].TimeSteps;
            IEnumerable<DateTime> timeStepsO3 = delwaqBinaryFileVariableDataList[2].TimeSteps;
            Assert.AreEqual(865, timeStepsO1.Count());
            Assert.AreEqual(new DateTime(2010, 1, 1, 0, 1, 30), timeStepsO1.ElementAt(0));
            Assert.AreEqual(865, timeStepsO3.Count());
            Assert.AreEqual(new DateTime(2010, 1, 1, 1, 0, 0), timeStepsO1.ElementAt(6));
            Assert.AreEqual(new DateTime(2010, 1, 1, 4, 10, 0), timeStepsO1.ElementAt(25));

            // Check the values of the first time step for two random observation variables
            List<double> valuesO1 = delwaqBinaryFileVariableDataList[0].GetValuesForTimeStep(timeStepsO1.ElementAt(0));
            List<double> valuesO3 = delwaqBinaryFileVariableDataList[2].GetValuesForTimeStep(timeStepsO3.ElementAt(0));
            Assert.AreEqual(5, valuesO1.Count);
            Assert.AreEqual(20.0, valuesO1[0]);
            Assert.AreEqual(0.0, valuesO1[1]);
            Assert.AreEqual(0.0, valuesO1[2]);
            Assert.AreEqual(0.0, valuesO1[3]);
            Assert.AreEqual(1.0, valuesO1[4]);
            Assert.AreEqual(5, valuesO3.Count);
            Assert.AreEqual(1.0, valuesO3[0]);
            Assert.AreEqual(0.0, valuesO3[1]);
            Assert.AreEqual(0.0, valuesO3[2]);
            Assert.AreEqual(0.0, valuesO3[3]);
            Assert.AreEqual(1.0, valuesO3[4]);

            // Check the values of the twentieth time step for two random observation variables
            valuesO1 = delwaqBinaryFileVariableDataList[0].GetValuesForTimeStep(timeStepsO1.ElementAt(19));
            valuesO3 = delwaqBinaryFileVariableDataList[2].GetValuesForTimeStep(timeStepsO3.ElementAt(19));
            Assert.AreEqual(5, valuesO1.Count);
            Assert.AreEqual(19.998634338378906, valuesO1[0]);
            Assert.AreEqual(1.4857294561299028E-10, valuesO1[1]);
            Assert.AreEqual(0.0, valuesO1[2]);
            Assert.AreEqual(2.9704591396225852E-18, valuesO1[3]);
            Assert.AreEqual(0.99993157386779785, valuesO1[4]);
            Assert.AreEqual(5, valuesO3.Count);
            Assert.AreEqual(0.99979805946350098, valuesO3[0]);
            Assert.AreEqual(3.1684836967259424E-18, valuesO3[1]);
            Assert.AreEqual(0.00013405717618297786, valuesO3[2]);
            Assert.AreEqual(6.4399062983713525E-18, valuesO3[3]);
            Assert.AreEqual(0.99993199110031128, valuesO3[4]);

            // Check the values of the last time step for two random observation variables
            valuesO1 = delwaqBinaryFileVariableDataList[0].GetValuesForTimeStep(timeStepsO1.ElementAt(864));
            valuesO3 = delwaqBinaryFileVariableDataList[2].GetValuesForTimeStep(timeStepsO3.ElementAt(864));
            Assert.AreEqual(5, valuesO1.Count);
            Assert.AreEqual(0.31385809183120728, valuesO1[0]);
            Assert.AreEqual(0.9842914342880249, valuesO1[1]);
            Assert.AreEqual(6.0544948922781926E-17, valuesO1[2]);
            Assert.AreEqual(2.7054664123582106E-17, valuesO1[3]);
            Assert.AreEqual(0.99998384714126587, valuesO1[4]);
            Assert.AreEqual(5, valuesO3.Count);
            Assert.AreEqual(4.5415983200073242, valuesO3[0]);
            Assert.AreEqual(0.50417757034301758, valuesO3[1]);
            Assert.AreEqual(0.10231129080057144, valuesO3[2]);
            Assert.AreEqual(0.068604849278926849, valuesO3[3]);
            Assert.AreEqual(0.99997609853744507, valuesO3[4]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestHisFileReaderReadWithNonExistingFile()
        {
            DelwaqHisFileData[] delwaqBinaryFileVariableDataList = DelwaqHistoryFileReader.Read("NonExisting.his");

            Assert.AreEqual(0, delwaqBinaryFileVariableDataList.Length);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestHisFileReaderReadWithEmptyFile()
        {
            string hisFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "EmptyHisFile.his");

            var fileStream = new FileStream(hisFilePath, FileMode.OpenOrCreate);
            fileStream.Close();

            DelwaqHisFileData[] delwaqBinaryFileVariableDataList = DelwaqHistoryFileReader.Read(hisFilePath);

            Assert.AreEqual(0, delwaqBinaryFileVariableDataList.Length);

            File.Delete(hisFilePath);
        }
    }
}