using System;
using System.Collections.Generic;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
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
            var called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate { called++; };
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
            var called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate { called++; };
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
            var called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate { called++; };

            var networkCoverage = new NetworkCoverage {IsTimeDependent = true};

            var dataItem = Mocks.StrictMock<IDataItem>();
            dataItem.Expect(di => di.Value).Return(networkCoverage).Repeat.Any();
            Mocks.ReplayAll();
            eventedCoverages.Add(dataItem);

            called.Should("Delegate was called before it has time values!").Be.EqualTo(0);

            networkCoverage.Time.AddValues(new[]
            {
                DateTime.MinValue
            });

            called.Should("Delegate was not called after adding time values").Be.EqualTo(1);
        }

        [Test]
        public void DelegateCalledForTimeDependentCoverageWithTimes()
        {
            var called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate { called++; };
            var dataItem = Mocks.StrictMock<IDataItem>();
            var networkCoverage = Mocks.StrictMock<INetworkCoverage>();
            dataItem.Expect(di => di.Value).Return(networkCoverage).Repeat.Any();
            networkCoverage.Expect(nc => nc.IsTimeDependent).Return(true).Repeat.Any();

            var time = new Variable<DateTime>();
            time.AddValues(new[]
            {
                DateTime.MinValue
            });

            networkCoverage.Expect(nc => nc.Time).Return(time).Repeat.Any();

            Mocks.ReplayAll();

            eventedCoverages.Add(dataItem);

            Mocks.VerifyAll();
            called.Should("Delegate not called once!").Be.EqualTo(1);
        }

        [Test]
        public void DelegateCalledForInitialCoverages()
        {
            var called = 0;

            var initialCoverages = new List<ICoverage>();
            var networkCoverage = Mocks.StrictMock<INetworkCoverage>();
            var featureCoverage = Mocks.StrictMock<IFeatureCoverage>();

            networkCoverage.Expect(nc => nc.IsTimeDependent).Return(false).Repeat.Any();
            featureCoverage.Expect(fc => fc.IsTimeDependent).Return(false).Repeat.Any();

            Mocks.ReplayAll();

            initialCoverages.Add(networkCoverage);
            initialCoverages.Add(featureCoverage);

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, null, initialCoverages);

            networkSideViewCoverageManager.OnCoverageAddedToProject = delegate(ICoverage c)
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
            var called = 0;

            var eventedCoverages = new EventedList<IDataItem>();

            var networkSideViewCoverageManager = new NetworkSideViewCoverageManager(null, eventedCoverages, null);

            networkSideViewCoverageManager.OnCoverageRemovedFromProject = delegate { called++; };

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
    }
}