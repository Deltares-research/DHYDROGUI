using System;
using System.Collections.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    public class SewerConnectionGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Sewer Connection

        [Test]
        public void SewerFeatureCannotBeCreatedIfTargetAndSourceAreNotGivenAndLogMessageIsGiven()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var nodeGwswElement = new GwswElement { ElementTypeName = SewerFeatureType.Connection.ToString() };

            var sewerConnection = CreateSewerFeature<SewerConnection>(nodeGwswElement, logHandler);
            logHandler.Received(1).ReportErrorFormat(Properties.Resources.SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields); ;
            Assert.IsNull(sewerConnection);
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfTargetIsNotGivenAndLogMessageIsGiven()
        {
            var sourceNode = "node001";
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, sourceNode, string.Empty)
                }
            };

            var sewerConnection = CreateSewerFeature<SewerConnection>(connectionGwswElement, logHandler);
            logHandler.Received(1).ReportErrorFormat(Properties.Resources.SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields); ;
            Assert.IsNull(sewerConnection);
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfSourceIsNotGivenAndLogMessageIsGiven()
        {
            var targetNode = "node002";
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, targetNode, string.Empty)
                }
            };

            var sewerConnection = CreateSewerFeature<SewerConnection>(connectionGwswElement, logHandler);
            logHandler.Received(1).ReportErrorFormat(Properties.Resources.SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields); ;
            Assert.IsNull(sewerConnection);
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