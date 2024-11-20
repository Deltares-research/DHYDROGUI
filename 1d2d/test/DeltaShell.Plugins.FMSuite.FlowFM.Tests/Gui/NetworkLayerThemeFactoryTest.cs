using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.Gui;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    class NetworkLayerThemeFactoryTest
    {
        private IEnumerable<IHydroNode> myHydroNodes;
        private IEnumerable<IChannel> myChannels;
        private IEnumerable<IPipe> myPipes;
        private IEnumerable<ISewerConnection> mySewerConnections;
        private IEnumerable<ICrossSection> myCrossSections;
        private ITheme theme;

        [OneTimeSetUp]
        public void SetUp()
        {
            IHydroNode myHydroNode = Substitute.For<IHydroNode>();
            myHydroNodes = Enumerable.Repeat(myHydroNode, 10);

            IChannel myChannel= Substitute.For<IChannel>();
            myChannels = Enumerable.Repeat(myChannel, 10);

            IPipe myPipe = Substitute.For<IPipe>();
            myPipes = Enumerable.Repeat(myPipe, 10);

            ISewerConnection mySewerConnection = Substitute.For<ISewerConnection>();
            mySewerConnections = Enumerable.Repeat(mySewerConnection, 10);

            ICrossSection myCrossSection = Substitute.For<ICrossSection>();
            myCrossSections = Enumerable.Repeat(myCrossSection, 10);

            
        }
        #region Theme

        [Test]
        public void GivenAnEnumerableOfObjectWhenGeneratingThemeThenExpectNullObjectForTheme()
        {
            object o = Substitute.For<object>();
            IEnumerable<object> typeLessObjects = Enumerable.Repeat(o, 10);
            Assert.That(NetworkLayerThemeFactory.CreateTheme(typeLessObjects), Is.Null);
        }
        #endregion

        #region HydroNode Theme
        
        private void SetUpHydroNodesTheme()
        {
            theme = NetworkLayerThemeFactory.CreateTheme(myHydroNodes);
        }

        [TestCase(true, "Boundary node")]
        [TestCase(false, "Connection node")]
        public void GivenAnEnumerableOfHydroNodesWhenGenerateThemeThenCheckIfDefaultColoursForHydroNodeTypesAreSet(bool onSingleBranch, string categoryName)
        {
            SetUpHydroNodesTheme();
            Bitmap themeTypeBitmap = onSingleBranch 
                                         ? NetworkEditor.Gui.Properties.Resources.NodeOnSingleBranch
                                         : NetworkEditor.Gui.Properties.Resources.NodeOnMultipleBranches;
            
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            foreach (var themeItem in theme.ThemeItems)
            {
                Assert.That(themeItem, Is.TypeOf<CategorialThemeItem>());
                Assert.That(themeItem.Style, Is.TypeOf<VectorStyle>());
                Assert.That(((VectorStyle)themeItem.Style).GeometryType, Is.EqualTo(typeof(IPoint)));
            }

            var categorialThemeItemForThisType = (CategorialThemeItem)theme
                                                                      .ThemeItems
                                                                           .SingleOrDefault(ti => NetworkLayerFactoryTestHelper.CompareImages(ti.Symbol, themeTypeBitmap));
            Assert.That(categorialThemeItemForThisType, Is.Not.Null);
            Assert.That(categorialThemeItemForThisType.Value, Is.EqualTo(onSingleBranch));
            Assert.That(categorialThemeItemForThisType.Category, Is.EqualTo(categoryName));
        }

        [Test]
        public void GivenAnEnumerableOfHydroNodesWhenGenerateThemeThenCheckIf2ThemesAreSet()
        {
            SetUpHydroNodesTheme();
            Assert.That(theme.ThemeItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenAnEnumerableOfHydroNodesWhenGenerateThemeThenCheckIfAttributeNameIsCorrect()
        {
            SetUpHydroNodesTheme();
            Assert.That(theme.AttributeName, Is.EqualTo(nameof(INode.IsOnSingleBranch)));
        }

        [Test]
        public void GivenAnEnumerableOfHydroNodesWhenGenerateThemeThenCheckIfDefaultStyleBitmapIsCorrect()
        {
            SetUpHydroNodesTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            Assert.That(((CategorialTheme)theme).DefaultStyle, Is.TypeOf<VectorStyle>());
            Assert.That(NetworkLayerFactoryTestHelper.CompareImages(((VectorStyle)((CategorialTheme)theme).DefaultStyle).Symbol, NetworkEditor.Gui.Properties.Resources.NodeOnSingleBranch), Is.True);
        }
        #endregion

        #region Channel Theme
        
        private void SetUpChannelsTheme()
        {
            theme = NetworkLayerThemeFactory.CreateTheme(myChannels);
        }

        [Test]
        public void GivenAnEnumerableOfChannelsWhenGenerateThemeThenCheckIfNoThemesAreSet()
        {
            SetUpChannelsTheme();
            Assert.That(theme.ThemeItems.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenAnEnumerableOfChannelsWhenGenerateThemeThenCheckIfAttributeNameIsCorrect()
        {
            SetUpChannelsTheme();
            Assert.That(theme.AttributeName, Is.EqualTo(nameof(IBranch.IsLengthCustom)));
        }

        [Test]
        public void GivenAnEnumerableOfChannelsWhenGenerateThemeThenCheckIfDefaultStyleColorAndCapIsCorrect()
        {
            SetUpChannelsTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            Assert.That(((CategorialTheme)theme).DefaultStyle, Is.TypeOf<VectorStyle>());
            Assert.That(((VectorStyle)((CategorialTheme)theme).DefaultStyle).GeometryType, Is.EqualTo(typeof(ILineString)));
            Assert.That(((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.Color.ToArgb(), Is.EqualTo(Color.FromArgb(255, 0, 0, 128).ToArgb()));
            Assert.That(((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.CustomEndCap, Is.TypeOf<AdjustableArrowCap>());
            Assert.That(((AdjustableArrowCap)((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.CustomEndCap).Filled, Is.True);
            Assert.That(((AdjustableArrowCap)((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.CustomEndCap).BaseCap, Is.EqualTo(LineCap.Triangle));
        }
        #endregion
        
        #region Pipe Theme
        
        private void SetUpPipesTheme()
        {
            theme = NetworkLayerThemeFactory.CreateTheme(myPipes);
        }

        [TestCase(SewerConnectionWaterType.None, KnownColor.SlateGray, "Default")]
        [TestCase(SewerConnectionWaterType.StormWater, KnownColor.RoyalBlue, "Storm water")]
        [TestCase(SewerConnectionWaterType.DryWater, KnownColor.OrangeRed, "Foul water")]
        [TestCase(SewerConnectionWaterType.Combined, KnownColor.Black, "Combined")]
        public void GivenAnEnumerableOfPipesWhenGenerateThemeThenCheckIfDefaultColoursForPipeTypesAreSet(SewerConnectionWaterType type, KnownColor themeTypeColor, string categoryName)
        {
            SetUpPipesTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            foreach (var themeItem in theme.ThemeItems)
            {
                Assert.That(themeItem, Is.TypeOf<CategorialThemeItem>());
                Assert.That(themeItem.Style, Is.TypeOf<VectorStyle>());
                Assert.That(((VectorStyle)themeItem.Style).GeometryType, Is.EqualTo(typeof(ILineString)));
            }

            var categorialThemeItemForThisType = (CategorialThemeItem)theme
                                                                      .ThemeItems
                                                                           .SingleOrDefault(ti => ti.Label.Equals(categoryName));
            Assert.That(categorialThemeItemForThisType, Is.Not.Null);
            Assert.That(categorialThemeItemForThisType.Category, Is.EqualTo(categoryName)); 
            
            var vectorStyleForThisType = (VectorStyle)categorialThemeItemForThisType.Style;
            Assert.That(vectorStyleForThisType, Is.Not.Null);
            Assert.That(vectorStyleForThisType.Line.Color.ToArgb(), Is.EqualTo(Color.FromKnownColor(themeTypeColor).ToArgb()));
        }

        [Test]
        public void GivenAnEnumerableOfPipesWhenGenerateThemeThenCheckIf4ThemesAreSet()
        {
            SetUpPipesTheme();
            Assert.That(theme.ThemeItems.Count, Is.EqualTo(4));
        }

        [Test]
        public void GivenAnEnumerableOfPipesWhenGenerateThemeThenCheckIfAttributeNameIsCorrect()
        {
            SetUpPipesTheme();
            Assert.That(theme.AttributeName, Is.EqualTo(nameof(IPipe.WaterType)));
        }

        [Test]
        public void GivenAnEnumerableOfPipesWhenGenerateThemeThenCheckIfDefaultStyleColorIsCorrect()
        {
            SetUpPipesTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            Assert.That(((CategorialTheme)theme).DefaultStyle, Is.TypeOf<VectorStyle>());
            Assert.That(((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.Color.ToArgb(), Is.EqualTo(Color.SlateGray.ToArgb()));
        }
        #endregion

        #region SewerConnection Theme
        
        private void SetUpSewerConnectionsTheme()
        {
            theme = NetworkLayerThemeFactory.CreateTheme(mySewerConnections);
        }

        [TestCase(SewerConnectionSpecialConnectionType.None, KnownColor.Pink, DashStyle.Solid)]
        [TestCase(SewerConnectionSpecialConnectionType.Pump, KnownColor.Red, DashStyle.Dash)]
        [TestCase(SewerConnectionSpecialConnectionType.Weir, KnownColor.LimeGreen, DashStyle.Dash)]
        public void GivenAnEnumerableOfSewerConnectionsWhenGenerateThemeThenCheckIfDefaultColoursForSewerConnectionTypesAreSet(SewerConnectionSpecialConnectionType type, KnownColor themeTypeColor, DashStyle dashStyle)
        {
            SetUpSewerConnectionsTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            foreach (var themeItem in theme.ThemeItems)
            {
                Assert.That(themeItem, Is.TypeOf<CategorialThemeItem>());
                Assert.That(themeItem.Style, Is.TypeOf<VectorStyle>());
                Assert.That(((VectorStyle)themeItem.Style).GeometryType, Is.EqualTo(typeof(ILineString)));
            }

            var categorialThemeItemForThisType = (CategorialThemeItem)theme
                                                                      .ThemeItems
                                                                           .SingleOrDefault(ti => ti.Label.Equals(type.GetDescription()));
            Assert.That(categorialThemeItemForThisType, Is.Not.Null);

            var vectorStyleForThisType = (VectorStyle)categorialThemeItemForThisType.Style;
            Assert.That(vectorStyleForThisType, Is.Not.Null);
            Assert.That(vectorStyleForThisType.Line.Color.ToArgb(), Is.EqualTo(Color.FromKnownColor(themeTypeColor).ToArgb()));
            Assert.That(vectorStyleForThisType.Line.DashStyle, Is.EqualTo(dashStyle));
        }

        [Test]
        public void GivenAnEnumerableOfSewerConnectionsWhenGenerateThemeThenCheckIf3ThemesAreSet()
        {
            SetUpSewerConnectionsTheme();
            Assert.That(theme.ThemeItems.Count, Is.EqualTo(3));
        }

        [Test]
        public void GivenAnEnumerableOfSewerConnectionsWhenGenerateThemeThenCheckIfAttributeNameIsCorrect()
        {
            SetUpSewerConnectionsTheme();
            Assert.That(theme.AttributeName, Is.EqualTo(nameof(ISewerConnection.SpecialConnectionType)));
        }

        [Test]
        public void GivenAnEnumerableOfSewerConnectionsWhenGenerateThemeThenCheckIfDefaultStyleColorIsCorrect()
        {
            SetUpSewerConnectionsTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            Assert.That(((CategorialTheme)theme).DefaultStyle, Is.TypeOf<VectorStyle>());
            Assert.That(((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.Color.ToArgb(), Is.EqualTo(Color.Pink.ToArgb()));
        }
        #endregion

        #region CrossSection Theme
        
        private void SetUpCrossSectionsTheme()
        {
            theme = NetworkLayerThemeFactory.CreateTheme(myCrossSections);
        }

        [TestCase(CrossSectionType.YZ, KnownColor.Silver)]
        [TestCase(CrossSectionType.ZW, KnownColor.Gray)]
        [TestCase(CrossSectionType.GeometryBased, KnownColor.OrangeRed)]
        [TestCase(CrossSectionType.Standard, KnownColor.Purple)]
        public void GivenAnEnumerableOfCrossSectionsWhenGenerateThemeThenCheckIfDefaultColoursForCrossSectionTypesAreSet(CrossSectionType type, KnownColor themeTypeColor)
        {
            SetUpCrossSectionsTheme();
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            foreach (var themeItem in theme.ThemeItems)
            {
                Assert.That(themeItem, Is.TypeOf<CategorialThemeItem>());
                Assert.That(themeItem.Style, Is.TypeOf<VectorStyle>());
                Assert.That(((VectorStyle)themeItem.Style).GeometryType, Is.EqualTo(typeof(ILineString)));
            }

            var categorialThemeItemForThisType = (CategorialThemeItem)theme
                                                                      .ThemeItems
                                                                           .SingleOrDefault(ti => ti.Label.Equals(type.GetDescription()));
            Assert.That(categorialThemeItemForThisType, Is.Not.Null);

            var vectorStyleForThisType = (VectorStyle)categorialThemeItemForThisType.Style;
            Assert.That(vectorStyleForThisType, Is.Not.Null); 
            Assert.That(vectorStyleForThisType.Line.Color.ToArgb(), Is.EqualTo(Color.FromKnownColor(themeTypeColor).ToArgb()));
        }

        [Test]
        public void GivenAnEnumerableOfCrossSectionsWhenGenerateThemeThenCheckIf4ThemesAreSet()
        {
            SetUpCrossSectionsTheme(); 
            Assert.That(theme.ThemeItems.Count, Is.EqualTo(4));
        }
        
        [Test]
        public void GivenAnEnumerableOfCrossSectionsWhenGenerateThemeThenCheckIfAttributeNameIsCorrect()
        {
            SetUpCrossSectionsTheme(); 
            Assert.That(theme.AttributeName, Is.EqualTo(nameof(ICrossSection.CrossSectionType)));
        }
        
        [Test]
        public void GivenAnEnumerableOfCrossSectionsWhenGenerateThemeThenCheckIfDefaultStyleColorIsCorrect()
        {
            SetUpCrossSectionsTheme(); 
            Assert.That(theme, Is.TypeOf<CategorialTheme>());
            Assert.That(((CategorialTheme)theme).DefaultStyle, Is.TypeOf<VectorStyle>());
            Assert.That(((VectorStyle)((CategorialTheme)theme).DefaultStyle).Line.Color.ToArgb(), Is.EqualTo(Color.OrangeRed.ToArgb()));
        }
        #endregion
    }
}