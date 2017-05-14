using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DTSharp.Helper;

namespace DTSharp.DecisionTree
{
    /// <summary>
    /// Decision Tree Learning is a supervised machine learning technique that uses decision tree models.
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class DecisionTreeLearning<T>
    {
        Feature outputFeature;
        List<Feature> features;
        DecisionTreeOptions options;

        private DecisionTreeLearning()
        {
            features = new List<Feature>();
        }

        /// <summary>
        /// Builds a decision tree using specified dataset
        /// </summary>
        /// <param name="data">Learning Dataset</param>
        /// <returns>A decision tree</returns>
        public TreeNode Learn(IEnumerable<T> data)
        {
            return Learn(data.AsQueryable());
        }
        /// <summary>
        /// Builds a decision tree using specified dataset
        /// </summary>
        /// <param name="data">Learning Dataset</param>
        /// <returns>A decision tree</returns>
        public TreeNode Learn(IQueryable<T> data)
        {
            var root = new TreeNode();
            root.OutputProperties = InitOutputProperties(data);
            Learn(root, data);
            return root;
        }


        /// <summary>
        /// Adds a discrete feature
        /// </summary>
        /// <typeparam name="TFeature">Type of feature</typeparam>
        /// <param name="name">Name of the feature</param>
        /// <param name="selector">Expression to get feature value</param>
        /// <param name="type">Type of feature value</param>
        public void AddDiscreteFeature<TFeature>(string name, Expression<Func<T, TFeature>> selector)
        {
            features.Add(new Feature<T, TFeature>()
            {
                Type = FeatureType.Discrete,
                Name = name,
                GenericSelector = selector,
            });
        }
        /// <summary>
        /// Adds a continious feature
        /// </summary>
        /// <typeparam name="TFeature">Type of feature</typeparam>
        /// <param name="name">Name of the feature</param>
        /// <param name="selector">Expression to get feature value</param>
        /// <param name="type">Type of feature value</param>
        public void AddContiniousFeature<TFeature>(string name, Expression<Func<T, TFeature>> selector) where TFeature : IComparable
        {
            features.Add(new Feature<T, TFeature>()
            {
                Type = FeatureType.Continious,
                Name = name,
                GenericSelector = selector,
            });
        }



        void Learn(TreeNode node, IQueryable<T> data)
        {
            var maxSplitQuality = node.OutputProperties == null ? double.MinValue : options.SplitQualifier(node.OutputProperties) * node.OutputProperties.DataCount;
            foreach (var feature in features)
            {
                double splitQuality;
                var childNodes = Branch(feature, data, node, out splitQuality);
                if (splitQuality > maxSplitQuality)
                {
                    node.Feature = feature;
                    node.ChildNodes = childNodes;
                    maxSplitQuality = splitQuality;
                }
            }
            if (node.Feature != null)
            {
                //TODO: Root node has no output properties
                node.ChildNodes.ToList().ForEach(x => x.Value.Parent = node);
                if (node.Ancestors.Count() < options.MaxDepth)
                {

                    foreach (var nodeChildNode in node.ChildNodes)
                    {
                        var outputProperties = nodeChildNode.Value.OutputProperties as DecisionTreeOutputProperties;
                        if (outputProperties.DataCount < options.MinimumDataCountForBranch) continue;
                        var maxProbability = outputProperties.OutputDistribution.Max(x => x.Value / (double)outputProperties.DataCount);
                        if (maxProbability > options.HigherProbabilityLimitForBranch) continue;


                        var childNodeData = ApplyFilter(data, node.Feature, nodeChildNode.Key);
                        Learn(nodeChildNode.Value, childNodeData);
                    }
                }
            }
        }

        private IQueryable<T> ApplyFilter(IQueryable<T> data, Feature feature, FeatureValue featureValue)
        {
            if (featureValue is ContiniousFeatureValue)
            {
                var continiousFeatureValue = featureValue as ContiniousFeatureValue;
                var featureExpression = (feature.Selector as LambdaExpression).Body;
                var result = data;

                if (continiousFeatureValue.From != null)
                {
                    var expression1 = Expression.GreaterThanOrEqual(featureExpression, Expression.Constant(continiousFeatureValue.From));
                    var lambdaExpression1 = Expression.Lambda<Func<T, bool>>(expression1, (feature.Selector as LambdaExpression).Parameters);
                    result = result.Where(lambdaExpression1);
                }
                if (continiousFeatureValue.To != null)
                {
                    var expression2 = Expression.LessThan(featureExpression, Expression.Constant(continiousFeatureValue.To));
                    var lambdaExpression2 = Expression.Lambda<Func<T, bool>>(expression2, (feature.Selector as LambdaExpression).Parameters);
                    result = result.Where(lambdaExpression2);
                }
                return result;
            }
            else
            {
                var discreteFeatureValue = featureValue as DiscreteFeatureValue;
                var expression = Expression.Equal((feature.Selector as LambdaExpression).Body, Expression.Constant(discreteFeatureValue.Value));
                var lambdaExpression = Expression.Lambda<Func<T, bool>>(expression, (feature.Selector as LambdaExpression).Parameters);
                return data.Where(lambdaExpression);
            }
        }
        private Dictionary<FeatureValue, TreeNode> Branch(Feature feature, IQueryable<T> data, TreeNode parentNode, out double splitQuality)
        {
            return feature.Type == FeatureType.Discrete ? BranchDiscrete(feature, data, out splitQuality) : BranchContinious(feature, data, parentNode, out splitQuality);
        }
        private Dictionary<FeatureValue, TreeNode> BranchDiscrete(Feature feature, IQueryable<T> data, out double splitQuality)
        {
            GroupInfo[] groups = GroupFeatureWithOutput(feature, data);

            var result = new Dictionary<FeatureValue, TreeNode>();
            foreach (var featureValueGroup in groups.GroupBy(x => x.Input))
            {
                var node = new TreeNode();
                node.OutputProperties = new DecisionTreeOutputProperties()
                {
                    DataCount = featureValueGroup.Sum(x => x.Count),
                    OutputDistribution = featureValueGroup.ToDictionary(x => x.Output, x => x.Count)
                };
                result.Add(new DiscreteFeatureValue() { Value = featureValueGroup.Key }, node);
            }

            splitQuality = result.Values.Select(x => x.OutputProperties.DataCount * options.SplitQualifier.Invoke(x.OutputProperties)).Sum();

            return result;
        }

        private GroupInfo[] GroupFeatureWithOutput(Feature feature, IQueryable<T> data)
        {
            var arg = Expression.Parameter(typeof(T), "x");
            var groupByType = typeof(GroupBy<,>).MakeGenericType(feature.DataType, outputFeature.DataType);
            var ctor = Expression.New(groupByType);

            var inputExp = new ReplaceExpressionVisitor((feature.Selector as LambdaExpression).Parameters[0], arg)
                .Visit((feature.Selector as LambdaExpression).Body);
            var outputExp = new ReplaceExpressionVisitor((outputFeature.Selector as LambdaExpression).Parameters[0], arg)
               .Visit((outputFeature.Selector as LambdaExpression).Body);

            var bindExpInput = Expression.Bind(groupByType.GetProperty("Input"), inputExp);//TODO: FIX THIS
            var bindExpOutput = Expression.Bind(groupByType.GetProperty("Output"), outputExp);

            var groupByExpression = Expression.MemberInit(ctor, new MemberBinding[] { bindExpInput, bindExpOutput });
            var lambdaExpression = Expression.Lambda(groupByExpression, arg);
            var groups = IQueryableHelper.GroupBy(data, lambdaExpression, typeof(T), groupByType);

            var selectArgs = Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(groupByType, typeof(T)), "x");
            var selectCtor = Expression.New(typeof(GroupInfo));

            //int Count<TSource>(this IEnumerable<TSource> source);
            var countMethod = typeof(Enumerable).GetMethods().First(x => x.Name == "Count").MakeGenericMethod(typeof(T));

            var selectMemberInit = Expression.MemberInit(selectCtor,
                   Expression.Bind(typeof(GroupInfo).GetProperty("Input"), Expression.Convert(Expression.Property(Expression.Property(selectArgs, "Key"), "Input"), typeof(object))),
                   Expression.Bind(typeof(GroupInfo).GetProperty("Output"), Expression.Convert(Expression.Property(Expression.Property(selectArgs, "Key"), "Output"), typeof(object))),
                   Expression.Bind(typeof(GroupInfo).GetProperty("Count"), Expression.Call(countMethod, selectArgs))
                );
            var selectLambdaExpression = Expression.Lambda(selectMemberInit, selectArgs);

            var selectedGroups = IQueryableHelper.Select(groups, selectLambdaExpression, typeof(IGrouping<,>)
                .MakeGenericType(groupByType, typeof(T)), typeof(GroupInfo)) as IQueryable<GroupInfo>;
            return selectedGroups.ToArray();
            //var groups = this.GetType().GetMethod("GenericGroup")
            //    .MakeGenericMethod(feature.DataType, OutputFeature.DataType)
            //    .Invoke(this, new object[] { data, groupByExpression, arg }) as GroupInfo[];
        }

