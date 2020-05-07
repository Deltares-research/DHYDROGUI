using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class UpdateSupportPointVisitorTests
    {
        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenNullParameterProvided()
        {
            void Call() => new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(null);
            
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Constructor_ConstructionSuccessful_WhenEmptyCollectionProvided()
        {
            var toUpdate = new Dictionary<SupportPoint, SupportPoint>();

            var result = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            Assert.IsNotNull(result);
        }

        [Test]
        public void Visit_UniformDataComponent_DoesNothing()
        {
            var toUpdate = new Dictionary<SupportPoint, SupportPoint>();
            toUpdate.Add(new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>()), 
                new SupportPoint(20, Substitute.For<IWaveBoundaryGeometricDefinition>()));

            var result = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            var bla = new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading()));

            result.Visit(bla);
            
            // what to Assert.
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SupportPointReplaced()
        {
            var supportPointToReplace = new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var replacedWithSupportPoint = new SupportPoint(20, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var toVisit = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            toVisit.AddParameters(supportPointToReplace, new FileBasedParameters("mock"));

            var toUpdate = new Dictionary<SupportPoint, SupportPoint>();
            toUpdate.Add(supportPointToReplace, replacedWithSupportPoint);

            var result = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            
            result.Visit(toVisit);

            // what to Assert.
            Assert.IsFalse(toVisit.Data.ContainsKey(supportPointToReplace));
            Assert.IsTrue(toVisit.Data.ContainsKey(replacedWithSupportPoint));
            Assert.AreEqual(1, toVisit.Data.Count);
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SupportPointRemoved()
        {
            var supportPointToReplace = new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>());
            
            var toVisit = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            toVisit.AddParameters(supportPointToReplace, new FileBasedParameters("mock"));

            var toUpdate = new Dictionary<SupportPoint, SupportPoint>();
            toUpdate.Add(supportPointToReplace, null);

            var result = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);


            result.Visit(toVisit);

            // what to Assert.
            Assert.IsFalse(toVisit.Data.ContainsKey(supportPointToReplace));
            Assert.AreEqual(0, toVisit.Data.Count);
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SupportPointIgnored()
        {
            var untouchedSupportPoint = new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var replacedWithSupportPoint = new SupportPoint(20, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var toVisit = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            toVisit.AddParameters(untouchedSupportPoint, new FileBasedParameters("mock"));

            var toUpdate = new Dictionary<SupportPoint, SupportPoint>();
            toUpdate.Add(new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>()), replacedWithSupportPoint);

            var result = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);


            result.Visit(toVisit);

            // what to Assert.
            Assert.IsTrue(toVisit.Data.ContainsKey(untouchedSupportPoint));
            Assert.IsFalse(toVisit.Data.ContainsKey(replacedWithSupportPoint));
            Assert.AreEqual(1, toVisit.Data.Count);
        }

    }
}