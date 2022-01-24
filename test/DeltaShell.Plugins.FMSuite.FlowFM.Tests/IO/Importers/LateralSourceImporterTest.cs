using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
     public class LateralSourceImporterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var importer = new LateralSourceImporter();

            // Assert
            Assert.That(importer, Is.InstanceOf<IFileImporter>());
            Assert.That(importer.FileImporter, Is.Not.Null);
            Assert.That(importer.Name, Is.EqualTo("Flow1D CSV Importer"));
        }

        [Test]
        public void Constructor_FileImporterNull_ThrowsArgumentNullException()
        {
            void Call() => new LateralSourceImporter(null, Substitute.For<ILog>());
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("fileImporter"));
        }

        [Test]
        public void Constructor_LoggerNull_ThrowsArgumentNullException()
        {
            void Call() => new LateralSourceImporter(new LateralSourceFileImporter(), null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("logger"));
        }

        [Test]
        public void GivenAnInvalidIFunctionList_ImportItemShouldProduceErrorLogAndStopImporting()
        {
            // arrange
            var logMock = Substitute.For<ILog>();
            var fileImporterMock = Substitute.For<LateralSourceFileImporter>();
            fileImporterMock.ImportFunctions(Arg.Any<string>()).Returns(new List<IFunction>());
            var lateralSourceImporter = new LateralSourceImporter(fileImporterMock, logMock);
            
            // act
            object importResult = lateralSourceImporter.ImportItem(null, null);

            // assert
            Assert.IsNull(importResult);
            logMock.Received(1).ErrorFormat(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<object>());
        }

        [Test]
        public void GivenAnInvalidTypeOfTarget_ImportItemsShouldProduceErrorLog()
        {
            // arrange
            var logMock = Substitute.For<ILog>();
            var fileImporterMock = Substitute.For<LateralSourceFileImporter>();
            fileImporterMock.ImportFunctions(Arg.Any<string>()).Returns(new List<IFunction>());
            var lateralSourceImporter = new LateralSourceImporter(fileImporterMock, logMock);
            const string invalidTarget = "this is an invalid target type";
            
            // act
            object result = lateralSourceImporter.ImportItem(null, invalidTarget);
            
            // assert
            Assert.AreEqual(invalidTarget, result);
            logMock.Received(1).ErrorFormat(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<object>());
        }

        [Test]
        public void GivenAValidImportedObject_WhenBoundaryRelationTypeIsQ_FlowValueShouldBeUpdated()
        {
            // Arrange
            const string modelSourceName1 = "lateralSource1";
            const double startFlowValue = 3.0;
            const double expectedFlowValue = 10.0;
            
            var startData = new EventedList<Model1DLateralSourceData>
            {
                new Model1DLateralSourceData
                {
                    Name = modelSourceName1,
                    Flow = startFlowValue,
                    Feature = new LateralSource()
                    {
                        Name = modelSourceName1
                    },
                    Data = new Function
                    {
                        Name = modelSourceName1,
                        Arguments = new EventedList<IVariable>
                        {
                            new Variable<object>()
                        }
                    }
                }
            };
            
            var importedFunctionList = new List<IFunction>
            {
                new Function
                {
                    Name = modelSourceName1,
                    Components = new EventedList<IVariable>
                    {
                        new Variable<double>
                        {
                            Values = new MultiDimensionalArray<double> { expectedFlowValue }
                        }
                    }
                }
            };
            
            var fileImporterMock = Substitute.For<LateralSourceFileImporter>();
            fileImporterMock.ImportFunctions(Arg.Any<string>()).Returns(importedFunctionList);
            fileImporterMock.BoundaryRelationType = BoundaryRelationType.Q;
            var lateralSourceImporter = new LateralSourceImporter(fileImporterMock, Substitute.For<ILog>());
            
            // Act
            var result = lateralSourceImporter.ImportItem(null, startData) as IEventedList<Model1DLateralSourceData>;
            
            // Assert
            Assert.NotNull(result);

            Model1DLateralSourceData lateralSource = result.First();
            Assert.AreEqual(expectedFlowValue, lateralSource.Flow);
            Assert.AreEqual(Model1DLateralDataType.FlowConstant, lateralSource.DataType);
        }

        [Test]
        public void GivenABoundaryRelationTypeOfQHAndQT_TheExistingDataShouldBeCompletelyOverriddenWithNewData()
        {
            // Arrange
            const string modelName = "LateralSource1";
            
            var startData = new EventedList<Model1DLateralSourceData>
            {
                new Model1DLateralSourceData
                {
                    Data = new Function
                    {
                        Name = modelName,
                        Arguments = new EventedList<IVariable>
                        {
                            new Variable<object>()
                        }
                    },
                    Feature = new LateralSource()
                    {
                        Name = modelName
                    }
                }
            };
            
            var importedFunctionList = new List<IFunction>
            {
                new Function
                {
                    Name = modelName,
                    Arguments = new EventedList<IVariable>
                    {
                        new Variable<object>()
                    }
                }
            };

            var fileImporterQhRelationType = Substitute.For<LateralSourceFileImporter>();
            fileImporterQhRelationType.ImportFunctions(Arg.Any<string>()).Returns(importedFunctionList);
            fileImporterQhRelationType.BoundaryRelationType = BoundaryRelationType.Qh;
            var sourceImporterQhRelationType = new LateralSourceImporter(fileImporterQhRelationType, Substitute.For<ILog>());
            
            var fileImporterQtRelationType = Substitute.For<LateralSourceFileImporter>();
            fileImporterQtRelationType.ImportFunctions(Arg.Any<string>()).Returns(importedFunctionList);
            fileImporterQtRelationType.BoundaryRelationType = BoundaryRelationType.Qt;
            var sourceImporterQtRelationType = new LateralSourceImporter(fileImporterQtRelationType, Substitute.For<ILog>());

            // Act
            var resultQhRelationType = sourceImporterQhRelationType.ImportItem(null, startData) as IEventedList<Model1DLateralSourceData>;
            var resultQtRelationType = sourceImporterQtRelationType.ImportItem(null, startData) as IEventedList<Model1DLateralSourceData>;

            // Assert
            Assert.NotNull(resultQhRelationType);
            Assert.NotNull(resultQtRelationType);

            Model1DLateralSourceData lateralSourceQh = resultQhRelationType.First();
            Model1DLateralSourceData lateralSourceQt = resultQtRelationType.First();
            Assert.AreEqual(modelName, lateralSourceQh.Data.Name);
            Assert.AreEqual(modelName, lateralSourceQt.Data.Name);
        }

        [Test]
        public void GivenABoundaryRelationTypeOfHAndHT_TheDataShouldNotBeChanged()
        {
            // Arrange
            const string modelName = "LateralSource1";
            const string functionFirstName = "function1";
            const string functionSecondName = "function2";
            
            var startData = new EventedList<Model1DLateralSourceData>
            {
                new Model1DLateralSourceData
                {
                    Data = new Function
                    {
                        Name = functionFirstName,
                        Arguments = new EventedList<IVariable>
                        {
                            new Variable<object>()
                        }
                    },
                    Feature = new LateralSource()
                    {
                        Name = modelName
                    }
                }
            };
            
            var importedFunctionList = new List<IFunction>
            {
                new Function(functionSecondName)
            };

            var fileImporterHRelationType = Substitute.For<LateralSourceFileImporter>();
            fileImporterHRelationType.ImportFunctions(Arg.Any<string>()).Returns(importedFunctionList);
            fileImporterHRelationType.BoundaryRelationType = BoundaryRelationType.Qh;
            var sourceImporterHRelationType = new LateralSourceImporter(fileImporterHRelationType, Substitute.For<ILog>());
            
            var fileImporterHtRelationType = Substitute.For<LateralSourceFileImporter>();
            fileImporterHtRelationType.ImportFunctions(Arg.Any<string>()).Returns(importedFunctionList);
            fileImporterHtRelationType.BoundaryRelationType = BoundaryRelationType.Qt;
            var sourceImporterHtRelationType = new LateralSourceImporter(fileImporterHtRelationType, Substitute.For<ILog>());

            // Act
            var resultHRelationType = sourceImporterHRelationType.ImportItem(null, startData) as IEventedList<Model1DLateralSourceData>;
            var resultHtRelationType = sourceImporterHtRelationType.ImportItem(null, startData) as IEventedList<Model1DLateralSourceData>;

            // Assert
            Assert.NotNull(resultHRelationType);
            Assert.NotNull(resultHtRelationType);

            Model1DLateralSourceData lateralSourceH = resultHRelationType.First();
            Model1DLateralSourceData lateralSourceHt = resultHtRelationType.First();
            Assert.AreEqual(startData.First().Data.Name, lateralSourceH.Data.Name);
            Assert.AreEqual(startData.First().Data.Name, lateralSourceHt.Data.Name);
        }

        [Test]
        public void WhenThereAreNoMatchingFunctionNames_ImportItemShouldProduceLogMessage()
        {
            // Arrange
            const string name1 = "lateralSource1";
            const string name2 = "lateralSource2";

            var startData = new EventedList<Model1DLateralSourceData>
            {
                new Model1DLateralSourceData
                {
                    Name = name1,
                    Data = new Function
                    {
                        Name = name1,
                        Arguments = new EventedList<IVariable>()
                        {
                            new Variable<object>()
                        }
                    },
                    Feature = new LateralSource()
                }
            };

            var importedData = new List<IFunction>
            {
                new Function
                {
                    Name = name2,
                    Arguments = new EventedList<IVariable>()
                    {
                        new Variable<object>()
                    }
                }
            };

            var logMock = Substitute.For<ILog>();
            var fileImporterMock = Substitute.For<LateralSourceFileImporter>();
            fileImporterMock.ImportFunctions(Arg.Any<string>()).Returns(importedData);
            var lateralSourceImporter = new LateralSourceImporter(fileImporterMock, logMock);

            // Act
            _ = lateralSourceImporter.ImportItem(null, startData) as IEventedList<Model1DLateralSourceData>;
            
            // Assert
            logMock.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        [TestCase(BoundaryRelationType.Qh, BoundaryRelationType.Q)]
        [TestCase(BoundaryRelationType.Ht, BoundaryRelationType.H)]
        public void SettingBoundaryRelationType_ShouldSetBoundaryRelationTypeOfFileImporter(
            BoundaryRelationType boundaryRelationTypeStartValue, BoundaryRelationType boundaryRelationTypeEndValue)
        {
            // Arrange
            var fileImportMock = Substitute.For<LateralSourceFileImporter>();
            fileImportMock.BoundaryRelationType = boundaryRelationTypeStartValue; // irrelevant what type this is
            var importer = new LateralSourceImporter(fileImportMock, Substitute.For<ILog>());

            // Act
            importer.BoundaryRelationType = boundaryRelationTypeEndValue;

            // Assert
            Assert.AreEqual(boundaryRelationTypeEndValue, importer.BoundaryRelationType);
        }
    }
}