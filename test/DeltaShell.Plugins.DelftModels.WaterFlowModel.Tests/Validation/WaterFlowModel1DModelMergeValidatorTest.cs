using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Validation
{
    [TestFixture]
    public class WaterFlowModel1DModelMergeValidatorTest
    {
        #region ALL
        [Test]
		public void Given2ModelsWhenValidateThenValidationReportIsFilled()
		{
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            var validationReport = new WaterFlowModel1DModelMergeValidator().Validate(model1, model2);
            Assert.That(validationReport.IsEmpty, Is.Not.True);
		}

        [Test]
        public void Validating2ModelsTwiceWithNoChangesShouldGenerateTheSameValidationReport()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            var validationReport1 = new WaterFlowModel1DModelMergeValidator().Validate(model1, model2);
            var validationReport2 = new WaterFlowModel1DModelMergeValidator().Validate(model1, model2);

            Assert.IsTrue(validationReport1.Equals(validationReport2), "Validation of 2 Models should generate the same Validation Report");
        }

        [Test]
        public void Validating2ModelsTwiceWithValidationAffectingChangesShouldGenerateDifferentValidationReports()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            var validationReport1 = new WaterFlowModel1DModelMergeValidator().Validate(model1, model2);

            model2.Network.HydroNodes.First().Geometry = new Point(200, 0); // simulate change to node position
            var validationReport2 = new WaterFlowModel1DModelMergeValidator().Validate(model1, model2);
            Assert.IsFalse(validationReport1.Equals(validationReport2), "Validation of 2 Models should not generate the same Validation Report");
        }
        #endregion

        #region Node Connection
        [Test]
        public void Given2ModelsWith1ConnectingNodeWhenValidateConnectionThenValidationReportStates1NodeIsConnected()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Contains.Substring("will be connected to"));
        }

        [Test]
        public void Given2ModelsWith1ConnectingNodeWithin10mRangeWhenValidateConnectionThenValidationReportStates1NodeIsConnected()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(110, 120);
            
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Contains.Substring("will be connected to"));
        }

        [Test]
        public void Given2ModelsWith2PossibleConnectingDestinationNodesWithin10mRangeWhenValidateConnectionThenValidationReportStates1NodeIsConnected()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 105);
            model1.Name = "DEST";
            
            //add extra node within 10m range where source model can connect to.
            var extraNode1 = new HydroNode("extraNode1") { Geometry = new Point(100, 0) };
            model1.Network.Nodes.Add(extraNode1);

            //add extra node outside 10m range where source model can connect to.
            var extraNode2 = new HydroNode("extraNode2") { Geometry = new Point(90, 0) };
            model1.Network.Nodes.Add(extraNode2);
            
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(110, 120);
            model2.Name = "SRC";
            
            //add extra node within 10m range where destination model can connect to.
            var extraNode3 = new HydroNode("extraNode3") { Geometry = new Point(111, 0) };
            model2.Network.Nodes.Add(extraNode3);
            
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Contains.Substring("will be connected to"));
            Assert.That(validationIssue.Message, Is.Not.StringContaining(extraNode1.Name));
        }
        
        [Test]
        public void Given2ModelsWith1ConnectingNodeWithin10mRangeAndDifferentCoordinateSytemsWhenValidateConnectionThenValidationReportStates1NodeIsConnected()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            model2.Network.CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857); // pseudo web mercator - OpenStreetMap
            
            var model2Node1 = model2.Network.Nodes.First();
            model2Node1.Geometry = new Point(369022.384806,6102661.94842); // = +/- 6 meter off...so should be in range

            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);
            
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Contains.Substring("will be connected to"));
        }

        [Test]
        public void Given2ModelsWith2ConnectingNodeWithin10mRangeAndDifferentCoordinateSytemsWhenValidateConnectionThenValidationReportStates2NodeAreConnected()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            model1.Name = "DEST";
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            model2.Name = "SRC"; 
            model2.Network.CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857); // pseudo web mercator - OpenStreetMap

            var model2Node1 = model2.Network.Nodes.First();
            model2Node1.Geometry = new Point(369022.384806, 6102661.94842); // = +/- 6 meter off...(106,0) so should be in range
            var extraNode1 = new HydroNode("extraNode1"){Geometry = new Point(369006.431349, 6102661.78266)};// +/- 4 meter off other side (94,0) so should be in range
            var extraNode2 = new HydroNode("extraNode2"){Geometry = new Point(369042.152097, 6102662.77716)};// +/- 20 meter off (120,0) so should be out of range!

            model2.Network.Nodes.Add(extraNode1);
            model2.Network.Nodes.Add(extraNode2);

            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);

            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Is.StringContaining("of source model (SRC) will be connected to node node2 of destination model DEST").And.StringContaining("extraNode1").And.StringContaining("node1"));
        }

        [Test]
        public void Given2ModelsWith1ConnectingNodeOutside10mRangeAndDifferentCoordinateSytemsWhenValidateConnectionThenValidationReportStatesThereAreNoConnectingNodes()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            model2.Network.CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857); // pseudo web mercator - OpenStreetMap
            
            var model2Node1 = model2.Network.Nodes.First();
            model2Node1.Geometry = new Point(369028.384806, 6102661.94842); // = +/- 10.7 meter off...so is out of range
            
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);
            
            Assert.That(validationReport.WarningCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Contains.Substring("has no connecting nodes with the destination model"));
        }
        
        [Test]
        public void Given2ModelsWithNoConnectingNodeWhenValidateConnectionThenValidationReportStatesThereAreNoConnectingNodes()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(111, 120);
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateConnection(model1, model2);
            Assert.That(validationReport.WarningCount, Is.EqualTo(1));
            var validationIssue = validationReport.Issues.FirstOrDefault();
            Assert.That(validationIssue, Is.Not.Null);
            Assert.That(validationIssue.Message, Contains.Substring("has no connecting nodes with the destination model"));
        }    
        #endregion
        
        #region Salinity

        [Test]
        public void ValidateSalinity_ReturnsEmptyErrorReportWhenSourceModelDoesNotUseSalt()
        {
            var destinationModel = new WaterFlowModel1D();
            var sourceModel = new WaterFlowModel1D() { UseSalt = false };
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateSalility(destinationModel, sourceModel);
            Assert.That(validationReport.IsEmpty, Is.True);
        }

        [Test]
        public void ValidateSalinity_ReturnsWarningWhenSourceModelUsesSaltButDestinationModelDoesNot()
        {
            var destinationModel = new WaterFlowModel1D() { UseSalt = false };
            var sourceModel = new WaterFlowModel1D() { UseSalt = true };
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateSalility(destinationModel, sourceModel);
            Assert.That(validationReport.IsEmpty, Is.False);
            
            var issue = validationReport.Issues.First();
            Assert.AreEqual(sourceModel, issue.Subject);
        }

        [Test]
        public void ValidateSalinity_ReturnsWarningWhenF3AndF4CoverageDatasWillBeLostInMerge()
        {
            var destinationModel = new WaterFlowModel1D() { UseSalt = true, DispersionFormulationType = DispersionFormulationType.Constant};
            var sourceModel = new WaterFlowModel1D() { UseSalt = true, DispersionFormulationType = DispersionFormulationType.ThatcherHarleman};
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateSalility(destinationModel, sourceModel);
            Assert.That(validationReport.IsEmpty, Is.False);

            var issue = validationReport.Issues.First();
            Assert.AreEqual(sourceModel.DispersionFormulationType, issue.Subject);
        }

        #endregion

        #region Temperature

        [Test]
        public void ValidateTemperature_ReturnsEmptyErrorReportWhenSourceModelDoesNotUseTemp()
        {
            var destinationModel = new WaterFlowModel1D();
            var sourceModel = new WaterFlowModel1D() { UseTemperature = false };
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateTemperature(destinationModel, sourceModel);
            Assert.That(validationReport.IsEmpty, Is.True);
        }

        [Test]
        public void ValidateTemperature_ReturnsWarningWhenSourceModelUsesTempButDestinationModelDoesNot()
        {
            var destinationModel = new WaterFlowModel1D() { UseTemperature = false };
            var sourceModel = new WaterFlowModel1D() { UseTemperature = true };
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateTemperature(destinationModel, sourceModel);
            Assert.That(validationReport.IsEmpty, Is.False);

            var issue = validationReport.Issues.First();
            Assert.AreEqual(sourceModel, issue.Subject);
        }

        [Test]
        public void ValidateTemperature_ReturnsWarningWhenMeteoDataCoverageDatasWillBeLostInMerge()
        {
            var destinationModel = new WaterFlowModel1D() { UseTemperature = true, TemperatureModelType = TemperatureModelType.Transport };
            var sourceModel = new WaterFlowModel1D() { UseTemperature = true, TemperatureModelType = TemperatureModelType.Composite };
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateTemperature(destinationModel, sourceModel);
            Assert.That(validationReport.IsEmpty, Is.False);

            var issue = validationReport.Issues.First();
            Assert.AreEqual(sourceModel.TemperatureModelType, issue.Subject);
        }

        #endregion

        #region Coordinate System
        [Test]
        public void GivenModelsWithSameCoordinateSystemSetWhenValidateCoordinateSystemThenValidationReportIsEmpty()
        {
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            var model1 = new WaterFlowModel1D()
            {
                Network = { CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992) } //rd new
            };
            var validationReport = new WaterFlowModel1DModelMergeValidator().Validate(model1, model1);
            var coordinateValidationSubreport = validationReport.SubReports.FirstOrDefault(sr => sr.Category == "Coordinate system");
            Assert.That(coordinateValidationSubreport, Is.Not.Null);
            Assert.That(coordinateValidationSubreport.IsEmpty, Is.True);
        }
        
        [Test]
        public void GivenModelsWithDifferentCoordinateSystemSetWhenValidateCoordinateSystemThenValidationReportHasErrors()
        {
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            var model1 = new WaterFlowModel1D()
            {
                Network = { CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992) } //rd new
            };
            var model2 = new WaterFlowModel1D()
            {
                Network = { CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857) } // WGS 84 / Pseudo-Mercator
            };
            var validationReport = new WaterFlowModel1DModelMergeValidator().Validate(model1, model2);
            var coordinateValidationSubreport = validationReport.SubReports.FirstOrDefault(sr => sr.Category == "Coordinate system");
            Assert.That(coordinateValidationSubreport, Is.Not.Null);
            Assert.That(coordinateValidationSubreport.ErrorCount, Is.EqualTo(1));
            var error = coordinateValidationSubreport.AllErrors.FirstOrDefault();
            Assert.That(error, Is.Not.Null);
            Assert.That(error.Message, Is.StringContaining("different coordinate systems"));
        }
        
        [Test]
        public void GivenModelsWithNoCoordinateSystemSetWhenValidateCoordinateSystemThenValidationReportHasErrors()
        {
            var model1 = new WaterFlowModel1D()
            {
                Network = { CoordinateSystem = null } 
            };
            var validationReport = new WaterFlowModel1DModelMergeValidator().Validate(model1, model1);
            var coordinateValidationSubreport = validationReport.SubReports.FirstOrDefault(sr => sr.Category == "Coordinate system");
            Assert.That(coordinateValidationSubreport, Is.Not.Null);
            Assert.That(coordinateValidationSubreport.ErrorCount, Is.EqualTo(2));    
        }
        [Test]
        public void GivenModelsWithNodesAndWithNoCoordinateSystemSetWhenValidateCoordinateSystemThenValidationReportHasErrors()
        {
            var model1 = new WaterFlowModel1D()
            {
                Network = { CoordinateSystem = null }
                
            };
            model1.Network.Nodes.Add(new HydroNode());
            var validationReport = new WaterFlowModel1DModelMergeValidator().Validate(model1, model1);
            var coordinateValidationSubreport = validationReport.SubReports.FirstOrDefault(sr => sr.Category == "Coordinate system");
            Assert.That(coordinateValidationSubreport, Is.Not.Null);
            Assert.That(coordinateValidationSubreport.ErrorCount, Is.EqualTo(2));    
        }
        
        [Test]
		public void GivenModelWithGeometricCoordinateSystemWhenValidateCoordinateSystemThenValidationIssuesAreEmpty()
		{
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
			var model1 = new WaterFlowModel1D()
			{
                Network = { CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992) } //rd new
            };
            var validationIssues = WaterFlowModel1DModelMergeValidator.ValidateCoordinateSystem(model1);
            Assert.That(validationIssues.Any(), Is.False);
		}

        [Test]
        public void GivenModelWithoutGeometricCoordinateSystemWhenValidateCoordinateSystemThenValidationIssuesAreNotEmpty()
		{
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            
            var model1 = new WaterFlowModel1D
            {
                Network = {CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(4326)} //wsg84
            };

            var validationIssues = WaterFlowModel1DModelMergeValidator.ValidateCoordinateSystem(model1);
            Assert.That(validationIssues.Any(), Is.True);
		}
        #endregion

        #region Renaming
        [Test]
		public void Given2ModelsWhenValidateHydroObjectsRenamedThenValidationReportShowsRenamedHydroObjects()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            model1.Name = "Destination_Model";
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            model2.Name = "Source_Model";

            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateHydroObjectsWillBeRenamed(model1, model2);
            Assert.That(validationReport.IsEmpty, Is.Not.True);
            Assert.That(validationReport.SubReports.Count(), Is.EqualTo(3));

            var channelSubreport = validationReport.SubReports.ElementAt(0);
            Assert.That(channelSubreport, Is.Not.Null);
            Assert.That(channelSubreport.InfoCount, Is.EqualTo(1));

            Assert.That(channelSubreport.InfoCount, Is.EqualTo(1));
            var channelRenamedIssue = channelSubreport.Issues.FirstOrDefault();
            Assert.That(channelRenamedIssue, Is.Not.Null);
            Assert.That(channelRenamedIssue.Message, Contains.Substring("From model : Source_Model; element : Channel ; with name: channel will be renamed after the merge into model : Destination_Model to  : Source_Model0_channel"));
            
            var nodeSubreport = validationReport.SubReports.ElementAt(2);
            Assert.That(nodeSubreport, Is.Not.Null);
            Assert.That(nodeSubreport.InfoCount, Is.EqualTo(2));
            
            var nodeRenamedIssue = nodeSubreport.Issues.FirstOrDefault();
            Assert.That(nodeRenamedIssue, Is.Not.Null);
            Assert.That(nodeRenamedIssue.Message, Contains.Substring("From model : Source_Model; element : HydroNode ; with name: node1 will be renamed after the merge into model : Destination_Model to  : Source_Model0_node1"));
            
            nodeRenamedIssue = nodeSubreport.Issues.ElementAt(1);
            Assert.That(nodeRenamedIssue, Is.Not.Null);
            Assert.That(nodeRenamedIssue.Message, Contains.Substring("From model : Source_Model; element : HydroNode ; with name: node2 will be renamed after the merge into model : Destination_Model to  : Source_Model0_node2"));
        }

        [Test]
        public void Given2ModelsWhenValidateHydroObjectsRenamedThenValidationReportIsEmpty()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            foreach (var allHydroObject in model2.Network.AllHydroObjects)
            {
                allHydroObject.Name += "1";
            }
            
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateHydroObjectsWillBeRenamed(model1, model2);
            Assert.That(validationReport.IsEmpty, Is.True);
        }
        #endregion

        #region Boundary Conditions
        [Test]
		public void Given2ModelsWithConnectingNodesAndDifferentBoundaryConditionsWhenValidateBoundaryConditionClearCheckThenValidationReportStatesBoundaryConditionsWillBeCleared()
		{
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            model1.Name = "Destination Model";
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            model2.Name = "Source Model"; 
            var bc1 = model1.BoundaryConditions.FirstOrDefault(b => b.Node.Name == "node2");
            Assert.That(bc1, Is.Not.Null);
            Assert.That(bc1.DataType, Is.EqualTo(WaterFlowModel1DBoundaryNodeDataType.FlowConstant));
            Assert.That(bc1.Flow, Is.EqualTo(42));
            
            var bc2 = model2.BoundaryConditions.FirstOrDefault(b => b.Node.Name == "node1");
            Assert.That(bc2, Is.Not.Null);
            bc2.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            bc2.WaterLevel = 801;

            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateBoundaryConditionClearCheck(model1, model2);

            Assert.That(validationReport.IsEmpty, Is.Not.True);
            Assert.That(validationReport.WarningCount, Is.EqualTo(1));
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            Assert.That(validationReport.Issues.Count(), Is.EqualTo(2));
            
            var warningBoundaryCondition = validationReport.Issues.FirstOrDefault(issue => issue.Severity == ValidationSeverity.Warning);
            Assert.That(warningBoundaryCondition, Is.Not.Null);
            Assert.That(warningBoundaryCondition.Message, Contains.Substring(string.Format(", will be set to {0} AFTER the merge.", WaterFlowModel1DBoundaryNodeDataType.None)));
		}

        [Test]
        public void Given2ModelsWithConnectingNodesAndDestinationBoundaryConditionIsNoneWhenValidateBoundaryConditionClearCheckThenValidationReportStatesBoundaryConditionsWillBeCleared()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            model1.Name = "Destination Model";
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            model2.Name = "Source Model"; 
            var bc1 = model1.BoundaryConditions.FirstOrDefault(b => b.Node.Name == "node2");
            Assert.That(bc1, Is.Not.Null);
            bc1.DataType = WaterFlowModel1DBoundaryNodeDataType.None;
            
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateBoundaryConditionClearCheck(model1, model2);
            Assert.That(validationReport.IsEmpty, Is.Not.True);
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            Assert.That(validationReport.Issues.Count(), Is.EqualTo(1));
            var warningBoundaryCondition = validationReport.Issues.FirstOrDefault();
            Assert.That(warningBoundaryCondition, Is.Not.Null);
            Assert.That(warningBoundaryCondition.Message, Contains.Substring("Node : node1, in source model : Source Model, will be merged with node : node2, of the destination model : Destination Model. The boundary condition : node1 - Q: 42 m^3/s (of datatype: FlowConstant), will NOT be merged into the destination model : Destination Model."));
        }

        [Test]
        public void Given2ModelsWithConnectingNodesAndBoundaryConditionAreNoneWhenValidateBoundaryConditionClearCheckThenValidationReportIsEmpty()
        {
            var model1 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var model2 = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            var bc1 = model1.BoundaryConditions.FirstOrDefault(b => b.Node.Name == "node2");
            Assert.That(bc1, Is.Not.Null);
            bc1.DataType = WaterFlowModel1DBoundaryNodeDataType.None;

            var bc2 = model2.BoundaryConditions.FirstOrDefault(b => b.Node.Name == "node1");
            Assert.That(bc2, Is.Not.Null);
            bc2.DataType = WaterFlowModel1DBoundaryNodeDataType.None;
            
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateBoundaryConditionClearCheck(model1, model2);
            Assert.That(validationReport.IsEmpty, Is.True);
        }
        #endregion

        #region Channels
        [Test]
        public void Given2ModelsWithNotExtactConnectingNodesWhenValidateChannelsThenValidationReportHas1InfoIssue()
        {
            var destinationModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            destinationModel1D.Name = "Destination";
            var sourceModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(105, 200);
            sourceModel1D.Name = "Source";
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateChannels(destinationModel1D, sourceModel1D);
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var channelChangeInfo = validationReport.Issues.FirstOrDefault(i => i.Severity == ValidationSeverity.Info);
            Assert.That(channelChangeInfo, Is.Not.Null);
            Assert.That(channelChangeInfo.Message, Is.StringContaining("The geometry of channel").And.StringContaining("because the connecting nodes are not exactly at same location. The length of the channel will stay the same :").And.StringContaining("by setting the is custom length flag to true to keep the original source calculations the same."));

        } 
        [Test]
        public void Given2ModelsWithNotExtactConnectingNodes2WhenValidateChannelsThenValidationReportHas1InfoIssue()
        {
            var destinationModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(195, 300);
            destinationModel1D.Name = "Destination";
            var sourceModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            sourceModel1D.Name = "Source";
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateChannels(destinationModel1D, sourceModel1D);
            Assert.That(validationReport.InfoCount, Is.EqualTo(1));
            var channelChangeInfo = validationReport.Issues.FirstOrDefault(i => i.Severity == ValidationSeverity.Info);
            Assert.That(channelChangeInfo, Is.Not.Null);
            Assert.That(channelChangeInfo.Message, Is.StringContaining("The geometry of channel").And.StringContaining("because the connecting nodes are not exactly at same location. The length of the channel will stay the same :").And.StringContaining("by setting the is custom length flag to true to keep the original source calculations the same."));
        } 
        [Test]
        public void Given2ModelsWithExtactConnectingNodesWhenValidateChannelsThenValidationReportIsEmpty()
        {
            var destinationModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            destinationModel1D.Name = "Destination";
            var sourceModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            sourceModel1D.Name = "Source";
            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateChannels(destinationModel1D, sourceModel1D);
            Assert.That(validationReport.IsEmpty, Is.True);
        }
        #endregion

        #region InitialConditions
        [Test]
        public void TestValidationOfTwoFlow1DModelsWithDifferentInitialConditionTypesGeneratesAWarning()
        {
            var destinationModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            destinationModel1D.Name = "Destination";
            destinationModel1D.InitialConditionsType = InitialConditionsType.Depth;
            var sourceModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(105, 200);
            sourceModel1D.Name = "Source";
            sourceModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;

            var validationReport = WaterFlowModel1DModelMergeValidator.ValidateInitialConditions(destinationModel1D, sourceModel1D);
            Assert.AreEqual(1, validationReport.WarningCount);
            var validationWarning = validationReport.Issues.FirstOrDefault(i => i.Severity == ValidationSeverity.Warning);
            Assert.NotNull(validationWarning);
            Assert.IsTrue(validationWarning.Message.Contains("Initial Conditions will not be merged."));
        }
        #endregion
    }
}