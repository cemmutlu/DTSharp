using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp.DecisionTree
{
    public class DecisionTree : TreeNode
    {
        public object GetOutput(object data)
        {
            var node = this.FindNode(data);
            var outputProperties = node.OutputProperties as DecisionTreeOutputProperties;
            var bestOutput = outputProperties.OutputDistribution.OrderByDescending(x => x.Value).First();
            return bestOutput.Key;
        }
    }
}
