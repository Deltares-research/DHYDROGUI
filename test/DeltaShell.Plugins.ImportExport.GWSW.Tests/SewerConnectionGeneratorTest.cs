using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    public class SewerConnectionGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Sewer Connection

        [Test]
        public void SewerFeatureCannotBeCreatedIfTargetAndSourceAreNotGivenAndLogMessageIsGiven()
        {
            var nodeGwswElement = new GwswElement { ElementTypeName = SewerFeatureType.Connection.ToString() };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => CreateSewerFeature<SewerConnection>(nodeGwswElement), expectedMessage);
            Assert.IsNull(CreateSewerFeature<SewerConnection>(nodeGwswElement));
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfTargetIsNotGivenAndLogMessageIsGiven()
        {
            var sourceNode = "node001";
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, sourceNode, string.Empty)
                }
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => CreateSewerFeature<SewerConnection>(connectionGwswElement), expectedMessage);
            Assert.IsNull(CreateSewerFeature<SewerConnection>(connectionGwswElement));
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfSourceIsNotGivenAndLogMessageIsGiven()
        {
            var targetNode = "node002";
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, targetNode, string.Empty)
                }
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => CreateSewerFeature<SewerConnection>(connectionGwswElement), expectedMessage);
            Assert.IsNull(CreateSewerFeature<SewerConnection>(connectionGwswElement));
        }

        [Test]
        public void CreateSewerConnectionFromFactoryWithUnknownAttributes()
        {
            var startNode = "node001";
            var endNode = "node002";
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute("unkownCode", "ValueShouldNotBeSet", string.Empty, "unknownType"),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty)
                }
            };

            var sewerConnection = CreateSewerFeature<SewerConnection>(connectionGwswElement);
            Assert.IsNotNull(sewerConnection);
        }

        [Test]
        [TestCase("GSL", typeof(Pipe))]
        [TestCase("OVS", typeof(GwswConnectionWeir))]
        [TestCase("ITR", typeof(Pipe))]
        [TestCase("OPL", typeof(Pipe))]
        [TestCase("DRL", typeof(GwswConnectionOrifice))]
        [TestCase("PMP", typeof(GwswConnectionPump))]
        public void CreateSewerConnectionMapsConnectionTypeFromFactory(string typeOfConnection, Type expectedType)
        {
            var startNode = "node001";
            var endNode = "node002";
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty)
                }
            };
            var sewerConnection = CreateSewerFeature<ISewerFeature>(connectionGwswElement);
            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(expectedType, sewerConnection.GetType(), "Created Sewer Connection is not of the expected type.");
        }

        [Test]
        public void CreateSewerConnectionWithUnknownMapConnectionTypeFromFactory()
        {
            var typeOfConnection = "NotKnown";
            var startNode = "node001";
            var endNode = "node002";
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty)
                }
            };

            var sewerConnection = CreateSewerFeature<SewerConnection>(connectionGwswElement);
            Assert.IsNotNull(sewerConnection);
        }

        #endregion
    }
}