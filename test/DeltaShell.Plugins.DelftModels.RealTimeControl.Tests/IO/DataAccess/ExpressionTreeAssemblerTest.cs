using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ExpressionTreeAssemblerTest
    {
        [Test]
        public void Assemble_ExpressionObjectsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ExpressionTreeAssembler.Assemble(null, "controlGroupName ").ToList();

            // Assert
            var argumentNullException = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(argumentNullException.ParamName, Is.EqualTo("expressionObjects"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Assemble_ControlGroupNameNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ExpressionTreeAssembler.Assemble(Enumerable.Empty<ExpressionObject>(), null).ToList();

            // Assert
            var argumentNullException = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(argumentNullException.ParamName, Is.EqualTo("controlGroupName"),
                        "Expected a different ParamName:");
        }

        [Test]
        [TestCaseSource(nameof(GetAssembleExpectedResultsData))]
        public void Assemble_ExpectedResults(string groupName,
                                             ExpressionObject[] expressionObjects,
                                             ExpressionTree[] expectedResults)
        {
            // Setup
            var comparer = new ExpressionTreeEqualityComparer();

            // Call
            IList<ExpressionTree> results =
                ExpressionTreeAssembler.Assemble(expressionObjects, groupName).ToList();

            // Assert
            Assert.That(results, Is.EquivalentTo(expectedResults).Using(comparer),
                        "Expected the results to be equal to the expected results:");
        }

        private class ExpressionTreeEqualityComparer : IEqualityComparer<ExpressionTree>
        {
            private readonly ExpressionNodeEqualityComparer nodeComparer = new ExpressionNodeEqualityComparer();

            public bool Equals(ExpressionTree x, ExpressionTree y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (!(Equals(x.ControlGroupName, y.ControlGroupName) &&
                      Equals(x.Id, y.Id) &&
                      Equals(x.Object.Name, y.Object.Name)))
                {
                    return false;
                }

                return nodeComparer.Equals(x.RootNode, y.RootNode);
            }

            public int GetHashCode(ExpressionTree obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return Tuple.Create(obj.Id, obj.ControlGroupName).GetHashCode() ^
                       nodeComparer.GetHashCode(obj.RootNode);
            }
        }

        private static Tuple<ExpressionObject[], ExpressionTree[]> CombineData(params Tuple<ExpressionObject[], ExpressionTree[]>[] tuples)
        {
            int expressionObjectSize = tuples.Select(x => x.Item1.Length).Sum();
            var expressionObjects = new ExpressionObject[expressionObjectSize];

            int expressionTreeSize = tuples.Select(x => x.Item2.Length).Sum();
            var expressionTrees = new ExpressionTree[expressionTreeSize];

            var offsetObjects = 0;
            var offsetTrees = 0;

            foreach (Tuple<ExpressionObject[], ExpressionTree[]> tuple in tuples)
            {
                tuple.Item1.CopyTo(expressionObjects, offsetObjects);
                offsetObjects += tuple.Item1.Length;

                tuple.Item2.CopyTo(expressionTrees, offsetTrees);
                offsetTrees += tuple.Item2.Length;
            }

            return Tuple.Create(expressionObjects, expressionTrees);
        }

        private static Tuple<ExpressionObject, BranchNode> ConstructBranchNodeItems(string id,
                                                                                    Operator opp,
                                                                                    string yValue)
        {
            var param1 = $"{id}_Param1";
            var param2 = $"{id}_Param2";

            var expressionObject = new ExpressionObject(id,
                                                        opp,
                                                        new ParameterLeafReference(param1),
                                                        new ParameterLeafReference(param2),
                                                        yValue);

            var branchNode = new BranchNode(opp, id)
            {
                FirstNode = new ParameterLeafNode(param1),
                SecondNode = new ParameterLeafNode(param2)
            };

            return Tuple.Create(expressionObject, branchNode);
        }

        private static TestCaseData GetEmptyTestData()
        {
            return new TestCaseData("groupName", Enumerable.Empty<ExpressionObject>(), Enumerable.Empty<ExpressionTree>());
        }

        private static Tuple<ExpressionObject[], ExpressionTree[]> GetTwoLeavesData(string groupName, string postFix)
        {
            var id = $"[{groupName}]twoLeavesOneBranch_{postFix}";
            const Operator opp = Operator.Add;

            Tuple<ExpressionObject, BranchNode> results = ConstructBranchNodeItems(id, opp, id);

            BranchNode rootBranch = results.Item2;
            var expectedResult = new ExpressionTree(rootBranch, groupName, id,
                                                    new MathematicalExpression {Name = rootBranch.YName});

            return Tuple.Create(new[]
            {
                results.Item1
            }, new[]
            {
                expectedResult
            });
        }

        private static TestCaseData GetTwoLeavesOneTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data = GetTwoLeavesData(groupName, "1");
            return new TestCaseData(groupName, data.Item1, data.Item2);
        }

        private static TestCaseData GetTwoLeavesTwoTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data =
                CombineData(GetTwoLeavesData(groupName, "1"), GetTwoLeavesData(groupName, "2"));

            return new TestCaseData(groupName, data.Item1, data.Item2);
        }

        private static Tuple<ExpressionObject[], ExpressionTree[]> GetOneLeafData(string groupName, string postFix)
        {
            var idBottom = $"[{groupName}]oneLeafOneBranch_{postFix}";
            const Operator opp = Operator.Add;

            Tuple<ExpressionObject, BranchNode> results = ConstructBranchNodeItems(idBottom, opp, idBottom);

            var idTop = $"[{groupName}]parentBranch{postFix}";
            var parentLeafParam = $"[{groupName}]parentLeaf{postFix}";

            var rootBranch = new BranchNode(Operator.Multiply, idTop)
            {
                FirstNode = results.Item2,
                SecondNode = new ParameterLeafNode(parentLeafParam)
            };

            var rootBranchObject = new ExpressionObject(idTop,
                                                        Operator.Multiply,
                                                        new ExpressionReference(idBottom),
                                                        new ParameterLeafReference(parentLeafParam),
                                                        idTop);
            var expectedResult = new ExpressionTree(rootBranch, groupName, idTop,
                                                    new MathematicalExpression {Name = rootBranch.YName});

            return Tuple.Create(new[]
            {
                results.Item1,
                rootBranchObject
            }, new[]
            {
                expectedResult
            });
        }

        private static TestCaseData GetOneLeafOneTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data = GetOneLeafData(groupName, "1");
            return new TestCaseData(groupName, data.Item1, data.Item2);
        }

        private static TestCaseData GetOneLeafTwoTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data =
                CombineData(GetOneLeafData(groupName, "1"), GetOneLeafData(groupName, "2"));

            return new TestCaseData(groupName, data.Item1, data.Item2);
        }

        private static Tuple<ExpressionObject[], ExpressionTree[]> GetTwoBranchData(string groupName, string postFix)
        {
            var idBottom1 = $"[{groupName}]twoLeavesOneBranch1_{postFix}";
            const Operator opp = Operator.Add;

            Tuple<ExpressionObject, BranchNode> results1 = ConstructBranchNodeItems(idBottom1, opp, idBottom1);

            var idBottom2 = $"[{groupName}]twoLeavesOneBranch2_{postFix}";
            Tuple<ExpressionObject, BranchNode> results2 = ConstructBranchNodeItems(idBottom2, opp, idBottom2);

            var idTop = $"[{groupName}]ParentBranch_{postFix}";
            var rootBranch = new BranchNode(Operator.Multiply, idTop)
            {
                FirstNode = results1.Item2,
                SecondNode = results2.Item2
            };

            var rootBranchObject = new ExpressionObject(idTop,
                                                        Operator.Multiply,
                                                        new ExpressionReference(idBottom1),
                                                        new ExpressionReference(idBottom2),
                                                        idTop);
            var expectedResult = new ExpressionTree(rootBranch, groupName, idTop,
                                                    new MathematicalExpression {Name = rootBranch.YName});

            return Tuple.Create(new[]
            {
                results1.Item1,
                rootBranchObject,
                results2.Item1
            }, new[]
            {
                expectedResult
            });
        }

        private static TestCaseData GetTwoBranchOneTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data = GetTwoBranchData(groupName, "1");
            return new TestCaseData(groupName, data.Item1, data.Item2);
        }

        private static TestCaseData GetTwoBranchTwoTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data =
                CombineData(GetTwoBranchData(groupName, "1"), GetTwoBranchData(groupName, "2"));

            return new TestCaseData(groupName, data.Item1, data.Item2);
        }

        private static TestCaseData GetTwoLeavesOneTreeWithReferences()
        {
            const string groupName = "groupName";
            const string postFix = "1";

            var idBottom1 = $"[{groupName}]twoLeavesOneBranch1_{postFix}";
            var idBottom2 = $"[{groupName}]twoLeavesOneBranch2_{postFix}";

            const Operator opp = Operator.Add;

            var idTop = $"[{groupName}]ParentBranch_{postFix}";
            var rootBranch = new BranchNode(opp, idTop)
            {
                FirstNode = new ParameterLeafNode(idBottom1),
                SecondNode = new ParameterLeafNode(idBottom2)
            };

            var rootBranchObject = new ExpressionObject(idTop,
                                                        opp,
                                                        new ExpressionReference(idBottom1),
                                                        new ExpressionReference(idBottom2),
                                                        idTop);
            var expectedResult = new ExpressionTree(rootBranch, groupName, idTop,
                                                    new MathematicalExpression {Name = rootBranch.YName});

            return new TestCaseData(groupName, new[]
            {
                rootBranchObject
            }, new[]
            {
                expectedResult
            });
        }

        private static IEnumerable<TestCaseData> GetAssembleExpectedResultsData()
        {
            yield return GetEmptyTestData();
            yield return GetTwoLeavesOneTree();
            yield return GetOneLeafOneTree();
            yield return GetOneLeafTwoTree();
            yield return GetTwoBranchOneTree();
            yield return GetTwoBranchTwoTree();
            yield return GetTwoLeavesTwoTree();

            yield return GetTwoLeavesOneTreeWithReferences();
        }
    }
}