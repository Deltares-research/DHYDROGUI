using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView.ProfileMutators
{
    [TestFixture]
    public class ZWProfileMutatorTest
    {
        [Test]
        public void CanSetProfile()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            crossSection.GetProfileMutator().MovePoint(1, -30, 7);

            var profileY = new double[] { -50, -30, 0, 30, 50 };
            var profileZ = new double[] { 10, 7, 0, 7, 10 };

            var flowProfileY = new double[] { -30, -10, 0, 10, 30 };
            var flowProfileZ = new double[] { 10, 7, 0, 7, 10 };

            Assert.AreEqual(profileY, crossSection.GetProfile().Select(c => c.X).ToArray());
            Assert.AreEqual(profileZ, crossSection.GetProfile().Select(c => c.Y).ToArray());
            Assert.AreEqual(flowProfileY, crossSection.FlowProfile.Select(c => c.X).ToArray());
            Assert.AreEqual(flowProfileZ, crossSection.FlowProfile.Select(c => c.Y).ToArray());
        }

        [Test]
        public void SetProfileWithOutOfRangeHeightThrowsException()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            TestHelper.AssertLogMessageIsGenerated(() => crossSection.GetProfileMutator().MovePoint(0, -100, 5),
                                                   "Attempt to add invalid point to ZW-profile has been ignored.");
        }

        [Test]
        //[ExpectedException(typeof(ArgumentException), ExpectedMessage = "Change of level would change internal ordering of crossection values, which is not supported.")]
        public void SetFlowProfileWithOutOfRangeHeightThrowsException()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            TestHelper.AssertLogMessageIsGenerated(() => crossSection.GetFlowProfileMutator().MovePoint(0, 10, 5),
                                                   "Attempt to add invalid point to ZW-flow profile has been ignored.");
        }

        [Test]
        public void SetFlowProfileBiggerThanProfileIsClipped()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            crossSection.GetFlowProfileMutator().MovePoint(0, -60, 10);

            var flowProfile = crossSection.FlowProfile;

            Assert.AreEqual(-50.0, flowProfile.ElementAt(0).X);
        }

        [Test]
        public void CanSetFlowProfile()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            crossSection.GetFlowProfileMutator().MovePoint(1, -10, 7);

            var profileY = new double[] { -50, -25, 0, 25, 50 };
            var profileZ = new double[] { 10, 7, 0, 7, 10 };

            var flowProfileY = new double[] { -30, -10, 0, 10, 30 };
            var flowProfileZ = new double[] { 10, 7, 0, 7, 10 };

            Assert.AreEqual(profileY, crossSection.GetProfile().Select(c => c.X).ToArray());
            Assert.AreEqual(profileZ, crossSection.GetProfile().Select(c => c.Y).ToArray());
            Assert.AreEqual(flowProfileY, crossSection.FlowProfile.Select(c => c.X).ToArray());
            Assert.AreEqual(flowProfileZ, crossSection.FlowProfile.Select(c => c.Y).ToArray());
        }

        [Test]
        public void CanSetProfileWithMirroredIndex()
        {
            var crossSection = new CrossSectionDefinitionZW();

            //simple V profile (unsorted order)
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(20, 200, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(15, 150, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
            crossSection.ZWDataTable.AddCrossSectionZWRow(5, 50, 40);

            crossSection.GetProfileMutator().MovePoint(8, 90, 20);

            var profileY = new double[] { -90, -75, -50, -25, 0, 25, 50, 75, 90 };
            var profileZ = new double[] { 20, 15, 10, 5, 0, 5, 10, 15, 20 };

            Assert.AreEqual(profileY, crossSection.GetProfile().Select(c => c.X).ToArray());
            Assert.AreEqual(profileZ, crossSection.GetProfile().Select(c => c.Y).ToArray());
        }

        [Test]
        public void NonZeroWidthForAllPointsGivesEvenNumberOfProfilePoints()
        {
            var crossSection = new CrossSectionDefinitionZW();

            //simple \_/ profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(20, 200, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(15, 150, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);

            Assert.AreEqual(6, crossSection.GetProfile().Count());
            Assert.AreEqual(6, crossSection.FlowProfile.Count());

            Assert.AreEqual(0, crossSection.GetRawDataTableIndex(5));
            Assert.AreEqual(2, crossSection.GetRawDataTableIndex(2));
            Assert.AreEqual(2, crossSection.GetRawDataTableIndex(3));

            crossSection.GetProfileMutator().MovePoint(5, 90, 20);

            var profileY = new double[] { -90, -75, -50, 50, 75, 90 };
            var profileZ = new double[] { 20, 15, 10, 10, 15, 20 };

            Assert.AreEqual(profileY, crossSection.GetProfile().Select(c => c.X).ToArray());
            Assert.AreEqual(profileZ, crossSection.GetProfile().Select(c => c.Y).ToArray());
        }

        [Test]
        public void CannotSetZeroWidthAnywhereButBottomWithMutator()
        {
            var crossSection = new CrossSectionDefinitionZW();

            //simple \_/ profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(20, 200, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(15, 150, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);

            crossSection.GetProfileMutator().MovePoint(5, 0, 20);

            Assert.AreNotEqual(0.0, crossSection.GetProfile().Select(c => c.X).ElementAt(0));
        }

        [Test]
        public void AddPointSetsStorageWidth()
        {
            var crossSection = new CrossSectionDefinitionZW();

            var mutator = new ZWProfileMutator(crossSection);
            mutator.AddPoint(50, 0);
            Assert.AreEqual(0, crossSection.ZWDataTable[0].StorageWidth);

            crossSection.ZWDataTable[0].StorageWidth = 10;
            mutator.AddPoint(40, -10);
            Assert.AreEqual(10, crossSection.ZWDataTable.First(r => r.Z == -10).StorageWidth, 
                "Should have set to StorageWidth of direct neighbor");

            crossSection.ZWDataTable[0].StorageWidth = 0;
            mutator.AddPoint(30, -5);
            Assert.AreEqual(5, crossSection.ZWDataTable.First(r => r.Z == -5).StorageWidth,
                "Should have linearly interpolated StorageWidth of direct neighbors");
        }
    }
}