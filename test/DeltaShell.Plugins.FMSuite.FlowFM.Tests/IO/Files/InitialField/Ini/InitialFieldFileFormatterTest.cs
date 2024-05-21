using System.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Ini
{
    [TestFixture]
    public class InitialFieldFileFormatterTest
    {
        private InitialFieldFileFormatter formatter;

        [SetUp]
        public void SetUp()
        {
            formatter = new InitialFieldFileFormatter();
        }

        [Test]
        public void Format_InitialFieldFileDataNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = formatter.Format(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Format_InitialFieldFileDataNullAndStreamNotNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => formatter.Format(null, Stream.Null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Format_InitialFieldFileDataNotNullAndStreamNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => formatter.Format(new InitialFieldFileData(), null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Format_WithEmptyInitialFieldFileData_FormatsCorrectIniData()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WithEmptyInitialCondition_FormatsCorrectIniSection()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddInitialCondition(new InitialFieldData());

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = all                 

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WithEmptyParameter_FormatsCorrectIniSection()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddParameter(new InitialFieldData());

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Parameter]
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = all                 

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WithRequiredValues_FormatsCorrectIniSection()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddInitialCondition(InitialFieldDataBuilder.Start().AddRequiredValues().Build());

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = water_level.xyz     
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = all                 

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_With1DFieldDataFileType_ReturnsCorrectIniSection()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddInitialCondition(InitialFieldDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .Add1DFieldDataFileType()
                                                                        .Build());

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = water_level.xyz     
    dataFileType          = 1dField             

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WithAveragingInterpolation_ReturnsCorrectIniSection()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddInitialCondition(InitialFieldDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .AddAveragingInterpolation()
                                                                        .Build());

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = water_level.xyz     
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = invDist             
    averagingRelSize      = 1.2300000e+000      
    averagingNumMin       = 2                   
    averagingPercentile   = 3.4500000e+000      
    extrapolationMethod   = no                  
    locationType          = all                 

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertInitialCondition_WithPolygonDataFileType_ReturnsCorrectIniSection()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddInitialCondition(InitialFieldDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .AddPolygonDataFileType()
                                                                        .Build());

            // Act
            string ini = formatter.Format(initialFieldFileData);

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = water_level.xyz     
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = all                 
    value                 = 7.0000000e+000      

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_InitialFieldFileDataToStream_FormatsCorrectIniData()
        {
            // Arrange
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddParameter(InitialFieldDataBuilder.Start().AddRequiredValues().Build());
            initialFieldFileData.AddInitialCondition(InitialFieldDataBuilder.Start().AddRequiredValues().Build());

            // Act
            string ini;

            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream))
            {
                formatter.Format(initialFieldFileData, stream);
                stream.Seek(0, SeekOrigin.Begin);
                ini = streamReader.ReadToEnd();
            }

            // Assert
            const string expected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = water_level.xyz     
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = all                 

[Parameter]
    quantity              = waterlevel          
    dataFile              = water_level.xyz     
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = all                 

";

            Assert.That(ini, Is.EqualTo(expected));
        }
    }
}