        private OutputProperties InitOutputProperties(IQueryable<T> data)
        {
            var arg = Expression.Parameter(typeof(T), "x");

            var selectArgs = Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(outputFeature.DataType, typeof(T)), "x");
            var selectCtor = Expression.New(typeof(GroupInfo));
            //int Count<TSource>(this IEnumerable<TSource> source);
            var countMethod = typeof(Enumerable).GetMethods().First(x => x.Name == "Count").MakeGenericMethod(typeof(T));

            var selectMemberInit = Expression.MemberInit(selectCtor,
                   Expression.Bind(typeof(GroupInfo).GetProperty("Output"), Expression.Convert(Expression.Property(selectArgs, "Key"), typeof(object))),
                   Expression.Bind(typeof(GroupInfo).GetProperty("Count"), Expression.Call(countMethod, selectArgs))
                );
            var selectLambdaExpression = Expression.Lambda(selectMemberInit, selectArgs);


            var groupedData = IQueryableHelper.GroupBy(data, outputFeature.Selector, typeof(T), outputFeature.DataType);
            var groups = (IQueryableHelper.Select(groupedData, selectLambdaExpression, typeof(IGrouping<,>).MakeGenericType(outputFeature.DataType, typeof(T)),
                typeof(GroupInfo)) as IQueryable<GroupInfo>).ToArray();

