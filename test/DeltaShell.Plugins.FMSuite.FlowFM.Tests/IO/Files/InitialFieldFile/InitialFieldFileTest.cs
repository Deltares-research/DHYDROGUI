using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class InitialFieldFileTest
    {
        [Test]
        public void WhenReadingAndWritingInitialFieldFile_OriginalFileAndWrittenFileContainSameData()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var fileSystem = new FileSystem();
                var reader = new InitialFieldFileReader(fileSystem);
                var writer = new InitialFieldFileWriter(fileSystem, Substitute.For<ISpatialDataFileWriter>());

                var modelDefinition = new WaterFlowFMModelDefinition();

                const string fileNameRead = "initialFields_read.ini";
                const string fileNameWrite = "initialFields_write.ini";

                AddFiles(temp, fileNameRead);

                string filePathRead = fileSystem.Path.Combine(temp.Path, fileNameRead);
                string filePathWrite = fileSystem.Path.Combine(temp.Path, fileNameWrite);

                // Calls
                reader.Read(filePathRead, filePathRead, modelDefinition);
                writer.Write(filePathWrite, modelDefinition);

                // Assert
                Assert.That(IsEqualInitialFieldFileData(fileSystem, filePathRead, filePathWrite));
            }
        }

        private void AddFiles(TemporaryDirectory temp, string initialFieldFileName)
        {
            temp.CreateFile(initialFieldFileName, GetInitialFieldFileContent());
            temp.CreateFile("initialwaterlevel.asc", GetArcInfoFileContent());
            temp.CreateFile("waterlevel_initialwaterlevel_Set_value_1.pol", GetPolyFileContent());
            temp.CreateFile("frictioncoefficient_samples.xyz", GetSamplesFileContent());
            temp.CreateFile("frictioncoefficient_frictioncoefficient_Set_value_1.pol", GetPolyFileContent());
        }

        private static bool IsEqualInitialFieldFileData(IFileSystem fileSystem, string fileName1, string fileName2)
        {
            string fileContent1 = fileSystem.File.ReadAllText(fileName1);
            string fileContent2 = fileSystem.File.ReadAllText(fileName2);

            var iniParser = new IniParser();
            IniData iniData1 = iniParser.Parse(fileContent1);
            IniData iniData2 = iniParser.Parse(fileContent2);

            InitialFieldFileParser initialFieldFileParser = GetInitialFieldFileParser();
            InitialFieldFileData initialFieldFileData1 = initialFieldFileParser.Parse(iniData1);
            InitialFieldFileData initialFieldFileData2 = initialFieldFileParser.Parse(iniData2);

            return new InitialFieldFileDataEqualityComparer().Equals(initialFieldFileData1, initialFieldFileData2);
        }

        private static InitialFieldFileParser GetInitialFieldFileParser()
        {
            var logHandler = Substitute.For<ILogHandler>();
            var initialFieldValidator = new InitialFieldValidator(logHandler, Substitute.For<IValidator<InitialField>>());
            var initialFieldFileParser = new InitialFieldFileParser(logHandler, initialFieldValidator);
            return initialFieldFileParser;
        }

        private static string GetInitialFieldFileContent()
        {
            return @"
[General]
    fileVersion           = 2.00                
    fileType              = initialField                                                         

[Initial]
    quantity              = waterlevel          
    dataFile              = initialwaterlevel.asc   
    dataFileType          = arcinfo              
    interpolationMethod   = triangulation           
    operand               = A                        

[Initial]
    quantity              = waterlevel          
    dataFile              = waterlevel_initialwaterlevel_Set_value_1.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                                
    locationType          = 2d                  
    value                 = 3.0000000e+000      

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = frictioncoefficient_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = +                   
    averagingType         = mean           
    averagingRelSize      = 2.01    
    averagingNumMin       = 2                   
    locationType          = 2d                  

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = frictioncoefficient_frictioncoefficient_Set_value_1.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = X                                                 
    value                 = 5.0 
";
        }

        private string GetArcInfoFileContent()
        {
            return @"
1 2 3 4
5 6 7 8
9 10 11 12
13 14 15 16
";
        }

        private string GetSamplesFileContent()
        {
            return @"
1 2 3 
4 5 6 
7 8 9
10 11 12
";
        }

        private string GetPolyFileContent()
        {
            return @"
water_level
3 2
1 2
3 4
5 6";
        }

        private class InitialFieldFileDataEqualityComparer : IEqualityComparer<InitialFieldFileData>
        {
            public bool Equals(InitialFieldFileData x, InitialFieldFileData y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return false;
                }

                var initialFieldComparer = new InitialFieldEqualityComparer();
                if (!SequenceEquivalent(x.InitialConditions, y.InitialConditions,
                                        initialFieldComparer))
                {
                    return false;
                }

                if (!SequenceEquivalent(x.Parameters, y.Parameters,
                                        initialFieldComparer))
                {
                    return false;
                }

                return true;
            }

            public int GetHashCode(InitialFieldFileData obj)
            {
                return obj.General != null ? obj.General.GetHashCode() : 0;
            }

            private static bool SequenceEquivalent<T>(IEnumerable<T> x, IEnumerable<T> y, IEqualityComparer<T> comparer)
            {
                return !x.Except(y, comparer).Any() && !y.Except(x, comparer).Any();
            }
        }

        private class InitialFieldEqualityComparer : IEqualityComparer<InitialField>
        {
            public bool Equals(InitialField x, InitialField y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return false;
                }

                return x.Quantity == y.Quantity &&
                       x.DataFile == y.DataFile &&
                       x.DataFileType == y.DataFileType &&
                       x.InterpolationMethod == y.InterpolationMethod &&
                       x.Operand == y.Operand &&
                       x.AveragingType == y.AveragingType &&
                       Equals(x.AveragingRelSize, y.AveragingRelSize) &&
                       x.AveragingNumMin == y.AveragingNumMin &&
                       Equals(x.AveragingPercentile, y.AveragingPercentile) &&
                       x.ExtrapolationMethod == y.ExtrapolationMethod &&
                       Equals(x.Value, y.Value);
            }

            public int GetHashCode(InitialField obj)
            {
                unchecked
                {
                    var hashCode = (int)obj.Quantity;
                    hashCode = (hashCode * 397) ^ (obj.DataFile != null ? obj.DataFile.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int)obj.DataFileType;
                    hashCode = (hashCode * 397) ^ (int)obj.InterpolationMethod;
                    hashCode = (hashCode * 397) ^ (int)obj.Operand;
                    hashCode = (hashCode * 397) ^ (int)obj.AveragingType;
                    hashCode = (hashCode * 397) ^ obj.AveragingRelSize.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.AveragingNumMin.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.AveragingPercentile.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ExtrapolationMethod.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Value.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}