/*
 * Created by Ranorex
 * User: rodriqu_dd
 * Date: 17/06/2022
 * Time: 14:51
 * 
 * To change this template use Tools > Options > Coding > Edit standard headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Ranorex;
using Ranorex.Core.Testing;

namespace DHYDRO.Modules.ProjectTree
{
    /// <summary>
    /// Description of SelectNodeInProjectTree.
    /// </summary>
    [TestModule("3CCD6609-BB39-41DA-A091-205394A31109", ModuleType.UserCode, 1)]
    public class SelectNodeInProjectTree : ITestModule
    {
        
    	
    	string _fullPathToTreeItem = "";
    	[TestVariable("62ee49d4-3df6-48c9-9539-d1c2f7388f8f")]
    	public string fullPathToTreeItem
    	{
    		get { return _fullPathToTreeItem; }
    		set { _fullPathToTreeItem = value; }
    	}
    	
    	/// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SelectNodeInProjectTree()
        {
            // Do not delete - a parameterless constructor is required!
        }

        /// <summary>
        /// Performs the playback of actions in this module.
        /// </summary>
        /// <remarks>You should not call this method directly, instead pass the module
        /// instance to the <see cref="TestModuleRunner.Run(ITestModule)"/> method
        /// that will in turn invoke this method.</remarks>
        void ITestModule.Run()
        {
            SetDefaultTiming();
            var currentNode = LoadAllNodes();
            var subPaths = fullPathToTreeItem.Split('>').ToList();
            var lastSubPath = subPaths.Last();

            foreach (var subPath in subPaths)
            {
            	var children = currentNode.Children.Select(it => it.As<TreeItem>()).ToList();

                if (children.Count == 0)
                {
                    Report.Info("Subnodes found for tree item node " + currentNode.Text + ": ");
                    foreach (var child in children) {
                        Report.Info("Child name: " + child.Text);
                    }
                    throw new RanorexException($"Could not find Node with the name \"{subPath}\"");
                }

                if (!FindCurrentNode(subPath, children, out currentNode))
                {
                    continue;
                }

                var lastNode = string.Equals(lastSubPath, subPath, StringComparison.CurrentCultureIgnoreCase);
                ActionsTreeItem(currentNode, lastNode);
            }
        }

        private static bool FindCurrentNode(string subPath, IReadOnlyCollection<TreeItem> children, out TreeItem currentNode)
        {
            if (subPath == "*" && children.Count == 1)
            {
                Report.Info("Information", "One single tree item node found. Using wildcard '*'.");
                currentNode = children.Single();
                return true;
            }

            var nodesWithSubPath = children.Where(treeItem => treeItem.Text.Contains(subPath)).ToArray();
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

        private static TreeItem LoadAllNodes()
        {
            var rootChild = DHYDRO1D2DRepository.Instance.DSWindow.ListView.ProjectTree.RootTreeItem.Self.As<TreeItem>();
            rootChild.Focus();
            rootChild.Select();
            Keyboard.Press("{Apps}x");
            Delay.Duration(100, false);

            var stepChild = global::DHYDRO.DHYDRO1D2DRepository.Instance.DSWindow.ListView.ProjectTree.RootTreeItem.Self.As<TreeItem>();
            stepChild.CollapseAll();
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
                ti.ClickWithoutBoundsCheck(new Location(0.5, 0.5));
                return;
            }
            ti.Expand();
        }
        
        private static void SetDefaultTiming(int speedFactor = 0)
        {
            Mouse.DefaultMoveTime = speedFactor;
            Keyboard.DefaultKeyPressTime = speedFactor;
            Delay.SpeedFactor = speedFactor;
        }
    }
}
