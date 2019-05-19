using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SobekCompare.Tests
{
    public static class SobekTestBenchHelper
    {
        private const string TestlistTxtFileName = "TestList.txt";
        /// <summary>
        /// Creates a dictionary of int->test directory name. This serves as a ToDo-list. The dictionary contains the tests we would like to run
        /// </summary>
        /// <param name="testCasesDirectory"></param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTestsDictionary(string testCasesDirectory)
        {
            var testListFileName = Path.Combine(testCasesDirectory, TestlistTxtFileName);
            var result = new Dictionary<int, string>();

            //if the file does not exists we don't want to run any test
            if (!File.Exists(testListFileName))
            {
                return result;
            }

            var testListLines = File.ReadAllLines(testListFileName);
            for (var i = 0; i < testListLines.Length; i++)
            {
                result[i+1] = testListLines[i];
            }

            return result;
        }
        
        public static SobekTestInfo GetTestInfo(int testNumber, string testCasesDirectory, IDictionary<int, string> testDictionary)
        {
            if (!testDictionary.ContainsKey(testNumber) || testDictionary[testNumber] == "")
                Assert.Ignore("Skipped, No Test Data");

            //find the lit directory
            var testName = testDictionary[testNumber].Trim();

            var commentIndex = testName.IndexOf("#");
            if (commentIndex == 0)
            {
                testName = testName.Substring(1);
                //Assert.IsTrue(true, "{0} Skipped", testName);
                Assert.Ignore("{0} Skipped", testName);
            }
            else
            {
                string testDirectory;
                if (commentIndex == -1)
                {
                    testDirectory = testDictionary[testNumber].Trim();
                }
                else
                {
                    testDirectory = testDictionary[testNumber].Substring(0, commentIndex).Trim();
                }
                var testDirectoryWithPath = Path.Combine(testCasesDirectory, testDirectory);

                var litDirectory = Directory.GetDirectories(testDirectoryWithPath, "*.lit").FirstOrDefault();
                if (litDirectory == null)
                {
                    Assert.Fail("No Lit-directory found for test '{0}'", testName);
                }

                //find the cmt file path
                var cmtFilePath = Path.Combine(litDirectory, "CaseList.cmt");
                if (!File.Exists(cmtFilePath))
                {
                    Assert.Fail("File CaseList.cmt not found for test {0}", testName);
                }

                //find out the case directory
                var cmtFileLines = File.ReadAllLines(cmtFilePath);
                var caseNumber = cmtFileLines[0].Split(' ')[0];
                var caseDirectory = Path.Combine(litDirectory, caseNumber);

                return new SobekTestInfo { CaseDirectory = caseDirectory, Name = testName, TestDirectory = testDirectory };
            }
            return null;
        }


        // Test to regenerate the code for the tests.
        //[Test]
        //public void WriteTest()
        //{
        //    var stringBuilder = new StringBuilder();
        //    for (var i = 0; i < 9; i++)
        //    {
        //        stringBuilder.AppendLine("[Test]");
        //        stringBuilder.AppendLine(string.Format("public void RunTest00{0}()", i + 1));
        //        stringBuilder.AppendLine("{");
        //        stringBuilder.AppendLine(string.Format("RunTest({0});", i));
        //        stringBuilder.AppendLine("}");
        //    }
        //    for (var i = 9; i < 99; i++)
        //    {
        //        stringBuilder.AppendLine("[Test]");
        //        stringBuilder.AppendLine(string.Format("public void RunTest0{0}()", i + 1));
        //        stringBuilder.AppendLine("{");
        //        stringBuilder.AppendLine(string.Format("RunTest({0});", i));
        //        stringBuilder.AppendLine("}");
        //    }
        //    for (var i = 99; i < 500; i++)
        //    {
        //        stringBuilder.AppendLine("[Test]");
        //        stringBuilder.AppendLine(string.Format("public void RunTest{0}()", i + 1));
        //        stringBuilder.AppendLine("{");
        //        stringBuilder.AppendLine(string.Format("RunTest({0});", i));
        //        stringBuilder.AppendLine("}");
        //    }
        //    var code = stringBuilder.ToString();
        //}
    }
    
    public class SobekTestInfo
    {
        public string Name;
        public string CaseDirectory;
        public string TestDirectory;
    }
}