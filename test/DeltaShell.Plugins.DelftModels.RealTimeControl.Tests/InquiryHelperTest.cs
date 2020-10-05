using System;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    internal class InquiryHelperTest
    {
        [Test]
        public void InquireContinuation_QueryNull_ThrowsArgumentNullException()
        {
            // Setup
            var inquiryHelper = new InquiryHelper();

            // Call
            TestDelegate call = () => inquiryHelper.InquireContinuation(null);

            // Assert
            Assert.That(call, Throws.InstanceOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("query"));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var inquiryHelper = new InquiryHelper();

            // Assert
            Assert.That(inquiryHelper, Is.InstanceOf<IInquiryHelper>());
        }
    }
}