using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DTSharp
{
    /// <summary>
    /// Data structure for a decision tree
    /// </summary>
    public class TreeNode
    {
        [XmlIgnore]
        public TreeNode Parent { get; set; }



        public OutputProperties OutputProperties { get; set; }
        public Feature Feature { get; set; }
        public Dictionary<FeatureValue, TreeNode> ChildNodes { get; set; } = new Dictionary<FeatureValue, TreeNode>();



        [XmlIgnore]
        public IEnumerable<TreeNode> Ancestors
        {
            get
            {
                var cursor = Parent;
                while (cursor != null)
                {
                    yield return cursor;
                    cursor = cursor.Parent;
                }
            }
        }
        public bool IsRoot => Parent == null;
        public bool IsLeaf => ChildNodes.Count == 0;

        public IEnumerable<TreeNode> DescendantsAndSelf
        {
            get
            {
                return new[] { this }.Concat(ChildNodes.Select(x => x.Value).SelectMany(x => x.DescendantsAndSelf));
            }
        }


        public TreeNode FindNode(object obj)
        {
            if (Feature == null)
                return this;
            var featureValue = (Feature.Selector as LambdaExpression).Compile().DynamicInvoke(obj);
            if (Feature.Type == FeatureType.Discrete)
                return ChildNodes.Where(x => (x.Key as DiscreteFeatureValue).Value.Equals(featureValue))
                    .Select(x => x.Value.FindNode(obj))
                    .FirstOrDefault();
            return ChildNodes
                    .Where(x => (x.Key as ContiniousFeatureValue).From.CompareTo(featureValue as IComparable) <= 0)
                    .Where(x => (x.Key as ContiniousFeatureValue).To.CompareTo(featureValue as IComparable) > 0)
                    .Select(x => x.Value.FindNode(obj))
                    .FirstOrDefault();
        }
    }
}
