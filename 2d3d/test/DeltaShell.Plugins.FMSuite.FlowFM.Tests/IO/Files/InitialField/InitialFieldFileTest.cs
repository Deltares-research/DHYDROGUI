using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class InitialFieldFileTest
    {
        [Test]
        [TestCase("")]
        [TestCase("data/")]
        public void WhenReadingAndWritingInitialFieldFile_OriginalFileAndWrittenFileContainsSameData(string spatialDataDir)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Arrange
                var initialFieldFile = new InitialFieldFile();
                var modelDefinition = new WaterFlowFMModelDefinition();

                const string fileNameRead = "initialFields_read.ini";
                const string fileNameWrite = "initialFields_write.ini";
                
                string filePathRead = Path.Combine(temp.Path, fileNameRead);
                string filePathWrite = Path.Combine(temp.Path, fileNameWrite);

                temp.CreateDirectory(spatialDataDir);
                temp.CreateFile(fileNameRead, GetInitialFieldFileContent(spatialDataDir));
                temp.CreateFile($"{spatialDataDir}initialwaterlevel.asc", GetArcInfoFileContent());
                temp.CreateFile($"{spatialDataDir}waterlevel_set_value_1.pol", GetPolyFileContent());
                temp.CreateFile($"{spatialDataDir}frictioncoefficient_samples.xyz", GetSamplesFileContent());
                temp.CreateFile($"{spatialDataDir}frictioncoefficient_set_value_1.pol", GetPolyFileContent());

                // Act
                initialFieldFile.Read(filePathRead, filePathRead, modelDefinition);
                initialFieldFile.Write(filePathWrite, filePathWrite, false, modelDefinition);

                string expectedFileContents = File.ReadAllText(filePathRead);
                string actualFileContents = File.ReadAllText(filePathWrite);

                // Assert
                Assert.That(actualFileContents, Is.EqualTo(expectedFileContents));
            }
        }

        private static string GetInitialFieldFileContent(string spatialDataDir = "")
        {
            return $@"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = {spatialDataDir}initialwaterlevel.asc
    dataFileType          = arcinfo             
    interpolationMethod   = triangulation       
    operand               = A                   
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Initial]
    quantity              = waterlevel          
    dataFile              = {spatialDataDir}waterlevel_set_value_1.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 3.0000000e+000      

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = {spatialDataDir}frictioncoefficient_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = +                   
    averagingType         = mean                
    averagingRelSize      = 2.0100000e+000      
    averagingNumMin       = 2                   
    averagingPercentile   = 0.0000000e+000      
    extrapolationMethod   = no                  
    locationType          = 2d                  
    ifrctyp               = 1                   

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = {spatialDataDir}frictioncoefficient_set_value_1.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = X                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 5.0000000e+000      
    ifrctyp               = 1                   

";
        }

        private static string GetArcInfoFileContent()
        {
            return @"
1 2 3 4
5 6 7 8
9 10 11 12
13 14 15 16
";
        }

        private static string GetSamplesFileContent()
        {
            return @"
1 2 3 
4 5 6 
7 8 9
10 11 12
";
        }

        private static string GetPolyFileContent()
        {
            return @"
water_level
3 2
1 2
3 4
5 6";
        }
    }
}