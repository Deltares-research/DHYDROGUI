using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class LeveeBreachViewModelTest
    {
        [Test]
        public void ViewModelWithLeveeBreach_GettingSelectedGrowthFormula_ShouldGiveExpectedResults()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach
                {
                    LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002
                }
            };

            Assert.AreEqual(LeveeBreachGrowthFormula.VerheijvdKnaap2002, vm.SelectedGrowthFormula);

            vm.LeveeBreach.LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
            Assert.AreEqual(LeveeBreachGrowthFormula.UserDefinedBreach, vm.SelectedGrowthFormula);

            vm.LeveeBreach = null;
            Assert.AreEqual(LeveeBreachGrowthFormula.VerheijvdKnaap2002, vm.SelectedGrowthFormula);
        }

        [Test]
        public void ViewModelWithLeveeBreach_SettingSelectedGrowthFormula_ShouldSetFormulaInLeveeBreach()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach(),
                SelectedGrowthFormula = LeveeBreachGrowthFormula.UserDefinedBreach,
            };

            Assert.AreEqual(LeveeBreachGrowthFormula.UserDefinedBreach, vm.LeveeBreach.LeveeBreachFormula);
        }

        [Test]
        public void ViewModelWithoutLeveeBreach_SettingSelectedGrowthFormula_ShouldNotCauseCrash()
        {
            var vm = new LeveeBreachViewModel();
            vm.LeveeBreach = null;
            vm.SelectedGrowthFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
        }

        [Test]
        public void ViewModelWithLeveeBreach_GettingBreahSettings_ShouldReturnExpected()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002} };

            Assert.That(vm.LeveeBreach.LeveeBreachFormula == LeveeBreachGrowthFormula.VerheijvdKnaap2002);
            Assert.NotNull(vm.LeveeBreachSettings);
            Assert.That(vm.LeveeBreachSettings.GetType() == typeof(VerheijVdKnaap2002BreachSettings));
        }

        [Test]
        public void Given_LeveeBreachViewModelWithBranchLocationSnappingOn_When_BranchLocationXIsChanged_Then_BranchLocationYShouldSnapToLevee()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 100), })} };

            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            vm.BreachLocationX = 25;
            Assert.That(vm.BreachLocationX, Is.EqualTo(25));
            Assert.That(vm.BreachLocationY, Is.EqualTo(25));
        }

        [Test]
        public void Given_LeveeBreachViewModelWithBranchLocationSnappingOff_When_BranchLocationXIsChanged_Then_BranchLocationYShouldStayTheSame()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 100), })} };

            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            vm.UseSnapping = false;
            vm.BreachLocationX = 25;
            Assert.That(vm.BreachLocationX, Is.EqualTo(25));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
        }

        [Test]
        public void Given_LeveeBreachViewModelWithBranchLocationSnappingOff_When_BranchLocationXIsChanged_Then_BranchLocationYShouldStayTheSame_But_When_UseSnappingIsOn_XShouldRemain_ThenYMustSnapBackToLevee()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 200), })} };

            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(100));
            vm.UseSnapping = false;
            vm.BreachLocationX = 25;
            Assert.That(vm.BreachLocationX, Is.EqualTo(25));
            Assert.That(vm.BreachLocationY, Is.EqualTo(100));
            vm.UseSnapping = true;
            Assert.That(vm.BreachLocationX, Is.EqualTo(25));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));

        }

        [Test]
        public void Given_LeveeBreachViewModelWithBranchLocationSnappingOn_When_BranchLocationYIsChanged_Then_BranchLocationXShouldSnapToLevee()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 100), })} };

            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            vm.BreachLocationY = 25;
            Assert.That(vm.BreachLocationX, Is.EqualTo(25));
            Assert.That(vm.BreachLocationY, Is.EqualTo(25));
        }

        [Test]
        public void Given_LeveeBreachViewModelWithBranchLocationSnappingOff_When_BranchLocationYIsChanged_Then_BranchLocationXShouldStayTheSame()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 100), })} };

            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            vm.UseSnapping = false;
            vm.BreachLocationY = 25;
            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(25));
        }

        [Test]
        public void Given_LeveeBreachViewModelWithWaterLevelStreamOff_Then_WaterLevelStreamLocatioValuesAreDefaultDoubles_When_WaterLevelStreamIsActivated_Then_WaterLevelLocationValuesWillBecomeEqualToTheBranchLocation()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 100), }),WaterLevelFlowLocationsActive = false} };

            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            Assert.That(vm.WaterLevelUpstreamLocationX, Is.EqualTo(default(double)));
            Assert.That(vm.WaterLevelUpstreamLocationY, Is.EqualTo(default(double)));
            Assert.That(vm.WaterLevelDownstreamLocationX, Is.EqualTo(default(double)));
            Assert.That(vm.WaterLevelDownstreamLocationY, Is.EqualTo(default(double)));

            vm.UseWaterLevelFlowLocation = true;
            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            Assert.That(vm.WaterLevelUpstreamLocationX, Is.EqualTo(50));
            Assert.That(vm.WaterLevelUpstreamLocationY, Is.EqualTo(50));
            Assert.That(vm.WaterLevelDownstreamLocationX, Is.EqualTo(50));
            Assert.That(vm.WaterLevelDownstreamLocationY, Is.EqualTo(50));
        }

        [Test]
        public void Given_LeveeBreachViewModelWithWaterLevelStreamOff_Then_WaterLevelStreamLocatioValuesAreDefaultDoubles_When_WaterLevelStreamIsActivatedInLeveeBreach_Then_WaterLevelLocationValuesWillBecomeEqualToTheBranchLocation()
        {
            var leveeBreach = new LeveeBreach {Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(100, 100), }),WaterLevelFlowLocationsActive = false};
            var vm = new LeveeBreachViewModel { LeveeBreach = leveeBreach };

            Assert.That(vm.UseWaterLevelFlowLocation, Is.False);
            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            Assert.That(vm.WaterLevelUpstreamLocationX, Is.EqualTo(default(double)));
            Assert.That(vm.WaterLevelUpstreamLocationY, Is.EqualTo(default(double)));
            Assert.That(vm.WaterLevelDownstreamLocationX, Is.EqualTo(default(double)));
            Assert.That(vm.WaterLevelDownstreamLocationY, Is.EqualTo(default(double)));
            leveeBreach.WaterLevelFlowLocationsActive = true;
            Assert.That(vm.UseWaterLevelFlowLocation, Is.True);
            Assert.That(vm.BreachLocationX, Is.EqualTo(50));
            Assert.That(vm.BreachLocationY, Is.EqualTo(50));
            Assert.That(vm.WaterLevelUpstreamLocationX, Is.EqualTo(50));
            Assert.That(vm.WaterLevelUpstreamLocationY, Is.EqualTo(50));
            Assert.That(vm.WaterLevelDownstreamLocationX, Is.EqualTo(50));
            Assert.That(vm.WaterLevelDownstreamLocationY, Is.EqualTo(50));
        }
    }
}