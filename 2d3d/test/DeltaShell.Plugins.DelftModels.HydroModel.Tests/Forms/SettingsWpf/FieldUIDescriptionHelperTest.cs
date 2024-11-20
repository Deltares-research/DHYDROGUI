using System;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class FieldUIDescriptionHelperTest
    {
        [Test]
        public void CreateFieldDescription_FieldDescriptionNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => FieldUIDescriptionHelper.CreateFieldDescription(null, null, null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("fieldDescription"));
        }

        [Test]
        public void CreateFieldDescription_WithFieldDescription_ReturnsFieldUIDescriptionWithSameProperties()
        {
            // Setup
            var random = new Random(21);
            var fieldDescription = new FieldUIDescription(null, null)
            {
                Category = "Category",
                SubCategory = "SubCategory",
                Label = "Label",
                Name = "Name",
                ValueType = typeof(object),
                ToolTip = "ToolTip",
                MaxValue = random.NextDouble(),
                MinValue = random.NextDouble(),
                UnitSymbol = "Symbol",
                IsReadOnly = true
            };

            // Call
            FieldUIDescription newDescription = FieldUIDescriptionHelper.CreateFieldDescription(fieldDescription, null, null);

            // Assert
            Assert.That(newDescription.Category, Is.EqualTo(fieldDescription.Category));
            Assert.That(newDescription.SubCategory, Is.EqualTo(fieldDescription.SubCategory));
            Assert.That(newDescription.Label, Is.EqualTo(fieldDescription.Label));
            Assert.That(newDescription.Name, Is.EqualTo(fieldDescription.Name));
            Assert.That(newDescription.ValueType, Is.EqualTo(fieldDescription.ValueType));
            Assert.That(newDescription.ToolTip, Is.EqualTo(fieldDescription.ToolTip));
            Assert.That(newDescription.MaxValue, Is.EqualTo(fieldDescription.MaxValue));
            Assert.That(newDescription.MinValue, Is.EqualTo(fieldDescription.MinValue));
            Assert.That(newDescription.UnitSymbol, Is.EqualTo(fieldDescription.UnitSymbol));
            Assert.That(newDescription.IsReadOnly, Is.EqualTo(fieldDescription.IsReadOnly));
        }

        [Test]
        public void CreateFieldDescription_WithFieldDescriptionAndValidationMethod_ReturnsExpectedFieldDescription()
        {
            // Setup
            object dataFunctionArgument = null;
            object valueFunctionArgument = null;
            const string message = "ValidationMessage";
            Func<object, object, string> validationFunc = (data, value) =>
            {
                dataFunctionArgument = data;
                valueFunctionArgument = value;
                return message;
            };

            var fieldDescription = new FieldUIDescription(null, null) {ValidationMethod = validationFunc};

            // Call
            FieldUIDescription newDescription = FieldUIDescriptionHelper.CreateFieldDescription(fieldDescription, null, null);

            var dataArgument = new object();
            var valueArgument = new object();
            newDescription.Validate(dataArgument, valueArgument, out string validationMessage);

            // Assert
            Assert.That(dataFunctionArgument, Is.SameAs(dataArgument));
            Assert.That(valueFunctionArgument, Is.SameAs(valueArgument));
            Assert.That(validationMessage, Is.EqualTo(message));
        }

        [Test]
        public void CreateFieldDescription_WithFieldDescriptionAndIsVisibleMethod_ReturnsExpectedFieldDescription()
        {
            // Setup
            object valueFunctionArgument = null;
            const bool isVisible = true;
            Func<object, bool> isVisibleFunc = value =>
            {
                valueFunctionArgument = value;
                return isVisible;
            };

            var fieldDescription = new FieldUIDescription(null, null) {VisibilityMethod = isVisibleFunc};

            // Call
            FieldUIDescription newDescription = FieldUIDescriptionHelper.CreateFieldDescription(fieldDescription, null, null);

            var valueArgument = new object();
            bool actualIsVisible = newDescription.IsVisible(valueArgument);

            // Assert
            Assert.That(valueFunctionArgument, Is.SameAs(valueArgument));
            Assert.That(actualIsVisible, Is.EqualTo(isVisible));
        }

        [Test]
        public void CreateFieldDescription_WithFieldDescriptionAndIsEnabledMethod_ReturnsExpectedFieldDescription()
        {
            // Setup
            object valueFunctionArgument = null;
            const bool isEnabled = true;
            Func<object, bool> isEnabledFunc = value =>
            {
                valueFunctionArgument = value;
                return isEnabled;
            };

            var fieldDescription = new FieldUIDescription(null, null);
            fieldDescription.SetIsEnabledFunc(isEnabledFunc);

            // Call
            FieldUIDescription newDescription = FieldUIDescriptionHelper.CreateFieldDescription(fieldDescription, null, null);

            var valueArgument = new object();
            bool actualIsEnabled = newDescription.IsEnabled(valueArgument);

            // Assert
            Assert.That(valueFunctionArgument, Is.SameAs(valueArgument));
            Assert.That(actualIsEnabled, Is.EqualTo(isEnabled));
        }

        [Test]
        public void CreateFieldDescription_WithGetValueFunc_ReturnsExpectedFieldDescription()
        {
            // Setup
            object originalDataArgument = null;
            Func<object, object> originalGetValueFunc = data =>
            {
                originalDataArgument = data;
                return new object();
            };
            var fieldDescription = new FieldUIDescription(originalGetValueFunc, null);

            object newDataArgument = null;
            var newReturnValue = new object();
            Func<object, object> newGetValueFunc = data =>
            {
                newDataArgument = data;
                return newReturnValue;
            };

            // Call
            FieldUIDescription newDescription = FieldUIDescriptionHelper.CreateFieldDescription(fieldDescription, newGetValueFunc, null);

            var dataArgument = new object();
            object returnValue = newDescription.GetValue(dataArgument);

            // Assert
            Assert.That(originalDataArgument, Is.Null);
            Assert.That(newDataArgument, Is.SameAs(dataArgument));
            Assert.That(returnValue, Is.SameAs(newReturnValue));
        }

        [Test]
        public void CreateFieldDescription_WithSetValueAction_ReturnsExpectedFieldDescription()
        {
            // Setup
            object originalDataArgument = null;
            object originalValueArgument = null;
            Action<object, object> originalSetValueAction = (data, value) =>
            {
                originalDataArgument = data;
                originalValueArgument = value;
            };
            var fieldDescription = new FieldUIDescription(null, originalSetValueAction);

            object newDataArgument = null;
            object newValueArgument = null;
            Action<object, object> newSetValueAction = (data, value) =>
            {
                newDataArgument = data;
                newValueArgument = value;
            };

            // Call
            FieldUIDescription newDescription = FieldUIDescriptionHelper.CreateFieldDescription(fieldDescription, null, newSetValueAction);

            var dataArgument = new object();
            var valueArgument = new object();
            newDescription.SetValue(dataArgument, valueArgument);

            // Assert
            Assert.That(originalDataArgument, Is.Null);
            Assert.That(originalValueArgument, Is.Null);
            Assert.That(newDataArgument, Is.SameAs(dataArgument));
            Assert.That(newValueArgument, Is.SameAs(valueArgument));
        }
    }
}