            return new DecisionTreeOutputProperties()
            {
                DataCount = groups.Sum(x => x.Count),
                OutputDistribution = groups.ToDictionary(x => x.Output, x => x.Count),
            };
        }

        Dictionary<FeatureValue, TreeNode> BranchContinious(Feature feature, IQueryable<T> data, TreeNode parentNode, out double splitQuality)
        {
            IQueryable<InputOutputPair> orderedInputOutput = BranchContiniousData(feature, data);

            var lowerNode = new DecisionTreeOutputProperties()
            {
                DataCount = 0,
                OutputDistribution = (parentNode.OutputProperties as DecisionTreeOutputProperties).OutputDistribution.ToDictionary(x => x.Key, x => 0)
            };
            var higherNode = CloneOutputProperties(parentNode.OutputProperties as DecisionTreeOutputProperties);
            var maxSplitQuality = double.MinValue;
            DecisionTreeOutputProperties maxLowerNode = null;
            object limitInput = null;
            foreach (InputOutputPair value in orderedInputOutput)
            {
                lowerNode.DataCount++;
                higherNode.DataCount--;
                lowerNode.OutputDistribution[value.Output]++;
                higherNode.OutputDistribution[value.Output]--;
                var tempSplitQuality = options.SplitQualifier(lowerNode) * lowerNode.DataCount + options.SplitQualifier(higherNode) * higherNode.DataCount;
                if (tempSplitQuality > maxSplitQuality)
                {
                    maxSplitQuality = tempSplitQuality;
                    maxLowerNode = CloneOutputProperties(lowerNode);
                    limitInput = value.Input;
                }
            }
            splitQuality = maxSplitQuality;
            if (maxLowerNode == null) return new Dictionary<FeatureValue, TreeNode>();


            var result = new Dictionary<FeatureValue, TreeNode>();
            result.Add(new ContiniousFeatureValue() { To = limitInput as IComparable, },
                new TreeNode() { OutputProperties = lowerNode, Parent = parentNode });
            result.Add(new ContiniousFeatureValue() { From = limitInput as IComparable, },
                new TreeNode() { OutputProperties = higherNode, Parent = parentNode });
            return result;
        }

        private IQueryable<InputOutputPair> BranchContiniousData(Feature feature, IQueryable<T> data)
        {
            var orderedData = IQueryableHelper.OrderBy(data, feature.Selector, typeof(T), feature.DataType);

            var arg = Expression.Parameter(typeof(T), "x");
            var ctor = Expression.New(typeof(InputOutputPair));
            var inputExpression = ExpressionHelper.ReplaceLambdaParameter(feature.Selector as LambdaExpression, arg);
            var outputExpression = ExpressionHelper.ReplaceLambdaParameter(outputFeature.Selector as LambdaExpression, arg);

            var selectMemberInit = Expression.MemberInit(ctor,
                Expression.Bind(typeof(InputOutputPair).GetProperty("Input"), Expression.Convert(inputExpression, typeof(object))),
                Expression.Bind(typeof(InputOutputPair).GetProperty("Output"), Expression.Convert(outputExpression, typeof(object)))
             );
            var selectLambdaExpression = Expression.Lambda(selectMemberInit, arg);
            var orderedInputOutput = IQueryableHelper.Select(orderedData, selectLambdaExpression, typeof(T), typeof(InputOutputPair)) as IQueryable<InputOutputPair>;
            return orderedInputOutput;
        }

        private DecisionTreeOutputProperties CloneOutputProperties(DecisionTreeOutputProperties outputProperties)
        {
            return new DecisionTreeOutputProperties()
            {
                DataCount = outputProperties.DataCount,
                OutputDistribution = outputProperties.OutputDistribution.ToDictionary(x => x.Key, x => x.Value)
            };
        }


        /// <summary>
        /// Creates a decision tree learning class.
        /// </summary>
        /// <typeparam name="TOutputType">Output Feature Type</typeparam>
        /// <param name="outputSelector">Expression to get output value</param>
        /// <param name="options">Decision tree options</param>
        /// <returns>Decision tree learning class</returns>
        public static DecisionTreeLearning<T> Create<TOutputType>(Expression<Func<T, TOutputType>> outputSelector,
                DecisionTreeOptions options)
        {
            return new DecisionTreeLearning<T>()
            {
                outputFeature = new Feature<T, TOutputType>()
                {
                    Name = "Output",
                    GenericSelector = outputSelector,
                    Type = FeatureType.Discrete,
                },
                options = options
            };
        }


    }
    class GroupInfo
    {
        public object Input { get; set; }
        public object Output { get; set; }
        public int Count { get; set; }
    }
    class InputOutputPair
    {
        public object Input { get; set; }
        public object Output { get; set; }
    }

    class GroupBy<T1, T2>
    {
        public T1 Input { get; set; }
        public T2 Output { get; set; }

        public override int GetHashCode()
        {
            return Input.GetHashCode() + Output.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is GroupBy<T1, T2>)
            {
                return (obj as GroupBy<T1, T2>).Input.Equals(Input) && (obj as GroupBy<T1, T2>).Output.Equals(Output);
            }
            return false;
        }
    }
    class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _oldValue)
                return _newValue;
            return base.Visit(node);
        }
    }

}
