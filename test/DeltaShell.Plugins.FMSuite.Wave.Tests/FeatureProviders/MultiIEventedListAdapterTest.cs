using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.FeatureProviders
{
    [TestFixture]
    public class MultiIEventedListAdapterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;
            object CreateDisplayedValueFunc(object _) => null;

            // Call
            var list = new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Assert
            Assert.That(list, Is.InstanceOf<IEventedList<object>>());
            Assert.That(list, Is.InstanceOf<IList>());
            Assert.That(list, Has.Count.EqualTo(0));
        }

        [Test]
        public void Constructor_ObtainObservedValueFuncNull_ThrowsArgumentNullException()
        {
            // Setup
            object CreateDisplayedValueFunc(object _) => null;

            // Call
            void Call() => new MultiIEventedListAdapter<object, object>(null, CreateDisplayedValueFunc);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("obtainObservedValueFunc"));
        }

        [Test]
        public void Constructor_CreateDisplayedValueFuncNull_ThrowsArgumentNullException()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;

            // Call
            void Call() => new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("createDisplayedValueFunc"));
        }

        [Test]
        public void RegisterList_ObservedListNull_ThrowsArgumentNullException()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;
            object CreateDisplayedValueFunc(object _) => null;

            var list = new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Call
            void Call() => list.RegisterList(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("observedList"));
        }

        [Test]
        public void GivenAnObservedList_WhenRegisterListIsCalled_ThenTheContentsAreAddedToTheAdapter()
        {
            // Setup
            var object1 = new Tuple<object>(new object());
            var object2 = new Tuple<object>(new object());

            var list = new EventedList<Tuple<object>>
            {
                object1,
                object2,
            };

            Tuple<Tuple<object>, IEventedList<Tuple<object>>> ObtainObservedValueFunc(object val)
            {
                if (val == object1.Item1)
                {
                    return new Tuple<Tuple<object>, IEventedList<Tuple<object>>>(object1, list);
                }

                if (val == object2.Item1)
                {
                    return new Tuple<Tuple<object>, IEventedList<Tuple<object>>>(object2, list);
                }

                Assert.Fail("This function is called unexpectedly.");
                return null;
            }

            object CreateDisplayedValueFunc(Tuple<object> val) => val.Item1;

            var adapter = new MultiIEventedListAdapter<Tuple<object>, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Precondition
            Assert.That(adapter, Is.Empty, "Expected the list to be empty.");

            // Call
            adapter.RegisterList(list);

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(2), 
                        "Expected the list to contain the two elements from the added list.");
            Assert.That(adapter, Has.Member(object1.Item1),
                        $"Expected the object to be contained in the {nameof(MultiIEventedListAdapter<object, object>)}.");
            Assert.That(adapter, Has.Member(object2.Item1),
                        $"Expected the object to be contained in the {nameof(MultiIEventedListAdapter<object, object>)}.");

        }

        private IEventedList<IWaveBoundary> GetEventedList()
        {
            var geomDef1 = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var object1 = Substitute.For<IWaveBoundary>();
            object1.GeometricDefinition.Returns(geomDef1);

            var geomDef2 = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var object2 = Substitute.For<IWaveBoundary>();
            object2.GeometricDefinition.Returns(geomDef2);

            var list = new EventedList<IWaveBoundary>
            {
                object1,
                object2,
            };

            return list;
        }

        private MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> GetAdapterWithRegisteredList(IEventedList<IWaveBoundary> list)
        {
            var listOriginal = list.ToArray();
            Tuple<IWaveBoundary, IEventedList<IWaveBoundary>> ObtainObservedValueFunc(IWaveBoundaryGeometricDefinition geomDef)
            {
                foreach (var value in listOriginal)
                {
                    if (value.GeometricDefinition == geomDef)
                    {
                        return new Tuple<IWaveBoundary, IEventedList<IWaveBoundary>>(value, list);
                    }
                }

                return null;
            }

            IWaveBoundaryGeometricDefinition CreateDisplayedValueFunc(IWaveBoundary waveBoundary) =>
                waveBoundary.GeometricDefinition;

            var adapter = new MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition>(
                ObtainObservedValueFunc, 
                CreateDisplayedValueFunc);

            return adapter;

        }

        // TODO refactor these tests

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenAnElementIsAddedToThisList_ThenTheAdapterIsUpdated()
        { 
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newObservedObject = Substitute.For<IWaveBoundary>();
            newObservedObject.GeometricDefinition.Returns(geometricDefinition);

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list.Add(newObservedObject);

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(3), "Expected a single item to be added.");
            Assert.That(adapter, Has.Member(geometricDefinition), "Expected the specified object to be part of the adapter");
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenMultipleElementsAreAddedIsAddedToThisList_ThenTheAdapterIsUpdated()
        { 
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var geometricDefinition1 = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newObservedObject1 = Substitute.For<IWaveBoundary>();
            newObservedObject1.GeometricDefinition.Returns(geometricDefinition1);

            var geometricDefinition2 = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newObservedObject2 = Substitute.For<IWaveBoundary>();
            newObservedObject2.GeometricDefinition.Returns(geometricDefinition2);

            var newItems = new List<IWaveBoundary>
            {
                newObservedObject1,
                newObservedObject2,
            };

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list.AddRange(newItems);

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(4), "Expected a single item to be added.");
            Assert.That(adapter, Has.Member(geometricDefinition1), "Expected the specified object to be part of the adapter");
            Assert.That(adapter, Has.Member(geometricDefinition2), "Expected the specified object to be part of the adapter");
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenAnItemIsRemoved_ThenTheAdapterIsUpdated()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var itemToRemove = list.Last();

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list.Remove(itemToRemove);

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(1), "Expected a single item to be added.");
            Assert.That(adapter, Has.Member(list[0].GeometricDefinition), "Expected the specified object to be part of the adapter");
            Assert.That(adapter, Has.No.Member(itemToRemove.GeometricDefinition), "Expected the specified object to be removed.");
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenTheListIsReset_ThenTheAdapterIsUpdated()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var itemToRemove1 = list.First();
            var itemToRemove2 = list.Last();

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list.Clear();

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(0), "Expected a single item to be added.");
            Assert.That(adapter, Has.No.Member(itemToRemove1.GeometricDefinition), "Expected the specified object to be removed.");
            Assert.That(adapter, Has.No.Member(itemToRemove2.GeometricDefinition), "Expected the specified object to be removed.");
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenAnItemIsReplaced_ThenTheAdapterIsUpdated()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var itemToRemove = list[0];
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newItem = Substitute.For<IWaveBoundary>();
            newItem.GeometricDefinition.Returns(geometricDefinition);


            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list[0] = newItem;

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected a single item to be added.");
            Assert.That(adapter, Has.No.Member(itemToRemove), "Expected the specified object to be removed.");
            Assert.That(adapter, Has.Member(newItem.GeometricDefinition), "Expected the specified object to have been added.");
            Assert.That(adapter, Has.Member(list[1].GeometricDefinition), "Expected the specified object to be still part of the adapter.");
        }

        [Test]
        public void GivenAnObservedList_WhenAddListIsCalled_ThenACollectionChangedEventIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);

            object lastSender = null;
            NotifyCollectionChangedEventArgs lastArgs = null;
            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
                lastSender = sender;
                lastArgs = args;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Call
            adapter.RegisterList(list);

            // Assert
            Assert.That(nCalls, Is.EqualTo(1), "Expected one callback.");
            Assert.That(lastSender, Is.SameAs(adapter), "Expected the sender to be different:");

            Assert.That(lastArgs, Is.Not.Null, "Expected the event args to not be null:");
            Assert.That(lastArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add),
                        "Expected elements to be added.");

            Assert.That(lastArgs.OldItems, Is.Null);
            Assert.That(lastArgs.NewItems, Has.Count.EqualTo(list.Count));
            foreach (IWaveBoundary observedItem in list)
            {
                Assert.That(lastArgs.NewItems, Has.Member(observedItem.GeometricDefinition));
            }
        }

        [Test]
        public void GivenAnEmptyObservedList_WhenRegisterListIsCalled_ThenNoCollectionChangedEventIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = new EventedList<IWaveBoundary>();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);

            var nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Call
            adapter.RegisterList(list);

            // Assert
            Assert.That(nCalls, Is.EqualTo(0), "Expected no callback.");
        }


        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenAnElementIsAddedToThisList_ThenACollectionChangedEventIsInvoked()
        { 
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newObservedObject = Substitute.For<IWaveBoundary>();
            newObservedObject.GeometricDefinition.Returns(geometricDefinition);

            object lastSender = null;
            NotifyCollectionChangedEventArgs lastArgs = null;
            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
                lastSender = sender;
                lastArgs = args;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list.Add(newObservedObject);

            // Assert
            Assert.That(nCalls, Is.EqualTo(1), "Expected one callback.");
            Assert.That(lastSender, Is.SameAs(adapter), "Expected the sender to be different:");

            Assert.That(lastArgs, Is.Not.Null, "Expected the event args to not be null:");
            Assert.That(lastArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add),
                        "Expected elements to be added.");

            Assert.That(lastArgs.OldItems, Is.Null);
            Assert.That(lastArgs.NewItems, Has.Count.EqualTo(1));
            Assert.That(lastArgs.NewItems, Has.Member(newObservedObject.GeometricDefinition));
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenAnItemIsRemoved_ThenACollectionChangedEventIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var itemToRemove = list.Last();

            object lastSender = null;
            NotifyCollectionChangedEventArgs lastArgs = null;
            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
                lastSender = sender;
                lastArgs = args;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list.Remove(itemToRemove);

            // Assert
            Assert.That(nCalls, Is.EqualTo(1), "Expected one callback.");
            Assert.That(lastSender, Is.SameAs(adapter), "Expected the sender to be different:");

            Assert.That(lastArgs, Is.Not.Null, "Expected the event args to not be null:");
            Assert.That(lastArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove),
                        "Expected elements to be added.");

            Assert.That(lastArgs.NewItems, Is.Null);
            Assert.That(lastArgs.OldItems, Has.Count.EqualTo(1));
            Assert.That(lastArgs.OldItems, Has.Member(itemToRemove.GeometricDefinition));
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenAnItemIsReplaced_ThenACollectionChangedEventIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            var itemToRemove = list[0];
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newItem = Substitute.For<IWaveBoundary>();
            newItem.GeometricDefinition.Returns(geometricDefinition);

            object lastSender = null;
            NotifyCollectionChangedEventArgs lastArgs = null;
            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
                lastSender = sender;
                lastArgs = args;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), "Expected the adapter to have two members.");

            // Call
            list[0] = newItem;

            // Assert
            Assert.That(nCalls, Is.EqualTo(1), "Expected one callback.");
            Assert.That(lastSender, Is.SameAs(adapter), "Expected the sender to be different:");

            Assert.That(lastArgs, Is.Not.Null, "Expected the event args to not be null:");
            Assert.That(lastArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace),
                        "Expected elements to be added.");

            Assert.That(lastArgs.NewItems, Has.Count.EqualTo(1));
            Assert.That(lastArgs.NewItems, Has.Member(newItem.GeometricDefinition));
            Assert.That(lastArgs.OldItems, Has.Count.EqualTo(1));
            Assert.That(lastArgs.OldItems, Has.Member(itemToRemove.GeometricDefinition));
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenDeregisterListIsCalled_ThenTheListContentsAreRemoved()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), 
                        "Expected the adapter to have two members.");

            // Call
            adapter.DeregisterList(list);

            // Assert
            Assert.That(adapter, Has.Count.EqualTo(0),
                        "Expected the adapter to be empty.");
        }

        [Test]
        public void GivenAnAdapterWithARegisteredList_WhenDeregisterListIsCalled_ThenACollectionChangedIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            object lastSender = null;
            NotifyCollectionChangedEventArgs lastArgs = null;
            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
                lastSender = sender;
                lastArgs = args;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Precondition
            Assert.That(adapter, Has.Count.EqualTo(2), 
                        "Expected the adapter to have two members.");

            // Call
            adapter.DeregisterList(list);

            // Assert
            Assert.That(nCalls, Is.EqualTo(1), "Expected one callback.");
            Assert.That(lastSender, Is.SameAs(adapter), "Expected the sender to be different:");

            Assert.That(lastArgs, Is.Not.Null, "Expected the event args to not be null:");
            Assert.That(lastArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove),
                        "Expected elements to be added.");

            Assert.That(lastArgs.NewItems, Is.Null);
            Assert.That(lastArgs.OldItems, Has.Count.EqualTo(list.Count));
            foreach (var value in list)
            {
                Assert.That(lastArgs.OldItems, Has.Member(value.GeometricDefinition));
            }
        }

        [Test]
        public void GivenAnAdapterWithAnEmptyRegisteredList_WhenDeregisterListIsCalled_ThenNoCollectionChangedIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = new EventedList<IWaveBoundary>();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Call
            adapter.DeregisterList(list);

            // Assert
            Assert.That(nCalls, Is.EqualTo(0), "Expected no callback.");
        }


        [Test]
        public void DeregisterList_CallingArgumentNameNull_ThrowsArgumentNullException()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;
            object CreateDisplayedValueFunc(object _) => null;

            var list = new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Call
            void Call() => list.DeregisterList(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("observedList"));
        }

        [Test]
        public void GivenAnAdapterWithDeregisteredList_WhenAnElementIsAdded_ThenNoCollectionChangedIsInvoked()
        {
            // Setup
            IEventedList<IWaveBoundary> list = 
                GetEventedList();
            MultiIEventedListAdapter<IWaveBoundary, IWaveBoundaryGeometricDefinition> adapter =
                GetAdapterWithRegisteredList(list);
            adapter.RegisterList(list);

            Assert.That(adapter, Has.Count.EqualTo(2), 
                        "Expected the adapter to have two members.");

            adapter.DeregisterList(list);

            Assert.That(adapter, Has.Count.EqualTo(0), 
                        "Expected the adapter to empty.");

            object lastSender = null;
            NotifyCollectionChangedEventArgs lastArgs = null;
            int nCalls = 0;

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                nCalls += 1;
                lastSender = sender;
                lastArgs = args;
            }

            adapter.CollectionChanged += OnCollectionChanged;

            // Call
            list.Add(Substitute.For<IWaveBoundary>());

            // Assert
            Assert.That(nCalls, Is.EqualTo(0), "Expected no callback.");
        }

        [Test]
        public void Clear_ThrowsNotSupportedException()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;
            object CreateDisplayedValueFunc(object _) => null;

            ICollection<object> list = new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Call
            void Call() => list.Clear();

            // Assert
            var exception = Assert.Throws<NotSupportedException>(Call);
            Assert.That(exception, Has.Message.EqualTo("This operation is currently not supported."));
        }

        [Test]
        public void IsReadOnly_ReturnsFalse()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;
            object CreateDisplayedValueFunc(object _) => null;

            var list = new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Call
            bool result = list.IsReadOnly;

            // Assert
            Assert.That(result, Is.False, "Expected a different result for IsReadOnly:");
        }

        [Test]
        public void IsFixedSize_ReturnsFalse()
        {
            // Setup
            Tuple<object, IEventedList<object>> ObtainObservedValueFunc(object _) => null;
            object CreateDisplayedValueFunc(object _) => null;

            var list = new MultiIEventedListAdapter<object, object>(ObtainObservedValueFunc, CreateDisplayedValueFunc);

            // Call
            bool result = list.IsFixedSize;

            // Assert
            Assert.That(result, Is.False, "Expected a different result for IsFixedSize:");
        }
    }
}