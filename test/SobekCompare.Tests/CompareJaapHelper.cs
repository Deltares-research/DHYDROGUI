using System;
using System.IO;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;
using SobekCompare.Tests.Helpers;

namespace SobekCompare.Tests
{
    public static class CompareJaapHelper
    {
        private static bool CompareHisVariables(string refPath, string cmpPath, string varName, double tolerance, double toleranceErrorMargin)
        {
            var refHisFileReader = new HisFileReader(refPath);
            var cmpHisFileReader = new HisFileReader(cmpPath);

            var refVarName = refHisFileReader.GetHisFileHeader.Components.FirstOrDefault(c => c.StartsWith(varName));
            if (refVarName == null)
                return false;

            var cmpVarName = cmpHisFileReader.GetHisFileHeader.Components.FirstOrDefault(c => c.StartsWith(varName));
            if (cmpVarName == null)
                return false;

            var refDataList = refHisFileReader.ReadAllData(refVarName);
            var cmpDataList = cmpHisFileReader.ReadAllData(cmpVarName);

            refHisFileReader.Close();
            cmpHisFileReader.Close();

            var diffCount = 0;

            for (var i = 0; i < refDataList.Count; i++)
            {
                var valRef = refDataList[i].Value;
                var valCmp = cmpDataList[i].Value;

                var noDiffFound = Math.Abs(valRef - valCmp) <= toleranceErrorMargin || Math.Abs(valRef - valCmp) <= Math.Abs(valRef) * tolerance;

                if (!noDiffFound)
                {
                    diffCount++;
                    Console.WriteLine("Difference for {0} at {1}: Calculated: {2:0.0000} Reference: {3:0.0000}", varName,
                        refDataList[i].LocationName, valCmp, valRef);

                    if (diffCount >= 25) break;
                }
            }

            return diffCount == 0;

        }

        public static void RunAndCompareSobekAndWaterFlow1D(string pathDirSobek)
        {
            var modelRunnerAndResultComparer = new ModelRunnerAndResultComparer(pathDirSobek);

            double tolerance = 0.01;
            double toleranceErroMargin = 0.01;

            modelRunnerAndResultComparer.RunModels();

            string refCalcpntPath = Path.Combine(pathDirSobek, "calcpnt.his");
            string cmpCalcpntPath = Path.Combine(modelRunnerAndResultComparer.waterFlowModel1D.ExplicitWorkingDirectory, @"output\calcpnt.his");
            string refReachsegPath = Path.Combine(pathDirSobek, "reachseg.his");
            string cmpReachsegPath = Path.Combine(modelRunnerAndResultComparer.waterFlowModel1D.ExplicitWorkingDirectory, @"output\reachseg.his");

            bool checkWaterLevel = CompareHisVariables(refCalcpntPath, cmpCalcpntPath, "Waterlevel", tolerance, toleranceErroMargin);
            bool checkWaterDepth = CompareHisVariables(refCalcpntPath, cmpCalcpntPath, "Waterdepth", tolerance, toleranceErroMargin); ;
            bool checkDischarge = CompareHisVariables(refReachsegPath, cmpReachsegPath, "Discharge", tolerance, toleranceErroMargin);
            bool checkVelocity = CompareHisVariables(refReachsegPath, cmpReachsegPath, "Velocity", tolerance, toleranceErroMargin);

            Assert.That(checkWaterLevel, "Waterlevel check failed.");
            Assert.That(checkWaterDepth, "Waterdepth check failed.");
            Assert.That(checkDischarge, "Discharge check failed.");
            Assert.That(checkVelocity, "Velocity check failed.");
        }
    }
}
