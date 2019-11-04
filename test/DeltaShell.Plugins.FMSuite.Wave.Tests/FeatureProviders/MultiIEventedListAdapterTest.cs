using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders;
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