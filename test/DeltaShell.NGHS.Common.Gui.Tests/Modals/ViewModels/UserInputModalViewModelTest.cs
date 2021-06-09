using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.NGHS.Common.Gui.Modals.ViewModels;
using DeltaShell.NGHS.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.Modals.ViewModels
{
    [TestFixture]
    public class UserInputModalViewModelTest
    {
        public enum TestEnum
        {
            One,
            Two,
            Three,
        }

        [Test]
        public void ConstructorExpectedResults()
        {
            // Call
            var viewModel = new UserInputModalViewModel<TestEnum>();

            // Assert
            Assert.That(viewModel, Is.InstanceOf<UserInputModalViewModelBase>());
            Assert.That(viewModel.Title, Is.Null);
            Assert.That(viewModel.Text, Is.Null);
            Assert.That(viewModel.InternalResult, Is.Null);
            Assert.That(viewModel.UserInputOptions, Is.EqualTo(new[] { TestEnum.One, TestEnum.Two, TestEnum.Three }));
        }

        private static IEnumerable<TestCaseData> GetPropertyChangedData()
        {
            void InternalResultChanged(UserInputModalViewModel<TestEnum> vm) =>
                vm.InternalResult = TestEnum.One;
            yield return new TestCaseData((Action<UserInputModalViewModel<TestEnum>>)InternalResultChanged,
                                          nameof(UserInputModalViewModel<TestEnum>.InternalResult));

            void TextChanged(UserInputModalViewModel<TestEnum> vm) =>
                vm.Text = "SomeText";
            yield return new TestCaseData((Action<UserInputModalViewModel<TestEnum>>)TextChanged,
                                          nameof(UserInputModalViewModel<TestEnum>.Text));

            void TitleChanged(UserInputModalViewModel<TestEnum> vm) =>
                vm.Title = "SomeTitle";
            yield return new TestCaseData((Action<UserInputModalViewModel<TestEnum>>)TitleChanged,
                                          nameof(UserInputModalViewModel<TestEnum>.Title));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyChangedData))]
        public void PropertyChanged_CorrectlyRaisesEvent(Action<UserInputModalViewModel<TestEnum>> changeAction, string expectedPropertyName)
        {
            // Setup
            var viewModel = new UserInputModalViewModel<TestEnum>();
            var observer = new EventTestObserver<PropertyChangedEventArgs>();

            viewModel.PropertyChanged += observer.OnEventFired;

            // Call
            changeAction.Invoke(viewModel);

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders[0], Is.SameAs(viewModel));
            Assert.That(observer.EventArgses[0].PropertyName, Is.EqualTo(expectedPropertyName));
        }

        [Test]
        [TestCase(null)]
        [TestCase(TestEnum.One)]
        [TestCase(TestEnum.Two)]
        [TestCase(TestEnum.Three)]
        public void Result_ContainsExpectedValue(TestEnum? enumValue)
        {
            // Setup
            var viewModel = new UserInputModalViewModel<TestEnum>();

            // Call
            viewModel.InternalResult = enumValue;

            // Assert
            Assert.That(viewModel.Result, Is.EqualTo(enumValue));
        }
    }
}