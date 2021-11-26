using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ExpressionTreeAssemblerTest
    {
        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Array.Empty<RuleDataAccessObject>(), Array.Empty<ConditionDataAccessObject>(), "expressionObjects");
            yield return new TestCaseData(Array.Empty<ExpressionObject>(),null, Array.Empty<ConditionDataAccessObject>(), "rules");
            yield return new TestCaseData(Array.Empty<ExpressionObject>(),Array.Empty<RuleDataAccessObject>(), null, "conditions");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ExpressionObject[] expressionObjects,
                                                                         RuleDataAccessObject[] rules,
                                                                         ConditionDataAccessObject[] conditions,
                                                                         string expParamName)
        {
            // Call
            void Call() => new ExpressionTreeAssembler("controlGroupName", expressionObjects, rules, conditions);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void Constructor_ControlGroupNameNullOrEmpty_ThrowsArgumentException(string controlGroupName)
        {
            // Call
            void Call() => new ExpressionTreeAssembler(controlGroupName,
                                                       Array.Empty<ExpressionObject>(),
                                                       Array.Empty<RuleDataAccessObject>(),
                                                       Array.Empty<ConditionDataAccessObject>());

            // Assert
            var argumentNullException = Assert.Throws<ArgumentException>(Call);
            Assert.That(argumentNullException.ParamName, Is.EqualTo("controlGroupName"),
                        "Expected a different ParamName:");
        }
        
        [Test]
        [TestCaseSource(nameof(GetAssembleExpectedResultsData))]
        public void Assemble_ExpectedResults(string groupName,
                                             ExpressionObject[] expressionObjects,
                                             RuleDataAccessObject[] rules,
                                             ConditionDataAccessObject[] conditions,
                                             ExpressionTree[] expectedResults
)
        {
            // Setup
            var expressionTreeAssembler = new ExpressionTreeAssembler(groupName,
                                                                      expressionObjects,
                                                                      rules,
                                                                      conditions);
            var comparer = new ExpressionTreeEqualityComparer();

            // Call
            IList<ExpressionTree> results = expressionTreeAssembler.Assemble().ToList();

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
                                                                                    string groupName,
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
            return new TestCaseData("groupName", 
                                    Enumerable.Empty<ExpressionObject>(), 
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(), 
                                    Enumerable.Empty<ExpressionTree>());
        }

        /// <summary>
        /// Gets one expression object that references two leaf inputs.
        /// 
        ///   O   ---> 1 root expression
        ///  / \    
        /// *   * ---> 2 leaf inputs
        /// </summary>
        private static Tuple<ExpressionObject[], ExpressionTree[]> GetTwoLeavesData(string groupName, string postFix)
        {
            var id = $"[{groupName}]twoLeavesOneBranch_{postFix}";
            const Operator opp = Operator.Add;

            Tuple<ExpressionObject, BranchNode> results = ConstructBranchNodeItems(id, groupName, opp, id);

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

        /// <summary>
        /// Gets one expression object that references two leaf inputs.
        /// 
        ///   O   ---> 1 root expression
        ///  / \    
        /// *   * ---> 2 leaf inputs
        /// </summary>
        private static TestCaseData GetTwoLeavesOneTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data = GetTwoLeavesData(groupName, "1");
            return new TestCaseData(groupName, 
                                    data.Item1,                                     
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),  
                                    data.Item2);
        }

        /// <summary>
        /// Gets two expression objects that form two groups of one expression object, where each group has
        /// one expression object that reference two leaf inputs.
        /// 
        ///   O     O   ---> 2 root expressions
        ///  / \   / \    
        /// *   * *   * ---> 2 leaf inputs per root expression
        /// </summary>
        private static TestCaseData GetTwoLeavesTwoTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data =
                CombineData(GetTwoLeavesData(groupName, "1"), GetTwoLeavesData(groupName, "2"));

            return new TestCaseData(groupName,
                                    data.Item1,
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),
                                    data.Item2);
        }

        /// <summary>
        /// Gets two expression objects where one expression object references a leaf input and the other expression object, which has two leaf inputs.
        /// 
        ///    O      ---> 1 root expressions
        ///  /   \    
        /// *     O   ---> 1 leaf input & 1 sub-expression
        ///      / \  
        ///     *   * ---> 2 leaf inputs
        /// </summary>
        private static Tuple<ExpressionObject[], ExpressionTree[]> GetOneLeafData(string groupName, string postFix)
        {
            var idBottom = $"[{groupName}]oneLeafOneBranch_{postFix}";
            const Operator opp = Operator.Add;

            Tuple<ExpressionObject, BranchNode> results = ConstructBranchNodeItems(idBottom, groupName, opp, idBottom);

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

        /// <summary>
        /// Gets two expression objects where one expression object references an input and the other expression object, which has two leaf inputs.
        ///
        ///    O      ---> 1 root expressions
        ///  /   \    
        /// *     O   ---> 1 leaf input & 1 sub-expression
        ///      / \  
        ///     *   * ---> 2 leaf inputs
        /// </summary>
        private static TestCaseData GetOneLeafOneTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data = GetOneLeafData(groupName, "1");
            return new TestCaseData(groupName,
                                    data.Item1,
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),
                                    data.Item2);
        }

        /// <summary>
        /// Gets four expression objects that form two groups of two expression objects, where each group has
        /// one expression object that references a leaf input and the other expression object, which has two leaf inputs.
        /// 
        ///    O         O      ---> 2 root expressions
        ///  /   \     /   \    
        /// *     O   *     O   ---> 1 leaf input & 1 sub-expression per root expression
        ///      / \       / \  
        ///     *   *     *   * ---> 2 leaf inputs per sub-expression
        /// </summary>
        private static TestCaseData GetOneLeafTwoTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data =
                CombineData(GetOneLeafData(groupName, "1"), GetOneLeafData(groupName, "2"));

            return new TestCaseData(groupName,
                                    data.Item1,
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),
                                    data.Item2);
        }

        /// <summary>
        /// Gets three expression objects where one expression object references the other two expression objects.
        ///
        ///      O      ---> 1 root expressions
        ///    /   \    
        ///   O     O   ---> 2 sub-expressions
        ///  / \   / \  
        /// *   * *   * ---> 2 leaf inputs per sub-expression
        /// </summary>
        private static Tuple<ExpressionObject[], ExpressionTree[]> GetTwoBranchData(string groupName, string postFix)
        {
            var idBottom1 = $"[{groupName}]twoLeavesOneBranch1_{postFix}";
            const Operator opp = Operator.Add;

            Tuple<ExpressionObject, BranchNode> results1 = ConstructBranchNodeItems(idBottom1, groupName, opp, idBottom1);

            var idBottom2 = $"[{groupName}]twoLeavesOneBranch2_{postFix}";
            Tuple<ExpressionObject, BranchNode> results2 = ConstructBranchNodeItems(idBottom2, groupName, opp, idBottom2);

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

        /// <summary>
        /// Gets three expression objects where one expression object references the other two expression objects.
        ///
        ///      O      ---> 1 root expressions
        ///    /   \    
        ///   O     O   ---> 2 sub-expressions
        ///  / \   / \  
        /// *   * *   * ---> 2 leaf inputs per sub-expression
        /// </summary>
        private static TestCaseData GetTwoBranchOneTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data = GetTwoBranchData(groupName, "1");
            return new TestCaseData(groupName,
                                    data.Item1,
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),
                                    data.Item2);
        }

        /// <summary>
        /// Gets six expression objects that form two groups of three expression objects, where each group has
        /// one expression object that references the other two expression objects.
        ///
        ///      O           O      ---> 2 root expressions
        ///    /   \       /   \
        ///   O     O     O     O   ---> 2 sub-expressions per root expression
        ///  / \   / \   / \   / \
        /// *   * *   * *   * *   * ---> 2 leaf inputs per sub-expression
        /// </summary>
        private static TestCaseData GetTwoBranchTwoTree()
        {
            const string groupName = "groupName";
            Tuple<ExpressionObject[], ExpressionTree[]> data =
                CombineData(GetTwoBranchData(groupName, "1"), GetTwoBranchData(groupName, "2"));

            return new TestCaseData(groupName,
                                    data.Item1,
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),
                                    data.Item2);
        }

        /// <summary>
        /// Gets one expression object with references to an expression,
        /// but these referenced expressions are not included in this group.
        ///
        ///   O   ---> 1 root expression
        ///  / \
        /// *O *O ---> 2 leaf root expressions that are not part of this group 
        /// </summary>
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
            }, Array.Empty<RuleDataAccessObject>(), Array.Empty<ConditionDataAccessObject>(),  new[]
            {
                expectedResult
            });
        }

        /// <summary>
        ///   O   ? ---> 1 root expression & 1 other object (rule/condition)
        ///  / \ :
        /// *   O   ---> 1 leaf input & 1 sub-expression
        ///    / \
        ///   *   * ---> 2 leaf inputs for the sub-expression
        /// </summary>
        private static TestCaseData OneRootExpressionWithSubExpressionReferencedByOtherObject(string groupName, 
                                                                                              string idSharedExpression, 
                                                                                              RuleDataAccessObject[] rules, 
                                                                                              ConditionDataAccessObject[] conditions)
        {
            var idRootExpression = $"[{groupName}]root_expression";

            var sharedExpression = new ExpressionObject(idSharedExpression,
                                                        Operator.Divide,
                                                        new ParameterLeafReference("[Input]some_input_1"),
                                                        new ParameterLeafReference("[Input]some_input_2"),
                                                        idSharedExpression);
            var rootExpression = new ExpressionObject(idRootExpression,
                                                      Operator.Add,
                                                      new ParameterLeafReference("[Input]some_input_3"),
                                                      new ExpressionReference(idSharedExpression),
                                                      idRootExpression);

            ExpressionObject[] expressionObjects =
            {
                sharedExpression,
                rootExpression,
            };

            ExpressionTree[] expExpressionTrees =
            {
                CreateExpectedExpressionTree(sharedExpression, groupName),
                CreateExpectedExpressionTree(rootExpression, groupName),
            };

            return new TestCaseData(groupName,
                                    expressionObjects,
                                    rules,
                                    conditions,
                                    expExpressionTrees);
        }

        /// <summary>
        /// Gets three expression objects where one expression object is referenced by the two other expression objects.
        /// O   O   ---> 2 root expressions
        /// / \ / \
        /// *   O   * ---> 1 leaf input per root expression, 1 shared sub-expression between root expressions
        /// / \
        /// *   *   ---> 2 leaf inputs for the shared sub-expression
        /// </summary>
        private static TestCaseData TwoRootExpressionsReferencingSameSubExpression()
        {
            const string groupName = "groupName";

            var idRootExpression1 = $"[{groupName}]root_expression_1";
            var idRootExpression2 = $"[{groupName}]root_expression_2";
            var idSharedExpression = $"[{groupName}]shared_sub_expression";

            var sharedExpression = new ExpressionObject(idSharedExpression,
                                                        Operator.Divide,
                                                        new ParameterLeafReference("[Input]some_input_1"),
                                                        new ParameterLeafReference("[Input]some_input_2"),
                                                        idSharedExpression);
            var rootExpression1 = new ExpressionObject(idRootExpression1,
                                                       Operator.Add,
                                                       new ParameterLeafReference("[Input]some_input_3"),
                                                       new ExpressionReference(idSharedExpression),
                                                       idRootExpression1);
            var rootExpression2 = new ExpressionObject(idRootExpression2,
                                                       Operator.Subtract,
                                                       new ExpressionReference(idSharedExpression),
                                                       new ParameterLeafReference("[Input]some_input_4"),
                                                       idRootExpression2);

            ExpressionObject[] expressionObjects =
            {
                sharedExpression,
                rootExpression1,
                rootExpression2
            };

            ExpressionTree[] expExpressionTrees =
            {
                CreateExpectedExpressionTree(sharedExpression, groupName),
                CreateExpectedExpressionTree(rootExpression1, groupName),
                CreateExpectedExpressionTree(rootExpression2, groupName),
            };
            return new TestCaseData(groupName,
                                    expressionObjects,
                                    Array.Empty<RuleDataAccessObject>(),
                                    Array.Empty<ConditionDataAccessObject>(),
                                    expExpressionTrees);
        }

        private static ExpressionTree CreateExpectedExpressionTree(ExpressionObject expressionObject, string expControlGroupName)
        {
            var expectedBranchNode = new BranchNode(expressionObject.Operator, expressionObject.Y)
            {
                FirstNode = new ParameterLeafNode(expressionObject.FirstReference.Value),
                SecondNode = new ParameterLeafNode(expressionObject.SecondReference.Value),
            };

            return new ExpressionTree(expectedBranchNode,
                                      expControlGroupName,
                                      expressionObject.Id,
                                      new MathematicalExpression { Name = expressionObject.Y });
        }

        private static IEnumerable<TestCaseData> GetAssembleExpectedResultsData()
        {
            yield return GetEmptyTestData().SetName(nameof(GetEmptyTestData));

            //   O   ---> 1 root expression
            //  / \    
            // *   * ---> 2 leaf inputs
            yield return GetTwoLeavesOneTree().SetName(nameof(GetTwoLeavesOneTree));

            //    O      ---> 1 root expressions
            //  /   \    
            // *     O   ---> 1 leaf input & 1 sub-expression
            //      / \  
            //     *   * ---> 2 leaf inputs
            yield return GetOneLeafOneTree().SetName(nameof(GetOneLeafOneTree));

            //    O         O      ---> 2 root expressions
            //  /   \     /   \    
            // *     O   *     O   ---> 1 leaf input & 1 sub-expression per root expression
            //      / \       / \  
            //     *   *     *   * ---> 2 leaf inputs per sub-expression
            yield return GetOneLeafTwoTree().SetName(nameof(GetOneLeafTwoTree));

            //      O      ---> 1 root expressions
            //    /   \    
            //   O     O   ---> 2 sub-expressions
            //  / \   / \  
            // *   * *   * ---> 2 leaf inputs per sub-expression
            yield return GetTwoBranchOneTree().SetName(nameof(GetTwoBranchOneTree));

            //      O           O      ---> 2 root expressions
            //    /   \       /   \
            //   O     O     O     O   ---> 2 sub-expressions per root expression
            //  / \   / \   / \   / \
            // *   * *   * *   * *   * ---> 2 leaf inputs per sub-expression
            yield return GetTwoBranchTwoTree().SetName(nameof(GetTwoBranchTwoTree));

            //   O     O   ---> 2 root expressions
            //  / \   / \    
            // *   * *   * ---> 2 leaf inputs per root expression
            yield return GetTwoLeavesTwoTree().SetName(nameof(GetTwoLeavesTwoTree));

            //   O   ---> 1 root expression
            //  / \
            // *O *O ---> 2 leaf root expressions that are not part of this group 
            yield return GetTwoLeavesOneTreeWithReferences().SetName(nameof(GetTwoLeavesOneTreeWithReferences));
            
            //   O   O   ---> 2 root expressions
            //  / \ / \
            // *   O   * ---> 1 leaf input per root expression, 1 shared sub-expression between root expressions 
            //    / \
            //   *   *   ---> 2 leaf inputs for the shared sub-expression
            yield return TwoRootExpressionsReferencingSameSubExpression().SetName(nameof(TwoRootExpressionsReferencingSameSubExpression));

            const string groupName = "groupName";
            var idSubExpression = $"[{groupName}]shared_sub_expression";
            var rules = new[]
            {
                new RuleDataAccessObject("", Substitute.For<RuleBase>()) { InputReferences = { idSubExpression } }
            };
            
            var conditions = new[]
            {
                new ConditionDataAccessObject("", Substitute.For<ConditionBase>()) { InputReferences = { idSubExpression } }
            };
            
            //   O   R ---> 1 root expression & 1 rule
            //  / \ /
            // *   O   ---> 1 shared sub-expression between root expression and rule 
            //    / \
            //   *   * ---> 2 leaf inputs for the shared sub-expression
            yield return OneRootExpressionWithSubExpressionReferencedByOtherObject(groupName, idSubExpression, rules, Array.Empty<ConditionDataAccessObject>());
            
            //   O   C ---> 1 root expression & 1 condition
            //  / \ /
            // *   O   ---> 1 shared sub-expression between root expression and condition 
            //    / \
            //   *   * ---> 2 leaf inputs for the shared sub-expression
            yield return OneRootExpressionWithSubExpressionReferencedByOtherObject(groupName, idSubExpression, Array.Empty<RuleDataAccessObject>(), conditions);
        }
    }
}