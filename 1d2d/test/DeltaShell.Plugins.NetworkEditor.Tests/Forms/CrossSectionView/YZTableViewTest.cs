using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class YZTableViewTest
    {
        /// <summary>
        /// mimics yzTable
        /// </summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowAllowAdd()
        {
            var view = new YZTableView();
            //view.AllowAddRemove = false;
            //view.Data = null;
            //view.ReadOnly = false;
            //view.ReadOnlyYColumn = false;
            //view.Data = new EventedList<Coordinate>
            //                {
            //                    new Coordinate(0, 10),
            //                    new Coordinate(20, 0),
            //                    new Coordinate(30, 0),
            //                    new Coordinate(40, 10)
            //                };
            view.ReadOnly = false;
            view.AllowAddRemove = true;
            view.ReadOnlyYColumn = false;
            view.Data = new EventedList<Coordinate>
                            {
                                new Coordinate(0, 10),
                                new Coordinate(20, 0),
                                new Coordinate(30, 0),
                                new Coordinate(40, 10)
                            };

            //{
            //    //AllowAddRemove = true,
            //    ReadOnly = false,
            //    AllowAddRemove = true,
            //    ReadOnlyYColumn = false,
            //    Data = new EventedList<Coordinate> { new Coordinate(0, 10), new Coordinate(10, 10) }
            //};
            WindowsFormsTestHelper.ShowModal(view);
//            MessageBox.Show(view.Data.Count);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowNotAllowAdd()
        {
            var view = new YZTableView
            {
                AllowAddRemove = false,
                Data = new EventedList<Coordinate> { new Coordinate(0, 10), new Coordinate(10, 10) }
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowReadOnly()
        {
            var view = new YZTableView
            {
                ReadOnly = true,
                AllowAddRemove = false,
                Data = new EventedList<Coordinate> { new Coordinate(0, 10), new Coordinate(10, 10) }
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowReadOnlyYColumn()
        {
            var view = new YZTableView
            {
                ReadOnlyYColumn = true,
                AllowAddRemove = false,
                Data = new EventedList<Coordinate> { new Coordinate(0, 10), new Coordinate(10, 10) }
            };
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}
