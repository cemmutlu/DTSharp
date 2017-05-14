using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp.DecisionTree
{
    public class DecisionTreeOptions
    {
        public int MaxDepth { get; set; } = 5;
        public int MinimumDataCountForBranch { get; set; } = 2;
        public double HigherProbabilityLimitForBranch { get; set; } = 1.0;
        public DecisionTreeSplitQualifier SplitQualifier { get; set; } = DecisionTreeSplitQualifiers.Entropy;




        public double MinimumSplitQuality { get; set; } = double.MinValue;
        public double MaximumSplitQuality { get; set; } = double.MaxValue;

    }
}
