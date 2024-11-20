using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.InitialField
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class InitialFieldFileTest
    {
        [Test]
        public void WhenReadingAndWritingInitialFieldFile_OriginalFileAndWrittenFileContainsSameData2(
            [Values] InitialConditionQuantity globalQuantity)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Arrange
                var initialFieldFile = new InitialFieldFile();
                var modelDefinition = new WaterFlowFMModelDefinition();

                const string fileNameRead = @"computations\initialFields_read.ini";
                const string fileNameWrite = @"computations\initialFields_write.ini";
                
                string filePathRead = Path.Combine(temp.Path, fileNameRead);
                string filePathWrite = Path.Combine(temp.Path, fileNameWrite);

                string globalQuantityName = globalQuantity.ToString();
                SetGlobalQuantity(modelDefinition, globalQuantity);
                
                temp.CreateDirectory("data");
                temp.CreateDirectory("computations");
                temp.CreateFile(fileNameRead, GetInitialFieldFileContent(globalQuantityName));
                temp.CreateFile(@"data\bedlevel_samples.xyz", GetSamplesFileContent());
                temp.CreateFile($@"data\initial_{globalQuantityName}.asc", GetArcInfoFileContent());
                temp.CreateFile($@"data\{globalQuantityName}_set_value_1.pol", GetPolyFileContent());
                temp.CreateFile(@"data\infiltrationcapacity.xyz", GetSamplesFileContent());
                temp.CreateFile(@"data\frictioncoefficient_samples.xyz", GetSamplesFileContent());
                temp.CreateFile(@"data\frictioncoefficient_set_value_1.pol", GetPolyFileContent());

                // Act
                initialFieldFile.Read(filePathRead, filePathRead, modelDefinition);
                initialFieldFile.Write(filePathWrite, filePathWrite, false, modelDefinition);

                string expectedFileContents = File.ReadAllText(filePathRead);
                string actualFileContents = File.ReadAllText(filePathWrite);

                // Assert
                Assert.That(actualFileContents, Is.EqualTo(expectedFileContents).IgnoreCase);
            }
        }

        private static void SetGlobalQuantity(WaterFlowFMModelDefinition modelDefinition, InitialConditionQuantity globalQuantity) 
            => modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int) globalQuantity).ToString());

        private static string GetInitialFieldFileContent(string globalQuantityName)
        {
            return $@"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = {globalQuantityName}          
    dataFile              = Initial{globalQuantityName}.ini
    dataFileType          = 1dField             

[Initial]
    quantity              = bedlevel            
    dataFile              = ../data/bedlevel_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0000000e+000      
    averagingNumMin       = 1                   
    averagingPercentile   = 0.0000000e+000      
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Initial]
    quantity              = {globalQuantityName}          
    dataFile              = ../data/initial_{globalQuantityName}.asc
    dataFileType          = arcinfo             
    interpolationMethod   = triangulation       
    operand               = A                   
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Initial]
    quantity              = {globalQuantityName}          
    dataFile              = ../data/{globalQuantityName}_set_value_1.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 3.0000000e+000      

[Initial]
    quantity              = InfiltrationCapacity
    dataFile              = ../data/infiltrationcapacity.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = mean                
    averagingRelSize      = 1.0100000e+000      
    averagingNumMin       = 1                   
    averagingPercentile   = 0.0000000e+000      
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = ../data/frictioncoefficient_samples.xyz
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
    dataFile              = ../data/frictioncoefficient_set_value_1.pol
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