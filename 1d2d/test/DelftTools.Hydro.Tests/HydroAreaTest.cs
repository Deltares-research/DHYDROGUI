﻿using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroAreaTest
    {
        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenWeir2D_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<Weir2D>(setterName, expectedValue);
        }

        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenPump2D_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<Pump2D>(setterName, expectedValue);
        }

        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenGate2D_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<Gate2D>(setterName, expectedValue);
        }

        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenGroupableFeature2D_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<GroupableFeature2D>(setterName, expectedValue);
        }

        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenGroupableFeature2DPoint_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<GroupableFeature2DPoint>(setterName, expectedValue);
        }

        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenGroupableFeature2DPolygon_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<GroupableFeature2DPolygon>(setterName, expectedValue);
        }

        [Test]
        [TestCase("MyGroupName", "MyGroupName")]
        [TestCase(@"MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"MyGroupName\//\/MyFile", @"MyGroupName/MyFile")]
        [TestCase(@"//\/MyGroupName/MyFile", @"MyGroupName/MyFile")]
        [TestCase(null, null)]
        public void GivenGroupablePointFeature_WhenSettingGroupName_ThenItGetsStoredAsLowercaseAndForwardslashBecomesBackslash(string setterName, string expectedValue)
        {
            CheckGroupableGroupNameSetting<GroupablePointFeature>(setterName, expectedValue);
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void GivenGate2D_WhenCloning_ThenGroupableFeaturePropertiesAreTheSame(bool isDefaultGroupValue)
        {
            VerifyGroupableFeatureCloning<Gate2D>(isDefaultGroupValue);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenGroupableFeature2D_WhenCloning_ThenGroupableFeaturePropertiesAreTheSame(bool isDefaultGroupValue)
        {
            VerifyGroupableFeatureCloning<GroupableFeature2D>(isDefaultGroupValue);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenGroupableFeature2DPoint_WhenCloning_ThenGroupableFeaturePropertiesAreTheSame(bool isDefaultGroupValue)
        {
            VerifyGroupableFeatureCloning<GroupableFeature2DPoint>(isDefaultGroupValue);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenGroupableFeature2DPolygon_WhenCloning_ThenGroupableFeaturePropertiesAreTheSame(bool isDefaultGroupValue)
        {
            VerifyGroupableFeatureCloning<GroupableFeature2DPolygon>(isDefaultGroupValue);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenPump2D_WhenCloning_ThenGroupableFeaturePropertiesAreTheSame(bool isDefaultGroupValue)
        {
            VerifyGroupableFeatureCloning<Pump2D>(isDefaultGroupValue);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenWeir2D_WhenCloning_ThenGroupableFeaturePropertiesAreTheSame(bool isDefaultGroupValue)
        {
            VerifyGroupableFeatureCloning<Weir2D>(isDefaultGroupValue);
        }

        private static void VerifyGroupableFeatureCloning<T>(bool isDefaultGroupValue) where T : IGroupableFeature, new()
        {
            var groupName = "MyGroupName";
            var gate = new T
            {
                GroupName = groupName,
                IsDefaultGroup = isDefaultGroupValue
            };
            var clonedGate = (T) gate.Clone();
            Assert.That(clonedGate.GroupName, Is.EqualTo(groupName));
            Assert.That(clonedGate.IsDefaultGroup, Is.EqualTo(isDefaultGroupValue));
        }

        private void CheckGroupableGroupNameSetting<T>(string setterName, string expectedValue) where T : IGroupableFeature, new()
        {
            var feature2D = new T() { GroupName = setterName };
            Assert.That(feature2D.GroupName, Is.EqualTo(expectedValue));
        }
    }
}