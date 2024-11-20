using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewCoverageManagerTest
    {
        private static readonly MockRepository Mocks = new MockRepository();
        
        [Test]
        public void DelegateCalledForNonTimeDependentCoverage()
        {
            int called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate
                                                                          {
                                                                              called++;
                                                                          };
            var dataItem = Mocks.StrictMock<IDataItem>();
            var networkCoverage = Mocks.StrictMock<INetworkCoverage>();
            dataItem.Expect(di => di.Value).Return(networkCoverage).Repeat.Any();
            networkCoverage.Expect(nc => nc.IsTimeDependent).Return(false).Repeat.Any();

            Mocks.ReplayAll();

            eventedCoverages.Add(dataItem);

            Mocks.VerifyAll();
            called.Should("Delegate not called once!").Be.EqualTo(1);
        }

        [Test]
        public void DelegateNotCalledForTimeDependentCoverageWithoutTimes()
        {
            int called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate
            {
                called++;
            };
            var dataItem = Mocks.StrictMock<IDataItem>();
            var networkCoverage = Mocks.StrictMock<INetworkCoverage>();
            dataItem.Expect(di => di.Value).Return(networkCoverage).Repeat.Any();
            networkCoverage.Expect(nc => nc.IsTimeDependent).Return(true).Repeat.Any();
            networkCoverage.Expect(nc => nc.Time).Return(new Variable<DateTime>()).Repeat.Any();

            Mocks.ReplayAll();

            eventedCoverages.Add(dataItem);

            Mocks.VerifyAll();
            called.Should("Delegate was called!").Be.EqualTo(0);
        }

        [Test]
        public void DelegateCalledOnceTimesAreAdded()
        {
            int called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate
            {
                called++;
            };

            var networkCoverage = new NetworkCoverage {IsTimeDependent = true};

            var dataItem = Mocks.StrictMock<IDataItem>();
            dataItem.Expect(di => di.Value).Return(networkCoverage).Repeat.Any();
            Mocks.ReplayAll();
            eventedCoverages.Add(dataItem);

            called.Should("Delegate was called before it has time values!").Be.EqualTo(0);

            networkCoverage.Time.AddValues(new[] { DateTime.MinValue });

            called.Should("Delegate was not called after adding time values").Be.EqualTo(1);
        }

        [Test]
        public void DelegateCalledForTimeDependentCoverageWithTimes()
        {
            int called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate
            {
                called++;
            };
            var dataItem = Mocks.StrictMock<IDataItem>();
            var networkCoverage = Mocks.StrictMock<INetworkCoverage>();
            dataItem.Expect(di => di.Value).Return(networkCoverage).Repeat.Any();
            networkCoverage.Expect(nc => nc.IsTimeDependent).Return(true).Repeat.Any();

            var time = new Variable<DateTime>();
            time.AddValues(new []{DateTime.MinValue});

            networkCoverage.Expect(nc => nc.Time).Return(time).Repeat.Any();

            Mocks.ReplayAll();

            eventedCoverages.Add(dataItem);

            Mocks.VerifyAll();
            called.Should("Delegate not called once!").Be.EqualTo(1);
        }

        [Test]
        public void DelegateCalledForInitialCoverages()
        {
            int called = 0;

            var initialCoverages = new List<ICoverage>();
            var networkCoverage = Mocks.StrictMock<INetworkCoverage>();
            var featureCoverage = Mocks.StrictMock<IFeatureCoverage>();
            
            networkCoverage.Expect(nc => nc.IsTimeDependent).Return(false).Repeat.Any();
            featureCoverage.Expect(fc => fc.IsTimeDependent).Return(false).Repeat.Any();

            Mocks.ReplayAll();

            initialCoverages.Add(networkCoverage);
            initialCoverages.Add(featureCoverage);

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, null, initialCoverages);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate (ICoverage c)
                                                                          {
                                                                              c.Should("Not called as expected").Be.EqualTo(
                                                                                  initialCoverages[called]);

                                                                              called++;
                                                                          };

            networkSideViewCoverageManager.RequestInitialCoverages();

            called.Should("Not called for each initial coverage!").Be.EqualTo(initialCoverages.Count);
        }
        
        [Test]
        public void DelegateCalledOnRemove()
        {
            int called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageRemovedFromProject = delegate
            {
                called++;
            };

            var dataItem = Mocks.StrictMock<IDataItem>();
            var featureCoverage = Mocks.StrictMock<IFeatureCoverage>();
            dataItem.Expect(di => di.Value).Return(featureCoverage).Repeat.Any();
            featureCoverage.Expect(fc => fc.IsTimeDependent).Return(false).Repeat.Any();
            
            Mocks.ReplayAll();

            eventedCoverages.Add(dataItem);
            
            eventedCoverages.Remove(dataItem);
            
            Mocks.VerifyAll();

            called.Should("Delegate not called once!").Be.EqualTo(1);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DelegateCalledOnRouteRemove()
        {
            int called = 0;

            var project = new Project();
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var route = HydroNetworkHelper.AddNewRouteToNetwork(network);

            project.RootFolder.Add(network);

            new NetworkSideViewCoverageManager(route, (INotifyCollectionChange) project, null)
                {
                    OnRouteRemoved = delegate { called++; }
                };

            network.Routes.RemoveAt(0);

            called.Should("Delegate not called once!").Be.EqualTo(1);
        }

        [Test]
        public void ProjectPropertyChanged_IsEditingTrue_DisconnectingOutFilesEditAction_InvokesOnCoverageRemovedFromProjectDelegate()
        {
            // Setup
            const int numberOfCoverages = 3; // multiple coverages to see if the delegate is called once for each coverage
            int delegateInvokedCount = 0;

            var model = new WaterFlowFMModel();
            var initialNumberOfCoverages = model.GetAllItemsRecursive().OfType<ICoverage>().Distinct().Count();
            
            for (int i = 0; i < numberOfCoverages; i++)
            {
                model.Network.Routes.Add(new Route()); 
            }

            var project = new Project();
            project.RootFolder.Add(model);

            var manager = new NetworkSideViewCoverageManager(null, project, Enumerable.Empty<ICoverage>());
            manager.OnCoverageRemovedFromProject = coverage => delegateInvokedCount++;

            var editAction = Substitute.For<IEditAction>();
            editAction.Name.Returns("Disconnecting from output files");

            // Call
            model.BeginEdit(editAction);
            
            // Assert
            int expectedDelegateInvokedCount = numberOfCoverages + initialNumberOfCoverages;
            Assert.That(delegateInvokedCount, Is.EqualTo(expectedDelegateInvokedCount));
        }
        
        [Test]
        public void ProjectPropertyChanged_IsEditingFalse_ReconnectingOutputFilesEditAction_InvokesOnCoverageAddedToProjectDelegate()
        {
            // Setup
            const int numberOfCoverages = 3; // multiple coverages to see if the delegate is called once for each coverage
            int delegateInvokedCount = 0;

            var model = new WaterFlowFMModel();
            model.Status = ActivityStatus.Cleaning;
            var initialNumberOfCoverages = model.GetAllItemsRecursive().OfType<ICoverage>().Distinct().Count();
            
            for (int i = 0; i < numberOfCoverages; i++)
            {
                model.Network.Routes.Add(new Route()); 
            }

            var project = new Project();
            project.RootFolder.Add(model);

            var manager = new NetworkSideViewCoverageManager(null, project, Enumerable.Empty<ICoverage>());
            manager.OnCoverageAddedToProject = coverage => delegateInvokedCount++;

            var editAction = Substitute.For<IEditAction>();
            editAction.Name.Returns("Reconnect output files");
            model.BeginEdit(editAction);

            // Call
            model.EndEdit(); // set IsEditing to false
            
            // Assert
            int expectedDelegateInvokedCount = numberOfCoverages + initialNumberOfCoverages;
            Assert.That(delegateInvokedCount, Is.EqualTo(expectedDelegateInvokedCount));
        }
    }
}
