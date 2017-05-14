using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp.DecisionTree
{
    public delegate double DecisionTreeSplitQualifier(OutputProperties properties);
    public static class DecisionTreeSplitQualifiers
    {
        /// <summary>
        /// Gini Impurity 
        /// 1 is best
        /// 0.5 is worst
        /// </summary>
        public static DecisionTreeSplitQualifier GiniImpurity
        {
            get
            {
                return new DecisionTreeSplitQualifier(properties =>
                {
                    if (properties.DataCount == 0) return double.MinValue;
                    return 1 - (properties as DecisionTreeOutputProperties).OutputDistribution.Select(x => x.Value / (double)properties.DataCount).Sum(x => x * (1.0 - x));
                });
            }
        }

        /// <summary>
        /// Information Gain
        /// 0 is Best
        /// negative intinity worst
        /// </summary>
        public static DecisionTreeSplitQualifier InformationGain
        {
            get
            {
                return new DecisionTreeSplitQualifier(properties =>
                {
                    if (properties.DataCount == 0) return double.MinValue;
                    return (properties as DecisionTreeOutputProperties).OutputDistribution
                                    .Where(x => x.Value != 0)
                                    .Select(x => x.Value / (double)properties.DataCount)
                                    .Sum(x => x * Math.Log(x, 2));
                });
            }
        }
        /// <summary>
        /// Information Gain
        /// 0 is Best
        /// if all is equal return value is min.
        /// </summary>
        /// <summary>
        /// Information Gain
        /// 0 is Best
        /// negative infinity is worst
        /// if all is equal return value is min.
        /// </summary>
        public static DecisionTreeSplitQualifier Entropy
        {
            get
            {
                return new DecisionTreeSplitQualifier(properties =>
                {
                    if (properties.DataCount == 0) return double.MinValue;
                    return (properties as DecisionTreeOutputProperties).OutputDistribution
                                    .Where(x => x.Value != 0)
                                    .Where(x => x.Value != properties.DataCount)
                                    .Select(x => x.Value / (double)properties.DataCount)
                                    .Sum(x => x * Math.Log(x));
                });
            }
        }
    }
}
