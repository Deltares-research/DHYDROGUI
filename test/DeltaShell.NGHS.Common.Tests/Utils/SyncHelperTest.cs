using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class SyncHelperTest
    {
        [Test]
        public void GetSyncNotifyCollectionChangedEventHandler_ArgumentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => SyncHelper.GetSyncNotifyCollectionChangedEventHandler<object>(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetSyncNotifyCollectionChangedEventHandler_InvokeWithSenderOfOtherType_DoesNothing()
        {
            // Setup
            var list = new EventedList<string>
            {
                "a",
                "b",
                "c",
            };
            NotifyCollectionChangedEventHandler eventHandler = SyncHelper.GetSyncNotifyCollectionChangedEventHandler(list);

            // Call
            eventHandler.Invoke(new EventedList<object>(),
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            // Assert
            Assert.That(list, Has.Count.EqualTo(3));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(NotifyCollectionChangedEventArgsCases))]
        public void GetSyncNotifyCollectionChangedEventHandler_Invoke_SyncsList(NotifyCollectionChangedEventArgs eventArgs, IList<string> expList)
        {
            // Setup
            var list = new EventedList<string>
            {
                "a",
                "b",
                "c",
            };
            NotifyCollectionChangedEventHandler eventHandler = SyncHelper.GetSyncNotifyCollectionChangedEventHandler(list);

            // Call
            eventHandler.Invoke(new EventedList<string>(), eventArgs);

            // Assert
            Assert.That(list, Is.EqualTo(expList));
        }

        private static IEnumerable<TestCaseData> NotifyCollectionChangedEventArgsCases()
        {
            var changedItems = new List<string>()
            {
                "b",
                "d",
                "e"
            };
            var eventArgsAdd = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems);
            var expectedListAdd = new List<string>
            {
                "a",
                "b",
                "c",
                "b",
                "d",
                "e"
            };
            yield return new TestCaseData(eventArgsAdd, expectedListAdd);

            var eventArgsRemove = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems);
            var expectedListRemove = new List<string>
            {
                "a",
                "c",
            };
            yield return new TestCaseData(eventArgsRemove, expectedListRemove);
        }
    }
}