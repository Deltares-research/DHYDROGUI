using System.ComponentModel;
using DHYDRO.Common.Extensions;
using NUnit.Framework;
using CategoryAttribute = System.ComponentModel.CategoryAttribute;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace DHYDRO.Common.Tests.Extensions
{
    [TestFixture]
    public class EnumExtensionsTest
    {
        private const SimpleEnum invalidEnumValue = (SimpleEnum)(-1);

        [Test]
        public void GetDescription_UndefinedEnumValue_ThrowsInvalidEnumArgumentException()
        {
            Assert.Throws<InvalidEnumArgumentException>(() => invalidEnumValue.GetDescription());
        }

        [Test]
        public void GetDescription_EnumValueWithoutDescription_ReturnsEnumFieldName()
        {
            Assert.AreEqual(nameof(SimpleEnum.Option1), SimpleEnum.Option1.GetDescription());
        }

        [Test]
        public void GetDescription_EnumValueWitDescription_ReturnsDescription()
        {
            Assert.AreEqual("First Option Description", DescriptionEnum.Option1.GetDescription());
        }

        [Test]
        public void GetDescription_EnumValueWithCategoryAndDescription_ReturnsDescription()
        {
            Assert.AreEqual("Mixed First Option Description", MixedAttributesEnum.Option1.GetDescription());
        }

        private enum SimpleEnum
        {
            Option1
        }

        private enum DescriptionEnum
        {
            [Description("First Option Description")]
            Option1
        }

        private enum MixedAttributesEnum
        {
            [Category("Mixed Options")]
            [Description("Mixed First Option Description")]
            Option1
        }
    }
}