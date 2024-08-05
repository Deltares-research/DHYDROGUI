using System;
using System.Collections.Generic;
using System.Linq;
using Ranorex;
using Ranorex.Core.Testing;

namespace DHYDRO.Modules.MapTree
{
	/// <summary>
	/// Description of UserCodeModule1.
	/// </summary>
	[TestModule("26534CEE-E257-4419-97E9-1FCF1B1BB060", ModuleType.UserCode, 1)]
    public class SelectNodeInMapTree : ITestModule
    {
        private string _fullPathToTreeItem = "";

        [TestVariable("ce7ec59a-1783-4e2f-8209-c83881b8e390")]
        public string fullPathToTreeItem
        {
            get { return _fullPathToTreeItem; }
            set { _fullPathToTreeItem = value; }
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SelectNodeInMapTree()
        {
            // Do not delete - a parameterless constructor is required!
        }

        /// <summary>
        /// Performs the playback of actions in this module.
        /// </summary>
        /// <remarks>
        /// You should not call this method directly, instead pass the module
        /// instance to the <see cref="TestModuleRunner.Run(ITestModule)"/> method
        /// that will in turn invoke this method.
        /// </remarks>
        void ITestModule.Run()
        {
            SetDefaultTiming();
            SelectNode(fullPathToTreeItem);
        }

        public static TreeItem SelectNode(string path)
        {
            TreeItem currentNode = LoadAllNodes();
            List<string> subPaths = path.Split('>').ToList();
            string lastSubPath = subPaths.Last();

            foreach (string subPath in subPaths)
            {
                List<TreeItem> children = GetChildNodes(currentNode).ToList();

                if (children.Count == 0)
                {
                    Report.Info("Subnodes found for tree item node " + currentNode.Text + ": ");
                    foreach (TreeItem child in children)
                    {
                        Report.Info("Child name: " + child.Text);
                    }

                    throw new RanorexException($"Could not find Node with the name \"{subPath}\"");
                }

                if (!FindCurrentNode(subPath, children, out currentNode))
                {
                    continue;
                }

                bool lastNode = string.Equals(lastSubPath, subPath, StringComparison.CurrentCultureIgnoreCase);
                ActionsTreeItem(currentNode, lastNode);
            }

            return currentNode;
        }

        private static IEnumerable<TreeItem> GetChildNodes(Adapter treeitem)
        {
            var treeitems = new List<TreeItem>();

            foreach (Unknown child in treeitem.Children)
            {
                var childTreeitem = child.As<TreeItem>();
                if (childTreeitem != null)
                {
                    treeitems.Add(childTreeitem);
                    continue;
                }

                var childContainer = child.As<Container>();
                if (childContainer != null)
                {
                    treeitems.AddRange(GetChildNodes(childContainer));
                }
            }

            return treeitems;
        }

        private static bool FindCurrentNode(string subPath, IReadOnlyCollection<TreeItem> children, out TreeItem currentNode)
        {
            if (subPath == "*" && children.Count == 1)
            {
                Report.Info("Information", "One single tree item node found. Using wildcard '*'.");
                currentNode = children.Single();
                return true;
            }

            TreeItem[] nodesWithSubPath = children.Where(treeItem => GetTreeitemText(treeItem).Contains(subPath)).ToArray();
            if (nodesWithSubPath.Length == 0)
            {
                throw new RanorexException("No occurrence of tree item with name '" + subPath + "' found.");
            }

            currentNode = nodesWithSubPath.Length == 1
                              ? nodesWithSubPath[0]
                              : children.FirstOrDefault(c => c.Text == subPath);

            if (currentNode == null)
            {
                throw new RanorexException("No unique tree item node with name '" + subPath + "' found.");
            }

            return true;
        }

        private static string GetTreeitemText(TreeItem treeitem)
        {
            Text text = treeitem.Find<Text>("text").Single();
            return text.TextValue;
        }

        private static TreeItem LoadAllNodes()
        {
            var rootChild = DHYDRO1D2DRepository.Instance.DSWindow.DocumentsPaneRight.MapLegendTree.RootTreeItem.Self.As<TreeItem>();
            rootChild.Focus();
            rootChild.Select();
            rootChild.ExpandAll();
            Delay.Duration(300, false);

            var stepChild = DHYDRO1D2DRepository.Instance.DSWindow.DocumentsPaneRight.MapLegendTree.RootTreeItem.Self.As<TreeItem>();
            stepChild.Focus();
            stepChild.Select();

            return stepChild;
        }

        private static void ActionsTreeItem(TreeItem ti, bool lastNode = false)
        {
            ti.Focus();
            if (lastNode)
            {
                ti.Select();
                ti.ClickWithoutBoundsCheck(new Location(-0.02, 0.5));
                return;
            }

            ti.Expand();
            Delay.Duration(300, false);
        }

        private static void SetDefaultTiming(int speedFactor = 0)
        {
            Mouse.DefaultMoveTime = speedFactor;
            Keyboard.DefaultKeyPressTime = speedFactor;
            Delay.SpeedFactor = speedFactor;
        }
    }
}