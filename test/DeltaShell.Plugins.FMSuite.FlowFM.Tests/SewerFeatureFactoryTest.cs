using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerFeatureFactoryTest
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void CreatePipeFromFactory()
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Pipe.ToString()
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));
        }

        [Test]
        public void SewerFeatureTypeCanBeRetrievedWithAStringValue()
        {
            SewerFeatureType testValue;
            Assert.IsFalse(Enum.TryParse("failValue", out testValue));
            Assert.IsTrue(Enum.TryParse(SewerFeatureType.Pipe.ToString(), out testValue));
        }

        [Test]
        public void PipeTypeCanBeRetrieveWithAStringValue()
        {
            PipeType testValue;
            Assert.IsFalse(Enum.TryParse("failValue", out testValue));
            Assert.IsTrue(Enum.TryParse(PipeType.ClosedConnection.ToString(), out testValue));
        }

        [Test]
        public void CreatePipeFromFactoryWithUnknownAttributes()
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Pipe.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 0, "columnName", "unkownType", "unkownCode", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = "ValueShouldNotBeSet"
                    }
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);
        }

        [Test]
        public void CreatePipeFromFactoryWithKnownAttributes()
        {
            var startLevel = 30;
            var endLevel = 100;
            var length = 200;
            var crossSectionDef = "crossSectionDef001";
            var startNode = "node001";
            var endNode = "node002";
            var pipeType = PipeType.Open;

            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Pipe.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 0, "columnName", "string", "PIPE_TYPE", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = pipeType.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 1, "columnName", "double", "LEVEL_START", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startLevel.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 2, "columnName", "double", "LEVEL_END", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = endLevel.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 3, "columnName", "double", "LENGTH", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = length.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 4, "columnName", "string", "CROSS_SECTION_DEF", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = crossSectionDef
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", "NODE_ID_START", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startNode
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", "NODE_ID_END", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = endNode
                    },
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            //Not defined yet
            Assert.IsNull(createdPipe.Source);
            Assert.IsNull(createdPipe.Target);
            Assert.IsNull(createdPipe.CrossSectionShape);

            //Defined
            Assert.IsNotNull(createdPipe.LevelSource);
            Assert.AreEqual(startLevel, createdPipe.LevelSource);

            Assert.IsNotNull(createdPipe.LevelTarget);
            Assert.AreEqual(endLevel, createdPipe.LevelTarget);

            Assert.IsNotNull(createdPipe.PipeType);
            Assert.AreEqual(pipeType, createdPipe.PipeType);

            Assert.IsNotNull(createdPipe.Length);
            Assert.AreEqual(length, createdPipe.Length);
        }

        #region CompositeManholeNode

        [Test]
        public void GivenSimpleManholeData_WhenCreatingWithFactory_ThenManholeIsCorrectlyReturned()
        {
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement1);
            var manhole = element as CompositeManholeNode;

            Assert.NotNull(manhole);
            Assert.That(manhole.ManholeId, Is.EqualTo("01001"));
            Assert.That(manhole.XCoordinate, Is.EqualTo(400.0));
            Assert.That(manhole.YCoordinate, Is.EqualTo(50.0));
            Assert.NotNull(manhole.Compartments);
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            Assert.That(compartment.Id, Is.EqualTo("put1"));
            Assert.That(compartment.ManholeLength, Is.EqualTo(7071));
            Assert.That(compartment.ManholeWidth, Is.EqualTo(7071));
            Assert.That(compartment.Shape, Is.EqualTo(ManholeShape.Square));
            Assert.That(compartment.FloodableArea, Is.EqualTo(45.67));
            Assert.That(compartment.BottomLevel, Is.EqualTo(0.01));
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(2.75));
        }

        [Test]
        public void GivenTwoCompartmentsOfTheSameLocation_WhenCreatingWithFactory_ThenManholeNodeIsCorrectlyReturned()
        {
            var elementList = new List<GwswElement> {nodeGwswElement1, nodeGwswElement2};
            var element = SewerFeatureFactory.CreateInstance(elementList);

            var manhole = element as CompositeManholeNode;
            Assert.NotNull(manhole);
            Assert.That(manhole.ManholeId, Is.EqualTo("01001"));
            Assert.That(manhole.XCoordinate, Is.EqualTo(400.0));
            Assert.That(manhole.YCoordinate, Is.EqualTo(50.0));
        }

        #endregion

        #region Test helpers

        private GwswElement nodeGwswElement1 = new GwswElement
        {
            ElementTypeName = SewerFeatureType.Node.ToString(),
            GwswAttributeList =
            {
                new GwswAttribute
                {
                    ValueAsString = "put1",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", "UNIQUE_ID", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "01001",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", "MANHOLE_ID", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "400.00",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "X_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "50.00",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "Y_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "7071",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "integer", "NODE_LENGTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "7071",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "integer", "NODE_WIDTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "RND",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", "NODE_SHAPE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "45,67",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "FLOODABLE_AREA", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "0.01",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "BOTTOM_LEVEL", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "2.75",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "SURFACE_LEVEL", "MyDescription", null, null)
                }
            }
        };

        private GwswElement nodeGwswElement2 = new GwswElement
        {
            ElementTypeName = SewerFeatureType.Node.ToString(),
            GwswAttributeList =
            {
                new GwswAttribute
                {
                    ValueAsString = "put2",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", "UNIQUE_ID", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "01001",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", "MANHOLE_ID", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "400.20",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "X_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "50.20",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "Y_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "4561",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "integer", "NODE_LENGTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "5561",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "integer", "NODE_WIDTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "RHK",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", "NODE_SHAPE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "45,67",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "FLOODABLE_AREA", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "-0.45",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "BOTTOM_LEVEL", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "1.83",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "SURFACE_LEVEL", "MyDescription", null, null)
                }
            }
        }; 
        
        #endregion
    }
}