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
        public void CreateManholeFromFactory()
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString()
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Manhole)));
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
    }
}